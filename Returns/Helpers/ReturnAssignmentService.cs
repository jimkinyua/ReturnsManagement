using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Compliance;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System;
using System.Threading.Tasks;

namespace Returns.Helpers
{
    public class ReturnAssignmentService : IReturnAssignmentService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IComplianceService _complianceService;
        private readonly ILogger<ReturnAssignmentService> _logger;

        public ReturnAssignmentService(ReturnsDbContext context,
                                       IEmailService emailService,
                                       IComplianceService complianceService,
                                       ILogger<ReturnAssignmentService> logger)
        {
            _context = context;
            _emailService = emailService;
            _complianceService = complianceService;
            _logger = logger;
        }

        public async Task<OperationResult> AssignReturnAsync(Return returnEntity, string saccoId)
        {
            try
            {
                ComplianceOfficerInfo assignedUser = await _complianceService.GetAssignedComplianceOfficer(saccoId);
                if (assignedUser == null)
                {
                    string error = $"No compliance officer found for SACCO ID: {saccoId}. Return will not be assigned.";
                    _logger.LogWarning(error);
                    return OperationResult.Fail(error);
                }

                var assignment = new ReturnsAssigment
                {
                    ReturnId = returnEntity.Id,
                    UserId = assignedUser.Id,
                };

                _context.ReturnsAssigments.Add(assignment);
                await _context.SaveChangesAsync();

                await _emailService.SendEmailAsync(
                    to: assignedUser.Email,
                    subject: "New Return Assignment",
                    body: "A new return has been submitted and assigned to you for processing. Please log in to RBSS for further details."
                );

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while assigning return ID {returnEntity.Id}: {ex.Message}";
                _logger.LogError(ex, error);
                return OperationResult.Fail(error);
            }
        }

        public async Task<OperationResult> ReassignReturnAsync(string returnId, string newUserId)
        {
            try
            {
                var assignment = await _context.ReturnsAssigments.FirstOrDefaultAsync(a => a.ReturnId == returnId);
                if (assignment == null)
                {
                    assignment = new ReturnsAssigment
                    {
                        ReturnId = returnId,
                        UserId = newUserId,
                    };
                    _context.ReturnsAssigments.Add(assignment);
                }
                else
                {
                    assignment.UserId = newUserId;
                    _context.ReturnsAssigments.Update(assignment);
                }

                await _context.SaveChangesAsync();

                await _emailService.SendEmailAsync(
                    to: newUserId,
                    subject: "Return Reassignment",
                    body: "A return has been reassigned to you for processing. Please log in to RBSS to review the details."
                );

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while reassigning return ID {returnId}: {ex.Message}";
                _logger.LogError(ex, error);
                return OperationResult.Fail(error);
            }
        }

        public async Task<OperationResult> CheckSaccoAssignedUserAsync(string saccoId)
        {
            try
            {
                ComplianceOfficerInfo assignedUser = await _complianceService.GetAssignedComplianceOfficer(saccoId);
                if (assignedUser == null)
                {
                    string error = $"No compliance officer assigned for this SACCO Contact the System Admin";
                    _logger.LogWarning(error);
                    return OperationResult.Fail(error);
                }
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while checking assigned user for SACCO ID {saccoId}: {ex.Message}";
                _logger.LogError(ex, error);
                return OperationResult.Fail(error);
            }
        }
    }
}
