namespace Emar.Core.Templates.Model
{
    public enum ActionEnum
    {
        Acknowledge = 1,
        Cancel = 2,
        CompleteDiscontinue = 4,
        CoSign = 5,
        Delete = 6,
        Give = 8,
        Hold = 9,
        MissedDose = 10,
        OrderDiscontinue = 11,
        Repeat = 12,
        Reschedule = 13,
        UnHold = 14,
        FollowUp = 7,
        Complete = 3
    }

    public enum PromptType
    {
        CheckBox,
        CheckBoxCheckChildren,
        CheckBoxShowChildren,
        DateTime,
        DropDownListBox,
        FreeText,
        Information,
        MultiLineFreeText,
        Notify
    }
}