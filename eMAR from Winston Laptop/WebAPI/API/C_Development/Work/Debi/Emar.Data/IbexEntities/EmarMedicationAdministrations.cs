using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{

    [Table("emar_med_administrations")]
    public class EmarMedicationAdministrations
    {
        public EmarMedicationAdministrations()
        {

        }

        [Column("id", TypeName = "int"), Key] public int Id { get; set; }

        [Column("ibex", TypeName = "char(14)")] public string Ibex { get; set; }

        [Column("site", TypeName = "smallint")] public int Site { get; set; }

        [Column("losecs", TypeName = "int")] public int Losecs { get; set; }

        [Column("med_admin_type", TypeName = "varchar(50)")] public string MedAdminType { get; set; }

        [Column("med_admin_user", TypeName = "int")] public int MedAdminUser { get; set; }

        [Column("med_admin_date", TypeName = "char(14)")] public string MedAdminDate { get; set; }

        [Column("med_admin_sysdate", TypeName = "char(14)")] public string MedAdminSysdate { get; set; }

        [Column("stop_user", TypeName = "int")] public int StopUser { get; set; }

        [Column("stop_date", TypeName = "char(12)")] public string StopDate { get; set; }

        [Column("stop_sysdate", TypeName = "char(14)")] public string StopSysdate { get; set; }

        [Column("iv_site", TypeName = "int")] public int? IvSite { get; set; }

        [Column("iv_location", TypeName = "varchar(255)")] public string IvLocation { get; set; }

        [Column("iv_type", TypeName = "varchar(25)")] public string IvType { get; set; }

        [Column("iv_edit", TypeName = "char(1)")] public string IvEdit { get; set; }

        [Column("patient_order_id", TypeName = "bigint")] public long PatientOrderId { get; set; }

        [Column("order_administrations_id", TypeName = "bigint")] public long OrderAdministrationsId { get; set; }
    }
}
