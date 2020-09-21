using Emar.Core.Medications.Model;

namespace Emar.Core.HomeMedications.Model
{
    public class HomeMedicationDto
    {
        public long Id { get; set; }

        public long? PatientId { get; set; }

        string _class;
        public string Class
        {
            get => _class?.Trim();
            set => _class = value?.Trim();
        }

        string _category;
        public string Category

        {
            get => _category?.Trim();
            set => _category = value?.Trim();
        }

        string _internalDrugId;
        public string InternalDrugId
        {
            get => _internalDrugId?.Trim();
            set => _internalDrugId = value?.Trim();
        }

        string _ndc;
        public string Ndc
        {
            get => _ndc?.Trim();
            set => _ndc = value?.Trim();
        }

        string _drugId;
        public string DrugId
        {
            get => _drugId?.Trim();
            set => _drugId = value?.Trim();
        }

        string _name;
        public string Name
        {
            get => _name?.Trim();
            set => _name = value?.Trim();
        }

        string _alternateName;
        public string AlternateName
        {
            get => _alternateName?.Trim();
            set => _alternateName = value?.Trim();
        }

        public decimal? Dose { get; set; }
        internal int? MedicationUnitId { get; set; }
        public MedicationUnitDto MedicationUnit { get; set; }
        internal int? MedicationRouteId { get; set; }
        public MedicationRouteDto MedicationRoute { get; set; }

        string _medicationDrugId;
        public string MedicationDrugId
        {
            get => _medicationDrugId?.Trim();
            set => _medicationDrugId = value?.Trim();
        }

        public bool IsActive { get; set; }

        string _comment;
        public string Comment
        {
            get => _comment?.Trim();
            set => _comment = value?.Trim();
        }

        public string Schedule { get; set; }

        public string Reaction { get; set; }

        public string Severity { get; set; }

        string _parentDrugName;
        public string ParentDrugName
        {
            get => _parentDrugName?.Trim();
            set => _parentDrugName = value?.Trim();
        }

        //public int AddUserId { get; set; }
        //public DateTimeOffset? AddDatetime { get; set; }
        //public int ChangeUserId { get; set; }
        //public DateTimeOffset? ChangeDatetime { get; set; }

        string _actionStatus;
        public string ActionStatus
        {
            get => _actionStatus?.Trim();
            set => _actionStatus = value?.Trim();
        }
    }
}