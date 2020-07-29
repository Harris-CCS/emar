using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;

namespace Emar.Core.Orders.Service
{
    public partial class OrderService : IOrderService
    {
        //private readonly IOrderRepository _orderRepository;

        //public OrderService(IOrderRepository orderRepository)
        //{
        //    _orderRepository = orderRepository;
        //}



        public ComposerOptionsDto GetComposerSetupData(string brandName)
        {
            // Temporary for showing the shape
            var setupOptions = new ComposerOptionsDto
            {
                BrandName = "Ondansetron Hydrochloride", 
                AvailableFormStrength = new []
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
                            new PreferredDoseDto {DoseName = "4 mg", Dose = new decimal(4.0), DoseUnit = new UnitDto{Id = 40, UnitName = "mg"}},
                            new PreferredDoseDto {DoseName = "8 mg", Dose = new decimal(8.0), DoseUnit = new UnitDto{Id = 40, UnitName = "mg"}}
                        },
                        PreferredRoutes = new[] {new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1}},
                        PreferredFrequencies = new[]
                        {
                            new FrequencyDto {FrequencyName = "2 TIMES DAILY", Id = 7},
                            new FrequencyDto {FrequencyName = "Every 6 HOURS", Id = 5},
                            new FrequencyDto {FrequencyName = "ONCE", Id = 1}
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
                            new PreferredDoseDto {DoseName = "8 mg", Dose = new decimal(8.0), DoseUnit = new UnitDto{Id = 40, UnitName = "mg"}},
                            new PreferredDoseDto {DoseName = "16 mg", Dose = new decimal(16.0), DoseUnit = new UnitDto{Id = 40, UnitName = "mg"}}
                        }, 
                        PreferredRoutes = new[] { new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1} },
                        PreferredFrequencies = new[]
                        {
                            new FrequencyDto {FrequencyName = "2 TIMES DAILY", Id = 7},
                            new FrequencyDto {FrequencyName = "Every 6 HOURS", Id = 5},
                            new FrequencyDto {FrequencyName = "ONCE", Id = 1}
                        }
                    }
                }
            };
            
            return setupOptions;
        }

        public IEnumerable<FrequencyDto> GetFrequencies(int siteId)
        {
            return new[]
            {
                new FrequencyDto {Id = 1, FrequencyName = "ONCE"},
                new FrequencyDto {Id = 2, FrequencyName = "Every 2 HOURS"},
                new FrequencyDto {Id = 3, FrequencyName = "Every 3 HOURS"},
                new FrequencyDto {Id = 4, FrequencyName = "Every 4 HOURS"},
                new FrequencyDto {Id = 5, FrequencyName = "Every 6 HOURS"},
                new FrequencyDto {Id = 6, FrequencyName = "Every 12 HOURS"},
                new FrequencyDto {Id = 7, FrequencyName = "2 TIMES DAILY"},
                new FrequencyDto {Id = 8, FrequencyName = "3 TIMES DAILY"},
                new FrequencyDto {Id = 9, FrequencyName = "4 TIMES DAILY"},
                new FrequencyDto {Id = 10, FrequencyName = "6 TIMES DAILY"},
                new FrequencyDto {Id = 11, FrequencyName = "8 TIMES DAILY"},
                new FrequencyDto {Id = 12, FrequencyName = "BEFORE MEALS"},
                new FrequencyDto {Id = 13, FrequencyName = "AFTER MEALS"},
                new FrequencyDto {Id = 14, FrequencyName = "WITH EACH MEAL"},
                new FrequencyDto {Id = 15, FrequencyName = "BEFORE BED"},
                new FrequencyDto {Id = 16, FrequencyName = "AFTER WAKING"}
            };
        }

        public IEnumerable<UnitDto> GetUnits(in int siteId)
        {
            return new[]
            {
                new UnitDto {Id = 1, UnitName = "app"},
                new UnitDto {Id = 2, UnitName = "appful"},
                new UnitDto {Id = 3, UnitName = "application"},
                new UnitDto {Id = 4, UnitName = "applicator"},
                new UnitDto {Id = 5, UnitName = "apply"},
                new UnitDto {Id = 6, UnitName = "Bottle"},
                new UnitDto {Id = 7, UnitName = "cap(s)"},
                new UnitDto {Id = 8, UnitName = "desktest#"},
                new UnitDto {Id = 9, UnitName = "Diskus"},
                new UnitDto {Id = 10, UnitName = "drop"},
                new UnitDto {Id = 11, UnitName = "ea"},
                new UnitDto {Id = 12, UnitName = "enema"},
                new UnitDto {Id = 13, UnitName = "Film"},
                new UnitDto {Id = 14, UnitName = "g"},
                new UnitDto {Id = 15, UnitName = "g/kg"},
                new UnitDto {Id = 16, UnitName = "gm"},
                new UnitDto {Id = 17, UnitName = "gm/hr"},
                new UnitDto {Id = 18, UnitName = "GRAN(S)"},
                new UnitDto {Id = 19, UnitName = "gtt"},
                new UnitDto {Id = 20, UnitName = "in"},
                new UnitDto {Id = 21, UnitName = "INH"},
                new UnitDto {Id = 22, UnitName = "Injection"},
                new UnitDto {Id = 23, UnitName = "intl units"},
                new UnitDto {Id = 24, UnitName = "intl units/kg"},
                new UnitDto {Id = 25, UnitName = "intl units/m2"},
                new UnitDto {Id = 26, UnitName = "IUPSQ"},
                new UnitDto {Id = 27, UnitName = "L"},
                new UnitDto {Id = 28, UnitName = "lbs"},
                new UnitDto {Id = 29, UnitName = "LOW HeparinProtocolBolus60Units/kg"},
                new UnitDto {Id = 30, UnitName = "loz"},
                new UnitDto {Id = 31, UnitName = "mcg"},
                new UnitDto {Id = 32, UnitName = "mcg/hr"},
                new UnitDto {Id = 33, UnitName = "mcg/kg"},
                new UnitDto {Id = 34, UnitName = "mcg/kg/hr"},
                new UnitDto {Id = 35, UnitName = "mcg/kg/min"},
                new UnitDto {Id = 36, UnitName = "mcg/m2"},
                new UnitDto {Id = 37, UnitName = "mcg/min"},
                new UnitDto {Id = 38, UnitName = "mEq"},
                new UnitDto {Id = 39, UnitName = "mEq/kg"},
                new UnitDto {Id = 40, UnitName = "mg"},
                new UnitDto {Id = 41, UnitName = "mg PE"},
                new UnitDto {Id = 42, UnitName = "mg/hr"},
                new UnitDto {Id = 43, UnitName = "mg/kg"},
                new UnitDto {Id = 44, UnitName = "mg/kg/hr"},
                new UnitDto {Id = 45, UnitName = "mg/m2"},
                new UnitDto {Id = 46, UnitName = "mg/min"},
                new UnitDto {Id = 47, UnitName = "million units"},
                new UnitDto {Id = 48, UnitName = "milliunit/min"},
                new UnitDto {Id = 49, UnitName = "mL"},
                new UnitDto {Id = 50, UnitName = "mL/hr"},
                new UnitDto {Id = 51, UnitName = "mL/kg"},
                new UnitDto {Id = 52, UnitName = "ml/m2"},
                new UnitDto {Id = 53, UnitName = "mL/min"},
                new UnitDto {Id = 54, UnitName = "mmol"},
                new UnitDto {Id = 55, UnitName = "mmol/kg"},
                new UnitDto {Id = 56, UnitName = "ng"},
                new UnitDto {Id = 57, UnitName = "packet"},
                new UnitDto {Id = 58, UnitName = "pad(s)"},
                new UnitDto {Id = 59, UnitName = "Patch"},
                new UnitDto {Id = 60, UnitName = "patch"},
                new UnitDto {Id = 61, UnitName = "puff(s)"},
                new UnitDto {Id = 62, UnitName = "spray(s)"},
                new UnitDto {Id = 63, UnitName = "SuperLongTablets"},
                new UnitDto {Id = 64, UnitName = "suppository"},
                new UnitDto {Id = 65, UnitName = "Syringe"},
                new UnitDto {Id = 66, UnitName = "Tablespoon(s)"},
                new UnitDto {Id = 67, UnitName = "tablet"},
                new UnitDto {Id = 68, UnitName = "tablet(s)"},
                new UnitDto {Id = 69, UnitName = "teaspoonful(s)"},
                new UnitDto {Id = 70, UnitName = "TEST UNIT 1"},
                new UnitDto {Id = 71, UnitName = "testing@#"},
                new UnitDto {Id = 72, UnitName = "Tree unit"},
                new UnitDto {Id = 73, UnitName = "troche(s)"},
                new UnitDto {Id = 74, UnitName = "tsp"},
                new UnitDto {Id = 75, UnitName = "tsp(s)"},
                new UnitDto {Id = 76, UnitName = "Tube"},
                new UnitDto {Id = 77, UnitName = "Unit(s)"},
                new UnitDto {Id = 78, UnitName = "unit(s)/min"},
                new UnitDto {Id = 79, UnitName = "units/hr"},
                new UnitDto {Id = 80, UnitName = "units/kg"},
                new UnitDto {Id = 81, UnitName = "units/m2"},
                new UnitDto {Id = 82, UnitName = "unt"},
                new UnitDto {Id = 83, UnitName = "vial(s)"}
            };
        }
    }
}
