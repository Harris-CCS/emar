namespace Emar.Core.Medications.Model
{
    public class MedicationRoute : HateOasLinkDto
    {
        /// <summary>
        /// Unique medication route identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Medication route name.
        /// </summary>
        public string Name { get; set; }
    }
}
