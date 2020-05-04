using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModel;

namespace Interfaces.Repository
{
    public interface IAreaRepository
    {
        Task<List<Area>> GetAreasByDepartmentId(byte siteId, string department);
    }
}
