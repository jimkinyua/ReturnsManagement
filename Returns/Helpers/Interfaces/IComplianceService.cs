using Returns.DTOs.Compliance;

namespace Returns.Helpers.Interfaces
{
    public interface IComplianceService
    {
        Task<ComplianceOfficerInfo> GetAssignedComplianceOfficer(string saccoId);
        Task<SasraUser?> GetTeamLead(string teamId);
        Task<SasraUser?> GetUserByRole(string RoleId);
        Task<SasraRoleDetails?> GetRoleDetails(string RoleId);

    }
}
