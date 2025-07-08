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
                return new DTCapitalAdequacyReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = form1.StartDate,
                    EndDate = form1.EndDate,
                    Period = form1.Period,
                    IsDraft = true
                    // Map other fields as needed
                };
            }
            return null;
        }

        private object MapToLiquidityEntity(object importedData)
        {
            // Implement mapping logic
            return new DTLiquidityReturn { Id = Guid.NewGuid().ToString(), IsDraft = true };
        }

        private object MapToDepositReturnEntity(object importedData)
        {
            // Implement mapping logic
            return new List<DepositReturn>();
        }

        private object MapToRiskClassificationEntity(object importedData)
        {
            // Implement mapping logic
            return new List<DTRiskClassificationReturn>();
        }

        private object MapToInvestmentEntity(object importedData)
        {
            // Implement mapping logic
            return new DTInvestmentReturn { Id = Guid.NewGuid().ToString(), IsDraft = true };
        }

        private object MapToFinancialPositionEntity(object importedData)
        {
            // Implement mapping logic
            return new DTFinancialPositionReturn { Id = Guid.NewGuid().ToString(), IsDraft = true };
        }

        private object MapToComprehensiveIncomeEntity(object importedData)
        {
            // Implement mapping logic
            return new DTComprehensiveIncomeReturn { Id = Guid.NewGuid().ToString(), IsDraft = true };
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