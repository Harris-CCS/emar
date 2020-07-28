using System;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Emar.Api.Controllers
{
#pragma warning disable CS1591
    public class MediaTypes
    {
        public const string PcEmar = @"application/vnd.pcemar.hateoas+json";
        public const string Json = @"application/json";

        public static bool IsValidMediaType(string mediaType)
        {
            if (String.IsNullOrEmpty(mediaType))
            {
                // No media type header provided
                return false;
            }

            if (!MediaTypeHeaderValue.TryParseList(mediaType.Split(","), out IList<MediaTypeHeaderValue> parsedMediaType))
            {
                // Invalid media type header provided
                return false;
            }

            if (!parsedMediaType.Contains(new MediaTypeHeaderValue(MediaTypes.Json)))
            {
                // Unsupported media type header provided
                return false;
            }

            return true;
        }
    }
#pragma warning restore CS1591
}