using Emar.Data.Entities;

namespace Emar.Core.ExternalIds.Model.Mappings
{
    public static class ExternalIdMapper
    {
        public static ExternalIdDto MapExternalId(ExternalIdEntity dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            return new ExternalIdDto
            {
                InternalId = dbObj.InternalId,
                Vendor = dbObj.Vendor,
                Entity = dbObj.Entity,
                ExternalId = dbObj.ExternalId
            };
        }
    }
}
