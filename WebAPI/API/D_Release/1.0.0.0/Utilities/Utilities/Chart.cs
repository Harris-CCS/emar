using System.Data;
using System.Data.SqlClient;
using Interfaces.DomainModel;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Chart class for leftover lib34 functionality 
    /// </summary>
    public static class Chart
    {
        /// <summary>
        /// Event that fires when the chart is written to
        /// </summary>
        /// <param name="site">ISite instance with site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="userId">User identifier</param>
        public static void OnChartWrite(ISite site, string patientId, int userId)
        {
            var org = new DB.Select
            {
                Sql = "SELECT root,gottxt FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                }
            }.RunForDataRow();

            var root = org["root"].ToString().Trim();

            // Rules engine queue
            var rules = site.GetOrgOption("RULES_ENGINE");
            if ("ABCP".IndexOf(rules) >= 0)
            {
                var filePath = root + "link\\rul\\" + patientId;
                FileWriter.Write(filePath, "");
            }

            // Text interface queue 1
            var txtInfQueChart = site.GetOrgOption("TXT_INF_QUE_CHART");
            if (org["gottxt"].ToString().Trim().Equals("Y"))
            {
                // TODO: Implement Interfaces::Trigger::create()
            }

            // Clinical interface queue
            var clinicalInf = site.GetOrgOption("CLINICAL_INF");
            var hl7InfQueChart = site.GetOrgOption("HL7_INF_QUE_CHART");
            if (clinicalInf.Equals("Y") && hl7InfQueChart.Equals("Y"))
            {
                var filePath = root + "link\\cln\\" + patientId;
                FileWriter.Write(filePath, "4c");
            }

            // Chart view audit
            if (userId > 0)
            {
                var sigAud = new Signatures.AuditEntry(site.Id, patientId, userId);
                sigAud.Save();
            }
        }
    }
}