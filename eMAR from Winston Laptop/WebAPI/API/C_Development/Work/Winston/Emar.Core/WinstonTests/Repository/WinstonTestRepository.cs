using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Emar.Core.WinstonTests.Repository
{
    public class WinstonTestRepository : IWinstonTestRepository
    {
        private readonly EmarContext _context;

        public WinstonTestRepository(EmarContext emarContext)
        {
            _context = emarContext;
        }

        public List<WinstonTest> GetWinstonTests()
        {
            var query = from wt in _context.WinstonTests
                        select wt;

            var ret = query.ToList();

            return ret;
        } //end if

        public List<WinstonTest> GetActiveWinstonTests()
        {
            return (from wt in _context.WinstonTests
                    where wt.ColumnTwo == true
                    select wt).ToList();
        } //end if
    }
}
