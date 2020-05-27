namespace PulseCheck.Database.Management.Logic.Type
{
    public class SyncTableRequest
    {
        public string SourceConnectionStringName { get; set; }
        public string SourceTableName { get; set; }

        public string TargetConnectionStringName { get; set; }
        public string TargetTableName { get; set; }


    }
}
