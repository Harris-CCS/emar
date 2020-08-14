using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Emar.Data.Helpers
{
    public class EfToDbSynchHelper
    {
        private readonly EmarContext _context;

        public EfToDbSynchHelper(EmarContext context)
        {
            _context = context;
        }

        public EfDiscrepancyReportDto CompareEfToDb()
        {
            List<EfTableAttributes> tables = SurveyEfEntities();

            if (ProblemsExitInEfDefinitions(tables, out EfDiscrepancyReportDto report))
                return report;

            using (SqlConnection conn = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                conn.Open();
                foreach (var tbl in tables)
                {
                    using (SqlCommand comm = new SqlCommand(string.Format(COLUMN_QUERY, tbl.SqlTableName), conn))
                    {
                        using (var reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReadTheReader(reader, out string colName, out string sqlDataType, out bool sqlNullable,
                                    out bool primaryKey);
                                // Find the record in the list of columns
                                var col = tbl.Columns.FirstOrDefault(c => c.SqlName == colName);
                                if (col?.SqlName == null)
                                    RegisterMissingColumn(tbl.SqlTableName, tbl.EntityName, colName, 
                                        sqlDataType, sqlNullable, primaryKey, report);
                                else
                                    ConfirmColumnPropertiesMatchSql(tbl, col, sqlDataType, sqlNullable,
                                        primaryKey, report);
                            }
                        }
                    }
                }
            }

            if (report.Tables.Count == 0)
                return null;
            return report;
        }

        private void ReadTheReader(SqlDataReader reader, out string colName, out string dataType, out bool nullable,
            out bool primaryKey)
        {
            colName = reader["name"].ToString();
            dataType = reader["datatype"].ToString();
            nullable = Convert.ToByte(reader["is_nullable"]) != 0;
            primaryKey= Convert.ToByte(reader["KeyColumn"]) != 0;
        }

        private bool ProblemsExitInEfDefinitions(List<EfTableAttributes> tables, out EfDiscrepancyReportDto report)
        {
            report = new EfDiscrepancyReportDto();

            string errorTable = "<missing>";
            string errorColumn = "<missing>";
            try
            {
                foreach (var table in tables)
                {
                    errorTable = table.EntityName;
                    foreach (var column in table.Columns)
                    {
                        errorColumn = column.ClrName;
                        ConfirmColumnPropertiesMatchSql(table, column, column.SqlDataType,
                            !column.Required, column.KeyColumn, report);
                    }
                }
            }
            catch (NotImplementedException e)
            {
                throw new NotImplementedException($"When looking for problems in '{errorTable}'.'{errorColumn}'", e);
            }

            return report.Tables.Any();
        }

        private List<EfTableAttributes> SurveyEfEntities()
        {
            var tbls = new List<EfTableAttributes>();
            foreach (IEntityType entity in _context.Model.GetEntityTypes().OrderBy(a=>a.Name))
            {
                var columns = new List<EfColumnAttributes>();
                foreach (IProperty iProp in entity.GetProperties())
                {
                    var col = new EfColumnAttributes
                    {
                        ClrName = iProp.Name,
                        Required = !iProp.IsNullable,
                        IsUnicode = iProp.IsUnicode(),
                        ClrDataType = iProp.PropertyInfo.PropertyType,
                        SqlDataType = iProp.GetColumnType(),
                        SqlName = iProp.GetColumnName(),
                        MaxStringLength = iProp.GetMaxLength(),
                        IsFixWidth = iProp.IsFixedLength()
                    };
                    if (iProp.IsPrimaryKey() && !iProp.IsKey())
                        throw new AmbiguousImplementationException("Found Primary Key not marked as \"Key\" in SurveyEfEntities()");
                    if (iProp.IsKey())
                        col.SetKeyColumn();
                    columns.Add(col);
                }

                var foreignKeys = new List<EfForeignKeyAttributes>();
                //foreach (var fk in entity.GetForeignKeys())
                //{
                // EfForeignKeyAttributes fkObj = new EfForeignKeyAttributes();
                // var DeclaringEntityType = fk.DeclaringEntityType;
                // var DeleteBehavior = fk.DeleteBehavior;
                // var DependentToPrincipal = fk.DependentToPrincipal;
                // var IsOwnership = fk.IsOwnership;
                // var IsRequired = fk.IsRequired;
                // var IsUnique = fk.IsUnique;
                // var PrincipalEntityType = fk.PrincipalEntityType;
                // var PrincipalKey = fk.PrincipalKey;
                // var PrincipalToDependent = fk.PrincipalToDependent;
                // var Properties = fk.Properties;

                // fkObj.Property = fk.DependentToPrincipal.Name;
                // fkObj.DataType = fk.PrincipalEntityType.ClrType;
                // IEnumerable<INavigation> y = fk.FindNavigationsFrom(entity);

                //    foreignKeys.Add(fkObj);


                    //private EfForeignKeyAttributes SurveyForeignKey(PropertyInfo prop, List<CustomAttributeData> fkAttributes)
                    //{
                    //    var fk = new EfForeignKeyAttributes { Name = prop.Name, DataType = prop.PropertyType };

                    //    foreach (var fkAttribute in fkAttributes)
                    //    {
                    //        if (fkAttribute.AttributeType == typeof(InversePropertyAttribute))
                    //            fk.InversePropertyArgument = fkAttribute.ConstructorArguments[0].ToString().Trim('\"');
                    //        else if (fkAttribute.AttributeType == typeof(ForeignKeyAttribute))
                    //            fk.ForeignKeyArgument = fkAttribute.ConstructorArguments[0].ToString().Trim('\"');
                    //        else
                    //        {
                    //        }
                    //    }

                    //    return fk;
                    //}
            //}

                tbls.Add(
                    new EfTableAttributes
                    {
                        EntityName = entity.Name,
                        SqlTableName = entity.GetTableName(),
                        Columns = columns,
                        ForeignKeys = foreignKeys
                    });
            }

            return tbls;
        }

        private static void ConfirmColumnPropertiesMatchSql(EfTableAttributes tbl, EfColumnAttributes col, 
            string sqlDataType, bool? sqlNullable, bool keyColumn, EfDiscrepancyReportDto report)
        {
            ReportProblem error = ReportProblem.None;

            if (EntityColumnTypeNotMatchSql(sqlDataType, sqlNullable, col.ClrDataType, col.IsUnicode, 
                col.MaxStringLength, out string probDetails))
                error = ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype;
            else if (col.KeyColumn != keyColumn)
                error = keyColumn
                    ? ReportProblem.PropertyNotFlaggedAsKey
                    : ReportProblem.PropertyImproperlyFlaggedAsKey;
            else if (col.Required == sqlNullable)
                error = sqlNullable ?? true
                    ? ReportProblem.PropertyFlaggedRequiredButColumnNullable
                    : ReportProblem.PropertyNotFlaggedRequiredButColumnNotNullable;

            if (error == ReportProblem.None) return;
            report.RegisterProblem(tbl, col, error, probDetails);
        }

        private void RegisterMissingColumn(string sqlTableName, string entityName, string sqlColName, string sqlDataType, 
            bool nullable, bool keyColumn, EfDiscrepancyReportDto report)
        {
            var clrType = SqlToClrDataType(sqlDataType, out bool? unicode);
            var propertyName = PascalNameFromSqlName(sqlColName);

            Dictionary<string, string> correctionCode = EfDiscrepancyColumnDto.CreateCorrectionCode(entityName, sqlColName, 
                sqlDataType, keyColumn, nullable, clrType, propertyName, unicode);

            report.RegisterProblem(sqlTableName, sqlColName, ReportProblem.DbColumnMissing,
                correctionCode);
        }

        private static string PascalNameFromSqlName(string sqlName)
        {
            var propertyName = sqlName.Substring(0, 1).ToUpper() + sqlName.Substring(1).ToLower();
            int idx ;
            while ((idx = propertyName.IndexOf('_')) > -1)
            {
                propertyName = propertyName.Substring(0, idx)
                               + propertyName.Substring(idx + 1, 1).ToUpper()
                               + propertyName.Substring(idx + 2);
            }

            return propertyName;
        }

/*
        private ColumnAttributes SurveyColumn(PropertyInfo prop, List<CustomAttributeData> attributes)
        {
            if (prop == null) throw new ArgumentNullException(nameof(prop));

            var col = new ColumnAttributes {ClrDataType = prop.PropertyType, ClrName = prop.Name};

            var opt = prop.Attributes;
            var req = prop.CustomAttributes;
            
            if (col.ClrDataType == typeof(string))
                col.IsUnicode = prop.PropertyType.UnderlyingSystemType.IsUnicodeClass;
            //var b = PropertyExtensions.IsUnicode(prop);

            foreach (Attribute attribute in prop.GetCustomAttributes())
            {
                if (attribute.TypeId == typeof(KeyAttribute))
                    col.SetKeyColumn();
                else if (attribute.TypeId == typeof(ColumnAttribute))
                {
                    var colAttribue = (ColumnAttribute) attribute;

                    col.SqlName = colAttribue.Name;
                    if (((ColumnAttribute) attribute).TypeName != null)
                        col.SqlDataType = colAttribue.TypeName;
                }
                else if (attribute.TypeId == typeof(RequiredAttribute))
                    col.Required = true;
                else if (attribute.TypeId == typeof(StringLengthAttribute))
                    col.MaxStringLength = ((StringLengthAttribute) attribute).MaximumLength;
                else
                {
                }
            }
            //foreach (var x in attributes)
            //{
            //    if (x.AttributeType == typeof(KeyAttribute))
            //        col.SetKeyColumn();
            //    else if (x.AttributeType == typeof(ColumnAttribute))
            //    {
            //        ParseColumnAttribute(x, out string name, out string type);
            //        col.SqlName = name;
            //        if (type != null) col.SqlDataType = type;
            //    }
            //    else if (x.AttributeType == typeof(RequiredAttribute))
            //        col.Required = true;
            //    else if (x.AttributeType == typeof(StringLengthAttribute))
            //        col.MaxStringLength =
            //            Convert.ToInt32(x.ConstructorArguments[0].ToString().Split(')')[1]);
            //    else
            //    {
            //    }
            //}

            return col;
        }
*/

/*
        private void ParseColumnAttribute(CustomAttributeData customAttributeData, out string colName, out string sqlDataType)
        {
            // Column Name
            colName = customAttributeData.ConstructorArguments[0].ToString().Trim('\"');

            // Column Sql Datatype
            CustomAttributeNamedArgument art =
                customAttributeData.NamedArguments.FirstOrDefault(a => a.MemberName == "TypeName");
            sqlDataType = art.TypedValue.Value != null ? art.TypedValue.ToString().Trim('\"') : null;
        }
*/

        internal class EfTableAttributes
        {
            private string _entityName;

            internal string EntityName
            {
                get => _entityName;
                set
                {
                    if (value.Contains('.'))
                        _entityName = value.Substring(value.LastIndexOf('.') + 1);
                    else
                        _entityName = value;
                }
            }

            internal List<EfColumnAttributes> Columns { get; set; }
            internal List<EfForeignKeyAttributes> ForeignKeys { get; set; }
            public String SqlTableName { get; set; }
        }

        internal class EfColumnAttributes
        {
            internal bool KeyColumn { get; private set; }

            internal bool Required { get; set; }

            internal string SqlName { get; set; }

            internal string ClrName { get; set; }

            internal int? MaxStringLength { get; set; }

            private string _sqlDataType;
            public string SqlDataType
            {
                get => _sqlDataType;
                set => _sqlDataType = value.ToLower().Replace(" ", "");
            }

            internal Type ClrDataType { get; set; }

            internal bool? IsUnicode { get; set; } = true;
            internal bool IsFixWidth { get; set; }

            internal void SetKeyColumn()
            {
                Required = true;
                KeyColumn = true;
            }

            //public bool AnnotationNotMatchClrDataType(out string probDetails)
            //{
            //    probDetails = null;

            //    if (SqlDataType == null)
            //        return false;

            //    var parts = SqlDataType.ToLower().Split(new[] { '(', ',', ')' });
            //    if (parts[0].Contains("char"))
            //    {
            //        if(ClrDataType != typeof(string))
            //            return true;

            //        if (parts[1] == "max")
            //        {
            //            if (MaxStringLength != null)
            //                return true;
            //        }
            //        else if (MaxStringLength != null && MaxStringLength != Convert.ToInt32(parts[1]))
            //            return true;
            //        else
            //        {
            //            switch (parts[0].Trim())
            //            {
            //                case "varchar":
            //                case "char":
            //                    if (IsUnicode ?? true)
            //                        return true;
            //                    break;
            //                case "nvarchar":
            //                case "nchar":
            //                    if (!(IsUnicode??true))
            //                    {
            //                        probDetails = "n[var]char marked as NOT Unicode.";
            //                        return true;
            //                    }

            //                    break;
            //            }
            //        }
            //    }
        
            //    return false;
            //}


        }

        private static string SqlToClrDataType(string dataType, out bool? unicode)
        {
            unicode = null;
            var dtParts = dataType.Split(new[] { '(', ')', ',' });

            switch (dtParts[0])
            {
                case "varchar":
                case "char":
                    unicode = false;
                    return "string";
                case "nvarchar":
                case "nchar":
                    unicode = true;
                    return "string";
                case "datetimeoffset":
                    return "DateTimeOffset";
                default:
                    throw new NotImplementedException();
            }

        }

        private static bool EntityColumnTypeNotMatchSql(string sqlDataType, bool? sqlNullable,
            Type clrDataType, bool? isUnicode, int? maxStringLength, out string probDetails)
        {
            probDetails = "";

            if (sqlDataType == null)
                return false;

            var typeParts = sqlDataType.Split(new[] {'(', ')', ' '}, StringSplitOptions.RemoveEmptyEntries);
            switch (typeParts[0].Trim().ToLower())
            {
                case "varchar":
                case "char":
                    if (clrDataType != typeof(string) || (isUnicode ?? true))
                    { probDetails = "[var]char marked as Unicode."; break; }
                    if (CharLengthIncorrect(typeParts[1], maxStringLength))
                        probDetails = "[var]char string length doesn't match CLR string length from annotations";
                    break;
                case "nvarchar":
                case "nchar":
                    if (clrDataType != typeof(string) || !(isUnicode ?? true))
                    { probDetails = "n[var]char marked as NOT Unicode."; break; }
                    if (CharLengthIncorrect(typeParts[1], maxStringLength))
                        probDetails = "n[var]char string length doesn't match annotation.";
                    break;
                case "binary":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(byte[]), typeof(byte?[]), sqlNullable, out probDetails);
                    break;
                case "bigint":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType, 
                        typeof(long), typeof(long?), sqlNullable, out probDetails);
                    break;
                case "int":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(int), typeof(int?), sqlNullable, out probDetails);
                    break;
                case "smallint":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(short), typeof(short?), sqlNullable, out probDetails);
                    break;
                case "tinyint":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(byte), typeof(byte?), sqlNullable, out probDetails);
                    break;
                case "bit":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(bool), typeof(bool?), sqlNullable, out probDetails);
                    break;
                case "decimal":
                case "numeric":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(decimal), typeof(decimal?), sqlNullable, out probDetails);
                    break;
                case "datetimeoffset":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(DateTimeOffset), typeof(DateTimeOffset?), sqlNullable, out probDetails);
                    break;
                case "date":
                    CheckPrimitiveType(typeParts[0].Trim().ToLower(), clrDataType,
                        typeof(DateTime), typeof(DateTime?), sqlNullable, out probDetails);
                    break;
                default:
                    throw new NotImplementedException($"EntityColumnTypeNotMatchSql case missing {typeParts[0]}");
            }

            if(probDetails == "")
                return false;
            return true;
        }

        private static void CheckPrimitiveType(string sqlType, Type clrDataType, 
            Type clrNonNullTarget, Type clrNullTarget, bool? sqlNullable, out string probDetails)
        {
            probDetails = "";
            if ((sqlNullable ?? true) && clrDataType != clrNullTarget)
             probDetails = $"NULLABLE '{sqlType}' SQL data type doesn't have a property data type of {clrNullTarget}"; 
            else if (!(sqlNullable ?? false) && clrDataType != clrNonNullTarget)
                probDetails = $"NON NULLABLE '{sqlType}' SQL data type doesn't have a property data type of {clrNonNullTarget}";
        }

        private static bool CharLengthIncorrect(string dbCharLength, int? propertyMaxStringLength)
        {
            if (dbCharLength == "max")
            {
                if (propertyMaxStringLength != null)
                    return true;
            }
            else
            {
                if (propertyMaxStringLength == null)
                    return false;

                if (dbCharLength != (propertyMaxStringLength ?? -1).ToString())
                    return true;
            }

            return false;
        }

        internal class EfForeignKeyAttributes
        {
            internal string Name { get; set; }

            internal Type DataType { get; set; }
            internal string InversePropertyArgument { get; set; }
            internal string ForeignKeyArgument { get; set; }
            internal string Property { get; set; }
        }

        public class EfDiscrepancyReportDto
        {
            public readonly List<EfDiscrepancyTableDto> Tables = new List<EfDiscrepancyTableDto>();

            internal void RegisterProblem(EfTableAttributes problemTable, EfColumnAttributes problemColumn, 
                ReportProblem problem, string problemDetails)
            {
                var table = GetTable(problemTable.SqlTableName, problemTable.EntityName);
                var column = table.GetColumn(problemColumn.SqlName, problemColumn.ClrName);
                var clrDataType = problemColumn.ClrDataType.Name + ((problemColumn.Required || problemColumn.ClrDataType.Name == "string") ? "":"?");

                column.Error = problem;
                column.ErrorDetails = problemDetails;
                column.CorrectionCode = EfDiscrepancyColumnDto.CreateCorrectionCode(problemTable.EntityName,
                    problemColumn.SqlName, problemColumn.SqlDataType, problemColumn.KeyColumn,
                    !problemColumn.Required, clrDataType, problemColumn.ClrName,
                    problemColumn.IsUnicode);
            }

            internal void RegisterProblem(string sqlTableName, string sqlColumnName,
                ReportProblem problem, Dictionary<string,string> correctionCode)
            {
                var table = GetTable(sqlTableName);
                var column = table.GetColumn(sqlColumnName);

                column.Error = problem;
                column.CorrectionCode = correctionCode;
            }

            private EfDiscrepancyTableDto GetTable(string sqlTableName)
            {
                var tbl = Tables.FirstOrDefault(t => t.SqlTableName == sqlTableName);
                if (tbl?.SqlTableName == null)
                    Tables.Add(tbl = new EfDiscrepancyTableDto(sqlTableName));
                return tbl;
            }

            private EfDiscrepancyTableDto GetTable(string sqlTableName, string entityName)
            {
                var tbl = Tables.FirstOrDefault(t => t.SqlTableName == sqlTableName);
               if (tbl?.SqlTableName == null)
                        Tables.Add(tbl = new EfDiscrepancyTableDto(sqlTableName, entityName));
                
               return tbl;
            }
        }

        public class EfDiscrepancyTableDto
        {
            public string SqlTableName { get; private set; }
            public string EntityName { get; private set; }
            public readonly List<EfDiscrepancyColumnDto> Columns = new List<EfDiscrepancyColumnDto>();

            internal EfDiscrepancyTableDto(string sqlTableName)
            {
                SqlTableName = sqlTableName;
                EntityName = ConvertSqlNameToEntityName(sqlTableName);
            }

            internal EfDiscrepancyTableDto(string sqlTableName, string entityName)
            {
                SqlTableName = sqlTableName;
                EntityName = entityName;
            }

            private string ConvertSqlNameToEntityName(string sqlTableName)
            {
                var entityName = sqlTableName.Substring(0, 1).ToUpper()
                                 + sqlTableName.Substring(1).ToLower();
                int idx;
                while ((idx = entityName.LastIndexOf('_')) > -1)
                {
                    entityName = entityName.Substring(0, idx)
                                 + entityName.Substring(idx + 1, 1).ToUpper()
                                 + entityName.Substring(idx + 2);
                }

                return entityName;
            }

            internal EfDiscrepancyColumnDto GetColumn(string sqlColumnName)
            {
                EfDiscrepancyColumnDto column = Columns.FirstOrDefault(c => c.SqlColumnName == sqlColumnName);
                if (column == null) Columns.Add(column = new EfDiscrepancyColumnDto(sqlColumnName));
                return column;
            }

            internal EfDiscrepancyColumnDto GetColumn(string sqlColumnName, string propertyName)
            {
                EfDiscrepancyColumnDto column = Columns.FirstOrDefault(c => c.SqlColumnName == sqlColumnName);
                if (column == null) Columns.Add(column = new EfDiscrepancyColumnDto(sqlColumnName, propertyName));
                return column;
            }
        }

        public class EfDiscrepancyColumnDto
        {
            public string SqlColumnName { get; private set; }
            public string PropertyName { get; private set; }
            internal ReportProblem Error { get; set; }
            public string ErrorDescription => Error.ToString();
            public Dictionary<string,string> CorrectionCode { get; set; }
            public string ErrorDetails { get; set; }

            internal EfDiscrepancyColumnDto(string sqlColumnName)
            {
                SqlColumnName = sqlColumnName;
                PropertyName = PascalNameFromSqlName(sqlColumnName);
            }

            internal EfDiscrepancyColumnDto(string sqlColumnName, string propertyName)
            {
                SqlColumnName = sqlColumnName;
                PropertyName = propertyName;
            }

            public static Dictionary<string, string> CreateCorrectionCode(string entityName, string sqlColName, 
                string sqlDataType, in bool keyColumn, in bool nullable, string clrType, string propertyName, bool? unicode)
            {
                var keyRequiredText = keyColumn ? ", Key" : (nullable ? "" : ", Required");

                var ret = new Dictionary<string,string>
                {
                    {
                        $"{entityName}.cs", 
                        $"[Column(\"{sqlColName}\", TypeName = \"{sqlDataType}\"){keyRequiredText}]" +
                        $"public {clrType} {propertyName} {{ get; set; }}"
                    }
                };

                if (!(unicode ?? true))
                    ret.Add("EmarContext.cs",
                        $"modelBuilder.Entity<{entityName}>(entity => "
                        + Environment.NewLine
                        + "{" + Environment.NewLine
                        + $"    entity.Property(e => e.{propertyName}).IsUnicode(false);" + Environment.NewLine
                        + "});");

                return ret;
            }
        }

        public enum ReportProblem
        {
            AnnotationNotMatchClrDataType,
            AnnotationSqlDatatypeNotMatchClrDatatype,
            DbColumnMissing,
            PropertyNotFlaggedAsKey,
            PropertyImproperlyFlaggedAsKey,
            None,
            PropertyFlaggedRequiredButColumnNullable,
            PropertyNotFlaggedRequiredButColumnNotNullable
        }

        private const string COLUMN_QUERY = "SELECT	c.name, \n\r" +
                                           "TYPE_NAME(system_type_id) + \n\r" +
                                           "CASE\n\r" +
                                           "WHEN TYPE_NAME(system_type_id) LIKE 'n%char' \n\r" +
                                           "THEN CONCAT('(', max_length / 2, ')')\n\r" +
                                           "WHEN TYPE_NAME(system_type_id) LIKE '%char' \n\r" +
                                           "OR TYPE_NAME(system_type_id) = 'binary'  \n\r" +
                                           "THEN CONCAT('(', max_length, ')')\n\r" +
                                           "WHEN TYPE_NAME(system_type_id) = 'numeric' \n\r" +
                                           "THEN CONCAT('(', precision, ',', scale, ')')\n\r" +
                                           "ELSE ''\n\r" +
                                           "END AS datatype\n\r" +
                                           ", is_nullable\n\r" +
                                           ", CASE WHEN ic.index_id IS NULL THEN 0 ELSE 1 END AS KeyColumn\n\r" +
                                           "FROM    sys.columns c\n\r" +
                                           "JOIN sys.indexes i\n\r" +
                                           "ON c.object_id = i.object_id\n\r" +
                                           "AND i.is_primary_key = 1\n\r" +
                                           "LEFT JOIN sys.index_columns ic\n\r" +
                                           "ON ic.object_id = i.object_id\n\r" +
                                           "AND i.index_id = ic.index_id\n\r" +
                                           "AND c.column_id = ic.column_id\n\r" +
                                           "WHERE c.object_id = OBJECT_ID('{0}')\n\r";

    }
}