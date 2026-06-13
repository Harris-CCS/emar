using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IdentityManager;
using IdentityManager.Configuration;
using PulseCheck.Data.Repositories;
using PulseCheck.Logic;

namespace Host.Web.Mvc.IdMgr
{
    public static class PulseCheckIdentityManagerServiceExtensions
    {
        public static void Configure(this IdentityManagerServiceFactory factory, string connectionString)
        {
            factory.IdentityManagerService = new Registration<IIdentityManagerService, PulseCheckIdentityManagerService>();           
            factory.Register(new Registration<UserAccountManager>());
            factory.Register(new Registration<PulseCheckGroupService>());
            factory.Register(new Registration<UserAccountRepository>());
            factory.Register(new Registration<PulseCheckGroupRepository>());
            factory.Register(new Registration<MembershipDatabase>(resolver => new MembershipDatabase(connectionString)));            
        }
    }
}