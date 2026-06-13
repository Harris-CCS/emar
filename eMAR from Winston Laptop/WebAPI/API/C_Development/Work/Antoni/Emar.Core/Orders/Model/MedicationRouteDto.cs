namespace Emar.Core.Orders.Model
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
        string name;
        public string RouteName
        {
            get => name?.Trim();
            set => name = value?.Trim();
        }
    }
}