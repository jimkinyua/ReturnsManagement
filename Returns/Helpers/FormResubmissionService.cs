/*using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using static Returns.Helpers.Constants;

namespace Returns.Helpers
{
    public class FormResubmissionService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly FormProcessingService _formProcessingService;
        public FormResubmissionService(ReturnsDbContext context, IEmailService emailService, ILogger logger, FormProcessingService formProcessingService)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _formProcessingService = formProcessingService;
        }

        public async Task<(bool Success, string Message)> RequestFormResubmissionAsync(
             string ReturnId,
             ReturnForm form,
             string complianceOfficerId,
             string complianceOfficerName,
             string complianceOfficerEmail,
             string reason,
             string SaccoId,
             string saccoType,
             string SaccoEmail)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var returnDetails = await _context.Returns.FindAsync(ReturnId);
                if (returnDetails == null)
                {
                    return (false, "Return not found");
                }

                var (returnInfo, childFormId) = await GetReturnWithCurrentChildFormAsync(ReturnId, form.Id, returnDetails.SaccoType);
                if (string.IsNullOrEmpty(childFormId))
                {
                    return (false, "No current form found for this return and form type");
                }

                var existingRequest = await _context.FormResubmissionRequests
                    .FirstOrDefaultAsync(r => r.ChildId == childFormId && r.Status == "Pending");
                if (existingRequest != null)
                {
                    return (false, "There is already a pending resubmission request for this form");
                }

                var resubmissionRequest = new FormResubmissionRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    FormId = form.Id,
                    ReturnId = ReturnId,
                    SaccoId = returnDetails.SaccoId,
                    SaccoType = returnDetails.SaccoType,
                    RequestedBy = complianceOfficerId,
                    RequestedByEmail = complianceOfficerEmail,
                    RequestedByName = complianceOfficerName,
                    ChildId = childFormId,
                    Reason = reason,
                    RequestedAt = DateTime.Now,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                };

                await _context.FormResubmissionRequests.AddAsync(resubmissionRequest);

                var flagResult = await UpdateRequiresResubmissionFlagAsync(childFormId, form, true, reason, returnDetails.SaccoType);
                if (!flagResult.Success)
                {
                    // CRITICAL FAILURE - rollback everything
                    _logger.LogError($"Failed to set RequiresResubmission flag for {form.FormName} ({childFormId}): {flagResult.Message}");
                    await transaction.RollbackAsync();
                    return (false, $"Failed to mark form for resubmission: {flagResult.Message}");
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                try
                {
                    await SendResubmissionNotificationEmail(returnDetails, form.FormName, SaccoEmail, reason);
                }
                catch (Exception emailEx)
                {
                    // Log email failure but don't fail the entire operation
                    _logger.LogWarning(emailEx, $"Resubmission request created successfully but failed to send email notification to {SaccoEmail}");
                }

                _logger.LogInformation($"Form resubmission requested for {form.FormName} by {complianceOfficerName}. ChildId: {childFormId}");

                return (true, "Resubmission request sent successfully");
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error requesting form resubmission for ReturnId: {ReturnId}. Transaction rolled back.", ReturnId);
                return (false, $"Error: {ex.Message}");
            }
        }

        private async Task SendResubmissionNotificationEmail(Return returnDetails, string formName, string SaccoEmail, string reason)
        {
            try
            {
                var subject = $"Form Resubmission Required - {formName}";

                var body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .header {{ background-color: #f8f9fa; padding: 20px; border-left: 4px solid #007bff; }}
                            .content {{ padding: 20px; }}
                            .reason-box {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; margin: 15px 0; border-radius: 5px; }}
                            .action-box {{ background-color: #d1ecf1; border: 1px solid #bee5eb; padding: 15px; margin: 15px 0; border-radius: 5px; }}
                            .steps {{ margin: 10px 0; }}
                            .steps ol {{ padding-left: 20px; }}
                            .footer {{ background-color: #f8f9fa; padding: 15px; margin-top: 20px; font-size: 12px; color: #666; }}
                            .important {{ color: #dc3545; font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class=""header"">
                            <h2>Form Resubmission Required</h2>
                            <p><strong>SACCO:</strong> {returnDetails.SaccoName}</p>
                            <p><strong>Return Period:</strong> {returnDetails.Period} {returnDetails.Year}</p>
                            <p><strong>Form:</strong> {formName}</p>
                        </div>
    
                        <div class=""content"">
                            <p>Dear {returnDetails.SaccoName} Team,</p>
        
                            <p>Our compliance review has identified issues with your <strong>{formName}</strong> submission for the period <strong>{returnDetails.Period} {returnDetails.Year}</strong>.</p>
        
                            <div class=""reason-box"">
                                <h3>📋 Reason for Resubmission:</h3>
                                <p>{reason}</p>
                            </div>
        
                            <div class=""action-box"">
                                <h3>✅ Action Required:</h3>
                                <p>Please resubmit <strong>ONLY the {formName}</strong> with the necessary corrections. You do not need to resubmit other forms.</p>
                            </div>
        
                            
               
                            <p>If you have any questions or need clarification, please contact our compliance team.</p>
        
                            <p>Best regards,<br>
                            <strong>Compliance Team</strong><br>
                            Returns Management System</p>
                        </div>
    
                        <div class=""footer"">
                            <p>This is an automated message. Please do not reply to this email.</p>
                            <p>Return ID: {returnDetails.Id}</p>
                            <p>Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                    </body>
                    </html>";


                if (!string.IsNullOrEmpty(SaccoEmail))
                {
                    // Send the email
                    await _emailService.SendEmailAsync(SaccoEmail, subject, body);

                    _logger.LogInformation($"Resubmission notification sent to {returnDetails.SaccoName} at {SaccoEmail}");
                }
             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send resubmission notification to {returnDetails.SaccoName}");
                // Don't throw - this shouldn't break the resubmission request creation
            }
        }

        private async Task<(bool Success, string Message)> UpdateRequiresResubmissionFlagAsync(string childFormId,ReturnForm form,bool requiresResubmission,string reason, string saccoType)
        {
            try
            {
                // Use your existing GetFormTypeFromForm method
                string formType = GetFormTypeFromForm(form);
                bool isDepositTaking = saccoType == Constants.SaccoType.DepositTaking.ToString();

                // Determine form type and update the appropriate table
                var result = (formType, isDepositTaking) switch
                {
                    ("CapitalAdequacy", true) => await UpdateFormFlag<DTCapitalAdequacyReturn>(childFormId, requiresResubmission),
                    ("Liquidity", true) => await UpdateFormFlag<DTLiquidityReturn>(childFormId, requiresResubmission),
                    ("RiskClassification", true) => await UpdateFormFlag<DTRiskClassificationReturn>(childFormId, requiresResubmission),
                    ("Investment", true) => await UpdateFormFlag<DTInvestmentReturn>(childFormId, requiresResubmission),
                    ("FinancialPosition", true) => await UpdateFormFlag<DTFinancialPositionReturn>(childFormId, requiresResubmission),
                    ("ComprehensiveIncome", true) => await UpdateFormFlag<DTComprehensiveIncomeReturn>(childFormId, requiresResubmission),
                    ("DepositReturn", true) => await UpdateFormFlag<DepositReturn>(childFormId, requiresResubmission),

                    // NWDT Forms
                    ("CapitalAdequacy", false) => await UpdateFormFlag<NWDTCapitalAdequacyReturn>(childFormId, requiresResubmission),
                    ("Liquidity", false) => await UpdateFormFlag<NWDTLiquidityReturn>(childFormId, requiresResubmission),
                    ("Investment", false) => await UpdateFormFlag<NWDTInvestmentReturn>(childFormId, requiresResubmission),
                    ("FinancialPosition", false) => await UpdateFormFlag<NWDTFinancialPositionReturn>(childFormId, requiresResubmission),
                    ("ComprehensiveIncome", false) => await UpdateFormFlag<NWDTComprehensiveIncomeReturn>(childFormId, requiresResubmission),
                    ("RiskClassification", false) => await UpdateFormFlag<NWDTRiskClassificationReturn>(childFormId, requiresResubmission),
                    ("DepositReturn", false) => await UpdateFormFlag<NWDTDepositReturn>(childFormId, requiresResubmission),

                    // Handle forms that don't support RequiresResubmission
                    ("SectoralLending", _) => (true, "SectoralLending forms don't support RequiresResubmission flag"),
                    ("DailyLiquidity", _) => (true, "DailyLiquidity forms don't support RequiresResubmission flag"),
                    ("InsiderLending", _) => (true, "InsiderLending forms don't support RequiresResubmission flag"),
                    ("Management", _) => (true, "Management forms don't support RequiresResubmission flag"),
                    ("Other", _) => (true, "Other forms don't support RequiresResubmission flag"),

                    _ => (false, $"Unknown form type: {formType}")
                };

                if (result.Item1)
                {
                    var action = requiresResubmission ? "marked for resubmission" : "resubmission requirement cleared";
                    _logger.LogInformation($"{form.FormName} ({childFormId}) {action}. Reason: {reason}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating RequiresResubmission flag for {form.FormName} ({childFormId})");
                return (false, $"Error updating flag: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message, string NewFormId)> HandleSaccoFormResubmissionAsync(string returnId,ReturnForm form,IFormFile formFile,string saccoUserId, string SaccoType, string resubmissionNotes = "")
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pendingRequest = await _context.FormResubmissionRequests.FirstOrDefaultAsync(r => r.ReturnId == returnId && r.FormId == form.Id &&r.Status == "Pending");

                if (pendingRequest == null)
                {
                    return (false, "No pending resubmission request found for this form", string.Empty);
                }

                // Process the form based on type using your existing logic
                var (isProcessed, message) = await _formProcessingService.ProcessFormByType(formFile, form, returnId, SaccoType, true, pendingRequest.ChildId);

                if (!isProcessed)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Failed to process form: {message}", string.Empty);
                }

                // Clear RequiresResubmission flag on the old form
                var clearFlagResult = await UpdateRequiresResubmissionFlagAsync(pendingRequest.ChildId, form, false, "Resubmitted by SACCO", SaccoType);

                if (!clearFlagResult.Success)
                {
                    _logger.LogWarning($"Failed to clear RequiresResubmission flag: {clearFlagResult.Message}");
                    // Continue - not critical
                }

                // Update the resubmission request
                pendingRequest.Status = "Completed";
                pendingRequest.UploadedAt = DateTime.Now;
                pendingRequest.RespondedBy = saccoUserId;
                pendingRequest.ResubmissionNotes = resubmissionNotes;
                //pendingRequest.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send confirmation email to compliance officer
                await SendResubmissionCompletedNotification(pendingRequest, form.FormName);

                _logger.LogInformation($"SACCO resubmission completed for {form.FormName}. Request ID: {pendingRequest.Id}");

                return (true, $"{form.FormName} resubmitted successfully", string.Empty);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing SACCO form resubmission");
                return (false, $"Error: {ex.Message}", string.Empty);
            }
        }

        private async Task SendResubmissionCompletedNotification(FormResubmissionRequest request, string formName)
        {
            try
            {
                var subject = $"Form Resubmission Completed - {formName}";
                var body = $@"
                    Dear {request.RequestedByName},

                    The SACCO {request.SaccoId} has successfully resubmitted the {formName} that you requested for correction.

                    **Resubmission Details:**
                    - Form: {formName}
                    - Return ID: {request.ReturnId}
                    - Resubmitted: {DateTime.Now:dd/MM/yyyy HH:mm}
                    - SACCO Notes: {request.ResubmissionNotes ?? "No additional notes provided"}
                    - Original Request Reason: {request.Reason}

                    The resubmitted form is now available for your review in the returns management system.

                    Best regards,
                    Returns Management System
                            ";

                await _emailService.SendEmailAsync(request.RequestedByEmail, subject, body);
                _logger.LogInformation($"Resubmission completion notification sent to {request.RequestedByEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send resubmission completion notification");
            }
        }


        private async Task<(bool Success, string Message)> UpdateFormFlag<T>(string childFormId,bool requiresResubmission) where T : class
        {
            try
            {
                var form = await _context.Set<T>().FindAsync(childFormId);
                if (form == null)
                {
                    return (false, $"{typeof(T).Name} form not found");
                }

                var property = typeof(T).GetProperty("RequiresResubmission");
                if (property == null)
                {
                    return (false, $"{typeof(T).Name} does not have RequiresResubmission property");
                }

                property.SetValue(form, requiresResubmission);
                return (true, "Flag updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating {typeof(T).Name}: {ex.Message}");
            }
        }



        private async Task<ReturnForm?> GetFormTypeFromFormId(string formId)
        {
            var form = await _context.ReturnForms.FindAsync(formId);
            if (form == null)
            {
                _logger.LogWarning("Form with ID {FormId} not found", formId);
                return null;
            }
            return form;
        }

        public async Task<(Return? Return, string? ChildFormId)> GetReturnWithCurrentChildFormAsync(string returnId, string formId, string saccoType)
        {
            // Get the Return first
            var returnDetails = await _context.Returns.FindAsync(returnId);
            if (returnDetails == null)
            {
                return (null, null);
            }

            // Get form details to determine form type
            var formDetails = await _context.ReturnForms.FindAsync(formId);
            if (formDetails == null)
            {
                return (returnDetails, null);
            }

            bool isDepositTaking = saccoType == Constants.SaccoType.DepositTaking.ToString();
            string formType = GetFormTypeFromForm(formDetails);

            string? childFormId = null;

            if (isDepositTaking)
            {
                childFormId = formType switch
                {
                    "CapitalAdequacy" => await _context.DTCapitalAdequacyReturns
                        .Where(c => c.ReturnId == returnId )
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync(),

                    "Liquidity" => await _context.DTLiquidityReturns
                        .Where(l => l.ReturnId == returnId)
                        .Select(l => l.Id)
                        .FirstOrDefaultAsync(),

                    "DepositReturn" => await _context.DepositReturns
                        .Where(d => d.ReturnId == returnId)
                        .Select(d => d.Id)
                        .FirstOrDefaultAsync(),

                    "RiskClassification" => await _context.DTRiskClassificationReturns
                        .Where(r => r.ReturnId == returnId)
                        .Select(r => r.Id)
                        .FirstOrDefaultAsync(),

                    "Investment" => await _context.DTInvestmentReturns
                        .Where(i => i.ReturnId == returnId)
                        .Select(i => i.Id)
                        .FirstOrDefaultAsync(),

                    "FinancialPosition" => await _context.DTFinancialPositionReturns
                        .Where(f => f.ReturnId == returnId)
                        .Select(f => f.Id)
                        .FirstOrDefaultAsync(),

                    "ComprehensiveIncome" => await _context.DTComprehensiveIncomeReturns
                        .Where(c => c.ReturnId == returnId )
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync(),

                    _ => null
                };
            }
            else // NWDT
            {
                childFormId = formType switch
                {
                    "CapitalAdequacy" => await _context.NWDTCapitalAdequacyReturns
                        .Where(c => c.ReturnId == returnId )
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync(),

                    "Liquidity" => await _context.NDWTLiquidityReturns
                        .Where(l => l.ReturnId == returnId)
                        .Select(l => l.Id)
                        .FirstOrDefaultAsync(),

                    "DepositReturn" => await _context.NWDTDepositReturns
                        .Where(d => d.ReturnId == returnId)
                        .Select(d => d.Id)
                        .FirstOrDefaultAsync(),

                    "Investment" => await _context.NWDTInvestmentReturns
                        .Where(i => i.ReturnId == returnId)
                        .Select(i => i.Id)
                        .FirstOrDefaultAsync(),

                    "FinancialPosition" => await _context.NWDTFinancialPositionReturns
                        .Where(f => f.ReturnId == returnId )
                        .Select(f => f.Id)
                        .FirstOrDefaultAsync(),

                    "ComprehensiveIncome" => await _context.NWDTComprehensiveIncomeReturns
                        .Where(c => c.ReturnId == returnId )
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync(),

                    "RiskClassification" => await _context.NWDTRiskClassificationReturns
                        .Where(r => r.ReturnId == returnId)
                        .Select(r => r.Id)
                        .FirstOrDefaultAsync(),

                    _ => null
                };
            }

            return (returnDetails, childFormId);
        }


        public async Task<(bool Success, string Message)> UpdateRequiresResubmissionAsync<T>(string childFormId,bool requiresResubmission,string reason = null) where T : class
        {
            try
            {
                var form = await _context.Set<T>().FindAsync(childFormId);
                if (form == null)
                {
                    return (false, "Form not found");
                }

                // Use reflection to set RequiresResubmission property
                var property = typeof(T).GetProperty("RequiresResubmission");
                if (property == null)
                {
                    return (false, $"Form type {typeof(T).Name} does not have RequiresResubmission property");
                }

                property.SetValue(form, requiresResubmission);
                await _context.SaveChangesAsync();

                var action = requiresResubmission ? "marked for resubmission" : "resubmission requirement cleared";
                var logMessage = $"Form {typeof(T).Name} ({childFormId}) {action}";
                if (!string.IsNullOrEmpty(reason))
                {
                    logMessage += $". Reason: {reason}";
                }

                _logger.LogInformation(logMessage);

                return (true, $"Form {action} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating RequiresResubmission flag for form {childFormId}");
                return (false, $"Error: {ex.Message}");
            }
        }



        private string GetFormTypeFromForm(ReturnForm form)
        {
            if (form.IsStatementOfComprehensiveIncome) return "ComprehensiveIncome";
            if (form.IsCapitalAdequencyForm) return "CapitalAdequacy";
            if (form.IsLiquidityStatement) return "Liquidity";
            if (form.IsDepositReturnForm) return "DepositReturn";
            if (form.IsRiskClassification) return "RiskClassification";
            if (form.IsInvestmentReturn) return "Investment";
            if (form.IsFinancialPosition) return "FinancialPosition";
            if (form.IsSectoralLending) return "SectoralLending";
            if (form.IsDailyLiquidity) return "DailyLiquidity";
            if (form.IsInsiderLending) return "InsiderLending";
            if (form.IsManagement) return "Management";
            if (form.IsOtherForm) return "Other";
            return null;
        }

    }
}
*/