using Hangfire;
using Microsoft.EntityFrameworkCore;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Text;

namespace Returns.Helpers
{
    public class SaccoAssignmentService : ISaccoAssignmentService
    {
        private readonly ReturnsDbContext _context;
        private readonly IComplianceService _complianceService;
        private readonly IEmailService _emailService;
        private readonly ILogger<SaccoAssignmentService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public SaccoAssignmentService(
            ReturnsDbContext context,
            IComplianceService complianceService,
            IEmailService emailService,
            ILogger<SaccoAssignmentService> logger,
            IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _complianceService = complianceService;
            _emailService = emailService;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task AssignSaccoAsync(string saccoId, string memberUserId, string tlUserId)
        {
            // Validate TL using UserId
            var tlDetails = await _complianceService.GetUserDetailsAsync(tlUserId);
            if (tlDetails == null || !tlDetails.IsTeamLead)
            {
                _logger.LogWarning("User {UserId} is not a Team Lead.", tlUserId);
                throw new UnauthorizedAccessException("Only Team Leads can assign SACCOs.");
            }

            var teamId = tlDetails.TeamId;
            if (string.IsNullOrEmpty(teamId))
            {
                _logger.LogWarning("Team Lead {UserId} has no TeamId.", tlUserId);
                throw new InvalidOperationException("Team Lead has no assigned team.");
            }

            // Validate SACCO is in team
            var teamSaccos = await _complianceService.GetSaccosForTeamAsync(teamId);
            if (!teamSaccos.Any(s => s.SaccoId == saccoId))
            {
                _logger.LogWarning("SACCO {SaccoId} not in team {TeamId}.", saccoId, teamId);
                throw new InvalidOperationException("SACCO is not assigned to this team.");
            }

            // Validate member is in team
            var teamMembers = await _complianceService.GetTeamMembersAsync(teamId);
            if (!teamMembers.Any(m => m.UserId == memberUserId))
            {
                _logger.LogWarning("User {MemberUserId} not in team {TeamId}.", memberUserId, teamId);
                throw new InvalidOperationException("Assigned user is not a team member.");
            }

            var existing = await _context.SaccoAssignments.FirstOrDefaultAsync(a => a.SaccoId == saccoId);
            if (existing != null)
            {
                existing.AssignedUserId = memberUserId;
                existing.AssignedByUserId = tlUserId;
                existing.AssignedAt = DateTime.UtcNow;
            }
            else
            {
                var newAssignment = new SaccoAssignment
                {
                    SaccoId = saccoId,
                    AssignedUserId = memberUserId,
                    AssignedByUserId = tlUserId
                };
                await _context.SaccoAssignments.AddAsync(newAssignment);
            }

            await _context.SaveChangesAsync();

            _backgroundJobClient.Enqueue(() => SendAssignmentNotificationAsync(saccoId, memberUserId, tlUserId));
        }

        public async Task AssignSaccoToMemberAsync(string saccoId, string memberId, string teamLeadId)
        {
            // Validate TL
            var teamId = await _complianceService.GetTeamIdForSaccoAsync(saccoId);
            if (teamId == null)
            {
                _logger.LogWarning("No team found for SACCO {SaccoId}.", saccoId);
                throw new InvalidOperationException("SACCO is not assigned to any team.");
            }

            var teamLead = await _complianceService.GetTeamLeaderAsync(teamId);
            if (teamLead == null || teamLead.UserId != teamLeadId)
            {
                _logger.LogWarning("User {TeamLeadId} is not the team lead for team {TeamId}.", teamLeadId, teamId);
                throw new UnauthorizedAccessException("Only the team lead can assign SACCOs.");
            }

            // Validate member is in team
            var teamMembers = await _complianceService.GetTeamMembers(teamId);
            var member = teamMembers.FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                _logger.LogWarning("User {MemberId} is not in team {TeamId}.", memberId, teamId);
                throw new InvalidOperationException("Assigned member is not part of the team.");
            }

            // Upsert assignment
            var existingAssignment = await _context.SaccoAssignments
                .FirstOrDefaultAsync(a => a.SaccoId == saccoId);

            if (existingAssignment != null)
            {
                existingAssignment.AssignedUserId = memberId;
                existingAssignment.AssignedByUserId = teamLeadId;
                existingAssignment.AssignedAt = DateTime.UtcNow;
                _context.SaccoAssignments.Update(existingAssignment);
            }
            else
            {
                var newAssignment = new SaccoAssignment
                {
                    SaccoId = saccoId,
                    AssignedUserId = memberId,
                    AssignedByUserId = teamLeadId,
                    AssignedAt = DateTime.UtcNow
                };
                await _context.SaccoAssignments.AddAsync(newAssignment);
            }

            await _context.SaveChangesAsync();

            // Enqueue email notification
            _backgroundJobClient.Enqueue(() => SendAssignmentNotificationAsync(saccoId, memberId, teamLeadId));
        }

