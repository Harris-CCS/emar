using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PulseCheck.Data.Common.Repositories;
using PulseCheck.Data.Common.Rest;
using PulseCheck.QCPR.Domain.Constants;
using PulseCheck.Utilities;
using RestSharp;

namespace PulseCheck.QCPR.Data.Repository
{
    public class QcprInterfaceRepository : RestRepository, IQcprInterfaceRepository
    {
        private IRestSharpHandler Rest => (IRestSharpHandler)RestHandler;

        public QcprInterfaceRepository(IRestSharpHandler restHandler) : base(restHandler)
        {
        }

        public string GetProceduresJson()
        {
            string uri = PulseCheck.Data.Common.Configuration.Settings.GetString(AppSettingConstants.QcprProceduresUri);

            if(string.IsNullOrEmpty(uri))
                throw new InvalidOperationException($"appsetting {AppSettingConstants.QcprProceduresUri} was not configured.");

            return Rest.Get("qcpr.interface.config.PulseCheck.cls?ic=226");
        }
    }
}
