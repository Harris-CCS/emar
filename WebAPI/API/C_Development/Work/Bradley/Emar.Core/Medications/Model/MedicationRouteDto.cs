namespace Emar.Core.Medications.Model
{
    public class MedicationRouteDto
    {
        /// <summary>
        /// Unique medication route identifier.
        /// </summary>
        public int Id { get; set; }

        public int SiteId { get; set; }

        /// <summary>
        /// Medication route name.
        /// </summary>
        public string RouteName { get; set; }

        public bool PointInTime => true;
    }
}
