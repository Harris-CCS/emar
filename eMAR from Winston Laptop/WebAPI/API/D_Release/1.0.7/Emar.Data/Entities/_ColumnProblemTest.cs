using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("column_problem_tests")]
    public class _ColumnProblemTest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("title")]
        [StringLength(20)]
        public string Title { get; set; }
        
        [Required]
        [Column("description", TypeName = "varchar(90)")]
        [StringLength(100)]
        public string Description { get; set; }
        
        [Column("site_id")]
        public int SiteId { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("decimal_to_int", TypeName = "decimal")]
        public int DecimalToInt { get; set; }


        [Column("decimal_not_required", TypeName = "decimal")]
        public decimal DecimalNotRequired { get; set; }

        [Column("decimal_required", TypeName = "decimal(11,2)"), Required]
        public decimal DecimalRequired { get; set; }

        [Column("decimal_nullable_not_required", TypeName = "decimal(11,2)")]
        public decimal? DecimalNullableNotRequired { get; set; }

        [Column("decimal_nullable_required", TypeName = "decimal(11,2)"), Required]
        public decimal? DecimalNullableRequired { get; set; }


        [Column("numeric_not_required", TypeName = "decimal(11,2)")]
        public decimal NumericNotRequired { get; set; }

        [Column("numeric_required", TypeName = "numeric(11,2)"), Required]
        public decimal NumericRequired { get; set; }

        [Column("numeric_nullable_not_required", TypeName = "numeric(11,2)")]
        public decimal? NumericNullableNotRequired { get; set; }

        [Column("numeric_nullable_required", TypeName = "numeric(11,2)"), Required]
        public decimal? NumericNullableRequired { get; set; }


        [Column("bit_not_required", TypeName = "bit")]
        public bool BitNotRequired { get; set; }

        [Column("bit_required", TypeName = "bit"), Required]
        public bool BitRequired { get; set; }

        [Column("bit_nullable_not_required", TypeName = "bit")]
        public bool? BitNullableNotRequired { get; set; }

        [Column("bit_nullable_required", TypeName = "bit"), Required]
        public bool? BitNullableRequired { get; set; }


        [Column("datetimeoffset_not_required", TypeName = "datetimeoffset")]
        public DateTimeOffset DateTimeOffsetNotRequired { get; set; }

        [Column("datetimeoffset_required", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset DateTimeOffsetRequired { get; set; }

        [Column("datetimeoffset_nullable_not_required", TypeName = "datetimeoffset")]
        public DateTimeOffset? DateTimeOffsetNullableNotRequired { get; set; }

        [Column("datetimeoffset_nullable_required", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset? DateTimeOffsetNullableRequired { get; set; }


        [Column("date_not_required", TypeName = "date")]
        public DateTime DateNotRequired { get; set; }

        [Column("date_required", TypeName = "date"), Required]
        public DateTime DateRequired { get; set; }

        [Column("date_nullable_not_required", TypeName = "date")]
        public DateTime? DateNullableNotRequired { get; set; }

        [Column("date_nullable_required", TypeName = "date"), Required]
        public DateTime? DateNullableRequired { get; set; }


        [Column("bigint_not_required", TypeName = "bigint")]
        public long BigIntNotRequired { get; set; }

        [Column("bigint_required", TypeName = "bigint"), Required]
        public long BigIntRequired { get; set; }

        [Column("bigint_nullable_not_required", TypeName = "bigint")]
        public long? BigIntNullableNotRequired { get; set; }

        [Column("bigint_nullable_required", TypeName = "bigint"), Required]
        public long? BigIntNullableRequired { get; set; }
        

        [Column("col_binary", TypeName = "binary(12)")]
        public byte[] Binary { get; set; }

        [Required]
        [Column("col_varbinary", TypeName = "varbinary(12)")]
        public byte[] VarBinary { get; set; }

        [Column("col_binary12", TypeName = "varbinary(12)")]
        [MaxLength(10)]
        public byte[] Binary12 { get; set; }


        [Column("col_varchar_to_byte", TypeName = "varchar(20)")]
        public byte[] VarcharToByte { get; set; }

        [Column("col_varchar_1", TypeName = "varchar(20)")]
        public string Varchar1 { get; set; }

        [Column("col_char_1", TypeName = "char(20)")]
        public string Char1 { get; set; }

        [Column("col_nvarchar_1", TypeName = "nvarchar(20)")]
        public string Nvarchar1 { get; set; }

        [Column("col_nchar_1", TypeName = "nchar(20)")]
        public string Nchar1 { get; set; }

        [Column("col_varchar_2", TypeName = "varchar(20)")]
        public string Varchar2 { get; set; }

        [Column("col_char_2", TypeName = "char(20)")]
        public string Char2 { get; set; }

        [Column("col_nvarchar_2", TypeName = "nvarchar(20)")]
        public string Nvarchar2 { get; set; }

        [Column("col_nchar_2", TypeName = "nchar(20)")]
        public string Nchar2 { get; set; }

    }
}
