using System;

namespace Emar.Core.Orders.Model
{
    public class OrderAdministrationNotesDto : HateOasLinkDto
    {
        /// <summary>
        /// Unique order administration note identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique order administration identifier
        /// </summary>
        public long AdministrationId { get; set; }

        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Note identifier
        /// </summary>
        public int NoteId { get; set; }

        /// <summary>
        /// Note text
        /// </summary>
        public string NoteText { get; set; }

        /// <summary>
        /// Unique user identifier of the user that entered the note.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Date and time the order administration note was entered.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset NoteDateTime { get; set; }
    }
}
