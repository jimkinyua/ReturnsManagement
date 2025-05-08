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


        private static int GetRating(decimal actualValue,IEnumerable<IndicatorRatingThreshold> thresholds,bool higherIsBetter)
        {
            var ordered = thresholds.OrderBy(t => t.RatingLevel).ToList();
            if (ordered.Count == 0)
            {
                return 5;
            }

            int worstLevel = 5; //ordered.Max(t => t.RatingLevel); 

            if (higherIsBetter)
            {
                // strict ‘>’ → equality slides to the next (worse) band
                foreach (var t in ordered)
                {
                    if (actualValue > t.ThresholdValue)
                    {
                        // If the actual value is greater than the threshold, return the rating level
                        return t.RatingLevel;
                    }
                } 
            }
            else
            {
                // strict ‘<’
                foreach (var t in ordered)
                {
                    if (actualValue < t.ThresholdValue)
                    {
                        // If the actual value is less than the threshold, return the rating level
                        return t.RatingLevel;
                    }

                }
            }

            // Fell through every band ➜ assign the worst level present
            return worstLevel;
        }
    }
    }
