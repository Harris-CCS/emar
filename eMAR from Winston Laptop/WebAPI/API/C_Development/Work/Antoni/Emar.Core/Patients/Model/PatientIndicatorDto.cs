using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Patients.Model
{
    public class PatientIndicatorDto
    {
        public long Id { get; set; }

        public long PatientId { get; set; }

        public short OrdinalPosition { get; set; }

        private string _code;
        public string Code
        {
            get => _code?.Trim();
            set => _code = value?.Trim();
        }

        string _type;
        public string Type
        {
            get => _type?.Trim();
            set => _type = value?.Trim();
        }

        string _description;
        public string Description
        {
            get => _description?.Trim();
            set => _description = value?.Trim();
        }

        string _imageName;
        public string ImageName
        {
            get => _imageName?.Trim();
            set => _imageName = value?.Trim();
        }

        string _imageSrc;
        public string ImageSrc
        {
            get => _imageSrc?.Trim();
            set => _imageSrc = value?.Trim();
        }
    }
}
