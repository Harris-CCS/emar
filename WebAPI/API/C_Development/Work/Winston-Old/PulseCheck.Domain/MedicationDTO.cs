using System;
using System.Collections.Generic;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Medication data transfer object, because there is a lot in Medication that consumers don't care about
    /// </summary>
    public class MedicationDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Ibex { get; set; }
        public Int16 Site { get; set; }
        public int Losecs { get; set; }
        public string Route { get; set; }
        public string Unit { get; set; }
        public string Dose { get; set; }
        public string Schedule { get; set; }
        public string Time { get; set; }
        public string Repeat { get; set; }
        public string Notes { get; set; }

        DateTime? _orderDate;
        public DateTime? OrderDate { get { return _orderDate != null ? _orderDate.Value.ToUniversalTime() : _orderDate; } set { _orderDate = value; } }
        DateTime? _ackDate;
        public DateTime? AckDate { get { return _ackDate != null ? _ackDate.Value.ToUniversalTime() : _ackDate; } set { _ackDate = value; } }
        DateTime? _holdDate;
        public DateTime? HoldDate { get { return _holdDate != null ? _holdDate.Value.ToUniversalTime() : _holdDate; } set { _holdDate = value; } }
        DateTime? _holdSysDate;
        public DateTime? HoldSysdate { get { return _holdSysDate != null ? _holdSysDate.Value.ToUniversalTime() : _holdSysDate; } set { _holdSysDate = value; } }
        DateTime? _unholdDate;
        public DateTime? UnholdDate { get { return _unholdDate != null ? _unholdDate.Value.ToUniversalTime() : _unholdDate; } set { _unholdDate = value; } }
        DateTime? _unholdSysDate;
        public DateTime? UnholdSysdate { get { return _unholdSysDate != null ? _unholdSysDate.Value.ToUniversalTime() : _unholdSysDate; } set { _unholdSysDate = value; } }
        DateTime? _cancelDate;
        public DateTime? CancelDate { get { return _cancelDate != null ? _cancelDate.Value.ToUniversalTime() : _cancelDate; } set { _cancelDate = value; } }
        DateTime? _cancelSysDate;
        public DateTime? CancelSysdate { get { return _cancelSysDate != null ? _cancelSysDate.Value.ToUniversalTime() : _cancelSysDate; } set { _cancelSysDate = value; } }
        DateTime? _deleteDate;
        public DateTime? DeleteDate { get { return _deleteDate != null ? _deleteDate.Value.ToUniversalTime() : _deleteDate; } set { _deleteDate = value; } }
        DateTime? _giveDate;
        public DateTime? GiveDate { get { return _giveDate != null ? _giveDate.Value.ToUniversalTime() : _giveDate; } set { _giveDate = value; } }
        DateTime? _giveSysDate;
        public DateTime? GiveSysdate { get { return _giveSysDate != null ? _giveSysDate.Value.ToUniversalTime() : _giveSysDate; } set { _giveSysDate = value; } }
        DateTime? _stopDate;
        public DateTime? StopDate { get { return _stopDate != null ? _stopDate.Value.ToUniversalTime() : _stopDate; } set { _stopDate = value; } }
        DateTime? _stopSysDate;
        public DateTime? StopSysdate { get { return _stopSysDate != null ? _stopSysDate.Value.ToUniversalTime() : _stopSysDate; } set { _stopSysDate = value; } }
        DateTime? _discontinueDate;
        public DateTime? DiscontinueDate { get { return _discontinueDate != null ? _discontinueDate.Value.ToUniversalTime() : _discontinueDate; } set { _discontinueDate = value; } }
        DateTime? _discontinuedDate;
        public DateTime? DiscontinuedDate { get { return _discontinuedDate != null ? _discontinuedDate.Value.ToUniversalTime() : _discontinuedDate; } set { _discontinuedDate = value; } }
        DateTime? _discontinuteSysDate;
        public DateTime? DiscontinueSysdate { get { return _discontinuteSysDate != null ? _discontinuteSysDate.Value.ToUniversalTime() : _discontinuteSysDate; } set { _discontinuteSysDate = value; } }
        DateTime? _discontinuedSysDate;
        public DateTime? DiscontinuedSysdate { get { return _discontinuedSysDate != null ? _discontinuedSysDate.Value.ToUniversalTime() : _discontinuedSysDate; } set { _discontinuedSysDate = value; } }

        public MinimalUser OrderForUser { get; set; }
        public MinimalUser OrderUser { get; set; }
        public MinimalUser AckUser { get; set; }
        public MinimalUser HoldUser { get; set; }
        public MinimalUser UnholdUser { get; set; }
        public MinimalUser CancelUser { get; set; }
        public MinimalUser DeleteUser { get; set; }
        public MinimalUser GiveUser { get; set; }
        public MinimalUser StopUser { get; set; }
        public MinimalUser ExcludeUser { get; set; }
        public MinimalUser DiscontinueUser { get; set; }
        public MinimalUser DiscontinuedUser { get; set; }

        public string IVType { get; set; }
        public int? IVSite { get; set; }
        public string IVLocation { get; set; }

        public string Rate { get; set; }
        public string RateUnit { get; set; }
        public string Indication { get; set; }
        
        public List<Component> Components { get; set; }

        /// <summary>
        /// Default MedicationDTO constructor
        /// </summary>
        public MedicationDTO()
        {
            Components = new List<Component>();
        }

        /// <summary>
        /// Medication Component class
        /// </summary>
        public class Component
        {
            public int Id { get; set; }
            public string BrandName { get; set; }
            public string ActiveName { get; set; }
            public string DrugRoute { get; set; }
            public string DrugForm { get; set; }
            public string DrugStrength { get; set; }
            public string EnteredDose { get; set; }
            public string EnteredUnit { get; set; }
            public string DrugDBType { get; set; }
            public string ActiveId { get; set; }
            public string DrugId { get; set; }
            public string PackagingId { get; set; }
            public string DrugCategoryId { get; set; }
            public string Type { get; set; }
            public string DrugFormId { get; set; }
            public string GroupName { get; set; }
            public string GroupType { get; set; }
            public string ProductCode { get; set; }
            public string ProcedureCode { get; set; }

            public List<Dictionary<string, string>> Interactions { get; set; }
            public List<Dictionary<string, string>> Reactions { get; set; }

            /// <summary>
            /// Default MedicationDTO Component constructor
            /// </summary>
            public Component()
            {

            }
        }
    }
}
 