using System.Collections.Generic;

namespace Emar.Core.OutboundChart.Model
{
    public interface IMedication
    {
        string Authentication { get; set; }
        int? CPTLosecsLink { get; set; }
        string DefaultDose { get; set; }
        string DefaultRoute { get; set; }
        string DefaultUnit { get; set; }
        string Dose { get; set; }
        string GiveDate { get; set; }
        string GiveSysdate { get; set; }
        int? GiveUserId { get; set; }
        string Ibex { get; set; }
        int Id { get; set; }
        string IVLocation { get; set; }
        int? IVSite { get; set; }
        string IVType { get; set; }
        int Losecs { get; set; }
        string Name { get; set; }
        string Notes { get; set; }
        string OrderDate { get; set; }
        int? OrderForUserId { get; set; }
        int? OrderUserId { get; set; }
        string Rate { get; set; }
        string RateUnit { get; set; }
        string Repeat { get; set; }
        string Route { get; set; }
        short Site { get; set; }
        string Status { get; set; }
        string StopDate { get; set; }
        string StopSysdate { get; set; }
        int? StopUserId { get; set; }
        string Time { get; set; }
        string Type { get; set; }
        string Unit { get; set; }
        string GetName();
        bool IsCancelled();
        bool IsCombo();
        bool IsDeleted();
        bool IsDrug();
        bool IsFreeText();
        bool IsGiven();

        List<IComponent> GetComponents();
    }
}