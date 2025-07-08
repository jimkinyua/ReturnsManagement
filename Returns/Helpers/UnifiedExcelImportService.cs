using ClosedXML.Excel;
using Returns.DTOs.Forms;
using Returns.Helpers.Interfaces;
using Returns.Models;
using System.Reflection;
using static Returns.Helpers.ExcelService;

namespace Returns.Helpers
{
    public class UnifiedExcelImportService : IExcelImportService
    {
        private readonly ILogger<UnifiedExcelImportService> _logger;
        private readonly Dictionary<string, Func<IFormFile, ILogger, object>> _importStrategies;

        public UnifiedExcelImportService(ILogger<UnifiedExcelImportService> logger)
        {
            _logger = logger;
            _importStrategies = InitializeImportStrategies();
        }

        private Dictionary<string, Func<IFormFile, ILogger, object>> InitializeImportStrategies()
        {
            return new Dictionary<string, Func<IFormFile, ILogger, object>>
            {
                ["CapitalAdequacy_DT"] = (file, logger) => ExcelService.ImportCapitalAdequacyRows(file, logger),
                ["CapitalAdequacy_NWDT"] = (file, logger) => ExcelService.ImportForm2ARows(file, logger),
                ["Liquidity_DT"] = (file, logger) => ExcelService.ImportLiquidityStatementRows(file, logger),
                ["Liquidity_NWDT"] = (file, logger) => ExcelService.ImportForm2BStatement(file, logger),
                ["DepositReturn_DT"] = (file, logger) => ExcelService.ImportDepositRangeDataRows(file, logger),
                ["DepositReturn_NWDT"] = (file, logger) => ExcelService.ImportForm2CDataRows(file, logger),
                ["RiskClassification_DT"] = (file, logger) => ExcelService.ImportRiskClassificationRows(file, logger),
                ["RiskClassification_NWDT"] = (file, logger) => ExcelService.ImportForm2DRows(file, logger),
                ["Investment_DT"] = (file, logger) => ExcelService.ImportInvestmentRows(file, logger),
                ["Investment_NWDT"] = (file, logger) => ExcelService.ImportForm2ERows(file, logger),
                ["FinancialPosition_DT"] = (file, logger) => ExcelService.ImportFinancialPositionRows(file, logger),
                ["FinancialPosition_NWDT"] = (file, logger) => ExcelService.ImportForm2GRows(file, logger),
                ["ComprehensiveIncome_DT"] = (file, logger) => ExcelService.ImportStatementOfComprehensiveIncomeRows(file, logger),
                ["ComprehensiveIncome_NWDT"] = (file, logger) => ExcelService.ImportForm2FRows(file, logger),
                ["Management"] = (file, logger) => ExcelService.ImportManagementRows(file, logger),
                ["DailyLiquidity"] = (file, logger) => ExcelService.ImportDailyLiquidityRows(file, logger),
                ["SectoralLending"] = (file, logger) => ExcelService.ImportSectoralLendingReport(file, logger, ""),
                ["InsiderLending"] = (file, logger) => ExcelService.ImportInsiderLendingReport(file, logger),
            };
        }

        public async Task<T> ImportFormDataAsync<T>(IFormFile file, ReturnForm formMetadata) where T : class, new()
        {
            var result = await ImportFormDataAsync(file, formMetadata);
            return result as T ?? throw new InvalidCastException($"Cannot cast result to {typeof(T).Name}");
        }

        public async Task<object> ImportFormDataAsync(IFormFile file, ReturnForm formMetadata)
        {
            try
            {
                var formType = DetermineFormType(formMetadata);
                var saccoType = await GetSaccoTypeFromContext(); // You'll need to implement this based on your context
                
                var key = BuildStrategyKey(formType, saccoType, formMetadata);
                
                if (!_importStrategies.TryGetValue(key, out var importStrategy))
                {
                    throw new NotSupportedException($"No import strategy found for form type: {key}");
                }

                return await Task.Run(() => importStrategy(file, _logger));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing form data for {FormName}", formMetadata.FormName);
                throw;
            }
        }

        private string DetermineFormType(ReturnForm form)
        {
            if (form.IsCapitalAdequencyForm) return "CapitalAdequacy";
            if (form.IsLiquidityStatement) return "Liquidity";
            if (form.IsDepositReturnForm) return "DepositReturn";
            if (form.IsRiskClassification) return "RiskClassification";
            if (form.IsInvestmentReturn) return "Investment";
            if (form.IsFinancialPosition) return "FinancialPosition";
            if (form.IsStatementOfComprehensiveIncome) return "ComprehensiveIncome";
            if (form.IsManagement) return "Management";
            if (form.IsDailyLiquidity) return "DailyLiquidity";
            if (form.IsSectoralLending) return "SectoralLending";
            if (form.IsInsiderLending) return "InsiderLending";
            
            throw new ArgumentException($"Unknown form type for form: {form.FormName}");
        }

        private string BuildStrategyKey(string formType, string saccoType, ReturnForm form)
        {
            // Some forms are common across SACCO types
            var commonForms = new[] { "Management", "DailyLiquidity", "SectoralLending", "InsiderLending" };
            
            if (commonForms.Contains(formType))
            {
                return formType;
            }

            return $"{formType}_{saccoType}";
        }

        private async Task<string> GetSaccoTypeFromContext()
        {
            // This should be implemented based on your authentication/context
            // For now, returning a placeholder
            return await Task.FromResult("DT");
        }
    }
}