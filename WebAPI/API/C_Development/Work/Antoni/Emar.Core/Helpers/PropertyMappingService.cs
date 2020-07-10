using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Orders.Model;
using Emar.Core.Patients.Model;
using Emar.Data.Entities;

namespace Emar.Core
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _propertyMappingPatient =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string>() {"Id" } )},
                {"FullName", new PropertyMappingValue(new List<string>() {"FirstName", "LastName" } )},
                {"Age", new PropertyMappingValue(new List<string>() { "DateOfBirth" }, true )},
                {"DepartmentCode", new PropertyMappingValue(new List<string>() { "DepartmentCode" } )},
                {"WardCode", new PropertyMappingValue(new List<string>() { "WardCode" } )},
                {"RoomBedCode", new PropertyMappingValue(new List<string>() { "RoomBedCode" } )}
            };

        private Dictionary<string, PropertyMappingValue> _propertyMappingOrder =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string>() {"Id" } )},
                {"Priority", new PropertyMappingValue(new List<string>() { "Priority" } )},
                {"OrderStatus", new PropertyMappingValue(new List<string>() { "OrderStatus" }, true )},
                {"Begin", new PropertyMappingValue(new List<string>() { "BeginDateTime" } )},
                {"BeginDate", new PropertyMappingValue(new List<string>() { "BeginDateTime" } )},
                {"BeginTime", new PropertyMappingValue(new List<string>() { "BeginDateTime" } )}
                //{"OrderingProvider", new PropertyMappingValue(new List<string>() { "OrderingProvider" } )}
            };

        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<PatientDto, Patient>(_propertyMappingPatient));
            _propertyMappings.Add(new PropertyMapping<PatientOrderDto, PatientOrder>(_propertyMappingOrder));
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (String.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            //the string could be separated by ",", so we split it
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();

                //remove everything after first " "
                //if the fields are coming from an orderBy string,
                //this part must be ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace.Equals(-1) ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

                //find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count().Equals(1))
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}");
        }
    }
}
