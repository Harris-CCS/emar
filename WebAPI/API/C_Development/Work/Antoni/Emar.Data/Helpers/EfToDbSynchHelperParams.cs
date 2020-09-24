using System.Collections.Generic;

namespace Emar.Data.Helpers
{
    public class EfToDbSynchHelperParams
    {
        public List<string> EntitiesNotMapped { get; set; }
        public List<string> TablesToAdd { get; set; }
        public List<string> ForeignKeysToIgnore { get; set; } 
    }
}