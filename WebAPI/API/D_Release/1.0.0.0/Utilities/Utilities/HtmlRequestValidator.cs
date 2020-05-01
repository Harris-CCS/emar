using System.Web;
using System.Web.Util;

namespace PulseCheck.Utilities
{
    class HtmlRequestValidator : RequestValidator
    {
        public HtmlRequestValidator() { }

        protected override bool IsValidRequestString(HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex)
        {
            validationFailureIndex = -1;  //Set a default value for the out parameter.

            if (requestValidationSource == RequestValidationSource.RawUrl)
                return true;

            return base.IsValidRequestString(context, value, requestValidationSource, collectionKey, out validationFailureIndex);
        }
    }
}
