namespace Interfaces.DomainModel
{
    public interface IUser
    {
        int Id { get; set; }
        string Type { get; set; }
        string LastName { get; set; }
        string FirstName { get; set; }
        byte SiteId { get; set; }
    }
}
