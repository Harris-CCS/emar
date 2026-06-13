using System;

namespace Emar.Core.Helpers
{
    public class HateoasLinkHelper
    {
        #region OrderActionTemplateResult
        private string _fileOrderActionTemplateResult;

        public void SetOrderActionTemplateResultLink(string link)
        {
            if (!link.Contains("-99") || !link.Contains("-98") || !link.Contains("-97"))
                throw new ArgumentNullException(nameof(link), "link must contain '-99', '-98' and '-97'");

            _fileOrderActionTemplateResult =
                link
                    .Replace("-99", "{0}")
                    .Replace("-98", "{1}")
                    .Replace("-97", "{2}");
        }

        public HateOasLinkDto GetOrderActionTemplateResultLink(in long orderId, int actionId, int templateId)
        {
            var url = _fileOrderActionTemplateResult == null ? null :
                string.Format(_fileOrderActionTemplateResult, orderId, actionId, templateId);

            return url == null ? null : new HateOasLinkDto(url, "File Results of Template", "POST");
        }
        #endregion

        #region AdministrationActionTemplateResult
        private string _fileAdministrationActionTemplateResult;

        public void SetAdministrationActionTemplateResultLink(string link)
        {
            if (!link.Contains("-99") || !link.Contains("-98") || !link.Contains("-97"))
                throw new ArgumentNullException(nameof(link), "link must contain '-99', '-98' and '-97'");

            _fileAdministrationActionTemplateResult =
                link
                    .Replace("-99", "{0}")
                    .Replace("-98", "{1}")
                    .Replace("-97", "{2}");

        }

        public HateOasLinkDto GetAdministrationActionTemplateResultLink(in long adminId, int actionId, int templateId)
        {
            var url =  _fileAdministrationActionTemplateResult == null ? null :
                string.Format(_fileAdministrationActionTemplateResult, adminId, actionId, templateId);

            return url == null ? null : new HateOasLinkDto(url, "File Results of Template","POST" );
        }
        #endregion
    }
}