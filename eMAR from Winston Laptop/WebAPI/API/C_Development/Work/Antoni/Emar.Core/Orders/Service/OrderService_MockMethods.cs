using System.Collections.Generic;
using Emar.Core.Orders.Model;

namespace Emar.Core.Orders.Service
{
    public partial class OrderService_MockMethods
    {
        public SchedulerOptionsDto GetSchedulerSetupData(string brandName)
        {
            // Temporary for showing the shape
            var setupOptions = new SchedulerOptionsDto
            {
                BrandName = "Ondansetron Hydrochloride",
                AvailableFormStrength = new List<FormStrengthDto>()
                {
                    new FormStrengthDto
                    {
                        MedicationId = 1095,
                        FormStrengthName = "4mg orally disintegrating",
                        PreferredDoses = new List<PreferredDoseDto>
                        {
                            //new PreferredDoseDto {DoseName = "4 mg", Dose = new decimal(4.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}},
                            //new PreferredDoseDto {DoseName = "8 mg", Dose = new decimal(8.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}}
                        },
                        PreferredRoutes = new List<MedicationRouteDto>() {new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1}},
                        PreferredFrequencies = new List<FrequencyScheduleDto>()
                        {
                            //new MockFrequencyDto {FrequencyName = "2 TIMES DAILY", Id = 7},
                            //new MockFrequencyDto {FrequencyName = "Every 6 HOURS", Id = 5},
                            //new MockFrequencyDto {FrequencyName = "ONCE", Id = 1}
                        }
                    },
                    new FormStrengthDto
                    {
                        MedicationId = 1099,
                        FormStrengthName = "8mg orally disintegrating",
                        PreferredDoses = new List<PreferredDoseDto>
                        {
                            //new PreferredDoseDto {DoseName = "8 mg", Dose = new decimal(8.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}},
                            //new PreferredDoseDto {DoseName = "16 mg", Dose = new decimal(16.0), DoseUnit = new MockUnitDto{Id = 40, UnitName = "mg"}}
                        },
                        PreferredRoutes = new List<MedicationRouteDto>() {new MedicationRouteDto {Id = 5, RouteName = "sublingual", SiteId = -1}},
                        PreferredFrequencies = new List<FrequencyScheduleDto>()
                        {
                            //new MockFrequencyDto {FrequencyName = "2 TIMES DAILY", Id = 7},
                            //new MockFrequencyDto {FrequencyName = "Every 6 HOURS", Id = 5},
                            //new MockFrequencyDto {FrequencyName = "ONCE", Id = 1}
                        }
                    }
                }
            };

            return setupOptions;
        }
    }
}
