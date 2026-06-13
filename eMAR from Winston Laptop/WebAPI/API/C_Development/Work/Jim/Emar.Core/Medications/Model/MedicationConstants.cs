namespace Emar.Core.Medications.Model
{
    public static class SourceTables
    {
        public const string DepartmentPreferredListItems = @"department_preferred_list_items";
        public const string GroupListItems = @"group_list_items";
        public const string PatientCartOrders = @"patient_cart_orders";
        public const string PatientHomeMedications = @"patient_home_medications";
        public const string PatientOrders = @"patient_orders";
        public const string UserQuickListItems = @"user_quick_list_items";
        public const string PatientAllergies = @"patient_allergies";
        public const string Medications = @"medications";
    }

    public static class DrugDbVendors
    {
        public const string FDB = @"F";
        public const string Multum = @"M";
        public const string FDBCa = @"1";
        public const string MediSpan = @"2";
    }

    // Interaction severity text for Multum
    public enum MultumInteractionSeverity
    {
        MINOR = 1,
        MODERATE = 2,
        SEVERE = 3,
        ALLERGY = 4
    }

    // Interaction severity text from FDB (First Data Bank)
    public enum FdbInteractionSeverity
    {
        UNDETERMINED = 5,
        MODERATE = 6,
        SEVERE = 7,
        CONTRAINDICATED = 8
    }

    public enum EmarOrderType
    {
        UserQuickListItem,
        DepartmentPreferredListItem,
        GroupRememberedOrder,
        PatientCartOrder,
        PatientOrder,
        HomeMedication,
        SchedulerSearch,
        MedicationItem,
        PatientAllergy,
        FormularyItem,
        All
    }
}