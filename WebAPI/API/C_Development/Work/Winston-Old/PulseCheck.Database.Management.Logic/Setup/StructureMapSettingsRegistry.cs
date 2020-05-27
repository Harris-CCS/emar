using PulseCheck.Database.Management.Data.Connections;

namespace PulseCheck.Database.Management.Logic.Setup
{
    public class StructureMapSettingsRegistry : StructureMap.Registry
    {
        readonly IConnectionSettings _ibexConnectionSettings = ConnectionFactory.GetConnectionSettings("IbexConnection");

        public StructureMapSettingsRegistry()
        {
            For(typeof(IConnectionSettings)).Use(_ibexConnectionSettings);

        }
    }
}
