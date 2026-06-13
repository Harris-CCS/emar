using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Emar.Core.Medications.Model;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Repository
{
    internal interface IDrugDbRepository
    {
        IEnumerable<BrandNameReturnDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType, string deptCode);
        List<UserQuickListItem> ApplyFormularyFilterToQuickList(Expression<Func<UserQuickListItem, bool>> whereExpression, int siteId);
    }
}