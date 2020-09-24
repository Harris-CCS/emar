using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Emar.Core.Helpers
{
    public static class AppConstants
    {
        public const string ImagesRoute = @"api/images";
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Converts the specified string to title case.
        /// </summary>
        /// <param name="value">The string to convert to title case.</param>
        /// <returns>The specified string converted to title case.</returns>
        public static string ToTitleCase(this string value)
        {
            return (!String.IsNullOrEmpty(value) ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLowerInvariant()) : String.Empty);
        }

        public static string RemoveFirst(this string source, string remove)
        {
            int index = source.IndexOf(remove, StringComparison.Ordinal);
            return (index < 0)
                ? source
                : source.Remove(index, remove.Length);
        }
    }

    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset)
        {
            var CurrentDate = DateTime.UtcNow;
            int Age = CurrentDate.Year - dateTimeOffset.Year;

            if (CurrentDate < dateTimeOffset.AddYears(Age))
            {
                Age--;
            }

            return Age;
        }
    }

    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }

            if (String.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            var orderByString = String.Empty;

            // the orderBy string is separated by ",", so we split it.
            var orderByAfterSplit = orderBy.Split(',');

            //Apply each orderBy clause in reverse order,
            //otherwise the IQueryable will be ordered in the wrong order.
            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                //Trim the orderBy clause, as it might contain leading or trailing spaces.
                //Can't trim the var in foreach, so use another var.
                var trimmedOrderByClause = orderByClause.Trim();

                // if the sort option ends with with " desc", we order
                // descending, otherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                // remove " asc" or " desc" from the orderBy clause, so we 
                // get the property name to look for in the mapping dictionary
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

                // find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                // get the PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                // Run through the property names 
                // so the orderby clauses are applied in the correct order
                foreach (var destinationProperty in
                    propertyMappingValue.DestinationProperties)
                {
                    // revert sort order if necessary
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    orderByString = orderByString +
                        (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                        + destinationProperty
                        + (orderDescending ? " descending" : " ascending");
                }
            }

            return source.OrderBy(orderByString);
        }
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(this IEnumerable<TSource> source, string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // create a list to hold our ExpandoObjects
            var expandoObjectList = new List<ExpandoObject>();

            // create a list with PropertyInfo objects on TSource.  Reflection is
            // expensive, so rather than doing it for each object in the list, we do 
            // it once and reuse the results.  After all, part of the reflection is on the 
            // type of the object (TSource), not on the instance
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the ExpandoObject
                var propertyInfos = typeof(TSource)
                    .GetProperties(
                        BindingFlags.IgnoreCase |
                        BindingFlags.Public |
                        BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // the fields are separated by ",", so we split it.
                var fieldsAfterSplit = fields != null ? fields.Split(',') : new string[] { };

                foreach (var field in fieldsAfterSplit)
                {
                    // trim each field, as it might contain leading 
                    // or trailing spaces. Can't trim the var in foreach,
                    // so use another var.
                    var propertyName = field.Trim();

                    // use reflection to get the property on the source object
                    // we need to include public and instance, b/c specifying a binding 
                    // flag overwrites the already-existing binding flags.
                    var propertyInfo = typeof(TSource)
                        .GetProperty(
                            propertyName,
                            BindingFlags.IgnoreCase |
                            BindingFlags.Public |
                            BindingFlags.Instance);

                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                    }

                    // add propertyInfo to list 
                    propertyInfoList.Add(propertyInfo);
                }
            }

            // run through the source objects
            foreach (TSource sourceObject in source)
            {
                // create an ExpandoObject that will hold the 
                // selected properties & values
                var dataShapedObject = new ExpandoObject();

                // Get the value of each property we have to return.  For that,
                // we run through the list
                foreach (var propertyInfo in propertyInfoList)
                {
                    // GetValue returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    // add the field to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }

                // add the ExpandoObject to the list
                expandoObjectList.Add(dataShapedObject);
            }

            // return the list
            return expandoObjectList;
        }
    }

    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var dataShapedObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the ExpandoObject 
                var propertyInfos = typeof(TSource)
                        .GetProperties(
                            BindingFlags.IgnoreCase |
                            BindingFlags.Public |
                            BindingFlags.Instance);

                foreach (var propertyInfo in propertyInfos)
                {
                    // get the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(source);

                    // add the field to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }

                return dataShapedObject;
            }

            // the field are separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                // trim each field, as it might contain leading 
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var propertyName = field.Trim();

                ////   subfield  - BEGIN -  subfield   ////
                // if the field is a field of a structure then it is separated by ".", so we split it.
                var subFieldArray = field.Split('.');
                var subSourceName = propertyName = subFieldArray.Length > 0 ? subFieldArray[0].Trim() : null;
                ////   subfield  - END -  subfield   ////

                // use reflection to get the property on the source object
                // we need to include public and instance, b/c specifying a 
                // binding flag overwrites the already-existing binding flags.
                var propertyInfo = typeof(TSource)
                    .GetProperty(
                        propertyName,
                        BindingFlags.IgnoreCase |
                        BindingFlags.Public |
                        BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                }

                // get the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(source);

                // add the field to the ExpandoObject
                ((IDictionary<string, object>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);

                ////   subfield  - BEGIN -  subfield   ////
                if (subFieldArray.Length > 1)
                {
                    var subPropertyName = subFieldArray[1]?.Trim();

                    if (((IDictionary<string, object>)dataShapedObject).TryGetValue(subSourceName, out object subSource))
                    {
                        var subPropertyInfo = subSource.GetType()
                            .GetProperty(
                                subPropertyName,
                                BindingFlags.IgnoreCase |
                                BindingFlags.Public |
                                BindingFlags.Instance);

                        if (subPropertyInfo == null)
                        {
                            throw new Exception($"Property {subPropertyName} wasn't found on {subSource.GetType()}");
                        }

                        var subPropertyValue = subPropertyInfo.GetValue(subSource);

                        ((IDictionary<string, object>)dataShapedObject)
                            .Add(subSourceName + "." + subPropertyName, subPropertyValue);
                    }

                    ((IDictionary<string, object>)dataShapedObject)
                        .Remove(propertyInfo.Name);
                }
                ////   subfield  - END -  subfield   ////
            }

            // return the list
            return dataShapedObject;
        }
    }

    public static class NameHelper
    {
        public static string GetDisplayName(string firstName, string middleName, string lastName, string suffix)
        {
            firstName = (firstName ?? "").Trim();
            firstName += firstName.Length == 1 ? "." : "";

            middleName = (middleName ?? "").Trim();
            middleName += middleName.Length == 1 ? "." : "";

            lastName = (lastName ?? "").Trim();
            lastName += lastName.Length == 1 ? "." : "";

            var ret = firstName + (firstName != "" && middleName != "" ? " " : "") + middleName;
            ret += (ret != "" && lastName != "" ? " " : "") + lastName;
            ret += (ret != "" && !string.IsNullOrEmpty(suffix) ? ", " : "") + (suffix ?? "").Trim();

            return ret;
        }
    }


    //// Retiring class - replaced it with properties that have lambdas to standard format strings
    //{
    //    static string defaultDateFormat = @"yyyy-MM-dd";
    //    static string defaultTimeFormat = @"HH:mm:ss";

    //    public static string GetDateTime(DateTimeOffset? dateTime, string dateFormat = null, string timeFormat = null, bool includeTime = true)
    //    {
    //        dateFormat = (dateFormat ?? defaultDateFormat)
    //            // Ensure that the date format returned is properly capitalized.
    //            .Replace(@"YYYY", @"yyyy")
    //            .Replace(@"YY", @"yy")
    //            .Replace(@"mm", @"MM")
    //            .Replace(@"DD", @"dd");

    //        timeFormat ??= defaultTimeFormat;

    //        return dateFormat != null ? dateTime?.ToString(dateFormat + (includeTime ? @" " + timeFormat : @"")) : dateTime?.ToString();
    //    }

    //    public static string GetDate(DateTimeOffset? dateTime, string dateFormat)
    //    {
    //        return GetDateTime(dateTime, dateFormat, null, false);
    //    }

    //    public static string GetTime(DateTimeOffset? dateTime, string timeFormat = null)
    //    {
    //        return dateTime?.ToString(timeFormat ?? defaultTimeFormat);
    //    }
    //}

    public class EmarHttpContext
    {
        private static IHttpContextAccessor m_httpContextAccessor;

        public static HttpContext Current => m_httpContextAccessor.HttpContext;

        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            m_httpContextAccessor = contextAccessor;
        }
    }

    public static class HttpContextExtensions
    {
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
        {
            EmarHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
}
