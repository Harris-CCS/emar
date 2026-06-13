using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.IRepository
{
    public interface IAreaRepository
    {
        Task<List<Area>> GetAreasByDepartmentId(byte siteId, string department);
    }
}
