namespace Emar.Core.Patients.Model
{
    public class PatientDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Suffix { get; set; }

        public string FullName
        {
            get
            {
                var ret = (FirstName ?? "") +
                    (MiddleName ?? "") +
                    (LastName ?? "");
                ret += ((!string.IsNullOrWhiteSpace(ret) && !string.IsNullOrWhiteSpace(Suffix)) ? ", " : "") +
                    (Suffix ?? "");
                return ret;
            }
        }
    }
}
