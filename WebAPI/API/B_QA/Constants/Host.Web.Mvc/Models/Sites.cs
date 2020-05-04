using System.Collections.Generic;
using DomainModel;

namespace Host.Web.Mvc.Models
{
    public class Sites
    {
        public string Id { get; set; }
        public List<Site> AvailableSites { get; set; }
        public string ErrorMessage { get; set; }

    }
}