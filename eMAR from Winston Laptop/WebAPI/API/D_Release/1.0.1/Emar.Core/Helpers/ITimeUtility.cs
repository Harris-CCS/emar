using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Helpers
{
    public interface ITimeUtility
    {
        string Timestamp();
        string TimestampNoSeconds();
        string Datestamp();
    }
}