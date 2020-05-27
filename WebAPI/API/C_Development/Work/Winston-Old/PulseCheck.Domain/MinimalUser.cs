using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PulseCheck.IDomain;

namespace PulseCheck.Domain
{    
    public class MinimalUser : MinimalPerson, IMinimalUser
    {
        [Key]
        public int Id { get; set; }

        private string _initials { get; set; }
        [Column("init")]
        public string Initials
        {
            get { return this._initials != null ? this._initials.Trim() : ""; }
            set { this._initials = value?.Trim() ?? ""; }
        }

        //public string LoginId { get; set; }
        //public List<Identifier> Identifiers { get; set; }
        //public List<Patient> Patients { get; set; }

        public byte SiteId { get; set; }

        //public Site Site { get; set; }

        //Favorites
    }
}