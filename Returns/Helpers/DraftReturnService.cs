using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Reflection;

namespace Returns.Helpers
{
    public class DraftReturnService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<DraftReturnService> _logger;
        private readonly IExcelImportService _excelImportService;

        public DraftReturnService(
            ReturnsDbContext context, 
            ILogger<DraftReturnService> logger,
            IExcelImportService excelImportService)
        {
            _context = context;
            _logger = logger;
            _excelImportService = excelImportService;
        }

        public async Task<DraftReturnResult> SaveDraftAsync(DraftReturnDto draftDto, string saccoId, string saccoType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Create or update draft return
                var draftReturn = await GetOrCreateDraftReturn(draftDto.ReturnId, saccoId, saccoType, draftDto.Period);
                
                // 2. Process each form upload
                var processedForms = new List<ProcessedFormResult>();
                
                foreach (var upload in draftDto.FormUploads)
                {
                    if (upload.FormFile == null) continue;
                    
                    var form = await _context.ReturnForms
                        .Include(f => f.Period)
                        .FirstOrDefaultAsync(f => f.Id == upload.FormId);
                        
                    if (form == null)
                    {
                        processedForms.Add(new ProcessedFormResult
                        {
                            FormId = upload.FormId,
                            Success = false,
                            Message = $"Form with ID {upload.FormId} not found"
                        });
                        continue;
                    }
                    
                    // Import data from Excel
                    var importedData = await _excelImportService.ImportFormDataAsync(upload.FormFile, form);
                    
                    // Save draft data to appropriate child table
                    var saveResult = await SaveDraftFormDataAsync(draftReturn.Id, form, importedData, upload.FormFile);
                    
                    processedForms.Add(saveResult);
                }
                
                await transaction.CommitAsync();
                
                return new DraftReturnResult
                {
                    Success = true,
                    ReturnId = draftReturn.Id,
                    ProcessedForms = processedForms
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving draft return");
                
                return new DraftReturnResult
                {
                    Success = false,
                    Message = $"Error saving draft: {ex.Message}"
                };
            }
        }

        private async Task<Return> GetOrCreateDraftReturn(string? returnId, string saccoId, string saccoType, string period)
        {
            Return? draftReturn = null;
            
            if (!string.IsNullOrEmpty(returnId))
            {
                draftReturn = await _context.Returns
                    .FirstOrDefaultAsync(r => r.Id == returnId && r.IsDraft);
            }
            
            if (draftReturn == null)
            {
                draftReturn = new Return
                {
                    Id = Guid.NewGuid().ToString(),
                    SaccoId = saccoId,
                    SaccoType = saccoType,
                    Year = period,
                    IsDraft = true,
                    IsActiveVersion = false,
                    VersionNumber = 1,
                    CreatedAt = DateTime.Now,
                    SubmittedAt = DateTime.Now
                };
                
                await _context.Returns.AddAsync(draftReturn);
                await _context.SaveChangesAsync();
            }
            
            return draftReturn;
        }

