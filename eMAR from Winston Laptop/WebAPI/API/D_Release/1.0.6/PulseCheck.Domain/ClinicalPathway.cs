using System.Collections.Generic;

namespace PulseCheck.Domain
{
    public class ClinicalPathway
    {
        /// <summary>
        /// Pathway identifier
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// Pathway name
        /// </summary>
        private string _name;
        public string Name {
            get { return this._name.Trim(); }
            set { this._name = value.Trim(); }
        }

        /// <summary>
        /// Pathway status
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Groups within this pathway
        /// </summary>
        public List<Group> Groups { get; set; } = new List<Group>();

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public ClinicalPathway()
        {
        }

        /// <summary>
        /// Create a new pathway with the given id, name, and status
        /// </summary>
        /// <param name="num">Pathway identifier</param>
        /// <param name="name">Pathway name</param>
        /// <param name="status">Pathway status code</param>
        public ClinicalPathway(int num, string name, string status)
        {
            Num = num;
            Name = name;
            Status = Status.GetStatusByCode(status);
        }
    }
}