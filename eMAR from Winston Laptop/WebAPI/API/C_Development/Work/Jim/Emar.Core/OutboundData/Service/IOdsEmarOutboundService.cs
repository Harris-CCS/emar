using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.OutboundData.Model;

namespace Emar.Core.OutboundData.Service
{
    public interface IOdsEmarOutboundService
    {
      void SendNewPatientOrder(List<OdsPatientOrderParameters> odsOrder, int siteID);
      public OdsPatientOrderParameters ConvertOdsOrderData(OdsPatientOrderParameters odsOrder);
      public OdsMedicationDetails ConvertOdsMedicationDetailsData(OdsPatientOrderParameters odsOrder);
      void SendAdministrationAction(OdsAdministrationParameters parameters);
      public bool GetEmarMedAdministrationStatus(long adminId);
    }
}
