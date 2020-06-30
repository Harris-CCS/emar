namespace Emar.Core
{
    public class HateOasLinkDto
    {
        public string Href { get; private set; }
        public string Rel { get; private set; }
        public string Method { get; private set; }
        public HateOasLinkDto()
        {

        }
        public HateOasLinkDto(string href, string rel, string method)
        {
            this.Href = href; 
            this.Rel = rel; 
            this.Method = method; 
        }
    }
}
