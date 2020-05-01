using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DomainModel.Membership;
using IdentityManager.MembershipReboot;
using Services;

namespace Host.Web.Mvc.IdMgr
{
    public class PulseCheckIdentityManagerService : MembershipRebootIdentityManagerService<UserAccount, PulseCheckGroup>
    {
        public PulseCheckIdentityManagerService(UserAccountService userSvc, PulseCheckGroupService groupSvc)
            : base(userSvc, groupSvc)
        {
        }
    }
}