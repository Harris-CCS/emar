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
    public class AreaRepository : BaseRepository, IAreaRepository
    {
        public AreaRepository(IbexContext context) : base(context)
        {
        }

        public async Task<List<Area>> GetAreasByDepartmentId(byte siteId, string department)
        {
            var areas = await _context.Areas.Where(a => a.SiteId == siteId).ToListAsync();

            var result = areas.Where(a => string.Equals(a.Dept, department, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            //Set the Status object
            result.ForEach(x => x.Status = Status.GetStatusByCode(x.Status.Code));

            return result;
        }
    }
}
