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

        public void CompareEfToDb()
        {
            List<TableAttribues> tbls = SurveyEfEntities();
            var report = new EfDiscrepancyReportDto();

            using (SqlConnection conn = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                conn.Open();
                using (SqlCommand comm = new SqlCommand(ColumnQuery, conn))
                {
                    foreach (var tbl in tbls)
                    {
                        comm.CommandText = string.Format(ColumnQuery, tbl.TableName);
                        var reader = comm.ExecuteReader();
                        while (reader.Read())
                        {
                            // Find the record in the list of columns
                            var colName = reader["name"].ToString();
                            var col = tbl.Columns.FirstOrDefault(c => c.Name == colName);
                            if (col?.Name == null)
                                RegisterMissingColumn(tbl.TableName, reader, report);
                            else
                                ConfirmColumnProperties(tbl.TableName, reader, col, report);
                        }
                    }   
                }
            }

            return;
        }

        private void ConfirmColumnProperties(string tblName, SqlDataReader reader, ColumnAttributes col,
            EfDiscrepancyReportDto report)
        {
            EfDiscrepancyColumnDto rptColumn;

            if (col.AnnotationDoesntMatchDataType(reader))
            {
                rptColumn = new EfDiscrepancyColumnDto
                {
                    ColumnName = reader[0].ToString(),
                    Errors = "Annotation SQL datatype doesn't match the CLR datatype and properties",
                };
            }
            else if ((col.KeyColumn ? 1 : 0) != (int) reader["KeyColumn"])
            {
                rptColumn = new EfDiscrepancyColumnDto
                {
                    ColumnName = reader[0].ToString(),
                    Errors = "Column is part of primary key in the database, but not annotated as such",
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
                ColumnName = columnName,
                Errors = "Column Missing",
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
                        TableName = ((TableAttribute)entity.GetCustomAttributes(false)[0]).Name,
                        Columns = columns,
                        ForeignKeys = foreignKeys
                    });
            }

            return tbls;
        }

        private ColumnAttributes SurveyColumn(PropertyInfo prop, List<CustomAttributeData> attributes)
        {
            var col = new ColumnAttributes { DataType = prop.PropertyType };
            if (col.DataType == typeof(String))
                col.IsUnicode = prop.ReflectedType?.IsUnicodeClass ?? false;

            foreach (var x in attributes)
            {
                if (x.AttributeType == typeof(KeyAttribute))
                    col.Key();
                else if (x.AttributeType == typeof(ColumnAttribute))
                {
                    ParseColumnAttribute(x, out string name, out string type);
                    col.Name = name;
                    if (type != null) col.SqlDataType = type;
                }
                else if (x.AttributeType == typeof(RequiredAttribute))
                    col.Required = true;
                else if (x.AttributeType == typeof(StringLengthAttribute))
                    col.MaxStringLength =
                        Convert.ToInt32(x.ConstructorArguments[0].ToString().Split(')')[1]);
                else
                {
                }
            }

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

        private class TableAttribues
        {
            internal string EntityName { get; set; }
            internal List<ColumnAttributes> Columns { get; set; }
            internal List<ForeignKeyAttributes> ForeignKeys { get; set; }
            public String TableName { get; set; }
        }

        private class ColumnAttributes
        {
            
            private bool _isKey;
            internal bool KeyColumn => _isKey;
        
            internal int MaxStringLength { get; set; }
            
            internal string Name { get; set; }

            internal Type DataType { get; set; }

            internal bool IsUnicode { get; set; }

            internal bool VariableLength { get; }

            private bool _required;
            internal bool Required
            {
                get => _required;
                set => _required = value;
            }

            internal void Key()
            {
                _required = true;
                _isKey = true;
            }

            //internal void SetDataType(Type prop)
            //{
            //    _dataType = prop.Name;
            //}

            private string _sqlDataType;
            public string SqlDataType
            {
                get => _sqlDataType;
                set => _sqlDataType = value.ToLower().Replace(" ", "");
            }

            //var parts = sqlDataType.ToLower().Split(new[] {'(', ',', ')'});
            //    if (parts[0].Contains("char"))
            //    {
            //        _dataType = "String";
            //        MaxStringLength = Convert.ToInt32(parts[1]);
            //        switch (parts[0].Trim())
            //        {
            //            case "varchar":
            //                _variableLength = true;
            //                _isUnicode = false;
            //                break;
            //            case "char":
            //                _variableLength = false;
            //                _isUnicode = false;
            //                break;
            //            case "nvarchar":
            //                _variableLength = true;
            //                _isUnicode = true;
            //                break;
            //            case "nchar":
            //                _variableLength = false;
            //                _isUnicode = false;
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        switch (sqlDataType.Trim().ToLower())
            //        {
            //            case "bigint":
            //                _dataType = "Int64";
            //                break;
            //            case "bit":
            //                _dataType = "Boolean";
            //                break;
            //            case "datetimeoffset":
            //                _dataType = "DateTimeOffset";
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            public bool AnnotationDoesntMatchDataType(SqlDataReader reader)
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

        private const string ColumnQuery = "SELECT	c.name, \n\r" +
                                           "TYPE_NAME(system_type_id) + \n\r" +
                                           "CASE\n\r" +
                                           "WHEN TYPE_NAME(system_type_id) LIKE 'n%char' \n\r" +
                                           "THEN CONCAT('(', max_length / 2, ')')\n\r" +
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

    public class EfDiscrepancyReportDto
    {
        public List<EfDiscrepancyTableDto> Tables { get; set; }
    }

    public class EfDiscrepancyTableDto
    {
        public string TableName { get; set; }
        public List<EfDiscrepancyColumnDto> Columns = new List<EfDiscrepancyColumnDto>();
    }

    public class EfDiscrepancyColumnDto
    {
        public string ColumnName { get; set; }
        public string Errors { get; set; }
        public string CorrectionCode { get; set; }
    }
}