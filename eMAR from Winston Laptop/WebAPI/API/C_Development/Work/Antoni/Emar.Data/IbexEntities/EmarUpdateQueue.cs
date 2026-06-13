using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("emar_update_queue")]
    public class EmarUpdateQueue
    {
        [Key]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("entity", TypeName = "varchar(50)"), Required]
        public string Entity { get; set; }

        [Column("external_id", TypeName = "varchar(50)"), Required]
        public string ExternalId { get; set; }

        [Column("event_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? EventDatetime { get; set; }

        [Column("inprocess_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? InprocessDatetime { get; set; }

        [Column("complete_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? CompleteDatetime { get; set; }
    }
}
