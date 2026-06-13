using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseCheck.Domain
{
    public class Department
    {
        public Department()
        {
            _name = "";
            Status = new Status();
            Areas = new List<Area>();
            Patients = new List<Patient>();
        }

        [Key, Column(Order = 0)]
        public string Dept { get; set; }

        [Key, Column(Order = 1), ForeignKey("Site")]
        public byte SiteId { get; set; }

        private string _name;
        public string Name
        {
            get { return this._name.Trim(); }
            set { this._name = value.Trim(); }
        }

        public Status Status { get; set; }

        [NotMapped]
        public ICollection<Patient> Patients { get; set; }

        [NotMapped]
        public ICollection<Area> Areas { get; set; }

        public virtual Site Site { get; set; }
    }
}