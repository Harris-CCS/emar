namespace PulseCheck.IDomain
{
    public interface IUserMapping
    {
        short Ctr { get; set; }
        string DomainLogin { get; set; }
        int Id { get; set; }
        string Login { get; set; }
        byte Retry { get; set; }
        byte SiteId { get; set; }
        int UserNum { get; set; }
        string WindowsDomains { get; set; }
        string FullName { get; set; }
        string SiteName { get; set; }
    }
}