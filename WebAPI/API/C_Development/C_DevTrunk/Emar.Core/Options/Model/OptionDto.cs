using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.Sites.Model;

namespace Emar.Core.Options.Model
{
    public class OptionDto
    {
        public int Id { get; set; }

        string _name;
        public string Name
        {
            get => _name?.Trim();
            set => _name = value?.Trim();
        }

        string _description;
        public string Description
        {
            get => _description?.Trim();
            set => _description = value?.Trim();
        }

        public IEnumerable<SiteOptionDto> SiteOptions { get; set; }
    }

    public class SiteOptionDto
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int OptionId { get; set; }

        string _optionValue;
        public string OptionValue
        {
            get => _optionValue?.Trim();
            set => _optionValue = value?.Trim();
        }

        public SiteDto Site { get; set; }

        public IEnumerable<OptionDto> Options { get; set; }
    }
}
