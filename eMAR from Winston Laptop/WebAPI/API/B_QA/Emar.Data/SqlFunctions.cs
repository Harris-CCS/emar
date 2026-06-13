using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Emar.Data
{
    public static class SqlFunctions
    {
        public static string ToString(this decimal? value, int? length, int? decimalArg) => throw new NotSupportedException();
        public static string ToString(this double? value, int? length, int? decimalArg) => throw new NotSupportedException();
        public static ModelBuilder AddSqlFunctions(this ModelBuilder modelBuilder) => modelBuilder
            .MapToStr(() => ToString(default(decimal?), null, null))
            .MapToStr(() => ToString(default(double?), null, null));
        static ModelBuilder MapToStr(this ModelBuilder modelBuilder, Expression<Func<string>> method)
        {
            modelBuilder.HasDbFunction(method).HasTranslation(args =>
                new SqlFunctionExpression(null, null, "STR", false, args, true, typeof(string), null));
            return modelBuilder;
        }
    }
}
