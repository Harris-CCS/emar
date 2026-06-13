using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Emar.Core.OutboundChart.Model
{
    public class Status
    {
        public string Code { get; set; }
        [NotMapped]
        public string Description { get; set; }
        [NotMapped]
        public Style Style { get; set; }

        // comment this out for now - don't think will ever be needed
        //public static Status GetStatusByCode(string code)
        //{
        //   return Constants.Statuses.FirstOrDefault(
        //        y =>
        //            string.Compare(code, y.Code,
        //                StringComparison.CurrentCultureIgnoreCase) == 0);
        //}

        public override string ToString()
        {
            return Code;
        }
    }
}