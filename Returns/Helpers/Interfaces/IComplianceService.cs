using Returns.DTOs.Compliance;

namespace Returns.Helpers.Interfaces
{

    public class UserDTO
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsTeamLead { get; set; }
        public string? TeamId { get; set; }
    }

    public class SaccoAssignmentDTO
    {
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public UserDetailsDTO? AssignedUser { get; set; } 
        public DateTime? AssignedAt { get; set; }
    }

    public class TeamLeadDTO : UserDTO { }  // Inherits from UserDTO

    public class TeamMemberDTO : UserDTO { }  // Inherits from UserDTO

    public class SaccoDTO
    {
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string OfficialEmail { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
    }

    public class RoleDTO
    {
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string Description { get; set; } = null!;
    }


    public interface IComplianceService
    {
        Task<ComplianceOfficerInfo> GetAssignedComplianceOfficer(string saccoId);
        Task<SasraUser?> GetTeamLead(string teamId);
        Task<List<SasraUser>?> GetTeamMembers(string teamId);
        Task<SasraUser?> GetUserByRole(string RoleId);
        Task<SasraUser?> GetUserById(string UserId);
        Task<SasraRoleDetails?> GetRoleDetails(string RoleId);
        Task<List<Sacco>> GetAllSaccosAsync();
        Task<Sacco> GetSaccoByIdAsync(string saccoId);
        Task<List<Sacco>> GetSaccosAssignedToOfficerAsync(string userId);
        Task<string?> GetTeamIdForSaccoAsync(string saccoId);
        Task <UserDTO>GetUserDetailsAsync(string tlUserId);
        Task<List<SaccoDTO>> GetSaccosForTeamAsync(string teamId);  
        Task<List<TeamMemberDTO>> GetTeamMembersAsync(string teamId);  
        Task<string?> GetTeamIdForUserAsync(string userId);
        Task<TeamLeadDTO> GetTeamLeaderAsync(string teamId);
        Task<List<SaccoDTO>> GetSaccosForTheTeamAsync(string teamId);
        Task<SaccoDTO> GetSaccoByTheirIdAsync(string saccoId);



    }
}
