using System.Data;
using System.Data.SqlClient;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle signatures
    /// </summary>
    public class Signatures
    {
        // AuditEntry signatures
        public class AuditEntry
        {
            /// <summary>
            /// Site identifier
            /// </summary>
            public byte Site { get; set; }

            /// <summary>
            /// Patient identifier
            /// </summary>
            public string Ibex { get; set; }

            /// <summary>
            /// User identifier
            /// </summary>
            public int UserId { get; set; }

            /// <summary>
            /// Signatures.AuditEntry constructor
            /// </summary>
            /// <param name="siteId">Site identifier</param>
            /// <param name="patientId">Patient identifier</param>
            /// <param name="userId">User identifier</param>
            public AuditEntry(byte siteId, string patientId, int userId)
            {
                Site = siteId;
                Ibex = patientId;
                UserId = userId;
            }

            /// <summary>
            /// Add a new sigaud entry for tracking what users have unsigned chart entries
            /// </summary>
            /// <returns>Boolean flag for whether new entry is successfully entered</returns>
            public bool Save()
            {
                if (!IsUniqueEntry())
                {
                    return true;
                }

                var currentTime = (new Time(Site)).Timestamp();
                var result = new DB.Insert
                {
                    Sql = "INSERT INTO sigaud (ibex, usr, site, chgdate) VALUES (@ibex, @usr, @site, @chgdate)",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ibex", SqlDbType.Char) { Value = Ibex },
                        new SqlParameter("@usr", SqlDbType.Int) { Value = UserId },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site },
                        new SqlParameter("@chgdate", SqlDbType.Char) { Value = currentTime }
                    }
                }.Run();

                return (result >= 1);
            }

            /// <summary>
            /// Remove sigaud entry for tracking what users have unsigned chart entries
            /// </summary>
            /// <returns>Boolean flag for whether entry is successfully removed</returns>
            public bool Delete()
            {
                var result = new DB.Update
                {
                    Sql = "DELETE FROM sigaud WHERE ibex=@ibex AND usr=@usr AND site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ibex", SqlDbType.Char) { Value = Ibex },
                        new SqlParameter("@usr", SqlDbType.Int) { Value = UserId },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site }
                    }
                }.Run();

                return (result >= 1);
            }

            /// <summary>
            /// Make sure the entries are unique for ibex/usr/site in the table
            /// </summary>
            /// <returns>Boolean flag for whether the combination of values is unique</returns>
            private bool IsUniqueEntry()
            {
                var result = new DB.Select
                {
                    Sql = "SELECT usr FROM sigaud WHERE ibex=@ibex AND usr=@usr AND site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ibex", SqlDbType.Char) { Value = Ibex },
                        new SqlParameter("@usr", SqlDbType.Int) { Value = UserId },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site }
                    }
                }.RunForDataRow();

                if (result != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}