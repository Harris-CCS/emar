namespace Emar.Core.ResourceParameters
{
    public class PatientsResourceParameters : BaseResourceParameters
    {
        /// <summary>
        /// When the account number is supplied, then retrieve the patient with that account number
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// When the custom number is supplied, then retrieve the patient with that custom number
        /// </summary>
        public string CustomNumber { get; set; }
        /// <summary>
        /// When the person number is supplied, then retrieve the patient with that person number
        /// </summary>
        public string PersonNumber { get; set; }
        /// <summary>
        /// First key of the external patient Id. In PulseCheck, it is the Site id.
        /// </summary>
        public string ExtId1 { get; set; }
        /// <summary>
        /// Second key of the external patient Id. In PulseCheck, it is the Ibex number.
        /// </summary>
        public string ExtId2 { get; set; }
        /// <summary>
        /// Site (facility) identifier to restrict the list of returned patients to.
        /// </summary>
        public int? SiteId { get; set; }
        /// <summary>
        /// Department code to restrict the list of returned patients to.
        /// </summary>
        public string DepartmentCode { get; set; }
        /// <summary>
        /// Comma delimited of ward (area) codes to restrict the list of returned patients to.
        /// </summary>
        public string WardCodes { get; set; }
        /// <summary>
        /// Room and bed code to restrict the list of returned patients to.
        /// </summary>
        public string RoomBedCode { get; set; }
        /// <summary>
        /// Include the inactive patients in the list of returned patients.
        /// </summary>
        public bool IncludeInactive { get; set; } = false;
        /// <summary>
        /// Include the patients orders in the list of returned patients.
        /// </summary>
        public bool IncludeOrders { get; set; } = true;

        public bool AskingForLegacyPulseCheckPatient()
        {
            return ExtId1 != null && !string.IsNullOrWhiteSpace(ExtId2);
        }

        public bool AskingForPatientByAccountNumber()
        {
            return !string.IsNullOrWhiteSpace(AccountNumber);
        }

        public bool AskingForPatientByCustomNumber()
        {
            return !string.IsNullOrWhiteSpace(CustomNumber);
        }

        public bool AskingForPatientByPersonNumber()
        {
            return !string.IsNullOrWhiteSpace(PersonNumber);
        }
    }
}
