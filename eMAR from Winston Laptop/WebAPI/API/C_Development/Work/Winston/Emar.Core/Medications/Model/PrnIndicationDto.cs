using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Medications.Model
{
    public class PrnIndicationDto
    {
        //Since we're just returning a list of strings right now, we probably don't need this Dto.
        //But I'll keep it here just in case we ever do need it.
        public int Id { get; set; }
        //public int SiteId { get; set; }

        public string OptionDescription { get; set; }
    }
}
