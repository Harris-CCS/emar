using System;
using Emar.Core.Orders.Service;

namespace Emar.Core.Orders.Model
{
    public class AvailableActionDto
    {
        private readonly ActionEnum _availableAction;

        public AvailableActionDto(ActionEnum action, string orderLinkBase)
        {
            _availableAction = action;
            Link = orderLinkBase.Replace("XAction", action.ToString());

            switch (action)
            {
                case ActionEnum.Cancel:
                case ActionEnum.Delete:
                case ActionEnum.Repeat:
                case ActionEnum.Give:
                case ActionEnum.Acknowledge:
                case ActionEnum.Hold:
                case ActionEnum.Reschedule:
                case ActionEnum.Complete:
                    ButtonText = action.ToString();
                    break;
                case ActionEnum.OrderDiscontinue:
                    ButtonText = "Order Discontinue";
                    break;
                case ActionEnum.CompleteDiscontinue:
                    ButtonText = "Complete Discontinue";
                    break;
                case ActionEnum.CoSign:
                    ButtonText = "Co-Sign";
                    break;
                case ActionEnum.FollowUp:
                    ButtonText = "Follow Up";
                    break;
                case ActionEnum.MissedDose:
                    ButtonText = "Missed Dose";
                    break;
                case ActionEnum.UnHold:
                    ButtonText = "Un-Hold";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public string AvailableAction => _availableAction.ToString();
        public string ButtonText { get; }
        public string Link { get; }
    }
}