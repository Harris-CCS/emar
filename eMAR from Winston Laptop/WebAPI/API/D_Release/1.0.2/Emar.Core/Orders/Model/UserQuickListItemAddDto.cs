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

        //Added these three columns because they come back in the request body
        //and becase they are in the UserQuickListItem entity but were not in here.
        //We need to pass them into the UserQuickListItem entity so that we can
        //save them into the DB.
        //Winston Murdock, 03/01/2021.  EMAR-582
        public int? Duration { get; set; }

        public int? DurationUnitId { get; set; }

        //This is a byte in the table and entity.  But it's a string when coming in the request body from the webpage.
        //We'll convert from a string to a bute in the mapper.
        //Winston Murdock, 03/11/2021.  EMAR-582
        public OrderPriorities? Priority { get; set; }

        public string? Ndc { get; set; }

        public string? PrnIndication { get; set; }
    }
}
