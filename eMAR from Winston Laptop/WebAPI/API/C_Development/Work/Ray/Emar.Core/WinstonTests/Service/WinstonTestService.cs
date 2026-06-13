using System.Collections.Generic;
using System.Linq;
using Emar.Core.WinstonTests.Model;
using Emar.Core.WinstonTests.Model.Mappings;
using Emar.Core.WinstonTests.Repository;
using Emar.Data.Entities;

namespace Emar.Core.WinstonTests.Service
{
    public class WinstonTestService : IWinstonTestService
    {
        private readonly IWinstonTestRepository _winstonTestRepository;

        public WinstonTestService(IWinstonTestRepository winstonTestRepository)
        {
            _winstonTestRepository = winstonTestRepository;
        }


        public List<WinstonTestDto> GetWinstonTests()
        {
            var winstonTestList = _winstonTestRepository.GetWinstonTests();

            if (winstonTestList == null)
            {
                return null;
            }

            var winstonTestDtos = winstonTestList.Select(wt => WinstonTestMapper.MapWinstonTest(wt)).ToList();

            return winstonTestDtos;
        }

        public List<WinstonTestDto> GetActiveWinstonTests()
        {
            var winstonTestList = _winstonTestRepository.GetActiveWinstonTests();

            if (winstonTestList == null)
            {
                return null;
            }

            var winstonTestDtos = winstonTestList.Select(wt => WinstonTestMapper.MapWinstonTest(wt)).ToList();

            return winstonTestDtos;
        }


        public List<WinstonTestDto> GetWinstonTestsSortByColumnOneAscending()
        {
            var winstonTestList = _winstonTestRepository.GetWinstonTests();

            if (winstonTestList == null)
            {
                return null;
            }

            var winstonTestDtos = winstonTestList.Select(wt => WinstonTestMapper.MapWinstonTest(wt));

            var ret = from row in winstonTestDtos
                                     orderby row.ColumnOne
                                     select row;

            return ret.ToList();
        }

        public List<WinstonTestDto> GetWinstonTestsSortByColumnOneDescending()
        {
            var winstonTestList = _winstonTestRepository.GetWinstonTests();

            if (winstonTestList == null)
            {
                return null;
            }

            var winstonTestDtos = winstonTestList.Select(wt => WinstonTestMapper.MapWinstonTest(wt));

            var ret = from row in winstonTestDtos
                      orderby row.ColumnOne descending
                      select row;
            
            return ret.ToList();
        }



    }
}
