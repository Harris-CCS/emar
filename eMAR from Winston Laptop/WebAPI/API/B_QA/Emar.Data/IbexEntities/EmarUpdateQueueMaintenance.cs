using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    public class EmarUpdateQueueMaintenance
    {
        //This entity is the columns returned by the call to the emar_update_queue_maintenance SP.
        //We never run an insert or update on this.
        //Winston Murdock.  12/07/2021.

        [Key]
        [Column("id", TypeName = "bigint")]
        public long MaxId { get; set; }

        [Column("entity", TypeName = "varchar(50)"), Required]
        public string Entity { get; set; }

        [Column("external_id", TypeName = "varchar(50)"), Required]
        public string ExternalId { get; set; }

        //The id of the record we are processing from the emar_update_queue table.
        //I also had to add him to the emar_update_queue_maintenance SP
        //and NextQueueRecordToProcessDto.
        //Winston Murdock, 12/07/2021.  PC-26824
        [Column("queue_record_id", TypeName = "varchar(50)"), Required]
        public string QueueRecordId { get; set; }
    }
}
