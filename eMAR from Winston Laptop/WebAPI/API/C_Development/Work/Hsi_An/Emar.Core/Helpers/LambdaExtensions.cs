using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Emar.Core.Helpers
{
    public enum OperatorComparer
    {
        Contains,
        StartsWith,
        EndsWith,
        Equals = ExpressionType.Equal,
        GreaterThan = ExpressionType.GreaterThan,
        GreaterThanOrEqual = ExpressionType.GreaterThanOrEqual,
        LessThan = ExpressionType.LessThan,
        LessThanOrEqual = ExpressionType.LessThan,
        NotEqual = ExpressionType.NotEqual
    }
    public class SearchParams
    {
        public string PropertyName { get; set; }

        public OperatorComparer ComparerType { get; set; }

        public object PropertyValue { get; set; }

        public string PropertyType { get; set; }

        public string PropertyFormat { get; set; }
    }

    public class ExpressionRebinder : ExpressionVisitor
    {
        /// <summary>
        /// The map
        /// </summary>
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionRebinder"/> class.
        /// </summary>
        /// <param name="map">The map.</param>
        public ExpressionRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        /// <summary>
        /// Replacements the expression.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="exp">The exp.</param>
        /// <returns>Returns replaced expression</returns>
        public static Expression ReplacementExpression(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ExpressionRebinder(map).Visit(exp);
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ParameterExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            ParameterExpression replacement;
            if (this.map.TryGetValue(node, out replacement))
            {
                node = replacement;
            }

            return base.VisitParameter(node);
        }
    }

    /// <summary>
    /// Lambda expression extensions
    /// </summary>
    public static class LambdaExtensions
    {
        /// <summary>
        /// Composes the specified left expression.
        /// </summary>
        /// <typeparam name="T">Param Type</typeparam>
        /// <param name="leftExpression">The left expression.</param>
        /// <param name="rightExpression">The right expression.</param>
        /// <param name="merge">The merge.</param>
        /// <returns>Returns the expression</returns>
        public static Expression<T> Compose<T>(this Expression<T> leftExpression, Expression<T> rightExpression, Func<Expression, Expression, Expression> merge)
        {
            var map = leftExpression.Parameters.Select((left, i) => new
            {
                left,
                right = rightExpression.Parameters[i]
            }).ToDictionary(p => p.right, p => p.left);

            var rightBody = ExpressionRebinder.ReplacementExpression(map, rightExpression.Body);

            return Expression.Lambda<T>(merge(leftExpression.Body, rightBody), leftExpression.Parameters);
        }

        /// <summary>
        /// Performs an "AND" operation
        /// </summary>
        /// <typeparam name="T">Param Type</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Returns the expression</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            if (left == null)
            {
                return right;
            }

            return left.Compose(right, Expression.And);
        }

        /// <summary>
        /// Performs an "OR" operation
        /// </summary>
        /// <typeparam name="T">Param Type</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Returns the expression</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            if (left == null)
            {
                return right;
            }

            return left.Compose(right, Expression.Or);
        }
    }

    public static class ExpressionBuilder
    {
        //public static IQueryable<T> WherePredictBuilder<T>(this IQueryable<T> Source, List<SearchParams> SearchParams)
        //{

        //    try
        //    {
        //        ParameterExpression pe = Expression.Parameter(typeof(T), "t");
        //        List<Expression<Func<T, bool>>> _expressions = new List<Expression<Func<T, bool>>>();
        //        Expression<Func<T, bool>> ep = null;
        //        SearchParams.Select((s, i) => new { i, s }).ToList().ForEach(f =>
        //        {
        //            if (f.i == 0)
        //                ep = BuildCondition<T>(pe, f.s.PropertyName, f.s.ComparerType, f.s.PropertyValue);
        //            else
        //                ep = ep.And<T>(BuildCondition<T>(pe, f.s.PropertyName, f.s.ComparerType, f.s.PropertyValue));
        //        });

        //        MethodCallExpression whereCallExpression = Expression.Call(
        //            typeof(Queryable),
        //            "Where",
        //            new Type[] { Source.ElementType },
        //            Source.Expression,
        //            ep);

        //        IQueryable<T> results = Source.Provider.CreateQuery<T>(whereCallExpression);
        //        var data = results.ToList();
        //        return results;
        //    }
        //    catch (Exception ex)
        //    {
        //        string str = ex.Message;
        //    }
        //    return null;
        //}

        private static Expression<Func<T, bool>> BuildSubQuery<T>(Expression parameter, Type childType, Expression predicate)
        {
            var anyMethod = typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2);
            anyMethod = anyMethod.MakeGenericMethod(childType);
            predicate = Expression.Call(anyMethod, parameter, predicate);
            return MakeLambda<T>(parameter, predicate);
        }

        //private static Expression<Func<T, bool>> BuildCondition<T>(Expression parameter, string property, OperatorComparer comparer, object value)
        //{
        //    var childProperty = parameter.Type.GetProperty(property);
        //    var left = Expression.Property(parameter, childProperty);
        //    var right = Expression.Constant(value.TryCast(left.Type));
        //    var predicate = BuildComparsion(left, comparer, right);
        //    return MakeLambda<T>(parameter, predicate);
        //}

        private static Expression BuildComparsion(Expression left, OperatorComparer comparer, Expression right)
        {
            var mask = new List<OperatorComparer>{
            OperatorComparer.Contains,
            OperatorComparer.StartsWith,
            OperatorComparer.EndsWith
        };
            if (mask.Contains(comparer) && left.Type != typeof(string))
            {
                comparer = OperatorComparer.Equals;
            }
            if (!mask.Contains(comparer))
            {
                return Expression.MakeBinary((ExpressionType)comparer, left, Expression.Convert(right, left.Type));
            }
            return BuildStringCondition(left, comparer, right);
        }

        private static Expression BuildStringCondition(Expression left, OperatorComparer comparer, Expression right)
        {
            var compareMethod = typeof(string).GetMethods().Single(m => m.Name.Equals(Enum.GetName(typeof(OperatorComparer), comparer)) && m.GetParameters().Count() == 1);
            //we assume ignoreCase, so call ToLower on parameter and memberexpression
            var toLowerMethod = typeof(string).GetMethods().Single(m => m.Name.Equals("ToLower") && m.GetParameters().Count() == 0);
            left = Expression.Call(left, toLowerMethod);
            right = Expression.Call(right, toLowerMethod);
            return Expression.Call(left, compareMethod, right);
        }

        private static Expression<Func<T, bool>> MakeLambda<T>(Expression parameter, Expression predicate)
        {
            var resultParameterVisitor = new ParameterVisitor();
            resultParameterVisitor.Visit(parameter);
            var resultParameter = resultParameterVisitor.Parameter;
            return Expression.Lambda<Func<T, bool>>(predicate, (ParameterExpression)resultParameter);
        }

        private class ParameterVisitor : ExpressionVisitor
        {
            public Expression Parameter
            {
                get;
                private set;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                Parameter = node;
                return node;
            }
        }
    }
    public static class QuerableExtensions
    {
        public static IOrderedQueryable<T> DynamicOrderBy<T>(this IQueryable<T> source, string property, bool asc)
        {
            return ApplyOrder<T>(source, property, asc ? "OrderBy" : "OrderByDescending");
        }
        public static IOrderedQueryable<T> DynamicOrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }
        public static IOrderedQueryable<T> DynamicOrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }
        public static IOrderedQueryable<T> DynamicThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }
        public static IOrderedQueryable<T> DynamicThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }
        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
    }
    //public static class IQueryableExtensions
    //{
    //    /// <summary>
    //    /// Gets a single page of items from a sequence.
    //    /// </summary>
    //    /// <typeparam name="T">The data type of the result items.</typeparam>
    //    /// <param name="query">The sequence</param>
    //    /// <param name="pageNumber">The page number to retrieve, starting at 1.</param>
    //    /// <param name="pageSize">The number of items in each page.</param>
    //    /// <param name="pageCount">Provides the total number of pages available.</param>
    //    /// <returns></returns>
    //    public static IEnumerable<T> TakePage<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    //    {
    //        int pageCount;
    //        int itemCount;
    //        return TakePage(query, pageNumber, pageSize, out pageCount, out itemCount);
    //    }

    //    /// <summary>
    //    /// Gets a single page of items from a sequence.
    //    /// </summary>
    //    /// <typeparam name="T">The data type of the result items.</typeparam>
    //    /// <param name="query">The sequence</param>
    //    /// <param name="pageNumber">The page number to retrieve, starting at 1.</param>
    //    /// <param name="pageSize">The number of items in each page.</param>
    //    /// <param name="pageCount">Provides the total number of pages available.</param>
    //    /// <returns></returns>
    //    public static IEnumerable<T> TakePage<T>(this IQueryable<T> query, int pageNumber, int pageSize, out int pageCount)
    //    {
    //        int itemCount;
    //        return TakePage(query, pageNumber, pageSize, out pageCount, out itemCount);
    //    }
    //    /// <summary>
    //    /// Search Extension Method
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="source"></param>
    //    /// <param name="stringProperty"></param>
    //    /// <param name="searchTerm"></param>
    //    /// <returns></returns>
    //    public static IQueryable<T> Search<T>(this IQueryable<T> source, Expression<Func<T, string>> stringProperty, string searchTerm)
    //    {
    //        // Example :
    //        // string searchTerm = "test";
    //        // var results = context.Clubs.Search(club => club.Name, searchTerm).ToList();

    //        if (String.IsNullOrEmpty(searchTerm))
    //        {
    //            return source;
    //        }

    //        // The below represents the following lamda:
    //        // source.Where(x => x.[property] != null
    //        // && x.[property].Contains(searchTerm))

    //        //Create expression to represent x.[property] != null
    //        var isNotNullExpression = Expression.NotEqual(stringProperty.Body, Expression.Constant(null));

    //        //Create expression to represent x.[property].Contains(searchTerm)
    //        var searchTermExpression = Expression.Constant(searchTerm);
    //        var checkContainsExpression = Expression.Call(stringProperty.Body, typeof(string).GetMethod("Contains"), searchTermExpression);

    //        //Join not null and contains expressions
    //        var notNullAndContainsExpression = Expression.AndAlso(isNotNullExpression, checkContainsExpression);

    //        var methodCallExpression = Expression.Call(typeof(Queryable),
    //        "Where",
    //        new Type[] { source.ElementType },
    //        source.Expression,
    //        Expression.Lambda<Func<T, bool>>(notNullAndContainsExpression, stringProperty.Parameters));

    //        return source.Provider.CreateQuery<T>(methodCallExpression);
    //    }

    //    public static IQueryable<T> Sort<T>(this IQueryable<T> Source, string PropertyName, bool Ascending = true)
    //    {
    //        Type dataSourceType = Source.GetType();
    //        Type dataItemType = GetDataItemType(Source);
    //        Type propertyType = dataItemType.GetProperty(PropertyName).PropertyType;

    //        Type sorterType = typeof(IQueryableUtil<,>).MakeGenericType(dataItemType, propertyType);

    //        return (IQueryable<T>)sorterType.GetMethod("Sort", new Type[] { dataSourceType, typeof(string), typeof(bool) })
    //        .Invoke(null, new object[] { Source, PropertyName, Ascending });
    //    }

    //    public static Expression<Func<TClass, TProperty>> Build<TClass, TProperty>(string fieldName)
    //    {
    //        var param = Expression.Parameter(typeof(TClass));
    //        var field = Expression.PropertyOrField(param, fieldName);
    //        return Expression.Lambda<Func<TClass, TProperty>>(field, param);
    //    }

    //    public static Expression<Func<T, bool>> SearchAllFields<T>(string searchText)
    //    {
    //        var t = Expression.Parameter(typeof(T));
    //        Expression body = Expression.Constant(false);

    //        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
    //        var toStringMethod = typeof(object).GetMethod("ToString");

    //        var stringProperties = typeof(T).GetProperties()
    //            .Where(property => property.PropertyType == typeof(string));

    //        foreach (var property in stringProperties)
    //        {
    //            var stringValue = Expression.Call(Expression.Property(t, property.Name),
    //                toStringMethod);
    //            var nextExpression = Expression.Call(stringValue,
    //                containsMethod,
    //                Expression.Constant(searchText));

    //            body = Expression.OrElse(body, nextExpression);
    //        }

    //        return Expression.Lambda<Func<T, bool>>(body, t);
    //    }

    //    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
    //    {
    //        return ApplyOrder<T>(source, property, "OrderBy");
    //    }
    //    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
    //    {
    //        return ApplyOrder<T>(source, property, "OrderByDescending");
    //    }
    //    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
    //    {
    //        return ApplyOrder<T>(source, property, "ThenBy");
    //    }
    //    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
    //    {
    //        return ApplyOrder<T>(source, property, "ThenByDescending");
    //    }
    //    static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
    //    {
    //        string[] props = property.Split('.');
    //        Type type = typeof(T);
    //        ParameterExpression arg = Expression.Parameter(type, "x");
    //        Expression expr = arg;
    //        foreach (string prop in props)
    //        {
    //            // use reflection (not ComponentModel) to mirror LINQ
    //            PropertyInfo pi = type.GetProperty(prop);
    //            expr = Expression.Property(expr, pi);
    //            type = pi.PropertyType;
    //        }
    //        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
    //        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

    //        object result = typeof(Queryable).GetMethods().Single(
    //                method => method.Name == methodName
    //                        && method.IsGenericMethodDefinition
    //                        && method.GetGenericArguments().Length == 2
    //                        && method.GetParameters().Length == 2)
    //                .MakeGenericMethod(typeof(T), type)
    //                .Invoke(null, new object[] { source, lambda });
    //        return (IOrderedQueryable<T>)result;
    //    }

    //    /// <summary>
    //    /// Search Extension Method
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="source"></param>
    //    /// <param name="stringProperty"></param>
    //    /// <param name="searchTerm"></param>
    //    /// <returns></returns>
    //    public static IQueryable<T> Searchz<T>(this IQueryable<T> source, string property, string searchTerm)
    //    {
    //        Expression<Func<T, string>> stringProperty = Build<T, string>(property);
    //        if (String.IsNullOrEmpty(searchTerm))
    //        {
    //            return source;
    //        }

    //        // The below represents the following lambda:
    //        // source.Where(x => x.[property] != null
    //        // && x.[property].Contains(searchTerm))

    //        //Create expression to represent x.[property] != null
    //        var isNotNullExpression = Expression.NotEqual(stringProperty.Body, Expression.Constant(null));

    //        //Create expression to represent x.[property].Contains(searchTerm)
    //        var searchTermExpression = Expression.Constant(searchTerm);
    //        var checkContainsExpression = Expression.Call(stringProperty.Body, typeof(string).GetMethod("Contains"), searchTermExpression);

    //        //Join not null and contains expressions
    //        var notNullAndContainsExpression = Expression.AndAlso(isNotNullExpression, checkContainsExpression);

    //        var methodCallExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { source.ElementType }, source.Expression,
    //            Expression.Lambda<Func<T, bool>>(notNullAndContainsExpression, stringProperty.Parameters));

    //        return source.Provider.CreateQuery<T>(methodCallExpression);
    //    }

    //    /// <summary>
    //    /// Gets a single page of items from a sequence.
    //    /// </summary>
    //    /// <typeparam name="T">The data type of the result items.</typeparam>
    //    /// <param name="query">The sequence</param>
    //    /// <param name="pageNumber">The page number to retrieve, starting at 1.</param>
    //    /// <param name="pageSize">The number of items in each page.</param>
    //    /// <param name="pageCount">Provides the total number of pages available.</param>
    //    /// <param name="itemCount">Provides the total number of items available.</param>
    //    /// <returns></returns>
    //    public static IEnumerable<T> TakePage<T>(this IQueryable<T> query, int pageNumber, int pageSize, out int pageCount, out int itemCount)
    //    {
    //        if (pageNumber < 1)
    //            throw new ArgumentException("The value for 'page' must be greater than or equal to 1", "pageNumber");

    //        itemCount = query.Count();

    //        pageCount = (int)Math.Ceiling((double)itemCount / (double)pageSize);

    //        //if (pageNumber > pageCount)
    //        // pageNumber = pageCount;
    //        if (pageNumber > pageCount)
    //            return query.Take(0);
    //        if (pageNumber > 1)
    //            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    //        else
    //            return query.Take(pageSize);
    //    }

    //    public static Type GetDataItemType(this IQueryable DataSource)
    //    {
    //        Type dataSourceType = DataSource.GetType();
    //        Type dataItemType = typeof(object);

    //        if (dataSourceType.HasElementType)
    //        {
    //            dataItemType = dataSourceType.GetElementType();
    //        }
    //        else if (dataSourceType.IsGenericType)
    //        {
    //            dataItemType = dataSourceType.GetGenericArguments()[0];
    //        }
    //        else if (DataSource is IEnumerable)
    //        {
    //            IEnumerator dataEnumerator = DataSource.GetEnumerator();

    //            if (dataEnumerator.MoveNext() && dataEnumerator.Current != null)
    //            {
    //                dataItemType = dataEnumerator.Current.GetType();
    //            }
    //        }

    //        return dataItemType;
    //    }

    //    public static int GetTotalRowCount(this IQueryable Source)
    //    {
    //        Type dataSourceType = Source.GetType();
    //        Type dataItemType = GetDataItemType(Source);

    //        Type rowCounterType = typeof(IQueryableUtil<>).MakeGenericType(dataItemType);

    //        return (int)rowCounterType.GetMethod("Count", new Type[] { dataSourceType })
    //        .Invoke(null, new object[] { Source });
    //    }

    //    public static IQueryable Contains(this IQueryable Source, string PropertyName, string SearchClause)
    //    {
    //        Type dataSourceType = Source.GetType();
    //        Type dataItemType = GetDataItemType(Source);
    //        Type propertyType = dataItemType.GetProperty(PropertyName).PropertyType;

    //        Type containsType = typeof(IQueryableUtil<,>).MakeGenericType(dataItemType, propertyType);

    //        return (IQueryable)containsType.GetMethod("Contains", new Type[] { dataSourceType, typeof(string), typeof(string) })
    //        .Invoke(null, new object[] { Source, PropertyName, SearchClause });
    //    }

    //    public static IQueryable Page(this IQueryable Source, int PageSize, int CurrentPage)
    //    {
    //        Type dataSourceType = Source.GetType();
    //        Type dataItemType = GetDataItemType(Source);

    //        Type containsType = typeof(IQueryableUtil<>).MakeGenericType(dataItemType);

    //        IQueryable query = (IQueryable)containsType.GetMethod("Page", new Type[] { dataSourceType, typeof(int), typeof(int) })
    //        .Invoke(null, new object[] { Source, PageSize, CurrentPage });
    //        return query;
    //    }

    //    public static IQueryable Sort(this IQueryable Source, string PropertyName, bool Ascending)
    //    {
    //        Type dataSourceType = Source.GetType();
    //        Type dataItemType = GetDataItemType(Source);
    //        Type propertyType = dataItemType.GetProperty(PropertyName).PropertyType;

    //        Type sorterType = typeof(IQueryableUtil<,>).MakeGenericType(dataItemType, propertyType);

    //        return (IQueryable)sorterType.GetMethod("Sort", new Type[] { dataSourceType, typeof(string), typeof(bool) })
    //            .Invoke(null, new object[] { Source, PropertyName, Ascending });
    //    }

    //    internal static class IQueryableUtil<T>
    //    {
    //        public static int Count(IQueryable Source)
    //        {
    //            return Source.OfType<T>().AsQueryable<T>().Count<T>();
    //        }

    //        public static IQueryable Page(IQueryable Source, int PageSize, int PageIndex)
    //        {
    //            IQueryable query = Source.OfType<T>().AsQueryable<T>().Skip<T>(PageSize * (PageIndex - 1)).Take<T>(PageSize);
    //            return query;
    //        }
    //    }

    //    internal static class IQueryableUtil<T, PT>
    //    {
    //        public static IQueryable Sort(IQueryable source, string sortExpression, bool Ascending)
    //        {
    //            var param = Expression.Parameter(typeof(T), "item");

    //            var sortLambda = Expression.Lambda<Func<T, PT>>(Expression.Convert(Expression.Property(param, sortExpression), typeof(PT)), param);

    //            if (Ascending)
    //            {
    //                // (o=> 0.sortExpression)
    //                return source.OfType<T>().AsQueryable<T>().OrderBy<T, PT>(sortLambda);
    //            }
    //            else
    //            {
    //                return source.OfType<T>().AsQueryable<T>().OrderByDescending<T, PT>(sortLambda);
    //            }
    //        }

    //        public static IQueryable Contains(IQueryable Source, string PropertyName, string SearchClause)
    //        {
    //            var entityParam = Expression.Parameter(typeof(T), "item");

    //            //var containsLambda = Expression.Lambda<Func<T, PT>>(Expression.Convert(Expression.Property(param, PropertyName), typeof(PT)), param);

    //            MemberExpression memberExpression = Expression.Property(entityParam, PropertyName);
    //            Expression convertExpression = Expression.Convert(memberExpression, typeof(object));
    //            ConstantExpression searchClauseParam = Expression.Constant(SearchClause, typeof(string));

    //            MethodCallExpression containsExpression = Expression.Call(memberExpression, "Contains", new Type[] { }, new Expression[] { searchClauseParam });

    //            var containsLambda = Expression.Lambda<Func<T, bool>>(containsExpression, entityParam);
    //            return Source.OfType<T>().AsQueryable<T>().Where<T>(containsLambda);
    //        }
    //    }
    //}
}