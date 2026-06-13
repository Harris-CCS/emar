using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.OutboundData.Model;

namespace Emar.Core.OutboundData.Service.NonIbex
{
    public class OdsHl7OutboundService : IOdsEmarOutboundService
    {
        public void SendNewPatientOrder(List<OdsPatientOrderParameters> odsOrder)
        {

        }
        public OdsPatientOrderParameters? ConvertOdsOrderData(OdsPatientOrderParameters odsOrder)
        {
            return null;
        }
        public OdsMedicationDetails? ConvertOdsMedicationDetailsData(OdsPatientOrderParameters odsOrder)
        {
            return null;
        }
        public void SendAdministrationAction(OdsAdministrationParameters parameters)
        {

        }

        public bool GetEmarMedAdministrationStatus(long adminId)
        {
            return false;
        }
    }
}
