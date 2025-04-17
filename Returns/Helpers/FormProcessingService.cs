using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Models;
using Returns.Models.Data;
using static Returns.Helpers.Constants;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public class FormProcessingService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger _logger;

        public FormProcessingService(ReturnsDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }



        public async Task<(bool Success, string ReturnId, List<string> ProcessingSummary)> ProcessFormBatchAsync(
            NewReturnDTO batchDTO,
            LoggedInSacco loggedInSacco,
            bool isConsistent,
            List<string> consistencyErrors,
            string periodToUse)
        {
            var processingSummary = new List<string>();
            Boolean IsAmendment = false;
            string EffectiveReturnId = string.Empty;
            try
            {
                // Step 1: Determine if this is a batch amendment by checking any form
                string batchReturnId = null;
                bool isBatchAmendment = false;

                foreach (var upload in batchDTO.FormUploads)
                {
                    if (upload.formFile == null) continue;

                    var form = await _context.ReturnForms
                        .Include(x => x.Period)
                        .FirstOrDefaultAsync(f => f.Id == upload.FormId);

                    if (form == null) continue;

                    // Check if this form is an amendment
                   var AmendmentData = await IsAmendmentBasedOnReportingPeriod(upload.formFile, form, loggedInSacco.SaccoType, loggedInSacco.SaccoId);

                    if (AmendmentData.IsAmendment && !string.IsNullOrEmpty(AmendmentData.ReturnId))
                    {
                        isBatchAmendment = true;
                        batchReturnId = AmendmentData.ReturnId;
                        break; // We only need to find one amendment to determine it's a batch amendment
                    }
                }

                // Step 2: Handle the batch (either new or amendment)
                string effectiveReturnId;

                if (isBatchAmendment && !string.IsNullOrEmpty(batchReturnId))
                {
                    // This is a batch amendment - create a new version of the batch
                    var (amendmentSuccess, newBatchId, message) = await HandleBatchAmendmentAsync(batchReturnId, loggedInSacco, isConsistent, consistencyErrors, periodToUse, batchDTO.SubmissionDate);

                    if (!amendmentSuccess)
                    {
                        processingSummary.Add(message);
                        // Fall back to creating a new return
                        var newBatch = CreateNewReturn(
                            loggedInSacco,
                            isConsistent,
                            consistencyErrors,
                            batchDTO.SubmissionDate,
                            periodToUse);

                        await _context.Returns.AddAsync(newBatch);
                        await _context.SaveChangesAsync();
                        effectiveReturnId = newBatch.Id;
                    }
                    else
                    {
                        effectiveReturnId = newBatchId;
                        processingSummary.Add(message);
                    }
                }
                else
                {
                    // This is a new batch - create a new return
                    var newBatch = CreateNewReturn(
                        loggedInSacco,
                        isConsistent,
                        consistencyErrors,
                        batchDTO.SubmissionDate,
                        periodToUse);

                    await _context.Returns.AddAsync(newBatch);
                    await _context.SaveChangesAsync();
                    effectiveReturnId = newBatch.Id;
                }

                EffectiveReturnId = effectiveReturnId;
                IsAmendment = isBatchAmendment;

                // Step 3: Process each form in the batch
                foreach (var upload in batchDTO.FormUploads)
                {
                    if (upload.formFile == null) continue;

                    var form = await _context.ReturnForms.Include(x => x.Period).FirstOrDefaultAsync(f => f.Id == upload.FormId);

                    if (form == null)
                    {
                        processingSummary.Add($"Form with ID {upload.FormId} was not found.");
                        continue;
                    }

                    if (form.IsOtherForm)
                    {
                        var fileName = upload.formFile.FileName;
                        var path = await FormsHelper.SaveFileAsync(upload.formFile, "OtherReturns", form.FormName);

                        if (path == null)
                        {
                            processingSummary.Add($"Failed to save '{upload.formFile.FileName}'.");
                            continue;
                        }

                        var otherReturn = new OtherReturn
                        {
                            ReturnId = effectiveReturnId,
                            FormName = form.FormName,
                            FileUrl = path,
                            SaccoId = loggedInSacco.SaccoId,
                            SaccoType = loggedInSacco.SaccoType,
                            SaccoName = loggedInSacco.SaccoName,
                        };

                        await _context.OtherReturns.AddAsync(otherReturn);
                        await _context.SaveChangesAsync();
                        processingSummary.Add($"Successfully saved other form '{upload.formFile.FileName}'.");
                        continue;
                    }

                    // Process the form
                    var (isProcessed, message) = await ProcessFormAsync(
                        IsAmendment,
                        upload.formFile,
                        form,
                        effectiveReturnId,
                        loggedInSacco.SaccoId,
                        loggedInSacco.SaccoType,
                        batchReturnId
                        );

                    if (isProcessed)
                    {
                        processingSummary.Add($"Successfully processed '{upload.formFile.FileName}'.");
                    }
                    else
                    {
                        processingSummary.Add($"File '{upload.formFile.FileName}' was not processed: {message}");
                    }
                }

                return (true, effectiveReturnId, processingSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing form batch");
                processingSummary.Add($"Error processing batch: {ex.Message}");
                return (false, string.Empty, processingSummary);
            }
        }

        private Return CreateNewReturn(LoggedInSacco sacco, bool isConsistent, List<string> errors, DateTime submissionDate, string periodToUse)
        {
            return new Return
            {
                SaccoId = sacco.SaccoId,
                IsNotConsistent = isConsistent,
                ConsistentErrorMessage = string.Join(", ", errors),
                SaccoType = sacco.SaccoType,
                SaccoName = sacco.SaccoName,
                SubmittedAt = DateTime.Now,
                ReturnFor = submissionDate,
                Year = periodToUse,
                VersionNumber = 1,
                IsActiveVersion = true,
            };
        }

        public async Task<(bool Success, string NewBatchId, string Message)> HandleBatchAmendmentAsync(string batchReturnId, LoggedInSacco loggedInSacco, bool isConsistent, List<string> consistencyErrors, string periodToUse, DateTime submissionDate)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get the existing return
                var originalReturn = await _context.Returns
                    .FirstOrDefaultAsync(r => r.Id == batchReturnId && r.IsActiveVersion);

                if (originalReturn == null)
                {
                    return (false, string.Empty, $"Original batch with ID {batchReturnId} not found or is not the active version");
                }

                // Mark the current return as inactive
                originalReturn.IsActiveVersion = false;
                _context.Returns.Update(originalReturn);

                // Create a new return version based on the original
                var newReturn = new Return
                {
                    SaccoId = originalReturn.SaccoId,
                    SaccoType = originalReturn.SaccoType,
                    SaccoName = originalReturn.SaccoName,
                    Period = originalReturn.Period,
                    Year = originalReturn.Year,
                    SubmittedAt = DateTime.Now,
                    ReturnFor = originalReturn.ReturnFor,
                    IsNotConsistent = isConsistent,
                    ConsistentErrorMessage = string.Join(", ", consistencyErrors),
                    VersionNumber = originalReturn.VersionNumber + 1,
                    IsActiveVersion = true,
                    AmendmentDate = DateTime.Now,
                    PreviousVersionId = originalReturn.Id
                };

                await _context.Returns.AddAsync(newReturn);
                await _context.SaveChangesAsync();

                // Copy all child records that aren't being amended
                //await CopyChildRecords(originalReturn.Id, newReturn.Id, form, loggedInSacco.SaccoType);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, newReturn.Id, $"Processing amendment to batch Return ID: {batchReturnId}, new version: {newReturn.VersionNumber}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating batch amendment");
                return (false, string.Empty, $"Error creating batch amendment: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ProcessFormAsync( Boolean IsAmendment,  IFormFile formFile, ReturnForm form, string EffectiveReturnId, string saccoId, string saccoType, string OldReturnId = "")
        {
            try
            {
                // First, check if this form should be treated as an amendment
                //var AmendmentData = await IsAmendmentBasedOnReportingPeriod(formFile, form, saccoType, saccoId);
                string effectiveReturnId = EffectiveReturnId;

                // If it's an amendment, create a new version
                if (IsAmendment)
                {
                    _logger.LogInformation($"Form {form.FormName} is being processed as an amendment");

                    var (amendmentSuccess, newReturnId, respose) = await HandleAmendment(EffectiveReturnId, OldReturnId, saccoId, form, saccoType);

                    if (!amendmentSuccess)
                    {
                        return (false, respose);
                    }

                    effectiveReturnId = newReturnId;
                }

                // Process the form content
                var (success, message) = await ProcessFormByType(formFile, form, effectiveReturnId, saccoType, IsAmendment, EffectiveReturnId);

                // If this was an amendment and processing succeeded, mark the form as amended
                if (IsAmendment && success)
                {
                    await MarkFormAsAmended(OldReturnId, GetFormTypeFromForm(form), saccoType);
                }

                return (success, message + (IsAmendment ? " (processed as amendment)" : ""));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing form '{formFile.FileName}'");
                return (false, $"Error processing '{formFile.FileName}': {ex.Message}");
            }
        }




        private async Task<(bool Success, string NewReturnId, string Message)> HandleAmendment(string NewReturnID, string OldReturnId, string saccoId, ReturnForm form, string SaccoType)
        {
            try
            {
                if (string.IsNullOrEmpty(OldReturnId))
                {

                }
                if (OldReturnId.Equals(NewReturnID))
                {

                }
                // Get the current return
                var OldReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == OldReturnId);

                if (OldReturn == null)
                {
                    return (false, string.Empty, "Original return not found or is not the active version");
                }

                // Start a transaction for creating the amendment
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Mark the current return as inactive
                    OldReturn.IsActiveVersion = false;
                    _context.Returns.Update(OldReturn);

                    /*var newReturn = new Return
                    {
                        SaccoId = OldReturn.SaccoId,
                        SaccoType = OldReturn.SaccoType,
                        SaccoName = OldReturn.SaccoName,
                        Period = OldReturn.Period,
                        Year = OldReturn.Year,
                        SubmittedAt = DateTime.Now,
                        ReturnFor = OldReturn.ReturnFor,
                        IsNotConsistent = OldReturn.IsNotConsistent,
                        ConsistentErrorMessage = OldReturn.ConsistentErrorMessage,
                        VersionNumber = OldReturn.VersionNumber + 1,
                        IsActiveVersion = true,
                        AmendmentDate = DateTime.Now,
                        PreviousVersionId = OldReturn.Id
                    };

                    await _context.Returns.AddAsync(newReturn);*/
                    await _context.SaveChangesAsync();

                    // Copy all child records from the original return to the new one
                    await CopyChildRecords(OldReturn.Id, NewReturnID, form, SaccoType);

                    await transaction.CommitAsync();
                    return (true, NewReturnID, "Amendment created successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating amendment");
                    return (false, string.Empty, $"Error creating amendment: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing amendment");
                return (false, string.Empty, $"Error preparing amendment: {ex.Message}");
            }
        }


        private async Task<bool> CopyChildRecords(string OldReturnId, string NewReturnId, ReturnForm form, string SaccoType)
        {
            if (SaccoType == Constants.SaccoType.DepositTaking)
            {

                if (form.IsSectoralLending)
                {
                    var sectoralLending = await _context.SectorData.Where(s => s.ReturnId == OldReturnId).ToListAsync();
                    if (sectoralLending == null)
                    {
                    }
                    foreach (var OldItem in sectoralLending)
                    {
                        var newItem = new EconomicSectorData
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            SectoralLendingReportId = OldItem.SectoralLendingReportId,
                            Amount = OldItem.Amount,
                            EconomicSectorId = OldItem.EconomicSectorId,
                        };
                        await _context.SectorData.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.SectorData.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;

                }
               if (form.IsCapitalAdequencyForm)
                {
                    var capitalAdequacy = await _context.CapitalAdequacies.Where(c => c.ReturnId == OldReturnId).ToListAsync();
                    if (capitalAdequacy == null)
                    {
                    }

                    foreach (var item in capitalAdequacy)
                    {
                        var newItem = new CapitalAdequacy
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            ShareCapital = item.ShareCapital,
                            StatutoryReserves = item.StatutoryReserves,
                            RetainedEarningsAccumulatedLosses = item.RetainedEarningsAccumulatedLosses,
                            NetSurplusAfterTaxCurrentYearToDate = item.NetSurplusAfterTaxCurrentYearToDate,
                            CapitalGrantsEquityInNature = item.CapitalGrantsEquityInNature,
                            GeneralReserves = item.GeneralReserves,
                            OtherReserves = item.OtherReserves,
                            SubTotalCoreCapital = item.SubTotalCoreCapital,

                            InvestmentsInSubsidiaryAndEquityInstruments = item.InvestmentsInSubsidiaryAndEquityInstruments,
                            OtherDeductions = item.OtherDeductions,
                            TotalDeductions = item.TotalDeductions,
                            CoreCapital = item.CoreCapital,
                            InstitutionalCapital = item.InstitutionalCapital,

                            CashLocalAndForeignCurrency = item.CashLocalAndForeignCurrency,
                            GovernmentSecurities = item.GovernmentSecurities,
                            DepositsAndBalancesAtOtherInstitutions = item.DepositsAndBalancesAtOtherInstitutions,
                            LoansAndAdvances = item.LoansAndAdvances,
                            Investments = item.Investments,
                            PropertyAndEquipment = item.PropertyAndEquipment,
                            OtherAssets = item.OtherAssets,
                            TotalOnBalanceSheetAssets = item.TotalOnBalanceSheetAssets,
                            TotalAssetsPerBalanceSheet = item.TotalAssetsPerBalanceSheet,
                            Difference = item.Difference,

                            TotalOffBalanceSheetAssets = item.TotalOffBalanceSheetAssets,

                            TotalAssets = item.TotalAssets,
                            TotalDepositsLiabilities = item.TotalDepositsLiabilities,
                            CoreCapitalToAssetsRatio = item.CoreCapitalToAssetsRatio,
                            MinimumCoreCapitalToAssetsRatio = item.MinimumCoreCapitalToAssetsRatio,
                            CoreCapitalToAssetsRatioExcessDeficiency = item.CoreCapitalToAssetsRatioExcessDeficiency,
                            InstitutionalCapitalToAssetsRatio = item.InstitutionalCapitalToAssetsRatio,
                            MinimumInstitutionalToAssetsRatio = item.MinimumInstitutionalToAssetsRatio,
                            InstitutionalCapitalToAssetsRatioExcessDeficiency = item.InstitutionalCapitalToAssetsRatioExcessDeficiency,
                            CoreCapitalToDepositsRatio = item.CoreCapitalToDepositsRatio,
                            MinimumCoreCapitalToDepositsRatio = item.MinimumCoreCapitalToDepositsRatio,
                            CoreCapitalToDepositsRatioExcessDeficiency = item.CoreCapitalToDepositsRatioExcessDeficiency,

                            Year = item.Year,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            Frequency = item.Frequency,
                            FilePath = item.FilePath,
                            DaysLateBy = item.DaysLateBy,
                        };

                        await _context.CapitalAdequacies.AddAsync(newItem);
                        item.IsCurrent = false;
                        item.IsAmended = true;
                        _context.CapitalAdequacies.Update(item);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsLiquidityStatement)
                {
                    var liquidityReturns = await _context.LiquidityReturns.Where(l => l.ReturnId == OldReturnId).ToListAsync();

                    foreach (var OldItem in liquidityReturns)
                    {
                        var newItem = new LiquidityReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            // 1. Notes and Coins
                            LocalNotesAndCoins = OldItem.LocalNotesAndCoins,
                            ForeignNotesAndCoins = OldItem.ForeignNotesAndCoins,
                            TotalNotesAndCoins = OldItem.TotalNotesAndCoins,

                            // 2. Bank Balances
                            BalancesWithCommercialBanks = OldItem.BalancesWithCommercialBanks,
                            TimeDepositsWithBanksMoreThan90Days = OldItem.TimeDepositsWithBanksMoreThan90Days,
                            OverdraftsAndMaturedLoans = OldItem.OverdraftsAndMaturedLoans,
                            NetBankBalances = OldItem.NetBankBalances,

                            // 3. Balances with Financial Institutions
                            BalancesWithOtherSaccoSocieties = OldItem.BalancesWithOtherSaccoSocieties,
                            BalancesWithOtherFinancialInstitutions = OldItem.BalancesWithOtherFinancialInstitutions,
                            BalancesDueToOtherSaccoSocieties = OldItem.BalancesDueToOtherSaccoSocieties,
                            BalancesDueToFinancialInstitutions = OldItem.BalancesDueToFinancialInstitutions,
                            MaturedLoansFromFinancialInstitutions = OldItem.MaturedLoansFromFinancialInstitutions,
                            NetFinancialInstitutionBalances = OldItem.NetFinancialInstitutionBalances,

                            // 4. Government Securities
                            TreasuryBills = OldItem.TreasuryBills,
                            TreasuryBonds = OldItem.TreasuryBonds,
                            TotalGovernmentSecurities = OldItem.TotalGovernmentSecurities,

                            // 5. Net Liquid Assets
                            NetLiquidAssets = OldItem.NetLiquidAssets,

                            // 6. Deposit Balances
                            DepositsFromMembers = OldItem.DepositsFromMembers,
                            DepositsFromOtherSources = OldItem.DepositsFromOtherSources,
                            TotalDeposits = OldItem.TotalDeposits,
                            BalancesDueToSaccos = OldItem.BalancesDueToSaccos,
                            BalancesDueToBanks = OldItem.BalancesDueToBanks,
                            BalancesDueToOtherFinancialInst = OldItem.BalancesDueToOtherFinancialInst,
                            TotalDeductions = OldItem.TotalDeductions,
                            NetDepositLiabilities = OldItem.NetDepositLiabilities,

                            // 7. Other Liabilities
                            MaturedLiabilities = OldItem.MaturedLiabilities,
                            LiabilitiesMaturing91Days = OldItem.LiabilitiesMaturing91Days,
                            TotalOtherLiabilities = OldItem.TotalOtherLiabilities,

                            // 8. Liquidity Ratio
                            TotalShortTermLiabilities = OldItem.TotalShortTermLiabilities,
                            LiquidityRatio = OldItem.LiquidityRatio,
                            MinimumLiquidityRequirement = OldItem.MinimumLiquidityRequirement,
                            LiquidityRatioExcessDeficit = OldItem.LiquidityRatioExcessDeficit,

                            // Metadata
                            Year = OldItem.Year,
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.LiquidityReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.LiquidityReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsRiskClassification)
                {
                    var riskClassifications = await _context.RiskClassifications
                        .Where(r => r.ReturnId == OldReturnId)
                        .ToListAsync();

                    foreach (var item in riskClassifications)
                    {
                        var OldItem = new RiskClassificationReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            LoanType = item.LoanType,
                            Classification = item.Classification,
                            NumberOfAccounts = item.NumberOfAccounts,
                            OutstandingLoanPortfolio = item.OutstandingLoanPortfolio,
                            RequiredProvision = item.RequiredProvision,
                            RequiredProvisionAmount = item.RequiredProvisionAmount,

                            Year = item.Year,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            Frequency = item.Frequency,
                            FilePath = item.FilePath,
                            DaysLateBy = item.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.RiskClassifications.AddAsync(OldItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.RiskClassifications.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsInvestmentReturn)
                {
                    var investmentReturns = await _context.InvestmentReturns
                      .Where(i => i.ReturnId == OldReturnId)
                      .ToListAsync();

                    foreach (var OldItem in investmentReturns)
                    {
                        var newItem = new InvestmentReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            CoreCapital = OldItem.CoreCapital,
                            TotalAssets = OldItem.TotalAssets,
                            TotalDeposits = OldItem.TotalDeposits,
                            NonEarningAssets = OldItem.NonEarningAssets,
                            FinancialInvestments = OldItem.FinancialInvestments,
                            LandAndBuildings = OldItem.LandAndBuildings,

                            LandBuildingsToTotalAssetsRatio = OldItem.LandBuildingsToTotalAssetsRatio,
                            MaxLandBuildingsToTotalAssetsRatio = OldItem.MaxLandBuildingsToTotalAssetsRatio,
                            LandBuildingsRatioExcessDeficiency = OldItem.LandBuildingsRatioExcessDeficiency,

                            FinancialInvestmentsToCoreCapitalRatio = OldItem.FinancialInvestmentsToCoreCapitalRatio,
                            MaxFinancialInvestmentsToCoreCapitalRatio = OldItem.MaxFinancialInvestmentsToCoreCapitalRatio,
                            FinancialInvestmentsToCoreCapitalExcessDeficiency = OldItem.FinancialInvestmentsToCoreCapitalExcessDeficiency,

                            FinancialInvestmentsToDepositsRatio = OldItem.FinancialInvestmentsToDepositsRatio,
                            MaxFinancialInvestmentsToDepositsRatio = OldItem.MaxFinancialInvestmentsToDepositsRatio,
                            FinancialInvestmentsToDepositsExcessDeficiency = OldItem.FinancialInvestmentsToDepositsExcessDeficiency,

                            NonEarningAssetsToTotalAssetsRatio = OldItem.NonEarningAssetsToTotalAssetsRatio,
                            MaxNonEarningAssetsToTotalAssetsRatio = OldItem.MaxNonEarningAssetsToTotalAssetsRatio,
                            NonEarningAssetsRatioExcessDeficiency = OldItem.NonEarningAssetsRatioExcessDeficiency,

                            Year = OldItem.Year,
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.InvestmentReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.InvestmentReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;

                }

                if (form.IsStatementOfComprehensiveIncome)
                {
                    var comprehensiveIncomeReturns = await _context.StatementOfComprehensiveIncomeReturns
                      .Where(c => c.ReturnId == OldReturnId)
                      .ToListAsync();

                    foreach (var OldItem in comprehensiveIncomeReturns)
                    {
                        var newItem = new StatementOfComprehensiveIncomeReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            InterestOnLoanPortfolio = OldItem.InterestOnLoanPortfolio,
                            FeesAndCommissionOnLoanPortfolio = OldItem.FeesAndCommissionOnLoanPortfolio,

                            GovernmentSecurities = OldItem.GovernmentSecurities,
                            DepositsWithBanks = OldItem.DepositsWithBanks,
                            OtherInvestments = OldItem.OtherInvestments,
                            OtherOperatingIncome = OldItem.OtherOperatingIncome,

                            InterestExpenseOnDeposits = OldItem.InterestExpenseOnDeposits,
                            CostOfExternalBorrowings = OldItem.CostOfExternalBorrowings,
                            DividendExpenses = OldItem.DividendExpenses,
                            OtherFinancialExpense = OldItem.OtherFinancialExpense,
                            FeesAndCommissionExpense = OldItem.FeesAndCommissionExpense,
                            OtherExpense = OldItem.OtherExpense,

                            ProvisionForLoanLosses = OldItem.ProvisionForLoanLosses,
                            ValueOfLoansRecovered = OldItem.ValueOfLoansRecovered,

                            PersonnelExpenses = OldItem.PersonnelExpenses,
                            GovernanceExpenses = OldItem.GovernanceExpenses,
                            MarketingExpenses = OldItem.MarketingExpenses,
                            DepreciationAndAmortization = OldItem.DepreciationAndAmortization,
                            AdministrativeExpenses = OldItem.AdministrativeExpenses,

                            NonOperatingIncome = OldItem.NonOperatingIncome,
                            NonOperatingExpense = OldItem.NonOperatingExpense,

                            Taxes = OldItem.Taxes,
                            Donations = OldItem.Donations,

                            Year = OldItem.Year,
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.StatementOfComprehensiveIncomeReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.StatementOfComprehensiveIncomeReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsStatementOfComprehensiveIncome)
                {
                    var financialPositionReturns = await _context.StatementOfFinancialPositionReturns
                     .Where(f => f.ReturnId == OldReturnId)
                     .ToListAsync();

                    foreach (var OldItem in financialPositionReturns)
                    {
                        var newItem = new StatementOfFinancialPositionReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            CashInHand = OldItem.CashInHand,
                            CashAtBank = OldItem.CashAtBank,

                            PrepaymentsAndSundryReceivables = OldItem.PrepaymentsAndSundryReceivables,

                            GovernmentSecurities = OldItem.GovernmentSecurities,
                            OtherSecurities = OldItem.OtherSecurities,
                            BalancesWithOtherSaccos = OldItem.BalancesWithOtherSaccos,
                            InvestmentsInCompanies = OldItem.InvestmentsInCompanies,

                            GrossLoanPortfolio = OldItem.GrossLoanPortfolio,
                            AllowanceForLoanLoss = OldItem.AllowanceForLoanLoss,

                            TaxRecoverable = OldItem.TaxRecoverable,
                            DeferredTaxAssets = OldItem.DeferredTaxAssets,
                            RetirementBenefitAssets = OldItem.RetirementBenefitAssets,

                            InvestmentProperties = OldItem.InvestmentProperties,
                            PropertyAndEquipment = OldItem.PropertyAndEquipment,
                            PrepaidLeaseRentals = OldItem.PrepaidLeaseRentals,
                            IntangibleAssets = OldItem.IntangibleAssets,
                            OtherAssets = OldItem.OtherAssets,

                            SavingsDeposits = OldItem.SavingsDeposits,
                            ShortTermDeposits = OldItem.ShortTermDeposits,
                            NonWithdrawableDeposits = OldItem.NonWithdrawableDeposits,

                            TaxPayable = OldItem.TaxPayable,
                            DividendsPayable = OldItem.DividendsPayable,
                            DeferredTaxLiability = OldItem.DeferredTaxLiability,
                            RetirementBenefitsLiability = OldItem.RetirementBenefitsLiability,
                            OtherLiabilities = OldItem.OtherLiabilities,
                            ExternalBorrowings = OldItem.ExternalBorrowings,

                            ShareCapital = OldItem.ShareCapital,
                            CapitalGrants = OldItem.CapitalGrants,

                            PriorYearsRetainedEarnings = OldItem.PriorYearsRetainedEarnings,
                            CurrentYearSurplus = OldItem.CurrentYearSurplus,

                            StatutoryReserve = OldItem.StatutoryReserve,
                            OtherReserves = OldItem.OtherReserves,
                            RevaluationReserves = OldItem.RevaluationReserves,
                            ProposedDividends = OldItem.ProposedDividends,
                            AdjustmentToEquity = OldItem.AdjustmentToEquity,

                            Year = OldItem.Year,
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.StatementOfFinancialPositionReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.StatementOfFinancialPositionReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsDepositReturnForm)
                {

                    var depositReturns = await _context.DepositReturns
                        .Where(d => d.ReturnId == OldReturnId)
                        .ToListAsync();

                    foreach (var OldItem in depositReturns)
                    {
                        var newItem = new DepositReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            RangeName = OldItem.RangeName,
                            DepositType = OldItem.DepositType,
                            NumberOfAccounts = OldItem.NumberOfAccounts,
                            AmountInKshs000 = OldItem.AmountInKshs000,

                            Year = OldItem.Year,
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false
                        };

                        await _context.DepositReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.DepositReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsOtherForm)
                {

                    var otherReturns = await _context.OtherReturns
                        .Where(o => o.ReturnId == OldReturnId)
                        .ToListAsync();

                    foreach (var OldItem in otherReturns)
                    {
                        var newItem = new OtherReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            FormName = OldItem.FormName,
                            FileUrl = OldItem.FileUrl,
                            SaccoId = OldItem.SaccoId,
                            SaccoType = OldItem.SaccoType,
                            SaccoName = OldItem.SaccoName,

                            IsAmended = false
                        };

                        await _context.OtherReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.OtherReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

            }
            else
            {
                if (form.IsSectoralLending)
                {
                    var OldsectoralLendingReport = await _context.SectoralLendingReports.FirstOrDefaultAsync(x => x.ReturnId == OldReturnId);

                    if (OldsectoralLendingReport == null)
                    {
                        return false;
                    }

                    OldsectoralLendingReport.IsAmended = true;
                    OldsectoralLendingReport.IsCurrent = false;

                    var sectoralLending = await _context.SectorData.Where(s => s.SectoralLendingReportId == OldsectoralLendingReport.Id).ToListAsync();
                    if (sectoralLending == null)
                    {
                        return false;
                    }

                    var newSectoralLendingReport = new SectoralLendingReport
                    {
                        ReturnId = NewReturnId,
                        PreviousReturnId = OldReturnId,
                        FilePath = OldsectoralLendingReport.FilePath,
                        Version = OldsectoralLendingReport.Version + 1,
                        StartDate = OldsectoralLendingReport.StartDate,
                        EndDate = OldsectoralLendingReport.EndDate,
                        IsAmended = false,
                        IsCurrent = true,
                        Year = OldsectoralLendingReport.Year,
                        Month = OldsectoralLendingReport.Month,
                        SaccoName = OldsectoralLendingReport.SaccoName,
                        DaysLateBy = OldsectoralLendingReport.DaysLateBy,
                        SaccoId = OldsectoralLendingReport.SaccoId,
                    };
                    await _context.SectoralLendingReports.AddAsync(newSectoralLendingReport);
                    foreach (var OldItem in sectoralLending)
                    {
                        var newItem = new EconomicSectorData
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            SectoralLendingReportId = newSectoralLendingReport.Id,
                            Amount = OldItem.Amount,
                            EconomicSectorId = OldItem.EconomicSectorId,
                        };
                        await _context.SectorData.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.SectorData.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;

                }

                if (form.IsCapitalAdequencyForm)
                {
                    var nwdtCapitalAdequacy = await _context.NDWTCapitalAdequacyReturns
                       .Where(c => c.ReturnId == OldReturnId)
                       .ToListAsync();

                    foreach (var item in nwdtCapitalAdequacy)
                    {
                        var newItem = new NDWTCapitalAdequacyReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            ShareCapital = item.ShareCapital,
                            CapitalGrants = item.CapitalGrants,
                            RetainedEarnings = item.RetainedEarnings,
                            NetSurplusAfterTax = item.NetSurplusAfterTax,
                            StatutoryReserves = item.StatutoryReserves,
                            OtherReserves = item.OtherReserves,

                            InvestmentsInSubsidiary = item.InvestmentsInSubsidiary,
                            OtherDeductions = item.OtherDeductions,

                            CashLocalForeign = item.CashLocalForeign,
                            GovernmentSecurities = item.GovernmentSecurities,
                            DepositsBalancesAtOtherInstitutions = item.DepositsBalancesAtOtherInstitutions,
                            LoansAndAdvances = item.LoansAndAdvances,
                            Investments = item.Investments,
                            PropertyAndEquipment = item.PropertyAndEquipment,
                            OtherAssets = item.OtherAssets,
                            TotalAssetsPerBalanceSheet = item.TotalAssetsPerBalanceSheet,

                            OffBalanceSheetAssets = item.OffBalanceSheetAssets,

                            MinimumCoreCapitalToAssetsRatio = item.MinimumCoreCapitalToAssetsRatio,
                            MinimumRetainedEarningsToCoreCaptialRequirement = item.MinimumRetainedEarningsToCoreCaptialRequirement,
                            MinimumCoreCapitalToDepositsRequirement = item.MinimumCoreCapitalToDepositsRequirement,
                            TotalDepositsLiabilities = item.TotalDepositsLiabilities,

                            StoredSubTotalCoreCapital = item.StoredSubTotalCoreCapital,
                            StoredTotalDeductions = item.StoredTotalDeductions,
                            StoredCoreCapital = item.StoredCoreCapital,
                            StoredRetainedEarningsAndDisclosedReserves = item.StoredRetainedEarningsAndDisclosedReserves,
                            StoredTotalOnBalanceSheetAssets = item.StoredTotalOnBalanceSheetAssets,
                            StoredDifferenceInAssets = item.StoredDifferenceInAssets,
                            StoredTotalAssets = item.StoredTotalAssets,
                            StoredCoreCapitalToAssetsRatio = item.StoredCoreCapitalToAssetsRatio,
                            StoredCoreCapitalToAssetsExcessDeficiency = item.StoredCoreCapitalToAssetsExcessDeficiency,
                            StoredRetainedEarningsToCoreCaptialRatio = item.StoredRetainedEarningsToCoreCaptialRatio,
                            StoredRetainedEarningsToCoreCaptialExcessDeficiency = item.StoredRetainedEarningsToCoreCaptialExcessDeficiency,
                            StoredCoreCapitalToDepositsRatio = item.StoredCoreCapitalToDepositsRatio,
                            StoredCoreCapitalToDepositsExcessDeficiency = item.StoredCoreCapitalToDepositsExcessDeficiency,

                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            Period = item.Period,
                            Frequency = item.Frequency,
                            FilePath = item.FilePath,
                            DaysLateBy = item.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.NDWTCapitalAdequacyReturns.AddAsync(newItem);
                        item.IsCurrent = false;
                        item.IsAmended = true;
                        _context.NDWTCapitalAdequacyReturns.Update(item);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }
                if (form.IsRiskClassification)
                {
                    var nwdtRiskClassification = await _context.NWDTRiskClassificationReturns
                     .Where(r => r.ReturnId == OldReturnId)
                     .ToListAsync();

                    foreach (var OldItem in nwdtRiskClassification)
                    {
                        var newItem = new NWDTRiskClassificationReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            // NWDT Risk Classification fields
                            LoanType = OldItem.LoanType,
                            Classification = OldItem.Classification,
                            NumberOfAccounts = OldItem.NumberOfAccounts,
                            OutstandingLoanPortfolio = OldItem.OutstandingLoanPortfolio,
                            RequiredProvision = OldItem.RequiredProvision,
                            RequiredProvisionAmount = OldItem.RequiredProvisionAmount,

                            // Metadata
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Period = OldItem.Period,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.NWDTRiskClassificationReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.NWDTRiskClassificationReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }
                if (form.IsLiquidityStatement)
                {
                    var nwdtLiquidity = await _context.NDWTLiquidityReturns
                       .Where(l => l.ReturnId == OldReturnId)
                       .ToListAsync();

                    foreach (var OldItem in nwdtLiquidity)
                    {
                        var newItem = new NWDTLiquidityReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            // Section 1: Notes and Coins
                            LocalNotesAndCoins = OldItem.LocalNotesAndCoins,
                            ForeignNotesAndCoins = OldItem.ForeignNotesAndCoins,

                            // Section 2: Bank Balances
                            BalancesWithCommercialBanks = OldItem.BalancesWithCommercialBanks,
                            TimeDepositsWithBanksMoreThan90Days = OldItem.TimeDepositsWithBanksMoreThan90Days,
                            OverdraftsAndMaturedLoans = OldItem.OverdraftsAndMaturedLoans,

                            // Section 3: Other Financial Institutions
                            BalancesWithOtherSaccoSocieties = OldItem.BalancesWithOtherSaccoSocieties,
                            BalancesWithOtherFinancialInstitutions = OldItem.BalancesWithOtherFinancialInstitutions,
                            BalancesDueToOtherSaccoSocieties = OldItem.BalancesDueToOtherSaccoSocieties,
                            BalancesDueToFinancialInstitutions = OldItem.BalancesDueToFinancialInstitutions,
                            MaturedLoansAndAdvances = OldItem.MaturedLoansAndAdvances,

                            // Section 4: Government Securities
                            TreasuryBills = OldItem.TreasuryBills,
                            TreasuryBondsBearerBonds = OldItem.TreasuryBondsBearerBonds,

                            // Section 6: Other Liabilities
                            MaturedLiabilities = OldItem.MaturedLiabilities,
                            LiabilitiesMaturing91Days = OldItem.LiabilitiesMaturing91Days,

                            StoredTotalNotesAndCoins = OldItem.StoredTotalNotesAndCoins,
                            StoredNetBankBalances = OldItem.StoredNetBankBalances,
                            StoredNetFinancialInstitutionBalances = OldItem.StoredNetFinancialInstitutionBalances,
                            StoredTotalBankBalances = OldItem.StoredTotalBankBalances,
                            StoredTotalOtherFinancialInstitutions = OldItem.StoredTotalOtherFinancialInstitutions,
                            StoredTotalGovernmentSecurities = OldItem.StoredTotalGovernmentSecurities,
                            StoredNetLiquidAssets = OldItem.StoredNetLiquidAssets,
                            StoredTotalOtherLiabilities = OldItem.StoredTotalOtherLiabilities,
                            StoredLiquidityRatio = OldItem.StoredLiquidityRatio,
                            StoredLiquidityRatioExcessDeficit = OldItem.StoredLiquidityRatioExcessDeficit,

                            MinimumRequirement = OldItem.MinimumRequirement,

                            // Metadata
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Period = OldItem.Period,
                            Frequency = OldItem.Frequency,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.NDWTLiquidityReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.NDWTLiquidityReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;

                }
                if (form.IsDepositReturnForm)
                {
                    var nwdtDeposit = await _context.NWDTDepositReturns
                        .Where(d => d.ReturnId == OldReturnId)
                        .ToListAsync();

                    foreach (var OldItem in nwdtDeposit)
                    {
                        var newItem = new NWDTDepositReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,
                            // NWDT Deposit Return fields
                            RangeName = OldItem.RangeName,
                            DepositType = OldItem.DepositType,
                            NumberOfAccounts = OldItem.NumberOfAccounts,
                            AmountInKshs000 = OldItem.AmountInKshs000,

                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Period = OldItem.Period,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.NWDTDepositReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.NWDTDepositReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;

                }
                if (form.IsInvestmentReturn)
                {
                    var nwdtInvestment = await _context.NWDTInvestmentReturns
                                   .Where(i => i.ReturnId == OldReturnId)
                                   .ToListAsync();

                    foreach (var OldItem in nwdtInvestment)
                    {
                        var newItem = new NWDTInvestmentReturn
                        {
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            // Basic financial data
                            CoreCapital = OldItem.CoreCapital,
                            TotalAssets = OldItem.TotalAssets,
                            TotalDeposits = OldItem.TotalDeposits,
                            NonEarningAssets = OldItem.NonEarningAssets,

                            // Financial assets
                            SubsidiaryRelatedEntityInvestments = OldItem.SubsidiaryRelatedEntityInvestments,
                            EquityInvestments = OldItem.EquityInvestments,
                            OtherInvestments = OldItem.OtherInvestments,
                            OtherAssetsLandBuildingEquipment = OldItem.OtherAssetsLandBuildingEquipment,
                            LandAndBuilding = OldItem.LandAndBuilding,

                            // Maximum requirements
                            MaxLandBuildingEquipmentToTotalAssetRequirement = OldItem.MaxLandBuildingEquipmentToTotalAssetRequirement,
                            MaxLandBuildingToTotalAssetRequirement = OldItem.MaxLandBuildingToTotalAssetRequirement,
                            MaxFinancialInvestmentsToCoreCapital = OldItem.MaxFinancialInvestmentsToCoreCapital,
                            MaxEquityInvestmentsToTotalDeposits = OldItem.MaxEquityInvestmentsToTotalDeposits,
                            MaxSubsidiaryInvestmentToTotalAssets = OldItem.MaxSubsidiaryInvestmentToTotalAssets,
                            MaxOtherInvestmentsToCoreCapital = OldItem.MaxOtherInvestmentsToCoreCapital,

                            // Stored calculated values
                            StoredFinancialAssets = OldItem.StoredFinancialAssets,
                            StoredLandBuildingEquipmentToTotalAssetsRatio = OldItem.StoredLandBuildingEquipmentToTotalAssetsRatio,
                            StoredLandBuildingEquipmentExcessDeficiency = OldItem.StoredLandBuildingEquipmentExcessDeficiency,
                            StoredLandBuildingToTotalAssetsRatio = OldItem.StoredLandBuildingToTotalAssetsRatio,
                            StoredLandBuildingExcessDeficiency = OldItem.StoredLandBuildingExcessDeficiency,
                            StoredFinancialInvestmentsToCoreCapitalRatio = OldItem.StoredFinancialInvestmentsToCoreCapitalRatio,
                            StoredFinancialInvestmentsExcessDeficiency = OldItem.StoredFinancialInvestmentsExcessDeficiency,
                            StoredEquityInvestmentsToCoreCapitalRatio = OldItem.StoredEquityInvestmentsToCoreCapitalRatio,
                            StoredEquityInvestmentsExcessDeficiency = OldItem.StoredEquityInvestmentsExcessDeficiency,
                            StoredSubsidiaryInvestmentsToCoreCapitalRatio = OldItem.StoredSubsidiaryInvestmentsToCoreCapitalRatio,
                            StoredSubsidiaryInvestmentsExcessDeficiency = OldItem.StoredSubsidiaryInvestmentsExcessDeficiency,
                            StoredOtherInvestmentsToCoreCapitalRatio = OldItem.StoredOtherInvestmentsToCoreCapitalRatio,
                            StoredOtherInvestmentsExcessDeficiency = OldItem.StoredOtherInvestmentsExcessDeficiency,

                            // Metadata
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Period = OldItem.Period,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.NWDTInvestmentReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.NWDTInvestmentReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsFinancialPosition)
                {
                    var nwdtFinancialPosition = await _context.NWDTFinancialPositionReturns
                      .Where(f => f.ReturnId == OldReturnId)
                      .ToListAsync();

                    foreach (var OldItem in nwdtFinancialPosition)
                    {
                        var newItem = new NWDTFinancialPositionReturn
                        {
                            Id = Guid.NewGuid().ToString(),
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            // Cash & Cash Equivalent
                            CashInHand = OldItem.CashInHand,
                            CashAtBank = OldItem.CashAtBank,
                            StoredCashAndCashEquivalent = OldItem.StoredCashAndCashEquivalent,

                            // Prepayments & Sundry Receivables
                            PrepaymentsAndSundryReceivables = OldItem.PrepaymentsAndSundryReceivables,

                            // Financial Investments
                            GovernmentSecurities = OldItem.GovernmentSecurities,
                            PlacementInFinancialInstitutions = OldItem.PlacementInFinancialInstitutions,
                            CommercialPapers = OldItem.CommercialPapers,
                            CollectiveInvestmentSchemes = OldItem.CollectiveInvestmentSchemes,
                            Derivatives = OldItem.Derivatives,
                            EquityInvestments = OldItem.EquityInvestments,
                            InvestmentInCompanies = OldItem.InvestmentInCompanies,
                            StoredFinancialInvestments = OldItem.StoredFinancialInvestments,

                            // Loan Portfolio
                            GrossLoanPortfolio = OldItem.GrossLoanPortfolio,
                            AllowanceForLoanLoss = OldItem.AllowanceForLoanLoss,
                            StoredNetLoanPortfolio = OldItem.StoredNetLoanPortfolio,

                            // Accounts Receivables
                            TaxRecoverable = OldItem.TaxRecoverable,
                            DeferredTaxAssets = OldItem.DeferredTaxAssets,
                            RetirementBenefitAssets = OldItem.RetirementBenefitAssets,
                            StoredAccountsReceivables = OldItem.StoredAccountsReceivables,

                            // Property & Equipment & Other Assets
                            InvestmentProperties = OldItem.InvestmentProperties,
                            PropertyAndEquipment = OldItem.PropertyAndEquipment,
                            PrepaidLeaseRentals = OldItem.PrepaidLeaseRentals,
                            IntangibleAssets = OldItem.IntangibleAssets,
                            OtherAssets = OldItem.OtherAssets,
                            StoredPropertyEquipmentOtherAssets = OldItem.StoredPropertyEquipmentOtherAssets,

                            // Total Assets
                            StoredTotalAssets = OldItem.StoredTotalAssets,

                            // LIABILITIES - Deposits
                            NonWithdrawableDeposits = OldItem.NonWithdrawableDeposits,
                            StoredTotalDepositLiabilities = OldItem.StoredTotalDepositLiabilities,

                            // Accounts Payable & Other Liabilities
                            TaxPayable = OldItem.TaxPayable,
                            DividendsPayable = OldItem.DividendsPayable,
                            DeferredTaxLiability = OldItem.DeferredTaxLiability,
                            RetirementBenefitsLiability = OldItem.RetirementBenefitsLiability,
                            OtherLiabilities = OldItem.OtherLiabilities,
                            ExternalBorrowings = OldItem.ExternalBorrowings,
                            StoredAccountsPayableOtherLiabilities = OldItem.StoredAccountsPayableOtherLiabilities,

                            // Total Liabilities
                            StoredTotalLiabilities = OldItem.StoredTotalLiabilities,

                            // EQUITY
                            ShareCapital = OldItem.ShareCapital,
                            CapitalGrants = OldItem.CapitalGrants,

                            // Retained Earnings
                            PriorYearsRetainedEarnings = OldItem.PriorYearsRetainedEarnings,
                            CurrentYearSurplus = OldItem.CurrentYearSurplus,
                            StoredRetainedEarnings = OldItem.StoredRetainedEarnings,

                            // Other Equity Accounts
                            StatutoryReserve = OldItem.StatutoryReserve,
                            OtherReserves = OldItem.OtherReserves,
                            RevaluationReserves = OldItem.RevaluationReserves,
                            ProposedDividends = OldItem.ProposedDividends,
                            AdjustmentToEquity = OldItem.AdjustmentToEquity,
                            StoredOtherEquityAccounts = OldItem.StoredOtherEquityAccounts,

                            // Total Equity and Total Liabilities & Equity
                            StoredTotalEquity = OldItem.StoredTotalEquity,
                            StoredTotalLiabilitiesAndEquity = OldItem.StoredTotalLiabilitiesAndEquity,

                            // Metadata
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Period = OldItem.Period,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.NWDTFinancialPositionReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.NWDTFinancialPositionReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }

                if (form.IsStatementOfComprehensiveIncome)
                {
                    var nwdtComprehensiveIncome = await _context.NWDTComprehensiveIncomeReturns
                       .Where(c => c.ReturnId == OldReturnId)
                       .ToListAsync();

                    foreach (var OldItem in nwdtComprehensiveIncome)
                    {
                        var newItem = new NWDTComprehensiveIncomeReturn
                        {
                            Id = Guid.NewGuid().ToString(),
                            ReturnId = NewReturnId,
                            PreviousReturnId = OldReturnId,

                            // Financial Income - Loans Portfolio
                            InterestOnLoanPortfolio = OldItem.InterestOnLoanPortfolio,
                            FeesCommissionOnLoanPortfolio = OldItem.FeesCommissionOnLoanPortfolio,

                            // Financial Income - Investments
                            GovernmentSecuritiesIncome = OldItem.GovernmentSecuritiesIncome,
                            PlacementInBanksIncome = OldItem.PlacementInBanksIncome,
                            CommercialPapersIncome = OldItem.CommercialPapersIncome,
                            CollectiveInvestmentSchemesIncome = OldItem.CollectiveInvestmentSchemesIncome,
                            DerivativesIncome = OldItem.DerivativesIncome,
                            EquityInvestmentsIncome = OldItem.EquityInvestmentsIncome,
                            InvestmentInCompaniesIncome = OldItem.InvestmentInCompaniesIncome,

                            // Financial Expense
                            InterestExpenseOnDeposits = OldItem.InterestExpenseOnDeposits,
                            CostOfExternalBorrowings = OldItem.CostOfExternalBorrowings,
                            DividendExpenses = OldItem.DividendExpenses,
                            OtherFinancialExpense = OldItem.OtherFinancialExpense,
                            FeesCommissionExpense = OldItem.FeesCommissionExpense,
                            OtherExpense = OldItem.OtherExpense,

                            // Loan Loss
                            ProvisionForLoanLosses = OldItem.ProvisionForLoanLosses,
                            ValueOfLoansRecovered = OldItem.ValueOfLoansRecovered,

                            // Operating Expenses
                            PersonnelExpenses = OldItem.PersonnelExpenses,
                            GovernanceExpenses = OldItem.GovernanceExpenses,
                            MarketingExpenses = OldItem.MarketingExpenses,
                            DepreciationAmortizationCharges = OldItem.DepreciationAmortizationCharges,
                            AdministrativeExpenses = OldItem.AdministrativeExpenses,

                            // Non-Operating Income/Expense
                            NonOperatingIncome = OldItem.NonOperatingIncome,
                            NonOperatingExpense = OldItem.NonOperatingExpense,

                            // Taxes and Donations
                            Taxes = OldItem.Taxes,
                            Donations = OldItem.Donations,

                            // Stored calculated values
                            StoredFinancialIncomeFromLoansPortfolio = OldItem.StoredFinancialIncomeFromLoansPortfolio,
                            StoredFinancialIncomeFromInvestments = OldItem.StoredFinancialIncomeFromInvestments,
                            StoredFinancialIncome = OldItem.StoredFinancialIncome,
                            StoredFinancialExpense = OldItem.StoredFinancialExpense,
                            StoredNetFinancialIncome = OldItem.StoredNetFinancialIncome,
                            StoredAllowanceForLoanLoss = OldItem.StoredAllowanceForLoanLoss,
                            StoredOperatingExpenses = OldItem.StoredOperatingExpenses,
                            StoredNetOperatingIncome = OldItem.StoredNetOperatingIncome,
                            StoredNetNonOperatingIncome = OldItem.StoredNetNonOperatingIncome,
                            StoredNetIncomeBeforeTaxes = OldItem.StoredNetIncomeBeforeTaxes,
                            StoredNetIncomeAfterTaxesBeforeDonations = OldItem.StoredNetIncomeAfterTaxesBeforeDonations,
                            StoredNetIncomeAfterTaxesAndDonations = OldItem.StoredNetIncomeAfterTaxesAndDonations,

                            // Metadata
                            StartDate = OldItem.StartDate,
                            EndDate = OldItem.EndDate,
                            Period = OldItem.Period,
                            Frequency = OldItem.Frequency,
                            FilePath = OldItem.FilePath,
                            DaysLateBy = OldItem.DaysLateBy,
                            IsAmended = false  // Will be set to true later if this form is amended
                        };

                        await _context.NWDTComprehensiveIncomeReturns.AddAsync(newItem);
                        OldItem.IsCurrent = false;
                        OldItem.IsAmended = true;
                        _context.NWDTComprehensiveIncomeReturns.Update(OldItem);
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }


            }
            return false;
        }

        public async Task<(bool IsAmendment, string ReturnId)> IsAmendmentBasedOnReportingPeriod(IFormFile formFile, ReturnForm form, string SaccoType, string SaccoId)
        {
            try
            {
                DateTime reportingEndDate = DateTime.MinValue;
                string Year = string.Empty;
                bool hasExistingReturn = false;
                string returnId = string.Empty;

                // Extract the reporting period end date from the form
                (reportingEndDate, Year) = await ExtractReportingEndDate(formFile, form, SaccoType);

                if (reportingEndDate == DateTime.MinValue)
                {
                    // Couldn't determine the end date, assume it's not an amendment
                    return (false, string.Empty);
                }

                // Check if there's an existing return for this period
                if (SaccoType == Constants.SaccoType.NWDT)
                {
                    if (form.IsCapitalAdequencyForm)
                    {
                        var existingReturn = await _context
                            .NDWTCapitalAdequacyReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate == reportingEndDate && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsLiquidityStatement)
                    {
                        var existingReturn = await _context.NDWTLiquidityReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate == reportingEndDate && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsFinancialPosition)
                    {
                        var existingReturn = await _context.NWDTFinancialPositionReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate == reportingEndDate && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsInvestmentReturn)
                    {
                        var existingReturn = await _context.NWDTInvestmentReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate == reportingEndDate && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsStatementOfComprehensiveIncome)
                    {
                        var existingReturn = await _context.NWDTComprehensiveIncomeReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate == reportingEndDate && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsRiskClassification)
                    {
                        var existingReturn = await _context.NWDTRiskClassificationReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate == reportingEndDate && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsDepositReturnForm)
                    {
                        var existingReturn = await _context.NWDTDepositReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate == reportingEndDate && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                }
                else
                {
                    if (form.IsCapitalAdequencyForm)
                    {
                        var existingReturn = await _context
                            .CapitalAdequacies
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate.Date == reportingEndDate.Date && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsLiquidityStatement)
                    {
                        var existingReturn = await _context.LiquidityReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate.Date == reportingEndDate.Date && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsFinancialPosition)
                    {
                        var existingReturn = await _context.StatementOfFinancialPositionReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate.Date == reportingEndDate.Date && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsInvestmentReturn)
                    {
                        var existingReturn = await _context.InvestmentReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate.Date == reportingEndDate.Date && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsStatementOfComprehensiveIncome)
                    {
                        var existingReturn = await _context.StatementOfComprehensiveIncomeReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate.Date == reportingEndDate.Date && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsRiskClassification)
                    {
                        var existingReturn = await _context.RiskClassifications
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate.Date == reportingEndDate.Date && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                    else if (form.IsDepositReturnForm)
                    {
                        var existingReturn = await _context.DepositReturns
                            .Include(c => c.Return)
                            .FirstOrDefaultAsync(c => c.EndDate.Date == reportingEndDate.Date && c.Return.SaccoId == SaccoId && c.Return.Year == Year && c.IsAmended == false);

                        hasExistingReturn = existingReturn != null;
                        returnId = existingReturn?.Return?.Id ?? string.Empty;
                    }
                }

                // Calculate the due date based on the reporting period
                DateTime dueDate = CalculateDueDate(reportingEndDate, form.Period.Name);

                // It's an amendment if:
                // 1. There's an existing return AND
                // 2. The current date is after the due date
                bool isAmendment = hasExistingReturn; //&& DateTime.Now > dueDate;

                // Only return the ReturnId if it's an amendment
                return (isAmendment, isAmendment ? returnId : string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error determining if form is an amendment");
                // Default to false if there's an error
                return (false, string.Empty);
            }
        }


        private DateTime CalculateDueDate(DateTime reportingEndDate, string periodType)
        {
            switch (periodType)
            {
                case "Daily":
                    // Daily forms are due the same day
                    return reportingEndDate;

                case "Monthly":
                    // Monthly forms are due by the 15th of the next month
                    var monthlyDue = reportingEndDate.AddMonths(1);
                    return new DateTime(monthlyDue.Year, monthlyDue.Month, 15);

                case "Quarterly":
                    // Quarterly forms are due by the 15th of the month after the quarter ends
                    var quarter = (reportingEndDate.Month - 1) / 3 + 1;
                    var quarterEnd = new DateTime(reportingEndDate.Year, quarter * 3, 1).AddDays(-1);
                    var quarterlyDue = quarterEnd.AddMonths(1);
                    return new DateTime(quarterlyDue.Year, quarterlyDue.Month, 15);

                case "Annual":
                    // Annual forms are due by January 15th of the next year
                    return new DateTime(reportingEndDate.Year + 1, 1, 15);

                case "Semi-Annual":
                    // Semi-annual forms are due 15 days after the half-year ends
                    if (reportingEndDate.Month <= 6)
                    {
                        // First half ends June 30, due by July 15
                        return new DateTime(reportingEndDate.Year, 7, 15);
                    }
                    else
                    {
                        // Second half ends December 31, due by January 15
                        return new DateTime(reportingEndDate.Year + 1, 1, 15);
                    }

                case "Bi-Monthly":
                    // Bi-monthly forms are due on the 1st or 15th
                    if (reportingEndDate.Day <= 15)
                    {
                        return new DateTime(reportingEndDate.Year, reportingEndDate.Month, 15);
                    }
                    else
                    {
                        var nextMonth = reportingEndDate.AddMonths(1);
                        return new DateTime(nextMonth.Year, nextMonth.Month, 1);
                    }

                default:
                    // Default to 15 days after the end date if unknown period type
                    return reportingEndDate.AddDays(15);
            }
        }


        private async Task<(DateTime EndDate, string Year)> ExtractReportingEndDate(IFormFile formFile, ReturnForm form, string saccoType)
        {
            try
            {
                bool isDepositTaking = saccoType == Constants.SaccoType.DepositTaking;

                if (isDepositTaking)
                {
                    // Process Deposit Taking SACCO forms
                    if (form.IsCapitalAdequencyForm)
                    {
                        var formData = ExcelService.ImportCapitalAdequacyRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsLiquidityStatement)
                    {
                        var formData = ExcelService.ImportLiquidityStatementRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsDepositReturnForm)
                    {
                        var formData = ExcelService.ImportDepositRangeDataRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsRiskClassification)
                    {
                        var formData = ExcelService.ImportRiskClassificationRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsInvestmentReturn)
                    {
                        var formData = ExcelService.ImportInvestmentRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsFinancialPosition)
                    {
                        var formData = ExcelService.ImportFinancialPositionRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsStatementOfComprehensiveIncome)
                    {
                        var formData = ExcelService.ImportStatementOfComprehensiveIncomeRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                }
                else
                {
                    // Process Non-Deposit Taking SACCO forms
                    if (form.IsCapitalAdequencyForm)
                    {
                        var formData = ExcelService.ImportForm2ARows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsLiquidityStatement)
                    {
                        var formData = ExcelService.ImportForm2BStatement(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsDepositReturnForm)
                    {
                        var formData = ExcelService.ImportForm2CDataRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsRiskClassification)
                    {
                        var formData = ExcelService.ImportForm2DRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsInvestmentReturn)
                    {
                        var formData = ExcelService.ImportForm2ERows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsFinancialPosition)
                    {
                        var formData = ExcelService.ImportForm2GRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                    else if (form.IsStatementOfComprehensiveIncome)
                    {
                        var formData = ExcelService.ImportForm2FRows(formFile, _logger);
                        return (formData.EndDate, formData.Period);
                    }
                }

                return (DateTime.MinValue, "");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting reporting end date");
                return (DateTime.MinValue, "");
            }
        }



        private async Task<(bool Success, string Message)> ProcessFormByType(IFormFile formFile, ReturnForm form, string returnId, string saccoType, Boolean IsAmendMent, string PrevReturnId = "")
        {
            try
            {
                // Check if this is a Deposit Taking SACCO
                bool isDepositTaking = saccoType == Constants.SaccoType.DepositTaking;

                // Determine what type of form we're dealing with
                string formType = GetFormTypeFromForm(form);
                if (formType == null)
                {
                    return (false, "Unknown form type");
                }

                // Process the form based on SACCO type and form type
                if (isDepositTaking)
                {
                    // Handle Deposit Taking SACCO forms
                    switch (formType)
                    {
                        case "CapitalAdequacy":
                            await ReturnsHelper.ProcessCapitalAdequacyForm(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "Liquidity":
                            await ReturnsHelper.ProcessLiquidityForm(formFile, returnId, _logger, form);
                            break;
                        case "DepositReturn":
                            await ReturnsHelper.ProcessDepositReturnForm(formFile, returnId, _logger, form);
                            break;
                        case "RiskClassification":
                            await ReturnsHelper.ProcessRiskClassificationForm(formFile, returnId, _logger, form);
                            break;
                        case "Investment":
                            await ReturnsHelper.ProcessInvestmentReturnForm(formFile, returnId, _logger, form);
                            break;
                        case "FinancialPosition":
                            await ReturnsHelper.ProcessFinancialPositionForm(formFile, returnId, _logger, form);
                            break;
                        case "ComprehensiveIncome":
                            await ReturnsHelper.ProcessComprehensiveIncomeForm(formFile, returnId, _logger, form);
                            break;
                        case "SectoralLending":
                            await ReturnsHelper.ProcessSectoralLendingForm(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        default:
                            return (false, $"No processor found for form type: {formType}");
                    }
                }
                else
                {
                    // Handle Non-Deposit Taking SACCO forms
                    switch (formType)
                    {
                        case "CapitalAdequacy":
                            await ReturnsHelper.ProcessForm2A(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "Liquidity":
                            await ReturnsHelper.ProcessForm2B(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "DepositReturn":
                            await ReturnsHelper.ProcessForm2C(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "RiskClassification":
                            await ReturnsHelper.ProcessForm2D(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "Investment":
                            await ReturnsHelper.ProcessForm2E(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "FinancialPosition":
                            await ReturnsHelper.ProcessForm2G(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "ComprehensiveIncome":
                            await ReturnsHelper.ProcessForm2F(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        case "SectoralLending":
                           await ReturnsHelper.ProcessSectoralLendingForm(formFile, returnId, _logger, form, IsAmendMent, PrevReturnId);
                            break;
                        default:
                            return (false, $"No processor found for form type: {formType}");
                    }
                }

                return (true, $"Successfully processed {formType} form");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in form processing");
                return (false, $"Error in form processing: {ex.Message}");
            }
        }

        // Helper method to determine form type
        private string GetFormTypeFromForm(ReturnForm form)
        {
            if (form.IsCapitalAdequencyForm) return "CapitalAdequacy";
            if (form.IsLiquidityStatement) return "Liquidity";
            if (form.IsDepositReturnForm) return "DepositReturn";
            if (form.IsRiskClassification) return "RiskClassification";
            if (form.IsInvestmentReturn) return "Investment";
            if (form.IsFinancialPosition) return "FinancialPosition";
            if (form.IsSectoralLending) return "SectoralLending";
            if (form.IsStatementOfComprehensiveIncome) return "ComprehensiveIncome";
            return null;
        }

        private async Task MarkFormAsAmended(string returnId, string formType, string saccoType)
        {
            bool isDepositTaking = saccoType == Constants.SaccoType.DepositTaking.ToString();

            if (isDepositTaking)
            {
                // Handle Deposit Taking SACCO forms
                switch (formType)
                {
                    case "CapitalAdequacy":
                        var capItems = await _context.CapitalAdequacies
                            .Where(c => c.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in capItems)
                        {
                            item.IsAmended = true;
                            _context.CapitalAdequacies.Update(item);
                        }
                        break;

                    case "Liquidity":
                        var liqItems = await _context.LiquidityReturns
                            .Where(l => l.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in liqItems)
                        {
                            item.IsAmended = true;
                            _context.LiquidityReturns.Update(item);
                        }
                        break;

                    case "DepositReturn":
                        var depItems = await _context.DepositReturns
                            .Where(d => d.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in depItems)
                        {
                            item.IsAmended = true;
                            _context.DepositReturns.Update(item);
                        }
                        break;

                    case "RiskClassification":
                        var riskItems = await _context.RiskClassifications
                            .Where(r => r.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in riskItems)
                        {
                            item.IsAmended = true;
                            _context.RiskClassifications.Update(item);
                        }
                        break;

                    case "Investment":
                        var invItems = await _context.InvestmentReturns
                            .Where(i => i.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in invItems)
                        {
                            item.IsAmended = true;
                            _context.InvestmentReturns.Update(item);
                        }
                        break;

                    case "FinancialPosition":
                        var fpItems = await _context.StatementOfFinancialPositionReturns
                            .Where(s => s.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in fpItems)
                        {
                            item.IsAmended = true;
                            _context.StatementOfFinancialPositionReturns.Update(item);
                        }
                        break;

                    case "ComprehensiveIncome":
                        var ciItems = await _context.StatementOfComprehensiveIncomeReturns
                            .Where(s => s.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in ciItems)
                        {
                            item.IsAmended = true;
                            _context.StatementOfComprehensiveIncomeReturns.Update(item);
                        }
                        break;
                }
            }
            else
            {
                // Handle Non-Deposit Taking SACCO forms
                switch (formType)
                {
                    case "CapitalAdequacy":
                        var capItems = await _context.NDWTCapitalAdequacyReturns
                            .Where(c => c.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in capItems)
                        {
                            item.IsAmended = true;
                            _context.NDWTCapitalAdequacyReturns.Update(item);
                        }
                        break;

                    case "Liquidity":
                        var liqItems = await _context.NDWTLiquidityReturns
                            .Where(l => l.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in liqItems)
                        {
                            item.IsAmended = true;
                            _context.NDWTLiquidityReturns.Update(item);
                        }
                        break;

                    case "DepositReturn":
                        var depItems = await _context.NWDTDepositReturns
                            .Where(d => d.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in depItems)
                        {
                            item.IsAmended = true;
                            _context.NWDTDepositReturns.Update(item);
                        }
                        break;

                    case "RiskClassification":
                        var riskItems = await _context.NWDTRiskClassificationReturns
                            .Where(r => r.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in riskItems)
                        {
                            item.IsAmended = true;
                            _context.NWDTRiskClassificationReturns.Update(item);
                        }
                        break;

                    case "Investment":
                        var invItems = await _context.NWDTInvestmentReturns
                            .Where(i => i.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in invItems)
                        {
                            item.IsAmended = true;
                            _context.NWDTInvestmentReturns.Update(item);
                        }
                        break;

                    case "FinancialPosition":
                        var fpItems = await _context.NWDTFinancialPositionReturns
                            .Where(s => s.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in fpItems)
                        {
                            item.IsAmended = true;
                            _context.NWDTFinancialPositionReturns.Update(item);
                        }
                        break;

                    case "ComprehensiveIncome":
                        var ciItems = await _context.NWDTComprehensiveIncomeReturns
                            .Where(s => s.ReturnId == returnId)
                            .ToListAsync();
                        foreach (var item in ciItems)
                        {
                            item.IsAmended = true;
                            _context.NWDTComprehensiveIncomeReturns.Update(item);
                        }
                        break;
                }
            }

            if (formType == "Other")
            {
                var otherItems = await _context.OtherReturns
                    .Where(o => o.ReturnId == returnId)
                    .ToListAsync();
                foreach (var item in otherItems)
                {
                    item.IsAmended = true;
                    _context.OtherReturns.Update(item);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
