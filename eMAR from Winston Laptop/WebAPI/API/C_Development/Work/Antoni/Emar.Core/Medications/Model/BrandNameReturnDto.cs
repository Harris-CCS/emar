using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Medications.Model
{
    //This will be used by the vendor-specific methods to return the medication name
    //and the match levels.  The service will pass this into the mapper method and
    //will return a BrandNameSearchDto (which has all of these fields and the links.
    //Winston Murdock, 11/15/2020.
    public class BrandNameReturnDto
    {
        public string BrandName { get; set; }

        public byte? InpatientMatch { get; set; }

        public byte? OutpatientMatch { get; set; }

        public byte? PyxisMatch { get; set; }
    } //end class BrandNameReturnDTO
}
