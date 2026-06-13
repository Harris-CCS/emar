using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Emar.Api.Helpers
{
#pragma warning disable CS1591
    public class MediaTypes
    {
        public const string PcEmar = @"application/vnd.pcemar.hateoas+json";
        public const string Json = @"application/json";
        public const string Text = @"text/plain";
        public const string Jpeg = @"image/jpeg";
        public const string Png = @"image/png";
        public const string Gif = @"image/gif";

        public static bool IsValidMediaType(string mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
            {
                // No media type header provided
                return false;
            }

            return MediaTypeHeaderValue.TryParseList(
                    mediaType.Split(","), out IList<MediaTypeHeaderValue> parsedMediaType)
                   && parsedMediaType.Contains(new MediaTypeHeaderValue(Json));
        }
    }
#pragma warning restore CS1591
}
