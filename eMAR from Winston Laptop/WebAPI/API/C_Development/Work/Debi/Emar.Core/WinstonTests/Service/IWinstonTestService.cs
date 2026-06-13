using System.Collections.Generic;
using Emar.Core.WinstonTests.Model;

namespace Emar.Core.WinstonTests.Service
{
    public interface IWinstonTestService
    {
        List<WinstonTestDto> GetWinstonTests();

        List<WinstonTestDto> GetActiveWinstonTests();

        List<WinstonTestDto> GetWinstonTestsSortByColumnOneAscending();

        List<WinstonTestDto> GetWinstonTestsSortByColumnOneDescending();
    }
}