        private async Task<ProcessedFormResult> SaveDraftFormDataAsync(
            string returnId, 
            ReturnForm form, 
            object importedData,
            IFormFile file)
        {
            try
            {
                var formType = DetermineFormType(form);
                var childEntity = await CreateChildEntityAsync(returnId, form.Id, formType, importedData);
                
                if (childEntity == null)
                {
                    return new ProcessedFormResult
                    {
                        FormId = form.Id,
                        Success = false,
                        Message = $"Could not create child entity for form type: {formType}"
                    };
                }
                
                // Save file
                var filePath = await FormsHelper.SaveFileAsync(file, formType, form.FormName);
                SetFilePath(childEntity, filePath);
                
                // Update common fields using reflection
                UpdateCommonFields(childEntity, returnId, form.Id);
                
                // Add to context based on type
                await AddChildEntityToContextAsync(childEntity, formType);
                await _context.SaveChangesAsync();
                
                return new ProcessedFormResult
                {
                    FormId = form.Id,
                    FormName = form.FormName,
                    Success = true,
                    Message = "Draft saved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving draft form data for {FormName}", form.FormName);
                return new ProcessedFormResult
                {
                    FormId = form.Id,
                    FormName = form.FormName,
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private async Task<object?> CreateChildEntityAsync(string returnId, string formId, string formType, object importedData)
        {
            // Map imported data to entity based on form type
            return formType switch
            {
                "CapitalAdequacy" => MapToCapitalAdequacyEntity(importedData),
                "Liquidity" => MapToLiquidityEntity(importedData),
                "DepositReturn" => MapToDepositReturnEntity(importedData),
                "RiskClassification" => MapToRiskClassificationEntity(importedData),
                "Investment" => MapToInvestmentEntity(importedData),
                "FinancialPosition" => MapToFinancialPositionEntity(importedData),
                "ComprehensiveIncome" => MapToComprehensiveIncomeEntity(importedData),
                _ => null
            };
        }

        private void UpdateCommonFields(object entity, string returnId, string formId)
        {
            var returnIdProp = entity.GetType().GetProperty("ReturnId");
            var formIdProp = entity.GetType().GetProperty("FormId");
            var isDraftProp = entity.GetType().GetProperty("IsDraft");
            var createdAtProp = entity.GetType().GetProperty("CreatedAt");
            
            returnIdProp?.SetValue(entity, returnId);
            formIdProp?.SetValue(entity, formId);
            isDraftProp?.SetValue(entity, true);
            createdAtProp?.SetValue(entity, DateTime.Now);
        }

        private void SetFilePath(object entity, string? filePath)
        {
            var filePathProp = entity.GetType().GetProperty("FilePath");
            filePathProp?.SetValue(entity, filePath);
        }

        private async Task AddChildEntityToContextAsync(object entity, string formType)
        {
            // Handle single entities
            if (entity is DTCapitalAdequacyReturn dtCapital)
            {
                await _context.DTCapitalAdequacyReturns.AddAsync(dtCapital);
            }
            else if (entity is DTLiquidityReturn dtLiquidity)
            {
                await _context.DTLiquidityReturns.AddAsync(dtLiquidity);
            }
            else if (entity is DTInvestmentReturn dtInvestment)
            {
                await _context.DTInvestmentReturns.AddAsync(dtInvestment);
            }
            else if (entity is DTFinancialPositionReturn dtFinancial)
            {
                await _context.DTFinancialPositionReturns.AddAsync(dtFinancial);
            }
            else if (entity is DTComprehensiveIncomeReturn dtIncome)
            {
                await _context.DTComprehensiveIncomeReturns.AddAsync(dtIncome);
            }
            // NWDT entities
            else if (entity is NWDTCapitalAdequacyReturn nwdtCapital)
            {
                await _context.NWDTCapitalAdequacyReturns.AddAsync(nwdtCapital);
            }
            else if (entity is NWDTLiquidityReturn nwdtLiquidity)
            {
                await _context.NDWTLiquidityReturns.AddAsync(nwdtLiquidity);
            }
            else if (entity is NWDTInvestmentReturn nwdtInvestment)
            {
                await _context.NWDTInvestmentReturns.AddAsync(nwdtInvestment);
            }
            else if (entity is NWDTFinancialPositionReturn nwdtFinancial)
            {
                await _context.NWDTFinancialPositionReturns.AddAsync(nwdtFinancial);
            }
            else if (entity is NWDTComprehensiveIncomeReturn nwdtIncome)
            {
                await _context.NWDTComprehensiveIncomeReturns.AddAsync(nwdtIncome);
            }
            // Handle collections (like deposit returns and risk classifications)
            else if (entity is List<DepositReturn> deposits)
            {
                await _context.DepositReturns.AddRangeAsync(deposits);
            }
            else if (entity is List<DTRiskClassificationReturn> risks)
            {
                await _context.DTRiskClassificationReturns.AddRangeAsync(risks);
            }
            else if (entity is List<NWDTDepositReturn> nwdtDeposits)
            {
                await _context.NWDTDepositReturns.AddRangeAsync(nwdtDeposits);
            }
            else if (entity is List<NWDTRiskClassificationReturn> nwdtRisks)
            {
                await _context.NWDTRiskClassificationReturns.AddRangeAsync(nwdtRisks);
            }
            else
            {
                // Fallback to reflection-based approach for unknown types
                var contextProperty = _context.GetType()
                    .GetProperties()
                    .FirstOrDefault(p => p.PropertyType.IsGenericType && 
                                       p.PropertyType.GetGenericArguments()[0] == entity.GetType());
                                       
                if (contextProperty != null)
                {
                    var dbSet = contextProperty.GetValue(_context);
                    var addMethod = dbSet?.GetType().GetMethod("Add");
                    addMethod?.Invoke(dbSet, new[] { entity });
                }
                else
                {
                    throw new NotSupportedException($"Entity type {entity.GetType().Name} is not supported");
                }
            }
        }

        private string DetermineFormType(ReturnForm form)
        {
            if (form.IsCapitalAdequencyForm) return "CapitalAdequacy";
            if (form.IsLiquidityStatement) return "Liquidity";
            if (form.IsDepositReturnForm) return "DepositReturn";
            if (form.IsRiskClassification) return "RiskClassification";
            if (form.IsInvestmentReturn) return "Investment";
            if (form.IsFinancialPosition) return "FinancialPosition";
            if (form.IsStatementOfComprehensiveIncome) return "ComprehensiveIncome";
            return "Other";
        }

        // Mapping methods (simplified - you'll need to implement based on your actual mappings)
        private object MapToCapitalAdequacyEntity(object importedData)
        {
            if (importedData is Form1Statement form1)
            {
                var entity = new DTCapitalAdequacyReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form1.StartDate,
                    EndDate = form1.EndDate,
                    Period = form1.Period,
                    IsDraft = true,
                    Year = form1.EndDate.Year.ToString(),
                    Frequency = DetermineFrequency(form1.Period)
                };

                // Map rows to entity properties
                foreach (var row in form1.Rows)
                {
                    switch (row.Index)
                    {
                        case "1.1": entity.ShareCapital = row.Amount ?? 0; break;
                        case "1.2": entity.StatutoryReserves = row.Amount ?? 0; break;
                        case "1.3": entity.RetainedEarningsAccumulatedLosses = row.Amount ?? 0; break;
                        case "1.4": entity.NetSurplusAfterTaxCurrentYearToDate = row.Amount ?? 0; break;
                        case "1.5": entity.CapitalGrantsEquityInNature = row.Amount ?? 0; break;
                        case "1.6": entity.GeneralReserves = row.Amount ?? 0; break;
                        case "1.7": entity.OtherReserves = row.Amount ?? 0; break;
                        case "1": entity.SubTotalCoreCapital = row.Amount ?? 0; break;
                        case "2.1": entity.InvestmentsInSubsidiaryAndEquityInstruments = row.Amount ?? 0; break;
                        case "2.2": entity.OtherDeductions = row.Amount ?? 0; break;
                        case "2": entity.TotalDeductions = row.Amount ?? 0; break;
                        case "3": entity.CoreCapital = row.Amount ?? 0; break;
                        case "4": entity.InstitutionalCapital = row.Amount ?? 0; break;
                        // Add more mappings as needed
                    }
                }
                
                return entity;
            }
            else if (importedData is Form2AStatement form2A) // NWDT
            {
                var entity = new NWDTCapitalAdequacyReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form2A.StartDate,
                    EndDate = form2A.EndDate,
                    Period = form2A.Period,
                    IsDraft = true
                };
                
                // Similar mapping for NWDT forms
                foreach (var row in form2A.Rows)
                {
                    // Map NWDT specific fields
                }
                
                return entity;
            }
            
            return null;
        }

        private object MapToLiquidityEntity(object importedData)
        {
            if (importedData is Form2Statement form2)
            {
                var entity = new DTLiquidityReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form2.StartDate,
                    EndDate = form2.EndDate,
                    Period = form2.Period,
                    IsDraft = true,
                    Year = form2.EndDate.Year.ToString(),
                    Frequency = DetermineFrequency(form2.Period)
                };

                foreach (var row in form2.Rows)
                {
                    switch (row.Index)
                    {
                        case "1.1": entity.LocalNotesAndCoins = row.Amount ?? 0; break;
                        case "1.2": entity.ForeignNotesAndCoins = row.Amount ?? 0; break;
                        case "1": entity.TotalNotesAndCoins = row.Amount ?? 0; break;
                        case "2.1": entity.BalancesWithCommercialBanks = row.Amount ?? 0; break;
                        case "2.2": entity.TimeDepositsWithBanksMoreThan90Days = row.Amount ?? 0; break;
                        case "2.3": entity.OverdraftsAndMaturedLoans = row.Amount ?? 0; break;
                        case "2": entity.NetBankBalances = row.Amount ?? 0; break;
                        // Add more mappings
                    }
                }

                return entity;
            }
            else if (importedData is Form2BStatement form2B) // NWDT
            {
                var entity = new NWDTLiquidityReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form2B.StartDate,
                    EndDate = form2B.EndDate,
                    Period = form2B.Period,
                    IsDraft = true
                };

                // Map NWDT liquidity fields
                return entity;
            }

