using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DomainModel
{
    public class SiteRule
    {
        public long Id { get; set; }
        public long SiteId { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public bool? BoolValue { get; set; }
    }
}