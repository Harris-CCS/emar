using System;
using System.Diagnostics;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Data.Entities;

namespace Emar.Core.Templates.Model.Mappings
{
    static class TemplateMapper
    {
        private static string _siteTimeZoneName;

        public static TemplateDto MapTemplate(Template dbObj, string siteTimeZoneName, HateOasLinkDto linkDto)
        {
            _siteTimeZoneName = siteTimeZoneName;

            if (dbObj == null)
            {
                return null;
            }

            var ret = new TemplateDto
            {
                Id = dbObj.Id,
                Name = dbObj.Name,
                Active = dbObj.IsActive,
                Title = dbObj.Title,
                SaveButtonText = dbObj.SaveButtonText,
                CancelButtonText = dbObj.CancelButtonText,
                EventDatetimePromptId = dbObj.EventDatetimePromptId,
                PromptGroups = dbObj.TemplatePromptGroups
                    .Select(tpg => tpg.PromptGroup)
                    .Select(MapPromptGroup).OrderBy(e => e?.Sequence ?? 99).ToList(),
                Link = linkDto
            };

            return ret;
        }

        private static PromptGroupDto MapPromptGroup(PromptGroup dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            Debug.Assert(dbObj.TemplatePromptGroups.Count == 1);

            var ret = new PromptGroupDto()
            {
                Id = dbObj.Id,
                Name = dbObj.Name,
                DisplayTitle = dbObj.Title,
                Sequence = (dbObj.TemplatePromptGroups.FirstOrDefault()?.Sequence ?? 99),
                Prompts = dbObj.Prompts?.Select(MapPrompt).OrderBy(e => e.Sequence).ToList()
            };

            return ret;
        }

        private static PromptDto MapPrompt(Prompt dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new PromptDto()
            {
                Id = dbObj.Id,
                PromptGroupId = dbObj.PromptGroupId,
                Sequence = dbObj.Sequence,
                Prompt = dbObj.PromptText,
                IsActive = dbObj.IsActive,
                Type = dbObj.PromptType,
                Default = dbObj.PromptDefault,
                Required = dbObj.Required,
                IsOnNewline = dbObj.IsOnNewline,
                PlaceholderText = dbObj.PlaceholderText,
                DisplayChildPromptsValue = dbObj.DisplayChildPromptsValue,

                // Winston Murdock, 02/09/2021.  EMAR-649
                ChartMarkup = dbObj.ChartMarkup,

                PromptChildren = dbObj.PromptChoices?.Where(p => p.Sequence == 0).Select(MapPromptChild).ToList().OrderBy(a => a),
                PromptChoices = dbObj.PromptChoices?.Where(p => p.Sequence != 0).Select(MapPromptChoice).ToList().OrderBy(a => a.Sequence)
            };

            if (ret.Type.Equals("DateTime", StringComparison.InvariantCultureIgnoreCase)
                && ret.Default.Equals("Now", StringComparison.InvariantCultureIgnoreCase))
            {
                ret.Default = _siteTimeZoneName.NowWithTimeZoneOffset().ToString("O");
            }

            return ret;
        }

        private static int MapPromptChild(PromptChoice dbObj)
        {
            if (!int.TryParse(dbObj.ChoiceText, out int result))
                throw new ArgumentException($"Found record in the [prompt_choices] table (id = {dbObj.Id}) where the choice_text is not parseable to an integer.");

            return result;
        }

        private static PromptChoiceDto MapPromptChoice(PromptChoice dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new PromptChoiceDto
            {
                Id = dbObj.Id,
                PromptId = dbObj.PromptId,
                Sequence = dbObj.Sequence,
                ChoiceText = dbObj.ChoiceText,

                // Winston Murdock, 02/09/2021.  EMAR-649
                ChartMarkup = dbObj.ChartMarkup
            };

            return ret;
        }

        public static OrderAdministrationAvailableActionDto MapOrderAdministrationAvailableAction(OrderAdministrationAvailableAction dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            if (!Enum.TryParse(dbObj.OrderStatus, true, out OrderStatus orderStatus))
                throw new Exception($"The Value '{dbObj.OrderStatus}', retrieved from the database, is not a valid OrderStatus");

            if (!Enum.TryParse(dbObj.AdministrationStatus, true, out AdministrationStatusEnum adminStatus))
                throw new Exception($"The Value '{dbObj.AdministrationStatus}', retrieved from the database, is not a valid OrderAdministrationStatus");

            return new OrderAdministrationAvailableActionDto
            {
                OrderStatus = orderStatus,
                AdministrationStatus = adminStatus,
                AvailableActionId = dbObj.AvailableActionId,
                Action = MapAction(dbObj.Action),
                PointInTime = dbObj.PointInTime
            };
        }

        public static OrderAvailableActionDto MapOrderAvailableAction(OrderAvailableAction dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            if (!Enum.TryParse(dbObj.OrderStatus, true, out OrderStatus orderStatus))
                throw new Exception($"The Value '{dbObj.OrderStatus}', retrieved from the database, is not a valid OrderStatus");

            return new OrderAvailableActionDto
            {
                OrderStatus = orderStatus,
                AvailableActionId = dbObj.AvailableActionId,
                Action = MapAction(dbObj.Action),
                PointInTime = dbObj.IsPit,
                IsPrnOnly = dbObj.IsPrnOnly
            };
        }

        public static ActionDto MapAction(Data.Entities.Action dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            return new ActionDto
            {
                ActionId = dbObj.Id,
                ActionCode = dbObj.Name,
                ButtonText = dbObj.Description
            };
        }

        public static OrderEventDetailDto MapOrderEventDetail(OrderEventDetail dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            return new OrderEventDetailDto
            {
                EventDetailId = dbObj.Id,
                PromptId = dbObj.PromptId,
                PromptText = dbObj.PromptText,
                UserResponse = dbObj.EnteredText,

                // Winston Murdock, 02/09/2021.  EMAR-649
                ChartMarkup = dbObj.ChartMarkup
            };
        }
    }
}