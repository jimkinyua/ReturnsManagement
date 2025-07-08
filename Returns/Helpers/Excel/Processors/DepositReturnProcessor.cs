using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.DTOs.Returns.Returns_Submission.DT;
using Returns.Helpers.Excel.Configurations;
using Returns.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers.Excel.Processors
{
    public class DepositReturnProcessor : BaseFormProcessor<DepositReturnConfiguration, DepositReturnDto>
    {
        public override string FormType => "DEPOSIT_RETURN";

        public DepositReturnProcessor(
            ReturnsDbContext context,
            ILogger<DepositReturnProcessor> logger,
            IExcelImportService excelService) 
            : base(context, logger, excelService)
        {
        }

        protected override async Task SaveStatementAsync(
            DepositReturnDto statement, 
            string returnId, 
            string submissionId)
        {
            // Map DTO to entity
            var entity = new DepositReturn
            {
                ReturnId = returnId,
                SubmissionId = submissionId,
                SaccoCsNumber = statement.SaccoCsNumber,
                ReportingPeriod = statement.Period,
                StartDate = statement.StartDate,
                EndDate = statement.EndDate,
                MembersDeposits = statement.MembersDeposits,
                DepositAccountsWithBanks = statement.DepositAccountsWithBanks,
                FixedDepositsWithBanks = statement.FixedDepositsWithBanks,
                Cash = statement.Cash,
                TotalLiquidAssets = statement.TotalLiquidAssets,
                LoansToMembers = statement.LoansToMembers,
                ExternalBorrowings = statement.ExternalBorrowings,
                TotalIlliquidAssets = statement.TotalIlliquidAssets,
                TotalAssets = statement.TotalAssets,
                WithdrawableDeposits = statement.WithdrawableDeposits,
                NonWithdrawableDeposits = statement.NonWithdrawableDeposits,
                TotalDeposits = statement.TotalDeposits,
                LiquidityRatio = statement.LiquidityRatio,
                CoreCapital = statement.CoreCapital,
                TotalAssetsEnd = statement.TotalAssetsEnd,
                Version = 1,
                CreatedAt = DateTime.Now
            };

            // Check if this is an update
            var existing = await _context.DepositReturns
                .Where(d => d.ReturnId == returnId && d.IsLatest)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.IsLatest = false;
                entity.Version = existing.Version + 1;
            }

            entity.IsLatest = true;
            _context.DepositReturns.Add(entity);
        }

        protected override async Task<List<string>> ValidateStatementAsync(DepositReturnDto statement)
        {
            var errors = new List<string>();

            // Basic validation
            if (string.IsNullOrWhiteSpace(statement.SaccoCsNumber))
                errors.Add("SACCO CS Number is required");

            if (statement.StartDate >= statement.EndDate)
                errors.Add("Start date must be before end date");

            // Business rule validations
            if (statement.TotalLiquidAssets != 
                (statement.MembersDeposits + statement.DepositAccountsWithBanks + 
                 statement.FixedDepositsWithBanks + statement.Cash))
            {
                errors.Add("Total Liquid Assets does not match sum of components");
            }

            if (statement.TotalDeposits != 
                (statement.WithdrawableDeposits + statement.NonWithdrawableDeposits))
            {
                errors.Add("Total Deposits does not match sum of withdrawable and non-withdrawable deposits");
            }

            // Liquidity ratio check
            if (statement.TotalDeposits > 0)
            {
                var calculatedRatio = (statement.TotalLiquidAssets / statement.TotalDeposits) * 100;
                if (Math.Abs(calculatedRatio - statement.LiquidityRatio) > 0.1m)
                {
                    errors.Add($"Liquidity Ratio mismatch. Expected: {calculatedRatio:F2}%, Found: {statement.LiquidityRatio:F2}%");
                }
            }

            // Warning for low liquidity ratio
            if (statement.LiquidityRatio < 15m)
            {
                errors.Add("WARNING: Liquidity ratio is below the recommended minimum of 15%");
            }

            return errors;
        }
    }
}