            return null;
        }

        private object MapToDepositReturnEntity(object importedData)
        {
            var entities = new List<DepositReturn>();

            if (importedData is Form3Statement form3)
            {
                foreach (var row in form3.Rows)
                {
                    var entity = new DepositReturn
                    {
                        Id = Guid.NewGuid().ToString(),
                        RangeName = row.RangeName,
                        DepositType = row.DepositType,
                        NumberOfAccounts = row.NumberOfAccounts,
                        AmountInKshs000 = row.AmountInKshs000,
                        StartDate = form3.StartDate,
                        EndDate = form3.EndDate,
                        Period = form3.Period,
                        Year = form3.EndDate.Year.ToString(),
                        IsDraft = true
                    };
                    entities.Add(entity);
                }
            }
            else if (importedData is Form2CStatement form2C) // NWDT
            {
                foreach (var row in form2C.Rows)
                {
                    var entity = new NWDTDepositReturn
                    {
                        Id = Guid.NewGuid().ToString(),
                        RangeName = row.Range,
                        DepositType = row.DepositType,
                        NumberOfAccounts = row.NumberOfAccounts,
                        AmountInKshs000 = row.Amount,
                        StartDate = form2C.StartDate,
                        EndDate = form2C.EndDate,
                        Period = form2C.Period,
                        IsDraft = true
                    };
                    entities.Add(entity);
                }
            }

            return entities;
        }

        private object MapToRiskClassificationEntity(object importedData)
        {
            var entities = new List<DTRiskClassificationReturn>();

            if (importedData is Form4Statement form4)
            {
                foreach (var row in form4.Rows)
                {
                    if (row.LoanType != "Total") // Skip total rows
                    {
                        var entity = new DTRiskClassificationReturn
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoanType = row.LoanType,
                            Classification = row.Classification,
                            NumberOfAccounts = row.NumberOfAccounts ?? 0,
                            OutstandingLoanPortfolio = row.OutstandingLoanPortfolio ?? 0,
                            RequiredProvision = row.RequiredProvision ?? 0,
                            RequiredProvisionAmount = row.RequiredProvisionAmount ?? 0,
                            StartDate = form4.StartDate,
                            EndDate = form4.EndDate,
                            Period = form4.Period,
                            Year = form4.EndDate.Year.ToString(),
                            IsDraft = true
                        };
                        entities.Add(entity);
                    }
                }
            }
            else if (importedData is Form2DStatement form2D) // NWDT
            {
                foreach (var row in form2D.Rows)
                {
                    if (row.LoanType != "Total")
                    {
                        var entity = new NWDTRiskClassificationReturn
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoanType = row.LoanType,
                            Classification = row.Classification,
                            NumberOfAccounts = row.NumberOfAccounts ?? 0,
                            OutstandingLoanPortfolio = row.OutstandingLoanPortfolio ?? 0,
                            RequiredProvision = row.RequiredProvision ?? 0,
                            RequiredProvisionAmount = row.RequiredProvisionAmount ?? 0,
                            StartDate = form2D.StartDate,
                            EndDate = form2D.EndDate,
                            Period = form2D.Period,
                            IsDraft = true
                        };
                        entities.Add(entity);
                    }
                }
            }

            return entities;
        }

        private object MapToInvestmentEntity(object importedData)
        {
            if (importedData is Form5Statement form5)
            {
                var entity = new DTInvestmentReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form5.StartDate,
                    EndDate = form5.EndDate,
                    Period = form5.Period,
                    Year = form5.EndDate.Year.ToString(),
                    IsDraft = true
                };

                foreach (var row in form5.Rows)
                {
                    switch (row.Index)
                    {
                        case "1": entity.CoreCapital = row.Amount ?? 0; break;
                        case "2": entity.TotalAssets = row.Amount ?? 0; break;
                        case "3": entity.TotalDeposits = row.Amount ?? 0; break;
                        case "4": entity.NonEarningAssets = row.Amount ?? 0; break;
                        case "5": entity.FinancialInvestments = row.Amount ?? 0; break;
                        case "6": entity.LandAndBuildings = row.Amount ?? 0; break;
                        // Add ratio calculations
                    }
                }

                // Calculate ratios
                if (entity.TotalAssets > 0)
                {
                    entity.LandBuildingsToTotalAssetsRatio = (entity.LandAndBuildings / entity.TotalAssets) * 100;
                    entity.NonEarningAssetsToTotalAssetsRatio = (entity.NonEarningAssets / entity.TotalAssets) * 100;
                }

                return entity;
            }
            else if (importedData is Form2EStatement form2E) // NWDT
            {
                var entity = new NWDTInvestmentReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form2E.StartDate,
                    EndDate = form2E.EndDate,
                    Period = form2E.Period,
                    IsDraft = true
                };

                // Map NWDT investment fields
                return entity;
            }

            return null;
        }

        private object MapToFinancialPositionEntity(object importedData)
        {
            if (importedData is Form6Statement form6)
            {
                var entity = new DTFinancialPositionReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form6.StartDate,
                    EndDate = form6.EndDate,
                    Period = form6.Period,
                    Year = form6.EndDate.Year.ToString(),
                    IsDraft = true
                };

                foreach (var row in form6.Rows)
                {
                    switch (row.RefNumber)
                    {
                        case "1.1": entity.CashInHand = row.Amount ?? 0; break;
                        case "1.2": entity.CashAtBank = row.Amount ?? 0; break;
                        case "2": entity.PrepaymentsAndSundryReceivables = row.Amount ?? 0; break;
                        case "3.1": entity.GovernmentSecurities = row.Amount ?? 0; break;
                        case "3.2": entity.OtherSecurities = row.Amount ?? 0; break;
                        case "3.3": entity.BalancesWithOtherSaccos = row.Amount ?? 0; break;
                        case "3.4": entity.InvestmentsInCompanies = row.Amount ?? 0; break;
                        case "4.1": entity.GrossLoanPortfolio = row.Amount ?? 0; break;
                        case "4.2": entity.AllowanceForLoanLoss = row.Amount ?? 0; break;
                        // Add more mappings
                    }
                }

                return entity;
            }
            else if (importedData is Form2GStatement form2G) // NWDT
            {
                var entity = new NWDTFinancialPositionReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form2G.StartDate,
                    EndDate = form2G.EndDate,
                    Period = form2G.Period,
                    IsDraft = true
                };

                // Map NWDT financial position fields
                return entity;
            }

            return null;
        }

        private object MapToComprehensiveIncomeEntity(object importedData)
        {
            if (importedData is Form7Statement form7)
            {
                var entity = new DTComprehensiveIncomeReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form7.StartDate,
                    EndDate = form7.EndDate,
                    Period = form7.Period,
                    Year = form7.EndDate.Year.ToString(),
                    IsDraft = true
                };

                foreach (var row in form7.Rows)
                {
                    switch (row.RefNumber)
                    {
                        case "1.1": entity.InterestOnLoanPortfolio = row.Amount ?? 0; break;
                        case "1.2": entity.FeesAndCommissionOnLoanPortfolio = row.Amount ?? 0; break;
                        case "2.1": entity.GovernmentSecurities = row.Amount ?? 0; break;
                        case "2.2": entity.DepositsWithBanks = row.Amount ?? 0; break;
                        case "2.3": entity.OtherInvestments = row.Amount ?? 0; break;
                        case "2.4": entity.OtherOperatingIncome = row.Amount ?? 0; break;
                        case "3.1": entity.InterestExpenseOnDeposits = row.Amount ?? 0; break;
                        case "3.2": entity.CostOfExternalBorrowings = row.Amount ?? 0; break;
                        case "3.3": entity.DividendExpenses = row.Amount ?? 0; break;
                        // Add more mappings
                    }
                }

                return entity;
            }
            else if (importedData is Form2FStatement form2F) // NWDT
            {
                var entity = new NWDTComprehensiveIncomeReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form2F.StartDate,
                    EndDate = form2F.EndDate,
                    Period = form2F.Period,
                    IsDraft = true
                };

                // Map NWDT comprehensive income fields
                return entity;
            }

            return null;
        }

        private string DetermineFrequency(string period)
        {
            // Parse period string to determine frequency
            if (period.Contains("Quarter") || period.Contains("Q"))
                return "Quarterly";
            if (period.Contains("Month"))
                return "Monthly";
            if (period.Contains("Year"))
                return "Annual";
            if (period.Contains("Semi"))
                return "Semi-Annual";
            
            return "Unknown";
        }
    }

    // DTOs for the service
    public class DraftReturnDto
    {
        public string? ReturnId { get; set; } // Null for new drafts
        public string Period { get; set; }
        public List<FormUploadDto> FormUploads { get; set; } = new();
    }

    public class FormUploadDto
    {
        public string FormId { get; set; }
        public IFormFile? FormFile { get; set; }
        public string? ExpectedReturnId { get; set; }
    }

    public class DraftReturnResult
    {
        public bool Success { get; set; }
        public string? ReturnId { get; set; }
        public string? Message { get; set; }
        public List<ProcessedFormResult> ProcessedForms { get; set; } = new();
    }

    public class ProcessedFormResult
    {
        public string FormId { get; set; }
        public string? FormName { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}