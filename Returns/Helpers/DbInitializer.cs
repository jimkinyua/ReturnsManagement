using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using Returns.Helpers.Enums;
using Returns.Models;
using Returns.Models.CamelSetup;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class DbInitializer
    {

        public void SeedPeriods(ReturnsDbContext context)
        {
            // Check if any periods already exist
            if (!context.ReturnPeriods.Any())
            {
                // Define the standard periods
                /*    var periods = new List<Models.ReturnPeriods>
                    {
                        new Models.ReturnPeriods { Name = "Daily", CreatedBy = "System" },
                        new Models.ReturnPeriods { Name = "Monthly" , CreatedBy = "System" },
                        new Models.ReturnPeriods { Name = "Quarterly", CreatedBy = "System" },
                        new Models.ReturnPeriods { Name = "Semi-Annual", CreatedBy = "System" },
                        new Models.ReturnPeriods { Name = "Annual" , CreatedBy = "System"},
                        new Models.ReturnPeriods { Name = "Bi-Monthly", CreatedBy = "System" }
                };

                    context.ReturnPeriods.AddRange(periods);
                    context.SaveChanges();
                    Console.WriteLine("Period table seeded successfully.");
                }
                else
                {
                    Console.WriteLine("Period table already contains data. Skipping seed operation.");
                }*/
            }
        }

        public void SeedRatingDefinitionsForSaccoTypeDTSaccos(ReturnsDbContext context)
        {
            var currentDateTime = DateTime.Now;
            var saccoTypeId = 0;
            var saccoType = saccoTypeId.ToString();

            // Define desired RatingDefinitions for SaccoType = "0"
            var desiredRatings = new[]
            {
                new RatingDefination
                {
                    RatingName = "CAELS",
                    Description = "Capital, Asset Quality, Earnings, Liquidity, Structure Rating for SaccoType 0",
                    SaccoType = saccoType,
                    CreatedAt = currentDateTime,
                },
                new RatingDefination
                    {
                        RatingName = "Consistency",
                        Description = "Consistency Check Rating for DT Saccos",
                        SaccoType = saccoType,
                        CreatedAt = currentDateTime,
                    }
             };

            // Add or update RatingDefinitions
            foreach (var desired in desiredRatings)
            {
                var existing = context.RatingDefinations
                    .FirstOrDefault(r => r.RatingName == desired.RatingName && r.SaccoType == desired.SaccoType);

                if (existing != null)
                {
                    // Update existing
                    existing.Description = desired.Description;
                }
                else
                {
                    // Add new
                    context.RatingDefinations.Add(desired);
                }
            }
            context.SaveChanges();

            // Fetch active return forms for SaccoType "0"
            var returnForms = context.ReturnForms.Where(f => f.SaccoTypeId == "0" && f.IsActive).ToList();

            // Define category mappings
            var ratingCategoryMappings = new[]
            {
                new { RatingName = "CAELS", Categories = new[] {
                    FormCategory.CapitalAdequacy,
                    FormCategory.LiquidityStatement,
                    FormCategory.Management,
                    FormCategory.StatementOfComprehensiveIncome,
                    FormCategory.DepositReturn,
                    FormCategory.RiskClassification,
                    FormCategory.FinancialPosition,
                    FormCategory.InvestmentReturn,
                } },
                new { RatingName = "Consistency", Categories = new[] {
                    FormCategory.CapitalAdequacy,
                    FormCategory.LiquidityStatement,
                    FormCategory.StatementOfComprehensiveIncome,
                    FormCategory.DepositReturn,
                    FormCategory.RiskClassification,
                    FormCategory.FinancialPosition,
                    FormCategory.InvestmentReturn,
                } },
            };

            // Update RatingForms for each mapping
            foreach (var mapping in ratingCategoryMappings)
            {
                var rating = context.RatingDefinations
                    .FirstOrDefault(r => r.RatingName == mapping.RatingName && r.SaccoType == saccoType);

                if (rating == null)
                {
                    Console.WriteLine($"RatingDefinition '{mapping.RatingName}' not found. Skipping form associations.");
                    continue;
                }

                // Remove existing RatingForms for this RatingDefinition
                var existingForms = context.RatingForms.Where(rf => rf.RatingDefinationId == rating.Id).ToList();
                context.RatingForms.RemoveRange(existingForms);
                context.SaveChanges();

                // Add new RatingForms
                var requiredCategories = mapping.Categories;
                foreach (var category in requiredCategories)
                {
                    var form = returnForms.FirstOrDefault(f => f.Category == category && f.SaccoTypeId == "0");
                    if (form != null)
                    {
                        var newRatingForm = new RatingForm
                        {
                            FormCode = form.Code,
                            RatingDefinationId = rating.Id,
                            CreatedAt = currentDateTime,
                        };
                        rating.RatingForms.Add(newRatingForm);
                    }
                    else
                    {
                        Console.WriteLine($"No active form found for category {category} in SaccoTypeId {saccoTypeId}.");
                    }
                }
            }
            context.SaveChanges();
            Console.WriteLine("RatingDefinitions and RatingForms for SaccoTypeId 0 seeded/updated successfully.");
        }


        public void SeedRatingDefinitionsForSaccoTypeNWDTSaccos(ReturnsDbContext context)
        {
            var currentDateTime = DateTime.Now;
            var saccoTypeId = "1";
            var saccoType = saccoTypeId;

            // Define desired RatingDefinitions for SaccoType = "1"
            var desiredRatings = new[]
            {
        new RatingDefination
        {
            RatingName = "CAELS",
            Description = "Capital, Asset Quality, Earnings, Liquidity, Structure Rating for SaccoType 1",
            SaccoType = saccoType,
            CreatedAt = currentDateTime,
        },
        new RatingDefination
        {
            RatingName = "Consistency",
            Description = "Consistency Check Rating for NWDT Saccos",
            SaccoType = saccoType,
            CreatedAt = currentDateTime,
        }
    };

            // Add or update RatingDefinitions
            foreach (var desired in desiredRatings)
            {
                var existing = context.RatingDefinations
                    .FirstOrDefault(r => r.RatingName == desired.RatingName && r.SaccoType == desired.SaccoType);

                if (existing != null)
                {
                    // Update existing
                    existing.Description = desired.Description;
                    // Note: Not updating CreatedAt to preserve original creation time
                }
                else
                {
                    // Add new
                    context.RatingDefinations.Add(desired);
                }
            }
            context.SaveChanges();

            // Fetch active return forms for SaccoType "1"
            var returnForms = context.ReturnForms.Where(f => f.SaccoTypeId == saccoTypeId && f.IsActive).ToList();

            // Define category mappings
            var ratingCategoryMappings = new[]
            {
        new { RatingName = "CAELS", Categories = new[] {
            FormCategory.CapitalAdequacy,
            FormCategory.LiquidityStatement,
            FormCategory.Management,
            FormCategory.StatementOfComprehensiveIncome,
            FormCategory.DepositReturn,
            FormCategory.RiskClassification,
            FormCategory.FinancialPosition,
            FormCategory.InvestmentReturn,
        } },
        new { RatingName = "Consistency", Categories = new[] {
            FormCategory.CapitalAdequacy,
            FormCategory.LiquidityStatement,
            FormCategory.StatementOfComprehensiveIncome,
            FormCategory.DepositReturn,
            FormCategory.RiskClassification,
            FormCategory.FinancialPosition,
            FormCategory.InvestmentReturn,
        } },
    };

            // Update RatingForms for each mapping
            foreach (var mapping in ratingCategoryMappings)
            {
                var rating = context.RatingDefinations
                    .FirstOrDefault(r => r.RatingName == mapping.RatingName && r.SaccoType == saccoType);

                if (rating == null)
                {
                    Console.WriteLine($"RatingDefinition '{mapping.RatingName}' not found. Skipping form associations.");
                    continue;
                }

                // Remove existing RatingForms for this RatingDefinition
                var existingForms = context.RatingForms.Where(rf => rf.RatingDefinationId == rating.Id).ToList();
                context.RatingForms.RemoveRange(existingForms);
                context.SaveChanges();

                // Add new RatingForms
                var requiredCategories = mapping.Categories;
                foreach (var category in requiredCategories)
                {
                    var form = returnForms.FirstOrDefault(f => f.Category == category && f.SaccoTypeId == saccoTypeId);
                    if (form != null)
                    {
                        var newRatingForm = new RatingForm
                        {
                            FormCode = form.Code,
                            RatingDefinationId = rating.Id,
                            CreatedAt = currentDateTime,
                        };
                        rating.RatingForms.Add(newRatingForm);
                    }
                    else
                    {
                        Console.WriteLine($"No active form found for category {category} in SaccoTypeId {saccoTypeId}.");
                    }
                }
            }
            context.SaveChanges();
            Console.WriteLine("RatingDefinitions and RatingForms for SaccoTypeId 1 seeded/updated successfully.");
        }

        public void IntialiseCamelData(ReturnsDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if there is any data already.
            if (context.CamelCategories.Any())
            {
                return;   // Database has been seeded
            }

            // 1. Seed CamelCategories
            var categoryC = new CamelCategory { Code = "C", Name = "Capital Adequacy" };
            var categoryA = new CamelCategory { Code = "A", Name = "Asset Quality" };
            var categoryM = new CamelCategory { Code = "M", Name = "Management" };
            var categoryE = new CamelCategory { Code = "E", Name = "Earnings" };
            var categoryL = new CamelCategory { Code = "L", Name = "Liquidity" };
            var categoryS = new CamelCategory { Code = "S", Name = "Structure of Assets" };

            context.CamelCategories.AddRange(categoryC, categoryA, categoryM, categoryE, categoryL, categoryS);
            context.SaveChanges();

            // 2. Seed CamelIndicators
            var indicatorCCA = new CamelIndicator
            {
                CategoryId = categoryC.Id,
                Name = "Core Capital Ratio (CCA)",
                Weight = 0.20m,
                BetterHigher = true
            };
            var indicatorICA = new CamelIndicator
            {
                CategoryId = categoryC.Id,
                Name = "ICA",
                Weight = 0.10m,
                BetterHigher = true
            };
            var indicatorCCD = new CamelIndicator
            {
                CategoryId = categoryC.Id,
                Name = "CCD",
                Weight = 0.10m,
                BetterHigher = true
            };

            var indicatorNPL30 = new CamelIndicator
            {
                CategoryId = categoryA.Id,
                Name = "NPL30",
                Weight = 0.30m,
                BetterHigher = false
            };
            var indicatorALNPL30 = new CamelIndicator
            {
                CategoryId = categoryA.Id,
                Name = "A/L NPL30",
                Weight = 0.20m,
                BetterHigher = false
            };

            var indicatorMER = new CamelIndicator
            {
                CategoryId = categoryM.Id,
                Name = "Management Efficiency Ratio (MER)",
                Weight = 0.25m,
                BetterHigher = false
            };
            var indicatorSP = new CamelIndicator
            {
                CategoryId = categoryM.Id,
                Name = "Staff Productivity (SP)",
                Weight = 0.15m,
                BetterHigher = true
            };

            var indicatorROA = new CamelIndicator
            {
                CategoryId = categoryE.Id,
                Name = "Return on Assets (ROA)",
                Weight = 0.20m,
                BetterHigher = true
            };
            var indicatorROE = new CamelIndicator
            {
                CategoryId = categoryE.Id,
                Name = "Return on Equity (ROE)",
                Weight = 0.20m,
                BetterHigher = true
            };

            var indicatorLCR = new CamelIndicator
            {
                CategoryId = categoryL.Id,
                Name = "Liquidity Coverage Ratio (LCR)",
                Weight = 0.25m,
                BetterHigher = true
            };
            var indicatorLDR = new CamelIndicator
            {
                CategoryId = categoryL.Id,
                Name = "Loan to Deposit Ratio (LDR)",
                Weight = 0.15m,
                BetterHigher = false
            };
            var indicatorLB = new CamelIndicator
            {
                CategoryId = categoryS.Id,
                Name = "LB ratio",
                Weight = 0.10m,     // example weight, adjust as needed
                BetterHigher = false  // "Max 5%" => lower is better
            };
            var indicatorFICC = new CamelIndicator
            {
                CategoryId = categoryS.Id,
                Name = "FICC",
                Weight = 0.10m,      // example
                BetterHigher = false // "Max 40%" => lower is better
            };
            var indicatorFITD = new CamelIndicator
            {
                CategoryId = categoryS.Id,
                Name = "FITD",
                Weight = 0.10m,      // example
                BetterHigher = false // "Max 5%" => lower is better
            };
            var indicatorNEA = new CamelIndicator
            {
                CategoryId = categoryS.Id,
                Name = "NEA",
                Weight = 0.10m,      // example
                BetterHigher = false // "Max 10%" => lower is better
            };

            context.CamelIndicators.AddRange(
                indicatorCCA, indicatorICA, indicatorCCD,
                indicatorNPL30, indicatorALNPL30,
                indicatorMER, indicatorSP,
                indicatorROA, indicatorROE,
                indicatorLCR, indicatorLDR, indicatorLB, indicatorFICC, indicatorFITD, indicatorNEA
            );


            // 3. Seed IndicatorRatingThresholds
            var ratingThresholds = new List<IndicatorRatingThreshold>
        {
            // Core Capital Ratio (CCA)
            new IndicatorRatingThreshold { IndicatorId = indicatorCCA.Id, RatingLevel = 1, ThresholdValue = 15.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCA.Id, RatingLevel = 2, ThresholdValue = 12.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCA.Id, RatingLevel = 3, ThresholdValue = 10.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCA.Id, RatingLevel = 4, ThresholdValue = 8.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCA.Id, RatingLevel = 5, ThresholdValue = 0.00m },

            // ICA
            new IndicatorRatingThreshold { IndicatorId = indicatorICA.Id, RatingLevel = 1, ThresholdValue = 14.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorICA.Id, RatingLevel = 2, ThresholdValue = 11.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorICA.Id, RatingLevel = 3, ThresholdValue = 9.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorICA.Id, RatingLevel = 4, ThresholdValue = 7.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorICA.Id, RatingLevel = 5, ThresholdValue = 0.00m },

            // CCD
            new IndicatorRatingThreshold { IndicatorId = indicatorCCD.Id, RatingLevel = 1, ThresholdValue = 13.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCD.Id, RatingLevel = 2, ThresholdValue = 10.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCD.Id, RatingLevel = 3, ThresholdValue = 8.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCD.Id, RatingLevel = 4, ThresholdValue = 6.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorCCD.Id, RatingLevel = 5, ThresholdValue = 0.00m },

            // NPL30
            new IndicatorRatingThreshold { IndicatorId = indicatorNPL30.Id, RatingLevel = 1, ThresholdValue = 2.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNPL30.Id, RatingLevel = 2, ThresholdValue = 4.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNPL30.Id, RatingLevel = 3, ThresholdValue = 6.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNPL30.Id, RatingLevel = 4, ThresholdValue = 8.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNPL30.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

            // A/L NPL30
            new IndicatorRatingThreshold { IndicatorId = indicatorALNPL30.Id, RatingLevel = 1, ThresholdValue = 2.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorALNPL30.Id, RatingLevel = 2, ThresholdValue = 4.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorALNPL30.Id, RatingLevel = 3, ThresholdValue = 7.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorALNPL30.Id, RatingLevel = 4, ThresholdValue = 10.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorALNPL30.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

            // MER
            new IndicatorRatingThreshold { IndicatorId = indicatorMER.Id, RatingLevel = 1, ThresholdValue = 40.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorMER.Id, RatingLevel = 2, ThresholdValue = 50.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorMER.Id, RatingLevel = 3, ThresholdValue = 60.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorMER.Id, RatingLevel = 4, ThresholdValue = 70.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorMER.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

            // SP
            new IndicatorRatingThreshold { IndicatorId = indicatorSP.Id, RatingLevel = 1, ThresholdValue = 10.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorSP.Id, RatingLevel = 2, ThresholdValue = 8.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorSP.Id, RatingLevel = 3, ThresholdValue = 6.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorSP.Id, RatingLevel = 4, ThresholdValue = 4.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorSP.Id, RatingLevel = 5, ThresholdValue = 0.00m },

            // ROA
            new IndicatorRatingThreshold { IndicatorId = indicatorROA.Id, RatingLevel = 1, ThresholdValue = 2.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROA.Id, RatingLevel = 2, ThresholdValue = 1.50m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROA.Id, RatingLevel = 3, ThresholdValue = 1.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROA.Id, RatingLevel = 4, ThresholdValue = 0.50m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROA.Id, RatingLevel = 5, ThresholdValue = 0.00m },

            // ROE
            new IndicatorRatingThreshold { IndicatorId = indicatorROE.Id, RatingLevel = 1, ThresholdValue = 15.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROE.Id, RatingLevel = 2, ThresholdValue = 10.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROE.Id, RatingLevel = 3, ThresholdValue = 5.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROE.Id, RatingLevel = 4, ThresholdValue = 2.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorROE.Id, RatingLevel = 5, ThresholdValue = 0.00m },

            // LCR
            new IndicatorRatingThreshold { IndicatorId = indicatorLCR.Id, RatingLevel = 1, ThresholdValue = 150.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLCR.Id, RatingLevel = 2, ThresholdValue = 120.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLCR.Id, RatingLevel = 3, ThresholdValue = 100.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLCR.Id, RatingLevel = 4, ThresholdValue = 80.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLCR.Id, RatingLevel = 5, ThresholdValue = 0.00m },

            // LDR
            new IndicatorRatingThreshold { IndicatorId = indicatorLDR.Id, RatingLevel = 1, ThresholdValue = 70.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLDR.Id, RatingLevel = 2, ThresholdValue = 80.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLDR.Id, RatingLevel = 3, ThresholdValue = 90.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLDR.Id, RatingLevel = 4, ThresholdValue = 100.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLDR.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

              // LB ratio (Max 5%)
            new IndicatorRatingThreshold { IndicatorId = indicatorLB.Id, RatingLevel = 1, ThresholdValue = 2.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLB.Id, RatingLevel = 2, ThresholdValue = 3.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLB.Id, RatingLevel = 3, ThresholdValue = 5.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLB.Id, RatingLevel = 4, ThresholdValue = 7.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorLB.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

            // FICC (Max 40%)
            new IndicatorRatingThreshold { IndicatorId = indicatorFICC.Id, RatingLevel = 1, ThresholdValue = 20.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFICC.Id, RatingLevel = 2, ThresholdValue = 30.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFICC.Id, RatingLevel = 3, ThresholdValue = 40.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFICC.Id, RatingLevel = 4, ThresholdValue = 50.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFICC.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

            // FITD (Max 5%)
            new IndicatorRatingThreshold { IndicatorId = indicatorFITD.Id, RatingLevel = 1, ThresholdValue = 2.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFITD.Id, RatingLevel = 2, ThresholdValue = 3.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFITD.Id, RatingLevel = 3, ThresholdValue = 5.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFITD.Id, RatingLevel = 4, ThresholdValue = 7.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorFITD.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

            // NEA (Max 10%)
            new IndicatorRatingThreshold { IndicatorId = indicatorNEA.Id, RatingLevel = 1, ThresholdValue = 5.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNEA.Id, RatingLevel = 2, ThresholdValue = 7.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNEA.Id, RatingLevel = 3, ThresholdValue = 10.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNEA.Id, RatingLevel = 4, ThresholdValue = 12.00m },
            new IndicatorRatingThreshold { IndicatorId = indicatorNEA.Id, RatingLevel = 5, ThresholdValue = 99999.00m },

            };

            context.IndicatorRatingThresholds.AddRange(ratingThresholds);
            context.SaveChanges();
        }

        public void SeedFrequencyCatalog(ReturnsDbContext context)
        {
            // Check if any frequency catalogs already exist
            if (!context.FrequencyCatalogs.Any())
            {
                var frequencyCatalogs = new List<Models.FrequencyCatalog>
                {
                    new Models.FrequencyCatalog
                    {
                        Code = "DAY",
                        Name = "Daily",
                        IntervalDays = 1,
                        DefaultDeadlineOffset = 1,
                        LabelStrategy = "DATE",
                        IsActive = true,
                        CreatedBy = "System"
                    },
                    new Models.FrequencyCatalog
                    {
                        Code = "WK",
                        Name = "Weekly",
                        IntervalDays = 7,
                        DefaultDeadlineOffset = 3,
                        LabelStrategy = "ISO_WEEK",
                        IsActive = true,
                        CreatedBy = "System"
                    },
                    new Models.FrequencyCatalog
                    {
                        Code = "BWK",
                        Name = "Bi-Weekly",
                        IntervalDays = 14,
                        DefaultDeadlineOffset = 5,
                        LabelStrategy = "BI_WEEK",
                        IsActive = true,
                        CreatedBy = "System"
                    },
                    new Models.FrequencyCatalog
                    {
                        Code = "MTH",
                        Name = "Monthly",
                        IntervalDays = 30, // Approximate, actual generation will handle month variations
                        DefaultDeadlineOffset = 15,
                        LabelStrategy = "MONTH",
                        IsActive = true,
                        CreatedBy = "System"
                    },
                    new Models.FrequencyCatalog
                    {
                        Code = "QTR",
                        Name = "Quarterly",
                        IntervalDays = 90, // Approximate, actual generation will handle quarter variations
                        DefaultDeadlineOffset = 30,
                        LabelStrategy = "QUARTER",
                        IsActive = true,
                        CreatedBy = "System"
                    },
                    new Models.FrequencyCatalog
                    {
                        Code = "SEMI",
                        Name = "Semi-Annual",
                        IntervalDays = 180, // Approximate
                        DefaultDeadlineOffset = 45,
                        LabelStrategy = "SEMI_ANNUAL",
                        IsActive = true,
                        CreatedBy = "System"
                    },
                    new Models.FrequencyCatalog
                    {
                        Code = "FY",
                        Name = "Annual",
                        IntervalDays = 365, // Approximate, actual generation will handle year variations
                        DefaultDeadlineOffset = 60,
                        LabelStrategy = "YEAR",
                        IsActive = true,
                        CreatedBy = "System"
                    }
                };

                context.FrequencyCatalogs.AddRange(frequencyCatalogs);
                context.SaveChanges();
                Console.WriteLine("FrequencyCatalog table seeded successfully.");
            }
            else
            {
                Console.WriteLine("FrequencyCatalog table already contains data. Skipping seed operation.");
            }
        }
    }
}

