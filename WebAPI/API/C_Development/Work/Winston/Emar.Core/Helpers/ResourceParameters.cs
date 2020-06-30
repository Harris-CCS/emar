namespace Emar.Core
{
    public class ResourceParameters
    {
        public short? Site { get; set; }        // PulseCheck exclusive
        public string Ibex { get; set; }        // PulseCheck exclusive
        public string DepartmentCode { get; set; }
        public long? PatientId { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public bool IncludePatient { get; set; } = true;
        public bool IncludeAdministrations { get; set; } = true;
        public bool IncludeAdministrationsEvents { get; set; } = true;
    }
}
