namespace Emar.Core.Patients.Model
{
    /// <summary>
    /// Constants used in the Patients domain model
    /// </summary>
    public static class Constants
    {
        #region Patient Constants

        /// <summary>
        /// Get patient by Account Number, Custom Number or Person Number
        /// </summary>
        public enum GetPatientBy
        {
            None,
            Id,
            MedicalRecordNumber,
            AccountNumber,
            CustomNumber,
            PersonNumber
        }

        #endregion
    }
}
