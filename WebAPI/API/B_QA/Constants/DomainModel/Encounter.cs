using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    public class Encounter
    {
        [Key]
        public string Ibex { get; set; }
        public DateTime? Date { get; set; }
        public Object Site { get; set; }
        public Complaint Complaint { get; set; }
        public string Diagnosis { get; set; }
        public List<Comment> Comments { get; set; }
        public Chart Chart { get; set; }
        public Disposition DispoCode { get; set; }
        public Disposition DispoLocation { get; set; }
        public List<Object> Providers { get; set; }
    }
}