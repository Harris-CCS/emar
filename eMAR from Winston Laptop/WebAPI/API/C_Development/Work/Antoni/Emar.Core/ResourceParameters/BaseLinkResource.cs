namespace Emar.Core.ResourceParameters
{
    public class BaseLinkResource : BaseResourceParameters
    {
        public string LinkExecuteOrderAction { get; set; }
        public string LinkExecuteAdministrationAction { get; set; }
        public string LinkGetUserQuickListTab { get; set; }
        public string LinkCopyItemToCart { get; set; }
        public string LinkGetPatientOrder { get; set; }
        public string LinkGetCartOrder { get; set; }
        public string LinkGetHomeMedication { get; set; }
        public string LinkGetSchedulerOptions { get; set; }
        public string LinkGetSchedulerOptionsListItem { get; set; }
    }
}