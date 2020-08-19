using System;
using Emar.Core.Users.Model;

namespace Emar.Core.Patients.Model
{
    public class PatientAllergyDto
    {
        public long Id { get; set; }

        public long? PatientId { get; set; }

        private string _class;
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

        string _allergyDrugId;
        public string AllergyDrugId
        {
            get => _allergyDrugId?.Trim();
            set => _allergyDrugId = value?.Trim();
        }

        public bool IsActive { get; set; }

        string _comment;
        public string Comment
        {
            get => _comment?.Trim();
            set => _comment = value?.Trim();
        }

        string _schedule;
        public string Schedule
        {
            get => _schedule?.Trim();
            set => _schedule = value?.Trim();
        }

        string _reaction;
        public string Reaction
        {
            get => _reaction?.Trim();
            set => _reaction = value?.Trim();
        }

        string _severity;
        public string Severity
        {
            get => _severity?.Trim();
            set => _severity = value?.Trim();
        }

        string _parentDrugId;
        public string ParentDrugId
        {
            get => _parentDrugId?.Trim();
            set => _parentDrugId = value?.Trim();
        }

        string _parentDrugName;
        public string ParentDrugName
        {
            get => _parentDrugName?.Trim();
            set => _parentDrugName = value?.Trim();
        }

        public int AddUserId { get; set; }

        public DateTimeOffset? AddDatetime { get; set; }

        public int ChangeUserId { get; set; }

        public DateTimeOffset? ChangeDatetime { get; set; }

        string _actionStatus;
        public string ActionStatus
        {
            get => _actionStatus?.Trim();
            set => _actionStatus = value?.Trim();
        }

        string _informationSource;
        public string InformationSource
        {
            get => _informationSource?.Trim();
            set => _informationSource = value?.Trim();
        }

        string _personNumber;
        public string PersonNumber
        {
            get => _personNumber?.Trim();
            set => _personNumber = value?.Trim();
        }

        string _accountNumber;
        public string AccountNumber
        {
            get => _accountNumber?.Trim();
            set => _accountNumber = value?.Trim();
        }

        public UserDto AddUser { get; set; }

        public UserDto ChangeUser { get; set; }
    }
}
