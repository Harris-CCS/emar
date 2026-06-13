using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IdentityManager.MembershipReboot;
using PulseCheck.Domain.Membership;
using PulseCheck.Logic;

namespace Host.Web.Mvc.IdMgr
{
    public class PulseCheckIdentityManagerService : MembershipRebootIdentityManagerService<UserAccount, PulseCheckGroup>
    {
        public PulseCheckIdentityManagerService(UserAccountManager userSvc, PulseCheckGroupService groupSvc)
            : base(userSvc, groupSvc)
        {
        }
    }
}