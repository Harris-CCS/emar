using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.WinstonTests.Repository
{
    public interface IWinstonTestRepository
    {
        List<WinstonTest> GetWinstonTests();

        List<WinstonTest> GetActiveWinstonTests();
    }
}
