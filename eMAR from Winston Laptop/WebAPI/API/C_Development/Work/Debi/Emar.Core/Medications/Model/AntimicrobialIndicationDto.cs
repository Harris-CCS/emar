using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Medications.Model
{
    public class AntimicrobialIndicationDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int OrdinalPosition { get; set; }
    }
}
