using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Repositories;
using IdentityManager;
using IdentityManager.Configuration;

namespace Host.Web.Mvc.IdMgr
{
    public static class PulseCheckIdentityManagerServiceExtensions
    {
        public static void Configure(this IdentityManagerServiceFactory factory, string connectionString)
        {
            factory.IdentityManagerService = new Registration<IIdentityManagerService, PulseCheckIdentityManagerService>();           
            factory.Register(new Registration<Services.UserAccountService>());
            factory.Register(new Registration<Services.PulseCheckGroupService>());
            factory.Register(new Registration<Data.Repositories.UserAccountRepository>());
            factory.Register(new Registration<Data.Repositories.PulseCheckGroupRepository>());
            factory.Register(new Registration<Data.Repositories.MembershipDatabase>(resolver => new MembershipDatabase(connectionString)));            
        }
    }
}