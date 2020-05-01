using System.Collections.Generic;

namespace Interfaces.DomainModel
{
    public interface IMedication
    {
        string AckDate { get; set; }
        int? AckUserId { get; set; }
        string Authentication { get; set; }
        string Barcode { get; set; }
        string CancelDate { get; set; }
        string CancelSysdate { get; set; }
        int? CancelUserId { get; set; }
        string Code { get; set; }
        int? CPTLosecsLink { get; set; }
        string DefaultDose { get; set; }
        string DefaultRoute { get; set; }
        string DefaultUnit { get; set; }
        string DeleteDate { get; set; }
        int? DeleteUserId { get; set; }
        string DiscontinueDate { get; set; }
        string DiscontinuedDate { get; set; }
        string DiscontinuedSysdate { get; set; }
        int? DiscontinuedUserId { get; set; }
        string DiscontinueSysdate { get; set; }
        int? DiscontinueUserId { get; set; }
        string Dose { get; set; }
        int? ExcludeUserId { get; set; }
        string GiveDate { get; set; }
        string GiveSysdate { get; set; }
        int? GiveUserId { get; set; }
        string HoldDate { get; set; }
        string HoldSysdate { get; set; }
        int? HoldUserId { get; set; }
        string Ibex { get; set; }
        int Id { get; set; }
        string Indication { get; set; }
        string IVLocation { get; set; }
        int? IVSite { get; set; }
        string IVType { get; set; }
        int Losecs { get; set; }
        string Name { get; set; }
        string Notes { get; set; }
        string OrderDate { get; set; }
        int? OrderForUserId { get; set; }
        int? OrderUserId { get; set; }
        int ProcedureCode { get; set; }
        int ProductCode { get; set; }
        string Rate { get; set; }
        string RateUnit { get; set; }
        string Repeat { get; set; }
        string Route { get; set; }
        string Schedule { get; set; }
        short Site { get; set; }
        string Status { get; set; }
        string StopDate { get; set; }
        string StopSysdate { get; set; }
        int? StopUserId { get; set; }
        string Time { get; set; }
        string Type { get; set; }
        string UnholdDate { get; set; }
        string UnholdSysdate { get; set; }
        int? UnholdUserId { get; set; }
        string Unit { get; set; }

        string GetName();
        bool IsAcknowledged();
        bool IsActive();
        bool IsCancelled();
        bool IsCombo();
        bool IsDeleted();
        bool IsDiscontinued();
        bool IsDrug();
        bool IsFreeText();
        bool IsGiven();
        bool IsOnDiscontinue();
        bool IsOnHold();

        List<IComponent> GetComponents();
    }
}