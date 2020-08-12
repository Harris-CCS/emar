using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Emar.Data.Helpers
{
    public class EfToDbSynchHelper
    {
        private EmarContext _context;

        public EfToDbSynchHelper(EmarContext context)
        {
            _context = context;
        }

        public EfDiscrepancyReportDto CompareEfToDb()
        {
            List<TableAttribues> tables = SurveyEfEntities();

            if (ProblemsExitInEfDefinitions(tables, out EfDiscrepancyReportDto report))
                return report;

            using (SqlConnection conn = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                conn.Open();
                using (SqlCommand comm = new SqlCommand(ColumnQuery, conn))
                {
                    foreach (var tbl in tables)
                    {
                        comm.CommandText = string.Format(ColumnQuery, tbl.TableName);
                        var reader = comm.ExecuteReader();
                        while (reader.Read())
                        {
                            // Find the record in the list of columns
                            var colName = reader["name"].ToString();
                            var col = tbl.Columns.FirstOrDefault(c => c.SqlName == colName);
                            if (col?.SqlName == null)
                                RegisterMissingColumn(tbl.TableName, reader, report);
                            else
                                ConfirmColumnProperties(tbl.TableName, reader, col, report);
                        }
                    }   
                }
            }

            return report;
        }

        private bool ProblemsExitInEfDefinitions(List<TableAttribues> tables, out EfDiscrepancyReportDto report)
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
                        if (column.AnnotationNotMatchClrDataType(out string problemDetails))
                            report.RegisterProblem(table, column, ReportProblem.AnnotationNotMatchClrDataType,
                                problemDetails);
                    }
                }
            }
            catch (NotImplementedException e)
            {
                throw new NotImplementedException($"When looking for problems in '{errorTable}'.'{errorColumn}'", e);
            }

            return report.Tables.Any();
        }

        private List<TableAttribues> SurveyEfEntities()
        {
            List<TableAttribues> tbls = new List<TableAttribues>();
            foreach (var entity in _context.Model.GetEntityTypes().Select(t => t.ClrType).ToList())
            {
                var columns = new List<ColumnAttributes>();
                var foreignKeys = new List<ForeignKeyAttributes>();
                foreach (var prop in entity.GetProperties())
                {
                    var attributes = prop.CustomAttributes.ToList();

                    // If "NotMapped", then we don't do anything with it
                    var notMappedAttribute = attributes.Where(a => a.AttributeType == typeof(NotMappedAttribute));
                    if (notMappedAttribute.Any())
                        continue;

                    // If it is a foreign key, needs both ForeignKeyAttribute and InversePropertyAttribute
                    var fkAttributes = attributes.Where(a =>
                        a.AttributeType == typeof(ForeignKeyAttribute)
                        || a.AttributeType == typeof(InversePropertyAttribute)).ToList();

                    if (fkAttributes.Any())
                        foreignKeys.Add(SurveyForeignKey(prop, attributes));
                    else
                        columns.Add(SurveyColumn(prop, attributes));
                }

                tbls.Add(
                    new TableAttribues
                    {
                        EntityName = entity.FullName,
                        TableName = ((TableAttribute) entity.GetCustomAttributes(false)[0]).Name,
                        Columns = columns,
                        ForeignKeys = foreignKeys
                    });
            }

            return tbls;
        }

        private void ConfirmColumnProperties(string tblName, SqlDataReader reader, ColumnAttributes col,
            EfDiscrepancyReportDto report)
        {
            EfDiscrepancyColumnDto rptColumn;

            if (col.EntityColumnTypeNotMatchSql(reader))
            {
                rptColumn = new EfDiscrepancyColumnDto
                {
                    ColumnSqlName = reader[0].ToString(),
                    Error = ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype,
                };
            }
            else if ((col.KeyColumn ? 1 : 0) != (int) reader["KeyColumn"])
            {
                rptColumn = new EfDiscrepancyColumnDto
                {
                    ColumnSqlName = reader[0].ToString(),
                    Error = ReportProblem.DbColumnInPrimaryKeyButNotAnnotated,
                };

            }

        }

        private void RegisterMissingColumn(string table, SqlDataReader reader, EfDiscrepancyReportDto report)
        {
            var rptTable = report.Tables.FirstOrDefault(t => t.TableName == table);
            if (rptTable?.TableName == null) rptTable = new EfDiscrepancyTableDto();

            var columnName = reader["name"].ToString();
            var dataType = (reader["datatype"].ToString() ?? "sqlvariant").ToLower();
            var nullable = Convert.ToBoolean(reader["is_nullable"]) ? "" : ", Required";
            var keyColumn = Convert.ToBoolean(reader["KeyColumn"]) ? "" : ", Key";

            var clrType = SqlToClrDataType(dataType, out bool unicode);

            var propertyName = columnName.Substring(0,1).ToUpper() + columnName.Substring(1).ToLower();
            var idx = propertyName.IndexOf('_');
            while (idx > -1)
            {
                propertyName = propertyName.Substring(0, idx)
                               + propertyName.Substring(idx, 1).ToUpper()
                               + propertyName.Substring(idx + 1);
                idx = propertyName.IndexOf('_');
            }


            var rptColumn = new EfDiscrepancyColumnDto
            {
                ColumnSqlName = columnName,
                Error = ReportProblem.DbColumnMissing,
                CorrectionCode = $"[Column(\"{reader["columnName"]}\", TypeName = \"{dataType}\"){nullable}{keyColumn}]" 
                                 + Environment.NewLine
                                 + $"public long {propertyName} {{ get; set; }}"
            };

            if (dataType == "string" && !unicode)
                rptColumn.CorrectionCode += Environment.NewLine + Environment.NewLine
                                                                + $"modelBuilder.Entity<{table}>(entity => " +
                                                                Environment.NewLine
                                                                + "{" + Environment.NewLine
                                                                + $"    entity.Property(e => e.{propertyName}).IsUnicode(false);" +
                                                                Environment.NewLine
                                                                + "});";


            rptTable.Columns.Add(rptColumn);
        }

        private string SqlToClrDataType(string dataType, out bool unicode)
        {
            var dtParts = dataType.Split(new[] {'(', ')', ','});
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
                default:
                    throw new NotImplementedException();
            }

        }

        private ColumnAttributes SurveyColumn(PropertyInfo prop, List<CustomAttributeData> attributes)
        {
            var col = new ColumnAttributes {ClrDataType = prop.PropertyType, ClrName = prop.Name};
            if (col.ClrDataType == typeof(string))
                col.IsUnicode = prop.PropertyType.IsUnicodeClass;

            foreach (Attribute attribute in prop.GetCustomAttributes())
            {
                if (attribute.TypeId == typeof(KeyAttribute))
                    col.SetKeyColumn();
                else if (attribute.TypeId == typeof(ColumnAttribute))
                {
                    col.SqlName = ((ColumnAttribute) attribute).Name;
                    if (((ColumnAttribute) attribute).TypeName != null)
                        col.SqlDataType = ((ColumnAttribute) attribute).TypeName;
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

        private ForeignKeyAttributes SurveyForeignKey(PropertyInfo prop, List<CustomAttributeData> fkAttributes)
        {
            var fk = new ForeignKeyAttributes { Name = prop.Name, DataType = prop.PropertyType };

            foreach (var fkAttribute in fkAttributes)
            {
                if (fkAttribute.AttributeType == typeof(InversePropertyAttribute))
                    fk.InversePropertyArgument = fkAttribute.ConstructorArguments[0].ToString().Trim('\"');
                else if (fkAttribute.AttributeType == typeof(ForeignKeyAttribute))
                    fk.ForeignKeyArgument = fkAttribute.ConstructorArguments[0].ToString().Trim('\"');
                else
                {
                }
            }

            return fk;
        }

        private void ParseColumnAttribute(CustomAttributeData customAttributeData, out string colName, out string sqlDataType)
        {
            // Column Name
            colName = customAttributeData.ConstructorArguments[0].ToString().Trim('\"');

            // Column Sql Datatype
            CustomAttributeNamedArgument art =
                customAttributeData.NamedArguments.FirstOrDefault(a => a.MemberName == "TypeName");
            sqlDataType = art.TypedValue.Value != null ? art.TypedValue.ToString().Trim('\"') : null;
        }

        internal class TableAttribues
        {
            internal string EntityName { get; set; }
            internal List<ColumnAttributes> Columns { get; set; }
            internal List<ForeignKeyAttributes> ForeignKeys { get; set; }
            public String TableName { get; set; }
        }

        internal class ColumnAttributes
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

            internal bool IsUnicode { get; set; } = true;

            internal void SetKeyColumn()
            {
                Required = true;
                KeyColumn = true;
            }

            public bool AnnotationNotMatchClrDataType(out string probDetails)
            {
                probDetails = null;

                if (SqlDataType == null)
                    return false;

                var parts = SqlDataType.ToLower().Split(new[] { '(', ',', ')' });
                if (parts[0].Contains("char"))
                {
                    if(ClrDataType != typeof(string))
                        return true;

                    if (parts[1] == "max")
                    {
                        if (MaxStringLength != null)
                            return true;
                    }
                    else if (MaxStringLength != null && MaxStringLength != Convert.ToInt32(parts[1]))
                        return true;
                    else
                    {
                        switch (parts[0].Trim())
                        {
                            case "varchar":
                            case "char":
                                if (IsUnicode)
                                    return true;
                                break;
                            case "nvarchar":
                            case "nchar":
                                if (!IsUnicode)
                                {
                                    probDetails = "n[var]char marked as NOT Unicode.";
                                    return true;
                                }

                                break;
                        }
                    }
                }
                else
                {
                    switch ((parts[0].Trim()))
                    {
                        case "int":
                            if (Required && ClrDataType != typeof(int))
                                return true;
                            if (!Required && ClrDataType != typeof(int?))
                                return true;
                            break;
                        case "bigint":
                            if (Required && ClrDataType != typeof(long))
                                return true;
                            if (!Required && ClrDataType != typeof(long?))
                            {
                                probDetails = "'bigint' SQL data type doesn't have a property data type of long";
                                return true;
                            }

                            break;
                        case "tinyint":
                            if (Required && ClrDataType != typeof(byte))
                                return true;
                            else if (!Required && ClrDataType != typeof(byte?))
                                return true;
                            break;
                        case "bit":
                            if (Required && ClrDataType != typeof(bool))
                                return true;
                            else if (!Required && ClrDataType != typeof(bool?))
                                return true;
                            break;
                        case "date":
                            if (Required && ClrDataType != typeof(DateTime))
                                return true;
                            else if (!Required && ClrDataType != typeof(DateTime?))
                                return true;
                            break;
                        case "datetimeoffset":
                            if (Required && ClrDataType != typeof(DateTimeOffset))
                                return true;
                            else if (!Required && ClrDataType != typeof(DateTimeOffset?))
                                return true;
                            break;
                        case "numeric":
                        case "decimal":
                            if (Required && ClrDataType != typeof(decimal))
                                return true;
                            if (!Required && ClrDataType != typeof(decimal?))
                                return true;
                            break;
                        case "binary":
                            if (ClrDataType != typeof(byte[]))
                                return true;
                            break;
                        default:
                            throw new NotImplementedException(
                                $"AnnotationNotMatchClrDataType(), no case for SqlDataType.Trim().ToLower() " +
                                $"== '{SqlDataType.Trim().ToLower()}'");
                    }
                }

                return false;
            }

            public bool EntityColumnTypeNotMatchSql(SqlDataReader reader)
            {
                throw new NotImplementedException();
            }
        }

        internal class ForeignKeyAttributes
        {
            internal string Name { get; set; }

            internal Type DataType { get; set; }
            internal string InversePropertyArgument { get; set; }
            internal string ForeignKeyArgument { get; set; }
        }

        public class EfDiscrepancyReportDto
        {
            public readonly List<EfDiscrepancyTableDto> Tables = new List<EfDiscrepancyTableDto>();

            internal void RegisterProblem(TableAttribues problemTable, ColumnAttributes problemColumn, ReportProblem problem, 
                string problemDetails)
            {
                var table = Tables.FirstOrDefault(t => t.TableName == problemTable.EntityName);
                if (table == null)
                {
                    Tables.Add(new EfDiscrepancyTableDto {TableName = problemTable.EntityName});
                    table = Tables.First(t => t.TableName == problemTable.EntityName);
                }

                var column = table.Columns.FirstOrDefault(c => c.ColumnSqlName == problemColumn.SqlName);
                if (column == null)
                {
                    table.Columns.Add(new EfDiscrepancyColumnDto { ColumnSqlName = problemColumn.SqlName});
                    column = table.Columns.First(c => c.ColumnSqlName == problemColumn.SqlName);
                }

                column.Error = problem;
                column.ErrorDetails = problemDetails;
            }
        }

        public class EfDiscrepancyTableDto
        {
            public string TableName { get; set; }
            public readonly List<EfDiscrepancyColumnDto> Columns = new List<EfDiscrepancyColumnDto>();
        }

        public class EfDiscrepancyColumnDto
        {
            public string ColumnSqlName { get; set; }
            public ReportProblem Error { get; set; }
            public string CorrectionCode { get; set; }
            public string ErrorDetails { get; set; }
        }

        public enum ReportProblem
        {
            AnnotationNotMatchClrDataType,
            AnnotationSqlDatatypeNotMatchClrDatatype,
            DbColumnInPrimaryKeyButNotAnnotated,
            DbColumnMissing
        }

        private const string ColumnQuery = "SELECT	c.name, \n\r" +
                                           "TYPE_NAME(system_type_id) + \n\r" +
                                           "CASE\n\r" +
                                           "WHEN TYPE_NAME(system_type_id) LIKE 'n%char' \n\r" +
                                           "THEN CONCAT('(', max_length / 2, ')')\n\r" +
                                           "WHEN TYPE_NAME(system_type_id) LIKE '%char' \n\r" +
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
                                           "WHERE c.object_id = OBJECT_ID('%s')\n\r";

    }
}