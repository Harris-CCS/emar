using System.Data.SqlClient;

namespace Emar.Core.OutboundChart.Model
{
    public interface ISite
    {
        byte Id { get; set; }
        string Root { get; set; }
        string GetOrgOption(string optName, SqlConnection con = null);
    }
}
