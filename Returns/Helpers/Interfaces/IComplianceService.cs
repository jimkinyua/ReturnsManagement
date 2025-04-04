using Returns.DTOs.Compliance;

namespace Returns.Helpers.Interfaces
{
    public interface IComplianceService
    {
        Task<ComplianceOfficerInfo> GetAssignedComplianceOfficer(string saccoId);

    }
}
