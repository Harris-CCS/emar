using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    //We are not mapped to a DB table here.
    [NotMapped]

    public class StringList
    {
        //This guy is just a place to hold strings from the DB when we need to call FromSqlInterpolated
        //and return a string value (or list of values).
        //Winston Murdock, 03/13/2022.

        [Key]
        [Column("value")]
        public string Value { get; set; }
    }
}
