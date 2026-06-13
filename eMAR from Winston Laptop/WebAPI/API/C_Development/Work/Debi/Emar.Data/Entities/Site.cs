using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("sites")]
    public class Site
    {
        public Site()
        {
            // For Foreign Key: fk__action_route_templates__sites
            ActionRouteTemplates = new HashSet<ActionRouteTemplate>();
            // For Foreign Key: fk__antimicrobial_indications__sites
            AntimicrobialIndications = new HashSet<AntimicrobialIndication>();
            // For Foreign Key: fk__antimicrobial_indication_items__sites
            AntimicrobialIndicationItems = new HashSet<AntimicrobialIndicationItem>();
            // For Foreign Key: FK_get_code_share_site_view__frequency_schedules_sites_site_id
            CodeShareSiteFrequencySchedules = new HashSet<GetCodeShareSiteViewFrequencySchedule>();
            // For Foreign Key: FK_get_code_share_site_view__medication_routes_sites_site_id
            CodeShareSiteMedicationRoutes = new HashSet<GetCodeShareSiteViewMedicationRoute>();
            // For Foreign Key: FK_get_code_share_site_view__medication_units_sites_site_id
            CodeShareSiteMedicationUnits = new HashSet<GetCodeShareSiteViewMedicationUnit>();
            // For Foreign Key: FK_get_code_share_site_view__order_instructions_sites_site_id
            CodeShareSiteOrderInstructions = new HashSet<GetCodeShareSiteViewOrderInstruction>();
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            //For Foreign Key: fk__devices__sites
            Devices = new HashSet<Device>();
            // For Foreign Key: fk__frequency_schedules__sites
            FrequencySchedules = new HashSet<FrequencySchedule>();
            GroupListItems = new HashSet<GroupListItem>();
            MedicationRoutes = new HashSet<MedicationRoute>();
            //For Foreign Key: fk__medications__sites
            Medications = new HashSet<Medication>();
            MedicationUnits = new HashSet<MedicationUnit>();
            // For Foreign Key: fk__order_administration_available_actions__sites
            OrderAdministrationAvailableActions = new HashSet<OrderAdministrationAvailableAction>();
            // For Foreign Key: fk__order_available_actions__sites
            OrderAvailableActions = new HashSet<OrderAvailableAction>();
            // For Foreign Key: fk__order_instructions__sites
            OrderInstructions = new HashSet<OrderInstruction>();
            OverrideReasons = new HashSet<OverrideReason>();
            Patients = new HashSet<Patient>();
            // For Foreign Key: fk__preferred_frequency_schedules__sites
            PreferredFrequencySchedules = new HashSet<PreferredFrequencySchedule>();
            // For Foreign Key: fk__preferred_medication_doses__sites
            PreferredMedicationDoses = new HashSet<PreferredMedicationDose>();
            // For Foreign Key: fk__preferred_medication_routes__sites
            PreferredMedicationRoutes = new HashSet<PreferredMedicationRoute>();
            //For Foreign Key: fk__prn_indications__sites
            PrnIndications = new HashSet<PrnIndication>();
            // For Foreign Key: fk__site_formulary__sites
            SiteFormularys = new HashSet<SiteFormulary>();
            // For Foreign Key: fk__site_formulary_match__sites
            SiteFormularyMatchs = new HashSet<SiteFormularyMatch>();
            SiteOptions = new HashSet<SiteOption>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
            Users = new HashSet<User>();
            // For Foreign Key: fk__user_settings__sites
            UserSettings = new HashSet<UserSetting>();
        }

        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("name", TypeName = "nvarchar(60)"), Required]
        public string Name { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("time_zone_name", TypeName = "nvarchar(128)"), Required]
        public string TimeZoneName { get; set; }

        [Column("time_zone_offset", TypeName = "varchar(6)")]
        public string TimeZoneOffset { get; set; }


        // For Foreign Key: fk__action_route_templates__sites
        [InverseProperty("Site")]
        public virtual ICollection<ActionRouteTemplate> ActionRouteTemplates { get; set; }

        // For Foreign Key: fk__antimicrobial_indications__sites
        [InverseProperty("Site")]
        public virtual ICollection<AntimicrobialIndication> AntimicrobialIndications { get; set; }

        // For Foreign Key: fk__antimicrobial_indication_items__sites
        [InverseProperty("Site")]
        public virtual ICollection<AntimicrobialIndicationItem> AntimicrobialIndicationItems { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        // For Foreign Key: fk__frequency_schedules__sites
        [InverseProperty("Site")]
        public virtual ICollection<FrequencySchedule> FrequencySchedules { get; set; }

        // For Foreign Key: FK_get_code_share_site_view__frequency_schedules_sites_site_id
        [InverseProperty("Site")]
        public virtual ICollection<GetCodeShareSiteViewFrequencySchedule> CodeShareSiteFrequencySchedules { get; set; }

        // For Foreign Key: FK_get_code_share_site_view__medication_routes_sites_site_id
        [InverseProperty("Site")]
        public virtual ICollection<GetCodeShareSiteViewMedicationRoute> CodeShareSiteMedicationRoutes { get; set; }

        // For Foreign Key: FK_get_code_share_site_view__medication_units_sites_site_id
        [InverseProperty("Site")]
        public virtual ICollection<GetCodeShareSiteViewMedicationUnit> CodeShareSiteMedicationUnits { get; set; }

        // For Foreign Key: FK_get_code_share_site_view__order_instructions_sites_site_id
        [InverseProperty("Site")]
        public virtual ICollection<GetCodeShareSiteViewOrderInstruction> CodeShareSiteOrderInstructions { get; set; }

        //For Foreign Key: fk__devices__sites
        [InverseProperty("Site")]
        public virtual ICollection<Device> Devices { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        // For Foreign Key: fk__medications__sites
        [InverseProperty("Site")]
        public virtual ICollection<Medication> Medications { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<MedicationRoute> MedicationRoutes { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<MedicationUnit> MedicationUnits { get; set; }

        // For Foreign Key: fk__order_administration_available_actions__sites
        [InverseProperty("Site")]
        public virtual ICollection<OrderAdministrationAvailableAction> OrderAdministrationAvailableActions { get; set; }

        // For Foreign Key: fk__order_available_actions__sites
        [InverseProperty("Site")]
        public virtual ICollection<OrderAvailableAction> OrderAvailableActions { get; set; }

        // For Foreign Key: fk__order_instructions__sites
        [InverseProperty("Site")]
        public virtual ICollection<OrderInstruction> OrderInstructions { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<OverrideReason> OverrideReasons { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<Patient> Patients { get; set; }

        // For Foreign Key: fk__preferred_frequency_schedules__sites
        [InverseProperty("Site")]
        public virtual ICollection<PreferredFrequencySchedule> PreferredFrequencySchedules { get; set; }

        // For Foreign Key: fk__preferred_medication_doses__sites
        [InverseProperty("Site")]
        public virtual ICollection<PreferredMedicationDose> PreferredMedicationDoses { get; set; }

        // For Foreign Key: fk__preferred_medication_routes__sites
        [InverseProperty("Site")]
        public virtual ICollection<PreferredMedicationRoute> PreferredMedicationRoutes { get; set; }

        //For Foreign Key: fk__prn_indications__sites
        [InverseProperty("Site")]
        public virtual ICollection<PrnIndication> PrnIndications { get; set; }

        // For Foreign Key: fk__site_formulary__sites
        [InverseProperty("Site")]
        public virtual ICollection<SiteFormulary> SiteFormularys { get; set; }

        // For Foreign Key: fk__site_formulary_match__sites
        [InverseProperty("Site")]
        public virtual ICollection<SiteFormularyMatch> SiteFormularyMatchs { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<SiteOption> SiteOptions { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<User> Users { get; set; }

        // For Foreign Key: fk__user_settings__sites
        [InverseProperty("Site")]
        public virtual ICollection<UserSetting> UserSettings { get; set; }
    }
}