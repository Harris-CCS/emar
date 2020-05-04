namespace Interfaces.Data
{
    public interface IResultTypeWithProviders
    {
        int Doctor { get; set; }
        string DoctorInit { get; set; }
        int Resident { get; set; }
        string ResidentInit { get; set; }
        int Extender { get; set; }
        string ExtenderInit { get; set; }
        int DrExtender { get; set; }
        string DrExtenderInit { get; set; }
        int PrimaryNurse { get; set; }
        string PrimaryNurseInit { get; set; }
        int CareCoordinator { get; set; }
        string CareCoordinatorInit { get; set; }
        int Scribe { get; set; }
        string ScribeInit { get; set; }
    }
}