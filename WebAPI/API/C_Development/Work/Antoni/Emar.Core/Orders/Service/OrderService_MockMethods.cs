using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;

namespace Emar.Core.Orders.Service
{
    public partial class OrderService
    {
        public ComposerOptionsDto GetComposerSetupData(string brandName)
        {
            // Temporary for showing the shape
            var setupOptions = new ComposerOptionsDto
            {
                BrandName = "Ondansetron Hydrochloride",
                AvailableFormStrength = new[]
                {
                    new FormStrengthDto
                    {
                        Id = 1095,
                        FormStrengthName = "4mg orally disintegrating",
                        AvailableRoutes = new[]
                        {
                            new MedicationRouteDto {Id = 1, RouteName = "orally", SiteId = -1},
                            new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1}
                        },
                        PreferredDoses = new List<PreferredDoseDto>
                        {
                            new PreferredDoseDto {DoseName = "4 mg", Dose = new decimal(4.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}},
                            new PreferredDoseDto {DoseName = "8 mg", Dose = new decimal(8.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}}
                        },
                        PreferredRoutes = new[] {new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1}},
                        PreferredFrequencies = new[]
                        {
                            new MockFrequencyDto {FrequencyName = "2 TIMES DAILY", Id = 7},
                            new MockFrequencyDto {FrequencyName = "Every 6 HOURS", Id = 5},
                            new MockFrequencyDto {FrequencyName = "ONCE", Id = 1}
                        }
                    },
                    new FormStrengthDto
                    {
                        Id = 1099,
                        FormStrengthName = "8mg orally disintegrating",
                        AvailableRoutes = new []
                        {
                            new MedicationRouteDto {Id = 1, RouteName = "orally", SiteId = -1},
                            new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1}
                        },
                        PreferredDoses = new List<PreferredDoseDto>
                        {
                            new PreferredDoseDto {DoseName = "8 mg", Dose = new decimal(8.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}},
                            new PreferredDoseDto {DoseName = "16 mg", Dose = new decimal(16.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}}
                        },
                        PreferredRoutes = new[] { new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1} },
                        PreferredFrequencies = new[]
                        {
                            new MockFrequencyDto {FrequencyName = "2 TIMES DAILY", Id = 7},
                            new MockFrequencyDto {FrequencyName = "Every 6 HOURS", Id = 5},
                            new MockFrequencyDto {FrequencyName = "ONCE", Id = 1}
                        }
                    }
                }
            };

            return setupOptions;
        }

        public IEnumerable<MockFrequencyDto> GetFrequencies(int siteId)
        {
            return new[]
            {
                new MockFrequencyDto {Id = 1, FrequencyName = "ONCE"},
                new MockFrequencyDto {Id = 2, FrequencyName = "Every 2 HOURS"},
                new MockFrequencyDto {Id = 3, FrequencyName = "Every 3 HOURS"},
                new MockFrequencyDto {Id = 4, FrequencyName = "Every 4 HOURS"},
                new MockFrequencyDto {Id = 5, FrequencyName = "Every 6 HOURS"},
                new MockFrequencyDto {Id = 6, FrequencyName = "Every 12 HOURS"},
                new MockFrequencyDto {Id = 7, FrequencyName = "2 TIMES DAILY"},
                new MockFrequencyDto {Id = 8, FrequencyName = "3 TIMES DAILY"},
                new MockFrequencyDto {Id = 9, FrequencyName = "4 TIMES DAILY"},
                new MockFrequencyDto {Id = 10, FrequencyName = "6 TIMES DAILY"},
                new MockFrequencyDto {Id = 11, FrequencyName = "8 TIMES DAILY"},
                new MockFrequencyDto {Id = 12, FrequencyName = "BEFORE MEALS"},
                new MockFrequencyDto {Id = 13, FrequencyName = "AFTER MEALS"},
                new MockFrequencyDto {Id = 14, FrequencyName = "WITH EACH MEAL"},
                new MockFrequencyDto {Id = 15, FrequencyName = "BEFORE BED"},
                new MockFrequencyDto {Id = 16, FrequencyName = "AFTER WAKING"}
            };
        }

        public IEnumerable<MockUnitDto> GetUnits(in int siteId)
        {
            return new[]
            {
                new MockUnitDto {Id = 1, UnitName = "app"},
                new MockUnitDto {Id = 2, UnitName = "appful"},
                new MockUnitDto {Id = 3, UnitName = "application"},
                new MockUnitDto {Id = 4, UnitName = "applicator"},
                new MockUnitDto {Id = 5, UnitName = "apply"},
                new MockUnitDto {Id = 6, UnitName = "Bottle"},
                new MockUnitDto {Id = 7, UnitName = "cap(s)"},
                new MockUnitDto {Id = 8, UnitName = "desktest#"},
                new MockUnitDto {Id = 9, UnitName = "Diskus"},
                new MockUnitDto {Id = 10, UnitName = "drop"},
                new MockUnitDto {Id = 11, UnitName = "ea"},
                new MockUnitDto {Id = 12, UnitName = "enema"},
                new MockUnitDto {Id = 13, UnitName = "Film"},
                new MockUnitDto {Id = 14, UnitName = "g"},
                new MockUnitDto {Id = 15, UnitName = "g/kg"},
                new MockUnitDto {Id = 16, UnitName = "gm"},
                new MockUnitDto {Id = 17, UnitName = "gm/hr"},
                new MockUnitDto {Id = 18, UnitName = "GRAN(S)"},
                new MockUnitDto {Id = 19, UnitName = "gtt"},
                new MockUnitDto {Id = 20, UnitName = "in"},
                new MockUnitDto {Id = 21, UnitName = "INH"},
                new MockUnitDto {Id = 22, UnitName = "Injection"},
                new MockUnitDto {Id = 23, UnitName = "intl units"},
                new MockUnitDto {Id = 24, UnitName = "intl units/kg"},
                new MockUnitDto {Id = 25, UnitName = "intl units/m2"},
                new MockUnitDto {Id = 26, UnitName = "IUPSQ"},
                new MockUnitDto {Id = 27, UnitName = "L"},
                new MockUnitDto {Id = 28, UnitName = "lbs"},
                new MockUnitDto {Id = 29, UnitName = "LOW HeparinProtocolBolus60Units/kg"},
                new MockUnitDto {Id = 30, UnitName = "loz"},
                new MockUnitDto {Id = 31, UnitName = "mcg"},
                new MockUnitDto {Id = 32, UnitName = "mcg/hr"},
                new MockUnitDto {Id = 33, UnitName = "mcg/kg"},
                new MockUnitDto {Id = 34, UnitName = "mcg/kg/hr"},
                new MockUnitDto {Id = 35, UnitName = "mcg/kg/min"},
                new MockUnitDto {Id = 36, UnitName = "mcg/m2"},
                new MockUnitDto {Id = 37, UnitName = "mcg/min"},
                new MockUnitDto {Id = 38, UnitName = "mEq"},
                new MockUnitDto {Id = 39, UnitName = "mEq/kg"},
                new MockUnitDto {Id = 40, UnitName = "mg"},
                new MockUnitDto {Id = 41, UnitName = "mg PE"},
                new MockUnitDto {Id = 42, UnitName = "mg/hr"},
                new MockUnitDto {Id = 43, UnitName = "mg/kg"},
                new MockUnitDto {Id = 44, UnitName = "mg/kg/hr"},
                new MockUnitDto {Id = 45, UnitName = "mg/m2"},
                new MockUnitDto {Id = 46, UnitName = "mg/min"},
                new MockUnitDto {Id = 47, UnitName = "million units"},
                new MockUnitDto {Id = 48, UnitName = "milliunit/min"},
                new MockUnitDto {Id = 49, UnitName = "mL"},
                new MockUnitDto {Id = 50, UnitName = "mL/hr"},
                new MockUnitDto {Id = 51, UnitName = "mL/kg"},
                new MockUnitDto {Id = 52, UnitName = "ml/m2"},
                new MockUnitDto {Id = 53, UnitName = "mL/min"},
                new MockUnitDto {Id = 54, UnitName = "mmol"},
                new MockUnitDto {Id = 55, UnitName = "mmol/kg"},
                new MockUnitDto {Id = 56, UnitName = "ng"},
                new MockUnitDto {Id = 57, UnitName = "packet"},
                new MockUnitDto {Id = 58, UnitName = "pad(s)"},
                new MockUnitDto {Id = 59, UnitName = "Patch"},
                new MockUnitDto {Id = 60, UnitName = "patch"},
                new MockUnitDto {Id = 61, UnitName = "puff(s)"},
                new MockUnitDto {Id = 62, UnitName = "spray(s)"},
                new MockUnitDto {Id = 63, UnitName = "SuperLongTablets"},
                new MockUnitDto {Id = 64, UnitName = "suppository"},
                new MockUnitDto {Id = 65, UnitName = "Syringe"},
                new MockUnitDto {Id = 66, UnitName = "Tablespoon(s)"},
                new MockUnitDto {Id = 67, UnitName = "tablet"},
                new MockUnitDto {Id = 68, UnitName = "tablet(s)"},
                new MockUnitDto {Id = 69, UnitName = "teaspoonful(s)"},
                new MockUnitDto {Id = 70, UnitName = "TEST UNIT 1"},
                new MockUnitDto {Id = 71, UnitName = "testing@#"},
                new MockUnitDto {Id = 72, UnitName = "Tree unit"},
                new MockUnitDto {Id = 73, UnitName = "troche(s)"},
                new MockUnitDto {Id = 74, UnitName = "tsp"},
                new MockUnitDto {Id = 75, UnitName = "tsp(s)"},
                new MockUnitDto {Id = 76, UnitName = "Tube"},
                new MockUnitDto {Id = 77, UnitName = "Unit(s)"},
                new MockUnitDto {Id = 78, UnitName = "unit(s)/min"},
                new MockUnitDto {Id = 79, UnitName = "units/hr"},
                new MockUnitDto {Id = 80, UnitName = "units/kg"},
                new MockUnitDto {Id = 81, UnitName = "units/m2"},
                new MockUnitDto {Id = 82, UnitName = "unt"},
                new MockUnitDto {Id = 83, UnitName = "vial(s)"}
            };
        }
    }
}
