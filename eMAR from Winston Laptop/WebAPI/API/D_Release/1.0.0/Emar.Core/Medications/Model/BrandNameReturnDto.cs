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

        //We need to sort the return list by the match level.
        //Winston Murdock, 01/22/2021.EMAR-586.
        public byte MatchLevel { get; set; }

        //Whether this medication was found via matching on the brand name
        //or it was found in the ingredient list.
        //If true, then brand name search.
        //If false, then ingredients list.
        //Winston Murdock, 02/01/2021.
        public bool IsBrandNameMatch { get; set; }

        //The position within the brand name (or ingredient list) that the match starts at.
        //If we searched for "Tylenol", then "Tylenol" would have a value of 1 for this.
        //And "Children's Tylenol" would have a value of 12.
        //We need to sort the return by this field.
        //Winston Murdock, 02/01/2021.
        public int SearchPos { get; set; }
    } //end class BrandNameReturnDTO
}
