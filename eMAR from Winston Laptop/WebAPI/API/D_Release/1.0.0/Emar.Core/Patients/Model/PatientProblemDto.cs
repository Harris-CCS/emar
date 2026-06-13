namespace Emar.Core.Patients.Model
{
    public class PatientProblemDto
    {
        public long Id { get; set; }
        public long PatientId { get; set; }
        public string CodeSetName { get; set; }
        public string CodeSetValue { get; set; }
        public string ProblemName { get; set; }
        public string DiagnosisType { get; set; }
    }
}