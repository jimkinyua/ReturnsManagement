using Microsoft.EntityFrameworkCore;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public static class DynamicRatingHelper
    {
        public static async Task<(int Rating, decimal Weight)> GetRatingForIndicatorAsync(string indicatorName, decimal actualValue)
        {
            var _context = new ReturnsDbContext();
            var indicator = await _context.CamelIndicators
                .Include(i => i.RatingThresholds)
                .SingleOrDefaultAsync(i => i.Name == indicatorName);

            if (indicator == null)
            {
                throw new Exception($"Indicator '{indicatorName}' not found in configuration.");
            }

            // Order thresholds by RatingLevel (assumed 1 = best, 5 = worst)
            var thresholds = indicator.RatingThresholds
                .OrderBy(t => t.RatingLevel)
                .Select(t => t.ThresholdValue)
                .ToArray();

            int rating = GetRating(actualValue, thresholds, indicator.BetterHigher);
            return (rating, indicator.Weight);
        }


        private static int GetRating(decimal actualValue, decimal[] thresholds, bool higherIsBetter)
        {
            if (higherIsBetter)
            {
        
                for (int i = 0; i < thresholds.Length; i++)
                {
                    // Check if the actual value exceeds the threshold at index i.
                    if (actualValue > thresholds[i])
                    {
                        // Because ratings start at 1 (not 0), return i+1.
                        return i + 1;
                    }
                }
                // If none of the thresholds are exceeded, the value is too low,
                // so assign the worst rating (which is one level below the last threshold).
                if (thresholds.Length >= 5)
                {
                    return thresholds.Length;
                }
                return thresholds.Length + 1;
            }
            else
            {

                for (int i = 0; i < thresholds.Length; i++)
                {
                    if (actualValue < thresholds[i])
                    {
                        return i + 1;
                    }
                }
                // If none of the thresholds are met, then the value is too high,
                // so assign the worst rating.
                if (thresholds.Length >= 5)
                {
                    return thresholds.Length;
                }
                return thresholds.Length + 1;
            }
        }
    }
    }
