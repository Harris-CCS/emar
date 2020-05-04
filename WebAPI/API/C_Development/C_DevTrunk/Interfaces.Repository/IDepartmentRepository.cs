using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModel;

namespace Interfaces.Repository
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetDepartmentsBySiteIdAsync(byte siteId);
        Task<Department> GetDepartmentByKeyAsync(string dept, byte siteId, bool includeDetails = false);
    }
}
