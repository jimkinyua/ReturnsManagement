using Microsoft.EntityFrameworkCore;
using Returns.Models.CamelSetup;
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
                //.Select(t => t.ThresholdValue)
                .ToArray();

            int rating = GetRating(actualValue, thresholds, indicator.BetterHigher);
            return (rating, indicator.Weight);
        }


        private static int GetRating(decimal actualValue, IEnumerable<IndicatorRatingThreshold> thresholds, bool higherIsBetter)
        {
            foreach (var t in thresholds)  
            {
                if (higherIsBetter)
                {
                    if (actualValue >= t.ThresholdValue)
                    {
                        return t.RatingLevel;
                    }
                }
                else
                {
                    if (actualValue <= t.ThresholdValue)
                    {
                        return t.RatingLevel;
                    } 
                }
            }
            // If no thresholds matched, return the worst rating
            return thresholds.Max(t => t.RatingLevel);
        }
    }
    }
