using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PulseCheck.IDomain;

namespace PulseCheck.Domain
{    
    public class UserMapping : IUserMapping
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string DomainLogin { get; set; }
        public string WindowsDomains { get; set; }
        public int UserNum { get; set; }
        [Column("user_full_name")]
        public string FullName { get; set; }
        public byte SiteId { get; set; }        
        [Column("site_name")]
        public string SiteName { get; set; }
        [Column(TypeName="smallint")]
        public Int16 Ctr { get; set; }
        [Column(TypeName = "tinyint")]
        public byte Retry { get; set; }
        [Column("init")]
        public string Initials { get; set; }
        [Column("site_has_mobile")]
        public bool HasMobile { get; set; }
    }
}