        public async Task<List<SaccoDTO>> GetAssignableSaccosAsync(string teamLeadId)
        {
            var teamLead = await _complianceService.GetTeamLeaderAsync(null); // Fetch TL's team
            if (teamLead == null || teamLead.UserId != teamLeadId)
            {
                _logger.LogWarning("User {TeamLeadId} is not a team lead.", teamLeadId);
                throw new UnauthorizedAccessException("Only team leads can view assignable SACCOs.");
            }

            var teamId = teamLead.TeamId;
            var saccoIds = await _complianceService.GetSaccosForTheTeamAsync(teamId);
            var saccos = new List<SaccoDTO>();

            foreach (var saccoId in saccoIds)
            {
                var sacco = await _complianceService.GetSaccoByTheirIdAsync(saccoId.SaccoId);
                if (sacco != null)
                {
                    saccos.Add(sacco);
                }
            }

            return saccos;
        }

        public async Task<List<TeamMemberDTO>> GetTeamMembersAsync(string teamLeadId)
        {
            var teamLead = await _complianceService.GetTeamLeaderAsync(teamLeadId); // Fetch TL's team
            if (teamLead == null || teamLead.UserId != teamLeadId)
            {
                _logger.LogWarning("User {TeamLeadId} is not a team lead.", teamLeadId);
                throw new UnauthorizedAccessException("Only team leads can view team members.");
            }

            return await _complianceService.GetTeamMembersAsync(teamLead.TeamId);
        }

        public async Task UnassignSaccoAsync(string saccoId, string teamLeadId)
        {
            var teamId = await _complianceService.GetTeamIdForSaccoAsync(saccoId);
            if (teamId == null)
            {
                _logger.LogWarning("No team found for SACCO {SaccoId}.", saccoId);
                throw new InvalidOperationException("SACCO is not assigned to any team.");
            }

            var teamLead = await _complianceService.GetTeamLeaderAsync(teamId);
            if (teamLead == null || teamLead.UserId != teamLeadId)
            {
                _logger.LogWarning("User {TeamLeadId} is not the team lead for team {TeamId}.", teamLeadId, teamId);
                throw new UnauthorizedAccessException("Only the team lead can unassign SACCOs.");
            }

            var assignment = await _context.SaccoAssignments
                .FirstOrDefaultAsync(a => a.SaccoId == saccoId);

            if (assignment == null)
            {
                _logger.LogWarning("No assignment found for SACCO {SaccoId}.", saccoId);
                throw new InvalidOperationException("SACCO is not assigned.");
            }

            _context.SaccoAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            // Notify member of unassignment
            _backgroundJobClient.Enqueue(() => SendUnassignmentNotificationAsync(saccoId, assignment.AssignedUserId, teamLeadId));
        }

        public async Task SendAssignmentNotificationAsync(string saccoId, string memberId, string teamLeadId)
        {
            try
            {
                var sacco = await _complianceService.GetSaccoByIdAsync(saccoId);
                var member = await _complianceService.GetUserById(memberId);
                var teamLead = await _complianceService.GetUserById(teamLeadId);

                if (sacco == null || member == null || string.IsNullOrEmpty(member.Email))
                {
                    _logger.LogWarning("Failed to send assignment notification: Sacco {SaccoId} or Member {MemberId} not found.", saccoId, memberId);
                    return;
                }

                var subject = "SACCO Assignment Notification";
                var body = new StringBuilder();
                body.AppendLine($"Dear {member.FullName},");
                body.AppendLine();
                body.AppendLine($"You have been assigned to handle returns for SACCO {sacco.SaccoName} by Team Lead {teamLead?.FullName ?? "Unknown"}.");
                body.AppendLine("Please log in to the system to review any pending returns.");
                body.AppendLine();
                body.AppendLine("Best regards,");
                body.AppendLine("SASRA Team");

                await _emailService.SendEmailAsync(member.Email, subject, body.ToString());
                _logger.LogInformation("Assignment notification sent to {MemberId} for SACCO {SaccoId}.", memberId, saccoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending assignment notification for Sacco {SaccoId} to Member {MemberId}.", saccoId, memberId);
            }
        }

        public async Task SendUnassignmentNotificationAsync(string saccoId, string memberId, string teamLeadId)
        {
            try
            {
                var sacco = await _complianceService.GetSaccoByIdAsync(saccoId);
                var member = await _complianceService.GetUserById(memberId);
                var teamLead = await _complianceService.GetUserById(teamLeadId);

                if (sacco == null || member == null || string.IsNullOrEmpty(member.Email))
                {
                    _logger.LogWarning("Failed to send unassignment notification: Sacco {SaccoId} or Member {MemberId} not found.", saccoId, memberId);
                    return;
                }

                var subject = "SACCO Unassignment Notification";
                var body = new StringBuilder();
                body.AppendLine($"Dear {member.FullName},");
                body.AppendLine();
                body.AppendLine($"You have been unassigned from handling returns for SACCO {sacco.SaccoName} by Team Lead {teamLead?.FullName ?? "Unknown"}.");
                body.AppendLine("No further action is required for this SACCO.");
                body.AppendLine();
                body.AppendLine("Best regards,");
                body.AppendLine("SASRA Team");

                await _emailService.SendEmailAsync(member.Email, subject, body.ToString());
                _logger.LogInformation("Unassignment notification sent to {MemberId} for SACCO {SaccoId}.", memberId, saccoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending unassignment notification for Sacco {SaccoId} to Member {MemberId}.", saccoId, memberId);
            }
        }

        Task<List<TeamMemberDTO>> ISaccoAssignmentService.GetTeamMembersAsync(string teamLeadId)
        {
            throw new NotImplementedException();
        }
    }
}
