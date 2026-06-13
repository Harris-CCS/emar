using Emar.Core.Sites.Model.Mappings;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;
using System.Linq;

namespace Emar.Core.WinstonTests.Model.Mappings
{
    public static class WinstonTestMapper
    {

        public static WinstonTestDto MapWinstonTest (WinstonTest winstonTest)
        {
            if (winstonTest == null)
            {
                return null;
            }

            return new WinstonTestDto
            {
                Id = winstonTest.Id,
                ColumnOne = winstonTest.ColumnOne,
                ColumnTwo = winstonTest.ColumnTwo,
                ColumnThree = winstonTest.ColumnThree,
                OneAndThreeTogether = winstonTest.ColumnOne + " - " + winstonTest.ColumnThree                 
            };
        }
    }
}
