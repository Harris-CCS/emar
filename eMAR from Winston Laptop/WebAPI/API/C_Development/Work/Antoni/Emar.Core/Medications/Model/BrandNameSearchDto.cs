using Emar.Core.Helpers;

namespace Emar.Core.Medications.Model
{
    public class BrandNameSearchDto
    {
        public string BrandName { get; set; }

        public byte? InpatientMatch { get; set; }

        public byte? OutpatientMatch { get; set; }

        public byte? PyxisMatch { get; set; }

        public HateOasLinkDto Link { get; set; }
    }
}
