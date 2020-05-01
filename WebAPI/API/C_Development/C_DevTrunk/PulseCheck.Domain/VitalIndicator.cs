namespace PulseCheck.Domain
{
    public class VitalIndicator
    {
        public string Name { get; set; }
        private string _text { get; set; }
        public string Text
        {
            get { return this._text.Trim(); }
            set { this._text = value?.Trim() ?? ""; }
        }
        public Style Style { get; set; }
    }
}