#if PAGING || SORTING || EXPANDO
using System.Collections.Generic;

namespace Emar.Core.Patients.Service
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}
#endif
