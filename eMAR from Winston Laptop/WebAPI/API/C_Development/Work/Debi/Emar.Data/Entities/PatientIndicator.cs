using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_indicators")]
    public partial class PatientIndicator
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("patient_id")]
        public long PatientId { get; set; }
        [Column("ordinal_position")]
        public short OrdinalPosition { get; set; }
        [Required]
        [Column("code")]
        [StringLength(10)]
        public string Code { get; set; }
        [Required]
        [Column("type")]
        [StringLength(10)]
        public string Type { get; set; }
        [Required]
        [Column("description")]
        [StringLength(255)]
        public string Description { get; set; }
        [Column("type_description")]
        [StringLength(255)]
        public string TypeDescription { get; set; }
        [Required]
        [Column("image_name")]
        [StringLength(255)]
        public string ImageName { get; set; }

        //Storing the site option for the indicator image path here.
        //We need it in the patient mapper, and we can't call out to the DB there.
        //Obviously, this isn't stored in the DB.
        //That's why it's not mapped.
        //Winston Murdock, 02/25/2022.  PC-26953
        [NotMapped]
        public string ImagePath { get; set; }

        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PatientIndicators))]
        public virtual Patient Patient { get; set; }
    }
}
