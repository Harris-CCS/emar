using System.Collections.Generic;

namespace PulseCheck.Domain
{
    public class Style
    {
        private string[] props = new string[] { "ColorCode", "ColorName", "ColorValue1", "ColorValue2" };
        private string _colorCode { get; set; }
        public string ColorCode
        {
            get { return this._colorCode != null ? this._colorCode.Trim() : null; }
            set { this._colorCode = value?.Trim() ?? ""; }
        }
        public string ColorName { get; set; }
        public List<string> ColorValues { get; set; }

        public Style()
        {
            ColorValues = new List<string>();
        }

        public Style(Dictionary<string, string> styleDict)
        {
            ColorValues = new List<string>();
            foreach(string prop in props)
            {
                if (!styleDict.ContainsKey(prop))
                {
                    continue;
                }
                var value = styleDict[prop];

                switch (prop)
                {
                    case "ColorCode":
                        ColorCode = value;
                        break;
                    case "ColorName":
                        ColorName = value;
                        break;
                    case "ColorValue1":
                        ColorValues.Add(value);
                        break;
                    case "ColorValue2":
                        ColorValues.Add(value);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}