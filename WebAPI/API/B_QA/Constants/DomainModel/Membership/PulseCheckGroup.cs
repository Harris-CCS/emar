using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrockAllen.MembershipReboot;

namespace DomainModel.Membership
{
    public class PulseCheckGroup : RelationalGroup
    {
        public virtual string Description { get; set; }
    }
}