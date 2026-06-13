using System;
using System.Text;

namespace Emar.Api.Helpers
{
    public static class Utilities
    {
        internal static string ExtractExceptionMessages(Exception e)
        {
            var message = new StringBuilder(e.Message);
            Exception inner = e.InnerException;
            while (inner != null)
            {
                message.AppendLine($"Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }

            return message.ToString();
        }
    }
}
