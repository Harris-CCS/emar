namespace Emar.Core.Orders.Model
{
    public class UserQuickListItemAddDto
    {
        public int UserId { get; set; }

        public int SiteId { get; set; }

        public int MedicationId { get; set; }

        public decimal? Dose { get; set; }

        public int? MedicationUnitId { get; set; }

        public int? MedicationRouteId { get; set; }

        public int? FrequencyId { get; set; }

        private string _orderNotes;
        public string OrderNotes
        {
            get => _orderNotes?.Trim();
            set => _orderNotes = value?.Trim();
        }
    }
}
