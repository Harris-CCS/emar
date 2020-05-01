using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModel;
using Interfaces.Repository;

namespace Data.Repositories
{
    public class DepartmentRepository : BaseRepository, IDepartmentRepository
    {
        private IAreaRepository _areaRepository;

        public DepartmentRepository(IbexContext context, IAreaRepository areaRepository)
            : base(context)
        {
            _areaRepository = areaRepository;
        }

        public async Task<List<Department>> GetDepartmentsBySiteIdAsync(byte siteId)
        {
            var result = await _context.Departments.Where(dept => dept.SiteId == siteId).ToListAsync();

            //Set the Status object
            result.ForEach(x => x.Status = Status.GetStatusByCode(x.Status.Code));

            return result;
        }

        public async Task<Department> GetDepartmentByKeyAsync(string dept, byte siteId, bool includeDetails = false)
        {
            var result = await _context.Departments.FirstOrDefaultAsync(d => d.SiteId == siteId && d.Dept == dept);

            if (result != null)
            {
                //Set the Status object
                result.Status = Status.GetStatusByCode(result.Status.Code);

                if (includeDetails)
                    result.Areas = await _areaRepository.GetAreasByDepartmentId(result.SiteId, result.Dept);
            }

            return result;

        }
    }
}
