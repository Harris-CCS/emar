namespace PulseCheck.Database.Management.Data.Type
{
    public class ColumnInfo
    {

        public string COLUMN_NAME { get; set; }
        public string TYPE_NAME { get; set; }
        public string PRECISION { get; set; }

        public bool IsNullable { get; set; }

    }
}
