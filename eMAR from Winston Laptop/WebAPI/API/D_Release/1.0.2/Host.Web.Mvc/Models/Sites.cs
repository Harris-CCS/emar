using System.Collections.Generic;
using PulseCheck.Domain;

namespace Host.Web.Mvc.Models
{
    public class Sites
    {
        public string Id { get; set; }
        public List<Site> AvailableSites { get; set; }
        public string ErrorMessage { get; set; }

    }
}