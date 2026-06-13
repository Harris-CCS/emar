using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("devices")]
    public class Device
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        //[Column("address", TypeName = "nvarchar(50)")]
        [Column("address")]
        [StringLength(50)]
        public string Address { get; set; }

        //[Column("description", TypeName = "nvarchar(50)")]
        [Column("description")]
        [StringLength(50)]
        public string Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("print_queue_name")]
        [StringLength(80)]
        public string PrintQueueName { get; set; }

        [Column("tray")]
        [StringLength(1)]
        public string Tray { get; set; }

        [Column("device_type")]
        [StringLength(1)]
        public string DeviceType { get; set; }

        [Column("pcl_type")]
        [StringLength(1)]
        public string PclType { get; set; }
        
        //For Foreign Key: fk__devices__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.Devices))]
        public virtual Site Site { get; set; }
    }
}
