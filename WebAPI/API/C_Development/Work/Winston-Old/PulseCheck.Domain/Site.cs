using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using PulseCheck.IDomain;
using PulseCheck.Utilities;

namespace PulseCheck.Domain
{
    public class Site : ISite
    {
        public Site()
        {
            Name = "";
            Status = new Status();
            UserInfo = null;
        }

        public Site(byte siteId)
        {
            Id = siteId;
            Name = "";
            Status = new Status();
            UserInfo = null;
        }

        public byte Id { get; set; }

        private string _name;
        public string Name
        {
            get { return this._name.Trim(); }
            set { this._name = value.Trim(); }
        }

        public Status Status { get; set; }

        public Int16 Timeout { get; set; }
        public byte Refresh { get; set; }

        private string _root;
        public string Root
        {
            get { return this._root; }
            set { this._root = value.Trim(); }
        }

        [NotMapped]
        public ICollection<Department> Departments { get; set; }

        [NotMapped]
        public User UserInfo { get; set; }

        [NotMapped]
        public IEnumerable<SiteRule> Rules { get; set; }

        private Dictionary<string, string> OrgOptions = new Dictionary<string, string>();

        public string GetOrgOption(string optName, SqlConnection con = null)
        {
            if (!OrgOptions.ContainsKey(optName))
            {
                OrgOptions[optName] = new DB.Select
                {
                    Connection = con,
                    Sql = "SELECT [dbo].[fnGetOrgOption](@siteId, @optName)",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@siteId", SqlDbType.TinyInt) { Value = Id },
                        new SqlParameter("@optName", SqlDbType.VarChar) { Value = optName }
                    }
                }.RunForScalar().ToString();
            }

            return OrgOptions[optName];
        }
    }
}