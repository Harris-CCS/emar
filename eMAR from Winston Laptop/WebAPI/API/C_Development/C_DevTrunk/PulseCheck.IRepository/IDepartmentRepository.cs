using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.IRepository
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetDepartmentsBySiteIdAsync(byte siteId);
        Task<Department> GetDepartmentByKeyAsync(string dept, byte siteId, bool includeDetails = false);
    }
}
