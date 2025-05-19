using Returns.DTOs.Compliance;

namespace Returns.Helpers.Interfaces
{
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
    }
}
