using Returns.DTOs.Returns.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.NWDT;
using Returns.DTOs.Returns_Submission.Returns_Submission.DT;

namespace Returns.Helpers.Interfaces
{
    public interface IReturnChild
    {
        Task<ManagementReturnDTO?> GetManagementReturnDT(string childId);
        Task<CapitalAdequacyDTO?> GetCapitaAdequacyDT(string childId);
        Task<DepositReturnDto?> GetDepositReturnDT(string childId);
        Task<ComprehesiveIncomeStatementDTO?> GetComprehensiveIncomeDT(string childId);
        Task<FinancialPositionDTO?> GetFinancialPositionDT(string childId);
        Task<LiquidityStatementDTO?> GetLiquidityStatementDT(string childId);
        Task<RiskClassificationDTO?> GetRiskClassificationDT(string childId);
        Task<List<OtherReturnDTO>> GetOtherReturnsDT(string childId);
        Task<InvestmentReturnDTO?> GetInvestmentReturnDT(string childId);

        Task<object?> GetChildDetailsByFormType(string childId, string formType, string saccoType);

        // NWDT
        Task<NWDTCapitalAdequacyDTO?> GetNWDTCapitalAdequacyDT(string childId);
        Task<NWDTDepositReturnDto?> GetNWDTDepositReturnDT(string childId);
        Task<NWDTComprehesiveIncomeStatementDTO?> GetNWDTComprehensiveIncomeDT(string childId);
        Task<NWDTFinancialPositionDTO?> GetNWDTFinancialPositionDT(string childId);
        Task<NWDTLiquidityStatementDTO?> GetNWDTLiquidityStatementDT(string childId);
        Task<NWDTRiskClassificationDTO?> GetNWDTRiskClassificationDT(string childId);
        Task<List<OtherReturnDTO>> GetNWDTOtherReturnsDT(string childId);
        Task<NWDTInvestmentReturnDTO?> GetNWDTInvestmentReturnDT(string childId);
        Task<ManagementReturnDTO?> GetNWDTManagementReturnDT(string childId);

    }
}
