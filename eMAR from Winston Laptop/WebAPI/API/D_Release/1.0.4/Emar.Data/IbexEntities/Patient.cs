using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("pat")]
    public class IbexPatient
    {
        public IbexPatient()
        {

        }

        [Column("pat_id", TypeName = "bigint"), Key] public int PatId { get; set; }
        [Column("ibex", TypeName = "char(14)")] public string Ibex { get; set; }
        [Column("age", TypeName = "tinyint")] public int Age { get; set; }
        [Column("ageunits", TypeName = "char(1)")] public string AgeUnits { get; set; }
        [Column("site", TypeName = "tinyint")] public int Site { get; set; }
        [Column("ord30", TypeName = "char(1)")] public string Ord30 { get; set; }
        [Column("ord30_alt", TypeName = "char(1)")] public string Ord30Alternate { get; set; }
        [Column("ord30_dt", TypeName = "datetimeoffset(0)")] public DateTimeOffset? Ord30DateTime {get; set;}
        [Column("vsuser", TypeName = "int")] public int VSUser { get; set; }
        [Column("vsdate", TypeName = "char(12)")] public string VSDate { get; set; }
        [Column("vssys", TypeName = "char(14)")] public string VSSys { get; set; }
        [Column("vsdia", TypeName = "char(14)")] public string VSDia { get; set; }
        [Column("vspulse", TypeName = "char(14)")] public string VSPulse { get; set; }
        [Column("vsresp", TypeName = "char(14)")] public string VSResp { get; set; }
        [Column("vstemp", TypeName = "char(14)")] public string VSTemp { get; set; }
        [Column("vspain", TypeName = "char(14)")] public string VSPain { get; set; }
        [Column("vso2", TypeName = "varchar(50)")] public string VSO2 { get; set; }
        [Column("vsmap", TypeName = "varchar(14)")] public string VSMap { get; set; }
        [Column("vsendtidal", TypeName = "varchar(14)")] public string VSEndTidal { get; set; }
        [Column("vsmaplevel", TypeName = "char(1)")] public string VSMapLevel { get; set; }
        [Column("vsendtidallevel", TypeName = "char(1)")] public string VSEndTidalLevel { get; set; }
        [Column("ord11", TypeName = "char(1)")] public string Ord11 { get; set; }
        [Column("ord12", TypeName = "char(1)")] public string Ord12 { get; set; }
        [Column("ord13", TypeName = "char(1)")] public string Ord13 { get; set; }
        [Column("ord14", TypeName = "char(1)")] public string Ord14 { get; set; }
        [Column("ord15", TypeName = "char(1)")] public string Ord15 { get; set; }
        [Column("ord23", TypeName = "char(1)")] public string Ord23 { get; set; }
        [Column("ord57", TypeName = "char(1)")] public string Ord57 { get; set; }
    }
}
