using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseCheck.Domain
{
    public class Bed
    {
        public int Id { get; set; }

        [Column("bed")]
        public string Name { get; set; }

        public string Dept { get; set; }
        public string Ward { get; set; }
        public byte SiteId { get; set; }

        [NotMapped]
        public List<Patient> Patient { get; set; }
    }
}