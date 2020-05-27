using System;
using PulseCheck.Data.Common.Rest;

namespace PulseCheck.Data.Common.Repositories
{
    public class RestRepository : RepositoryBase
    {

        protected IRestHandler RestHandler;

        public RestRepository(IRestHandler restHandler)
        {
            RestHandler = restHandler ?? throw new ArgumentNullException(nameof(restHandler));
        }
    }
}
