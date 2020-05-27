using System;
using System.ComponentModel.DataAnnotations;

namespace PulseCheck.Domain
{
    public class Comment
    {
        public string Text { get; set; }
        public Int32 CommentNumOnTrackingBoard { get; set; }
        public MinimalUser User { get; set; }
        public DateTime? DateTime { get; set; } 
        public Style Style { get; set; }

        [Key]
        public Int32 Losecs { get; set; }
    }
}