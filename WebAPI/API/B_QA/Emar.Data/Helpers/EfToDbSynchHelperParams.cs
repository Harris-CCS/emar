using System.Collections.Generic;

namespace Emar.Data.Helpers
{
    public class EfToDbSynchHelperParams
    {
        public List<string> EntitiesNotMapped { get; set; }
        public List<string> TablesToAdd { get; set; }
        public List<string> ForeignKeysToIgnore { get; set; } 
        public List<ManufacturedKey> ManufacturedKeys { get; set; }
    }

    public class ManufacturedKey
    {
        public string Table { get; set; }
        public string KeyColumns { get; set; }
    }
}