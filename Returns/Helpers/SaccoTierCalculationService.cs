using Microsoft.EntityFrameworkCore;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    /// <summary>
    /// Service for calculating SACCO tiers based on various factors
    /// This service can be enhanced with more sophisticated algorithms and business rules
    /// </summary>
    public interface ISaccoTierCalculationService
    {
        /// <summary>
        /// Calculates the SACCO tier based on CAELS rating and other factors
        /// </summary>
        /// <param name="rating">CAELS rating (1-5)</param>
        /// <param name="classification">Classification from inspection request</param>
        /// <param name="riskLevel">Risk level from CAELS analysis</param>
        /// <returns>Tier as a string (1, 2, or 3)</returns>
        string CalculateTier(int rating, string? classification, string? riskLevel);

        /// <summary>
        /// Calculates the SACCO tier with additional context from workflow instance
        /// </summary>
        /// <param name="workflowInstance">Workflow instance containing SACCO data</param>
        /// <param name="classification">Classification from inspection request</param>
        /// <param name="riskLevel">Risk level from CAELS analysis</param>
        /// <returns>Tier as a string (1, 2, or 3)</returns>
        string CalculateTierFromWorkflow(WorkflowInstance workflowInstance, string? classification, string? riskLevel);
    }

    public class SaccoTierCalculationService : ISaccoTierCalculationService
    {
        private readonly ReturnsDbContext _db;
        private readonly ILogger<SaccoTierCalculationService> _logger;

        public SaccoTierCalculationService(ReturnsDbContext db, ILogger<SaccoTierCalculationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public string CalculateTier(int rating, string? classification, string? riskLevel)
        {
            // Base tier calculation on CAELS rating
            var baseTier = GetBaseTierFromRating(rating);

            // Apply adjustments based on classification and risk level
            var adjustedTier = ApplyClassificationAdjustments(baseTier, classification, riskLevel);

            return adjustedTier.ToString();
        }

        public string CalculateTierFromWorkflow(WorkflowInstance workflowInstance, string? classification, string? riskLevel)
        {
            var rating = workflowInstance.Rating ?? 0;
            return CalculateTier(rating, classification, riskLevel);
        }

        private int GetBaseTierFromRating(int rating)
        {
            // Base tier calculation based on CAELS rating
            // Rating 1-2: Tier 1 (High Risk)
            // Rating 3: Tier 2 (Medium Risk)  
            // Rating 4-5: Tier 3 (Low Risk)

            return rating switch
            {
                <= 2 => 1,  // High risk
                3 => 2,      // Medium risk
                >= 4 => 3,   // Low risk
            };
        }

        private int ApplyClassificationAdjustments(int baseTier, string? classification, string? riskLevel)
        {
            var adjustedTier = baseTier;

            // Apply classification adjustments
            if (!string.IsNullOrEmpty(classification))
            {
                adjustedTier = classification.ToLower() switch
                {
                    "critical" or "major" => Math.Max(1, adjustedTier - 1), // Move to higher risk tier
                    "minor" => Math.Min(3, adjustedTier + 1),                // Move to lower risk tier
                    _ => adjustedTier
                };
            }

            // Apply risk level adjustments
            if (!string.IsNullOrEmpty(riskLevel))
            {
                adjustedTier = riskLevel.ToLower() switch
                {
                    "high" => Math.Max(1, adjustedTier - 1),     // Move to higher risk tier
                    "low" => Math.Min(3, adjustedTier + 1),      // Move to lower risk tier
                    _ => adjustedTier
                };
            }

            return adjustedTier;
        }

        /// <summary>
        /// Enhanced tier calculation that can be implemented later with more sophisticated logic
        /// </summary>
        /// <param name="saccoId">SACCO ID for historical analysis</param>
        /// <param name="periodId">Period ID for context</param>
        /// <returns>Enhanced tier calculation</returns>
        private async Task<int> CalculateEnhancedTierAsync(string saccoId, string periodId)
        {
            // TODO: Implement enhanced tier calculation with:
            // - Historical performance analysis
            // - Complaints count and severity
            // - Regulatory violations history
            // - Financial ratios analysis
            // - Market conditions impact
            // - Peer comparison analysis
            // - Industry benchmarks

            // For now, return the base calculation
            var caelsRating = await _db.CAELSRatings
                .Where(r => r.SaccoId == saccoId && r.PeriodId == periodId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var rating = caelsRating?.OverallRating ?? 3;
            return GetBaseTierFromRating((int)rating);
        }
    }
}
