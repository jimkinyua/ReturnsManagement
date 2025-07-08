using Microsoft.EntityFrameworkCore;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using static Returns.Helpers.ReturnAnalysisHelper;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public class ReturnSubmissionService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<ReturnSubmissionService> _logger;
        private readonly IEmailService _emailService;
        private readonly IReturnAssignmentService _returnAssignmentService;
        private readonly IWorkflowEngineService _workflowService;
        private readonly ICamelsAnalysisService _camelsAnalysisService;
        private readonly IComplianceService _complianceService;

        public ReturnSubmissionService(
            ReturnsDbContext context,
            ILogger<ReturnSubmissionService> logger,
            IEmailService emailService,
            IReturnAssignmentService returnAssignmentService,
            IWorkflowEngineService workflowService,
            ICamelsAnalysisService camelsAnalysisService,
            IComplianceService complianceService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _returnAssignmentService = returnAssignmentService;
            _workflowService = workflowService;
            _camelsAnalysisService = camelsAnalysisService;
            _complianceService = complianceService;
        }

        public async Task<SubmissionResult> SubmitDraftReturnAsync(string draftReturnId, LoggedInEntity loggedInSacco)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Get draft return
                var draftReturn = await _context.Returns
                    .FirstOrDefaultAsync(r => r.Id == draftReturnId && r.IsDraft);
                    
                if (draftReturn == null)
                {
                    return new SubmissionResult
                    {
                        Success = false,
                        Message = "Draft return not found"
                    };
                }

                // 2. Validate draft has required forms
                var validationResult = await ValidateDraftCompleteness(draftReturn);
                if (!validationResult.IsValid)
                {
                    return new SubmissionResult
                    {
                        Success = false,
                        Message = validationResult.Message,
                        ValidationErrors = validationResult.Errors
                    };
                }

                // 3. Run consistency checks
                var consistencyResult = await RunConsistencyChecks(draftReturn, loggedInSacco.SaccoType);
                
                // 4. Update return status
                draftReturn.IsDraft = false;
                draftReturn.IsActiveVersion = true;
                draftReturn.SubmittedAt = DateTime.Now;
                draftReturn.IsNotConsistent = !consistencyResult.IsConsistent;
                draftReturn.ConsistentErrorMessage = string.Join("; ", consistencyResult.Errors);

                // 5. Update all child entities to non-draft
                await UpdateChildEntitiesToSubmitted(draftReturn.Id);

                // 6. Assign return to compliance officer
                var assignmentResult = await _returnAssignmentService.AssignReturnAsync(draftReturn, loggedInSacco.SaccoId);
                if (!assignmentResult.Success)
                {
                    throw new Exception($"Failed to assign return: {assignmentResult.ErrorMessage}");
                }

                // 7. Calculate CAMELS rating
                var ratingResult = await _camelsAnalysisService.CalculateAnalysisAsync(draftReturn.Id, draftReturn.SaccoType);

                // 8. Start workflow
                var workflowResult = await _workflowService.StartWorkflowAsync(draftReturn, ratingResult.OverallRating);

                // 9. Send confirmation email
                await SendSubmissionConfirmationEmail(loggedInSacco, draftReturn, consistencyResult);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SubmissionResult
                {
                    Success = true,
                    ReturnId = draftReturn.Id,
                    IsConsistent = consistencyResult.IsConsistent,
                    Message = consistencyResult.IsConsistent 
                        ? "Return submitted successfully" 
                        : "Return submitted with consistency errors",
                    ConsistencyErrors = consistencyResult.Errors
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error submitting draft return {ReturnId}", draftReturnId);
                
                return new SubmissionResult
                {
                    Success = false,
                    Message = $"Error submitting return: {ex.Message}"
                };
            }
        }

        private async Task<ValidationResult> ValidateDraftCompleteness(Return draftReturn)
        {
            var errors = new List<string>();
            
            // Check if draft has minimum required forms based on SACCO type
            if (draftReturn.SaccoType == Constants.SaccoType.DepositTaking.ToString())
            {
                var hasCapitalAdequacy = await _context.DTCapitalAdequacyReturns
                    .AnyAsync(c => c.ReturnId == draftReturn.Id);
                var hasLiquidity = await _context.DTLiquidityReturns
                    .AnyAsync(l => l.ReturnId == draftReturn.Id);
                    
                if (!hasCapitalAdequacy) errors.Add("Capital Adequacy form is required");
                if (!hasLiquidity) errors.Add("Liquidity Statement is required");
            }
            else
            {
                var hasCapitalAdequacy = await _context.NWDTCapitalAdequacyReturns
                    .AnyAsync(c => c.ReturnId == draftReturn.Id);
                var hasLiquidity = await _context.NDWTLiquidityReturns
                    .AnyAsync(l => l.ReturnId == draftReturn.Id);
                    
                if (!hasCapitalAdequacy) errors.Add("Capital Adequacy form is required");
                if (!hasLiquidity) errors.Add("Liquidity Statement is required");
            }
            
            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Message = errors.Count > 0 ? "Missing required forms" : "All required forms present",
                Errors = errors
            };
        }

        private async Task<ConsistencyCheckResult> RunConsistencyChecks(Return draftReturn, string saccoType)
        {
            var errors = new List<string>();
            var validationErrors = new List<ValidationError>();
            
            try
            {
                if (saccoType == Constants.SaccoType.DepositTaking.ToString())
                {
                    // Load all DT forms for consistency check
                    var capitalAdequacy = await _context.DTCapitalAdequacyReturns
                        .FirstOrDefaultAsync(c => c.ReturnId == draftReturn.Id);
                    var liquidity = await _context.DTLiquidityReturns
                        .FirstOrDefaultAsync(l => l.ReturnId == draftReturn.Id);
                    var deposits = await _context.DepositReturns
                        .Where(d => d.ReturnId == draftReturn.Id)
                        .ToListAsync();
                    var riskClassifications = await _context.DTRiskClassificationReturns
                        .Where(r => r.ReturnId == draftReturn.Id)
                        .ToListAsync();
                    var investment = await _context.DTInvestmentReturns
                        .FirstOrDefaultAsync(i => i.ReturnId == draftReturn.Id);
                    var financialPosition = await _context.DTFinancialPositionReturns
                        .FirstOrDefaultAsync(f => f.ReturnId == draftReturn.Id);
                    var comprehensiveIncome = await _context.DTComprehensiveIncomeReturns
                        .FirstOrDefaultAsync(c => c.ReturnId == draftReturn.Id);

                    // Run consistency validation
                    if (AllFormsPresent(capitalAdequacy, liquidity, deposits, riskClassifications, 
                        investment, financialPosition, comprehensiveIncome))
                    {
                        // Convert entities to rows for validation
                        var validationResult = ValidateDTConsistency(
                            capitalAdequacy, liquidity, deposits, riskClassifications,
                            investment, financialPosition, comprehensiveIncome);
                            
                        if (!validationResult.IsValid)
                        {
                            validationErrors.AddRange(validationResult.ValidationErrors);
                        }
                    }
                }
                else
                {
                    // Similar logic for NWDT forms
                    // ... implement NWDT consistency checks
                }
                
                return new ConsistencyCheckResult
                {
                    IsConsistent = validationErrors.Count == 0,
                    Errors = validationErrors.Select(e => $"{e.Category}: {e.Description}").ToList(),
                    ValidationErrors = validationErrors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running consistency checks");
                errors.Add($"Error during consistency check: {ex.Message}");
                
                return new ConsistencyCheckResult
                {
                    IsConsistent = false,
                    Errors = errors
                };
            }
        }

        private bool AllFormsPresent(params object[] forms)
        {
            return forms.All(f => f != null);
        }

        private ValidationResult ValidateDTConsistency(
            DTCapitalAdequacyReturn capitalAdequacy,
            DTLiquidityReturn liquidity,
            List<DepositReturn> deposits,
            List<DTRiskClassificationReturn> riskClassifications,
            DTInvestmentReturn investment,
            DTFinancialPositionReturn financialPosition,
            DTComprehensiveIncomeReturn comprehensiveIncome)
        {
            // This is a simplified version - you'll need to implement the actual validation logic
            // based on your business rules
            var errors = new List<ValidationError>();
            
            // Example validation: Check if total assets match across forms
            if (capitalAdequacy.TotalAssets != financialPosition.TotalAssets)
            {
                errors.Add(new ValidationError
                {
                    Category = "Asset Consistency",
                    Description = "Total assets mismatch between Capital Adequacy and Financial Position",
                    Details = new Dictionary<string, string>
                    {
                        ["Capital Adequacy Total Assets"] = capitalAdequacy.TotalAssets.ToString(),
                        ["Financial Position Total Assets"] = financialPosition.TotalAssets.ToString()
                    }
                });
            }
            
            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                ValidationErrors = errors
            };
        }

        private async Task UpdateChildEntitiesToSubmitted(string returnId)
        {
            // Update all child entities to mark them as submitted (not draft)
            var dtCapitalAdequacy = await _context.DTCapitalAdequacyReturns
                .Where(c => c.ReturnId == returnId)
                .ToListAsync();
            foreach (var item in dtCapitalAdequacy)
            {
                item.IsDraft = false;
            }

            var dtLiquidity = await _context.DTLiquidityReturns
                .Where(l => l.ReturnId == returnId)
                .ToListAsync();
            foreach (var item in dtLiquidity)
            {
                item.IsDraft = false;
            }

            // ... repeat for other entity types
            
            await _context.SaveChangesAsync();
        }

        private async Task SendSubmissionConfirmationEmail(
            LoggedInEntity loggedInSacco, 
            Return submittedReturn, 
            ConsistencyCheckResult consistencyResult)
        {
            var saccoDetails = await _complianceService.GetSaccoByIdAsync(loggedInSacco.SaccoId);
            
            string subject = consistencyResult.IsConsistent 
                ? "Return Submission Confirmation" 
                : "Return Submission Confirmation - Consistency Errors Found";
                
            string body = GenerateSubmissionEmailBody(
                saccoDetails.SaccoName, 
                submittedReturn.Year, 
                consistencyResult);
                
            await _emailService.SendEmailAsync(
                saccoDetails.OfficialSaccoEmail, 
                subject, 
                body);
        }

        private string GenerateSubmissionEmailBody(
            string saccoName, 
            string period, 
            ConsistencyCheckResult consistencyResult)
        {
            var body = $@"
                <p>Dear {saccoName},</p>
                <p>Your returns for period {period} have been successfully submitted.</p>
            ";
            
            if (!consistencyResult.IsConsistent)
            {
                body += @"
                    <p><strong>Please note:</strong> The following consistency errors were found in your submission:</p>
                    <ul>
                ";
                
                foreach (var error in consistencyResult.Errors)
                {
                    body += $"<li>{error}</li>";
                }
                
                body += "</ul>";
            }
            
            body += @"
                <p>Thank you for your submission.</p>
                <p>Best regards,<br>Compliance Team</p>
            ";
            
            return body;
        }
    }

    // Result classes
    public class SubmissionResult
    {
        public bool Success { get; set; }
        public string? ReturnId { get; set; }
        public string Message { get; set; }
        public bool IsConsistent { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> ConsistencyErrors { get; set; } = new();
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<ValidationError> ValidationErrors { get; set; } = new();
    }

    public class ConsistencyCheckResult
    {
        public bool IsConsistent { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<ValidationError> ValidationErrors { get; set; } = new();
    }
}
