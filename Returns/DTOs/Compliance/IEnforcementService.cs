using Returns.DTOs.Enforcement;

namespace Returns.DTOs.Compliance
{
    public interface IEnforcementService
    {
        Task SubmitCaseAsync(EnforcementCaseRequestDTO dto,string bearerToken,CancellationToken ct = default);
    }
}
