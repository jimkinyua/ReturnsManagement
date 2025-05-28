using Microsoft.AspNetCore.Http;
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
        public FormResubmissionService(ReturnsDbContext context, IEmailService emailService, ILogger logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> RequestFormResubmissionAsync(string ReturnId, ReturnForm form, string complianceOfficerId, string complianceOfficerName, string complianceOfficerEmail, string reason, string SaccoId, string saccoType, string SaccoEmail)
        {
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

                var existingRequest = await _context.FormResubmissionRequests.FirstOrDefaultAsync(r => r.ChildId == childFormId && r.Status == "Pending");

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
                await _context.SaveChangesAsync();

                await SendResubmissionNotificationEmail(returnDetails, form.FormName, SaccoEmail, reason);

                _logger.LogInformation($"Form resubmission requested for {form.FormName} by {complianceOfficerName}. ChildId: {childFormId}");

                return (true, "Resubmission request sent successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting form resubmission for ChildFormId: {ChildFormId}", ReturnId);
                return (false, ex.Message);
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
