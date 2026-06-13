using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Core.OutboundChart.Model
{
    public class MinimalUser : MinimalPerson, IMinimalUser
    {
        public int Id { get; set; }

        private string _initials { get; set; }
        public string Initials
        {
            get { return this._initials != null ? this._initials.Trim() : ""; }
            set { this._initials = value?.Trim() ?? ""; }
        }

        public byte SiteId { get; set; }

    }
}
