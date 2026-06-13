using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.Templates.Model;
using Emar.Data.Entities;

namespace Emar.Core.OutboundChart.Model
{
    public class OcsChartParameters
    {
        public byte site { get; set; }
        public Patient patient { get; set; }
        public int patiendId { get; set; }
        public List<OrderMedication> orders { get; set; }
        public int orderingPhysicianId { get; set; }
        public string ibex { get; set; }
        public int user { get; set; }
        public int medicationId { get; set; }
        public DateTimeOffset losecs { get; set; }
        public bool userQuicklistOrder { get; set; }
        public int interactionOverrideReasonId { get; set; }
        public int reactionOverrideReasonId { get; set; }
        public string medNotes { get; set; }
        public ICollection<OrderInteraction> orderInteractions { get; set; }
        public ICollection<OrderReaction> orderReactions { get; set; }
        public ICollection<AllergyReactionView> allergyReactionView { get; set; }
        public string Dose { get; set; }
        public string Route { get; set; }
        public string Unit { get; set; }
        public int FrequencyId { get; set; }
        public string? Duration { get; set; }
        public int? DurationId { get; set; }
        public long patientCartOrderId { get; set; }
        public long patientOrderId { get; set; }
        public string OrderDate { get; set; }
        public byte PharmVerifStatus { get; set; }
    }

    public class OcsPromptParameters
    {
        public int promptId { get; set; }
        public PromptType promptType { get; set; }
        public string promptLabel { get; set; }
        public string promptValue { get; set; }
        public string chartMarkup { get; set; }
    }
}

