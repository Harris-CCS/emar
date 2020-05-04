using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    public class Area
    {
        public Area()
        {
            _name = "";
            _type = "";            
            Status = new Status();
            //Patients = new List<Patient>();
        }

        [Key, Column(Order = 0)]
        public string Ward { get; set; }
        [Key, Column(Order = 1)]
        public string Dept { get; set; }
        [Key, Column(Order = 2)]
        public byte SiteId { get; set; }

        private string _name;
        public string Name
        {
            get { return this._name.Trim(); }
            set { this._name = value.Trim(); }
        }

        public Status Status { get; set; }

        private string _type;
        public string Type
        {
            get { return this._type.Trim(); }
            set { this._type = value.Trim(); }
        }

        ////public string _id;
        ////public string Id
        ////{
        ////    get { return this._id.Trim(); }
        ////    set { this._id = value.Trim(); }
        ////}

        //[NotMapped]
        //public List<Patient> Patients { get; set; }
    }

    //public class BedArea : Area
    //{
    //    public List<Bed> Beds { get; set; }
    //}
}