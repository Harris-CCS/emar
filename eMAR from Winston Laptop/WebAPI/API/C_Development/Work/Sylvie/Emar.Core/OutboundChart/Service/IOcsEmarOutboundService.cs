using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Emar.Core.OutboundChart.Model;
using Emar.Core.Templates.Model;

namespace Emar.Core.OutboundChart.Service
{
    public interface IOcsEmarOutboundService
    {
        //        Task<string> SendChartLinesAsync(OcsChartParameters ocsChartParams);
        string SendChartLinesAsync(OcsChartParameters ocsChartParams);
        string SendChartTemplateMarkup(List<OcsPromptParameters> orderedPromptList, int siteId, long patientOrderId, int userId, ActionEnum action, long adminId, bool newOrderAdmin);
    }
}