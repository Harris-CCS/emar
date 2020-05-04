using System.Data.SqlClient;

namespace Interfaces.DomainModel
{
    public interface ISite
    {
        byte Id { get; set; }
        string Root { get; set; }
        string GetOrgOption(string optName, SqlConnection con = null);
    }
}
