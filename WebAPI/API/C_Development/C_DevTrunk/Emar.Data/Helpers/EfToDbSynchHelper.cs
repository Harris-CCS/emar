using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Emar.Data.Helpers
{
    public class EfToDbSynchHelper
    {
        private readonly EmarContext _context;
        private const string CONTEXT_NAME = "EmarContext";


        public EfToDbSynchHelper(EmarContext context)
        {
            _context = context;
        }

        public EfDiscrepancyReport CompareEfToDb()
        {
            List<EfTableAttributes> tables = 
                new List<EfTableAttributes>(SurveyEfEntities().OrderBy(t => t.EntityName));

#if TestingInternalDatatypeProblems
            //for (int i = tables.Count - 1; i > 0; i--)
            //{
            //    if (tables[i].EntityName == "PatientCartOrder") continue;
            //    tables.RemoveAt(i);
            //}
            while (tables.Count > 1)
                tables.RemoveAt(1);

            // Check the bottom of this file for definition of the example SQL table
#endif

            if (ProblemsExitInEfDefinitions(tables, out EfDiscrepancyReport report))
                return report;

            CompareSurveyToDatabase(tables, report);

            if (!report.Files.Any()) return null;

            return report;
        }

        #region Survey Code
     
        private List<EfTableAttributes> SurveyEfEntities()
        {
            var tbls = new List<EfTableAttributes>();
            foreach (IEntityType entity in _context.Model.GetEntityTypes().OrderBy(a => a.Name))
            {
                var table =
                    new EfTableAttributes
                    {
                        EntityName = entity.Name,
                        SqlTableName = entity.GetTableName(),
                    };

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
                        IsFixWidth = iProp.IsFixedLength(),
                        Parent = table
                    };
                    if (iProp.IsPrimaryKey() && !iProp.IsKey())
                        throw new AmbiguousImplementationException("Found Primary Key not marked as \"Key\" in SurveyEfEntities()");
                    if (iProp.IsKey())
                        col.SetKeyColumn();
                    columns.Add(col);
                }

                var foreignKeys = new List<EfForeignKeyAttributes>();
                foreach (var fk in entity.GetForeignKeys())
                {
                    EfForeignKeyAttributes fkObj = new EfForeignKeyAttributes();
                    fkObj.DeclaringEntityType = fk.DeclaringEntityType;
                    fkObj.DeclaringEntityProperties = fk.Properties;
                    fkObj.DeclaringEntityNavigationProperty = fk.DependentToPrincipal.Name;
                    
                    fkObj.PrincipalEntityType = fk.PrincipalEntityType;
                    fkObj.PrincipalEntityNavigationProperty = fk.PrincipalToDependent.Name;


                    fkObj.DeleteBehavior = fk.DeleteBehavior;
                    fkObj.ConstraintName = fk.GetConstraintName();

                    //var DependentToPrincipal = fk.DependentToPrincipal;
                    //var IsOwnership = fk.IsOwnership;
                    //var IsRequired = fk.IsRequired;
                    //var IsUnique = fk.IsUnique;
                    //var PrincipalKey = fk.PrincipalKey;
                    //var PrincipalToDependent = fk.PrincipalToDependent;
                    //string constraintName = fk.GetConstraintName();
                    //var a = fk.AsForeignKey();
                    //var y = fk.GetDefaultName();
                    //var z = fk.GetNavigations();

                    //IEnumerable<INavigation> navigationsFrom = fk.FindNavigationsFrom(entity);
                    //IEnumerable<INavigation> navigationsTo = fk.FindNavigationsTo(entity);

                    foreignKeys.Add(fkObj);
                }

                table.Columns = columns;
                table.ForeignKeys = foreignKeys;

                tbls.Add(table);
            }

            return tbls;
        }

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
            internal EfTableAttributes Parent { get; set; }

            // Properties from the Annotations
            internal string SqlName { get; set; }
            internal bool KeyColumn { get; private set; }
            internal bool Required { get; set; }
            private string _sqlDataType;
            public string SqlDataType
            {
                get => _sqlDataType;
                set => _sqlDataType = value.ToLower().Replace(" ", "");
            }

            // ClrProperties
            internal string ClrName { get; set; }
            internal Type ClrDataType { get; set; }
            internal int? MaxStringLength { get; set; }
            internal bool? IsUnicode { get; set; } = true;
            internal bool? IsFixWidth { get; set; }

            // DB Properties
            private string _dbDataType;
            private bool? _dbNullable;
            private bool? _dbPrimaryKey;
            //public bool? SqlNullable { get; set; }

            public bool ExistsInDb => _dbNullable != null;

            internal void SetKeyColumn()
            {
                Required = true;
                KeyColumn = true;
            }

            public string PropertyDefinition()
            {
                bool nullable = _dbNullable ?? !Required;
                var sqlDataType = _dbDataType ?? SqlDataType 
                    ?? throw new ArgumentException("Don't have either a DbDataType or an AnnotationSqlDataType in EfColumnAttributes.PropertyDefinition()");
                
                bool includeRequired = false;
                string clrDataType;
                if (_dbDataType != null)
                    clrDataType = SqlToClrDataTypeString(_dbDataType, nullable, out includeRequired);
                else if (SqlDataType != null)
                    clrDataType = SqlToClrDataTypeString(SqlDataType, nullable, out includeRequired);
                else
                    clrDataType = ClrDataTypeToString(ClrDataType);

                var keyRequiredText = KeyColumn ? ", Key" : (includeRequired ? ", Required" : "");

                return $"[Column(\"{SqlName}\", TypeName = \"{sqlDataType}\"){keyRequiredText}]"
                       + Environment.NewLine +
                       $"public {clrDataType} {ClrName} {{ get; set; }}";
            }

            public void RecordDbPropertiesAndConfirm(string dataType, in bool nullable, in bool primaryKey,
                EfDiscrepancyReport report)
            {
                _dbDataType = dataType;
                _dbNullable = nullable;
                _dbPrimaryKey = primaryKey;
                ReportProblem error;

                if (!string.IsNullOrWhiteSpace(SqlDataType) && SqlDataType != _dbDataType)
                    report.RegisterProblem(this, FileSegment.EntityProperties,
                        ReportProblem.DbDataTypeNotMatchDefinedSqlType,
                        $"DB DataType = '{_dbDataType}', but annotations declare the data type to be '{SqlDataType}'.");

                if ((error = EntityColumnTypeNotMatchSql(out string probDetails)) == ReportProblem.None)
                {
                    if (KeyColumn != _dbPrimaryKey)
                    {
                        error = primaryKey
                            ? ReportProblem.PropertyNotFlaggedAsKey
                            : ReportProblem.PropertyImproperlyFlaggedAsKey;
                        probDetails = primaryKey
                            ? "Property is a Key Field, but not marked as such."
                            : "Property is not a Key Field, but is marked as such.";
                    }
                    else if (Required == nullable)
                    {
                        error = nullable
                            ? ReportProblem.DataTypeNotNullableButColumnTakesNulls
                            : ReportProblem.DataTypeNullableButColumnDoesntTakeNulls;
                        probDetails = nullable
                            ? "Data Type won't allow nulls, but Column Nullable."
                            : "Data Type allows nulls, but Column Not Nullable";
                    }
                }

                if (error == ReportProblem.None) return;
                report.RegisterProblem(this, FileSegment.EntityProperties, error, probDetails);
            }

            internal ReportProblem EntityColumnTypeNotMatchSql(out string probDetails)
            {
                //former parameters:
                //sqlDataType, sqlNullable, col.ClrDataType, col.IsUnicode,
                //col.MaxStringLength, col.IsFixWidth, out string probDetails

                ReportProblem returnProblem = ReportProblem.None;
                probDetails = "";
                //noteAsRequired = false;

                if (SqlDataType == null)
                    return returnProblem;

                var typeParts = SqlDataType.Split(new[] { '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                switch (typeParts[0].Trim().ToLower())
                {
                    case "binary":
                    case "varbinary":
                        if (!(IsFixWidth ?? false) && (typeParts[0] == "binary"))
                        {
                            probDetails = "binary not identified as Fixed Length";
                            returnProblem = ReportProblem.ContextFixedLengthPropertyMissing;
                        }
                        else if ((IsFixWidth ?? false) && (typeParts[0] == "varbinary"))
                        {
                            probDetails = "varbinary identified as Fixed Length";
                            returnProblem = ReportProblem.ContextFixedLengthPropertyToBeRemoved;
                        }
                        else if (MaxStringLength != null && typeParts.Length > 1 && MaxStringLength.ToString() != typeParts[1])
                        {
                            probDetails = "[var]binary length doesn't match CLR MaxLength annotation";
                            returnProblem = ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype;
                        }
                        break;
                    case "varchar":
                    case "char":
                    case "nvarchar":
                    case "nchar":
                        if (ClrDataType != typeof(string))
                        {
                            probDetails = $"{typeParts[0]} SQL data type not identified as 'String'.";
                            returnProblem = ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype;
                            break;
                        }
                        if (MaxStringLength != null && MaxStringLength.ToString() != typeParts[1].Trim())
                        {
                            probDetails = $"{typeParts[0]} max length doesn't match SQL data type.";
                            returnProblem = ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype;
                            break;
                        }

                        bool bFixed;
                        bool bUnicode;
                        switch (typeParts[0].Trim().ToLower())
                        {
                            case "varchar":
                                bFixed = false;
                                bUnicode = false;
                                break;
                            case "char":
                                bFixed = true;
                                bUnicode = false;
                                break;
                            case "nvarchar":
                                bFixed = false;
                                bUnicode = true;
                                break;
                            //case "nchar":
                            default:
                                bFixed = true;
                                bUnicode = true;
                                break;
                        }
                        if (bFixed != (IsFixWidth ?? false))
                        {
                            probDetails = (IsFixWidth ?? false)
                                ? $"Properties incorrectly declare '{typeParts[0]}' value as Fixed Length."
                                : $"Properties don't declare {typeParts[0]} value as Fixed Length.";
                            returnProblem = (IsFixWidth ?? false)
                                ? ReportProblem.ContextFixedLengthPropertyToBeRemoved
                                : ReportProblem.ContextFixedLengthPropertyMissing;
                        }
                        else if (bUnicode != (IsUnicode ?? true))
                        {
                            probDetails = (bUnicode)
                                ? $"Properties incorrectly declare {typeParts[0]} value as Non-Unicode."
                                : $"Properties don't declare '{typeParts[0]}' value as Non-Unicode.";
                            returnProblem = (bUnicode)
                                ? ReportProblem.ContextNotUnicodePropertyToBeRemoved
                                : ReportProblem.ContextNotUnicodePropertyMissing;
                        }
                        break;
                    case "bigint":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(long), typeof(long?), _dbNullable, out probDetails);
                        break;
                    case "int":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(int), typeof(int?), _dbNullable, out probDetails);
                        break;
                    case "smallint":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(short), typeof(short?), _dbNullable, out probDetails);
                        break;
                    case "tinyint":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(byte), typeof(byte?), _dbNullable, out probDetails);
                        break;
                    case "bit":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(bool), typeof(bool?), _dbNullable, out probDetails);
                        break;
                    case "decimal":
                    case "numeric":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(decimal), typeof(decimal?), _dbNullable, out probDetails);
                        break;
                    case "datetimeoffset":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(DateTimeOffset), typeof(DateTimeOffset?), _dbNullable, out probDetails);
                        break;
                    case "date":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(DateTime), typeof(DateTime?), _dbNullable, out probDetails);
                        break;
                    case "time":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(TimeSpan), typeof(TimeSpan?), _dbNullable, out probDetails);
                        break;
                    default:
                        throw new NotImplementedException($"EntityColumnTypeNotMatchSql case missing '{typeParts[0]}'");
                }

                return returnProblem != ReportProblem.None || probDetails == ""
                    ? returnProblem
                    : ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype;
            }

            private void CheckRequiredAssumedType(string sqlType, Type clrDataType,
                Type clrNonNullTarget, Type clrNullTarget, bool? sqlNullable, out string probDetails)
            {
                probDetails = "";
                if (_dbNullable == null)
                {
                    if (Required && clrDataType != clrNonNullTarget)
                        probDetails =
                            $"NON NULLABLE '{sqlType}' SQL data type doesn't have a property data type of \"{ClrDataTypeToString(clrNonNullTarget)}\"";
                    else if (!Required && clrDataType != clrNullTarget)
                        probDetails =
                            $"NULLABLE '{sqlType}' SQL data type doesn't have a property data type of \"{ClrDataTypeToString(clrNullTarget)}\"";
                }
            }
        }

        internal class EfForeignKeyAttributes
        {
            public IEntityType DeclaringEntityType;
            public IEnumerable<IProperty> DeclaringEntityProperties { get; set; }
            internal string DeclaringEntityNavigationProperty { get; set; }

            public IEntityType PrincipalEntityType { get; set; }

            public DeleteBehavior DeleteBehavior { get; set; }
            public string ConstraintName { get; set; }
            public string PrincipalEntityNavigationProperty { get; set; }
        }
        
        #endregion

        #region Model Checking

        private void CompareSurveyToDatabase(List<EfTableAttributes> tables, EfDiscrepancyReport report)
        {
            // Create a list of tables that need to be removed from the model because they don't exist in the DB
            var tablesToRemove = new List<EfTableAttributes>();
            using (var conn = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                conn.Open();
                foreach (var tbl in tables)
                {
                    using (var comm = new SqlCommand($"SELECT count(*) FROM sys.tables WHERE name = '{tbl.SqlTableName}'",
                        conn))
                    {
                        var x = Convert.ToInt16(comm.ExecuteScalar());
                        if (x == 0)
                        {
                            RegisterTableNotExists(tbl, report);
                            tablesToRemove.Add(tbl);
                        }
                        else
                        {
                            comm.CommandText = string.Format(COLUMN_QUERY, tbl.SqlTableName);
                            using (var reader = comm.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    ReadTheReader(reader, out string colName, out string sqlDataType,
                                        out bool sqlNullable,
                                        out bool primaryKey);
                                    // Find the record in the list of columns
                                    var col = tbl.Columns.FirstOrDefault(c => c.SqlName == colName);
                                    if (col?.SqlName == null)
                                        RegisterMissingColumn(tbl, colName, sqlDataType, sqlNullable, primaryKey,
                                            report);
                                    else
                                        col.RecordDbPropertiesAndConfirm(sqlDataType, sqlNullable, primaryKey, report);
                                }
                            }
                        }
                    }

                    //using (var comm = new SqlCommand(string.Format(FOREIGN_KEY_QUERY, tbl.SqlTableName), conn))
                    //{
                    //}
                }
            }

            foreach (var table in tablesToRemove) 
                tables.Remove(table);

            foreach (var table in tables)
            {
                foreach (var column in table.Columns)
                {
                    if (!column.ExistsInDb)
                        report.RegisterProblem(column, FileSegment.EntityProperties,
                            ReportProblem.ColumnNotInDatabase, null);
                }
            }
        }


        private void ReadTheReader(SqlDataReader reader, out string colName, out string dataType, out bool nullable,
            out bool primaryKey)
        {
            colName = reader["name"].ToString();
            dataType = reader["datatype"].ToString();
            nullable = Convert.ToByte(reader["is_nullable"]) != 0;
            primaryKey= Convert.ToByte(reader["KeyColumn"]) != 0;
        }

        private bool ProblemsExitInEfDefinitions(List<EfTableAttributes> tables, out EfDiscrepancyReport report)
        {
            report = new EfDiscrepancyReport();

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
                        ConfirmColumnPropertiesMatchSqlAnnotations(column, column.SqlDataType,
                            !column.Required, column.KeyColumn, report);
                    }
                }
            }
            catch (NotImplementedException e)
            {
                throw new NotImplementedException($"When looking for problems in '{errorTable}'.'{errorColumn}'", e);
            }

            return report.Files.Any();
        }

        private static void ConfirmColumnPropertiesMatchSqlAnnotations(EfColumnAttributes col,
            string sqlDataType, bool? sqlNullable, bool keyColumn, EfDiscrepancyReport report)
        {
            ReportProblem error;

            if ((error = col.EntityColumnTypeNotMatchSql(out string probDetails)) == ReportProblem.None)
                return;

            report.RegisterProblem(col, FileSegment.EntityProperties, error, probDetails);
        }

        #endregion

        #region Error Reporting

        private void RegisterTableNotExists(EfTableAttributes tblObj, EfDiscrepancyReport report)
        {
            report.RegisterProblemTableNotExists(tblObj);
        }

        private void RegisterMissingColumn(EfTableAttributes tblObj, string colName,
            string sqlDataType, bool nullable, bool keyColumn, EfDiscrepancyReport report)
        {
            report.RegisterProblem(tblObj, colName, sqlDataType, nullable, keyColumn, FileSegment.EntityProperties,
                ReportProblem.DbColumnMissing);


            //var clrType = SqlToClrDataType(sqlDataType, nullable); //, out bool? unicode, out bool noteAsRequired);
            //var propertyName = PascalNameFromSqlName(sqlColName);

            //RegisterProblem(string sqlTableName, string enetityName, string sqlColumnName,
            //    ReportProblem problem, List < ProblemDto > correctionCode)
            ////var correctionCode = EfDiscrepancyCodeCorrectionFragment.CreateCorrectionCode(entityName, sqlColName,
            ////    sqlDataType, keyColumn, nullable, clrType, propertyName, unicode, noteAsRequired,
            ////    ReportProblem.DbColumnMissing);

            ////report.RegisterProblem(sqlTableName, entityName, sqlColName, ReportProblem.DbColumnMissing,
            ////    correctionCode);

            //var ret = new EfColumnAttributes
            //{
            //    Required = !nullable,
            //    ClrDataType = clrType,
            //    ClrName = propertyName,
            //    IsUnicode = unicode,
            //    SqlDataType = sqlDataType,
            //    SqlName = sqlColName
            //};

            //return ret;
        }

        public class EfDiscrepancyReport
        {
            internal List<EfDiscrepancyFile> Files { get; set; }= new List<EfDiscrepancyFile>();

            private EfDiscrepancyFile GetFile(string sqlTableName, string entityName)
            {
                var file = Files.FirstOrDefault(t => t.SqlTableName == sqlTableName);
                if (file?.SqlTableName == null)
                    Files.Add(file = new EfDiscrepancyFile(sqlTableName, entityName));

                return file;
            }

            internal void RegisterProblem(EfColumnAttributes problemColumn, FileSegment segment,
                ReportProblem problem, string problemDetails)
            {
                EfDiscrepancyFile file;
                EfDiscrepancyFileSegment segmentObj;
                switch (problem)
                {
                    //case ReportProblem.AnnotationNotMatchClrDataType:
                    //    break;
                    case ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype:
                    case ReportProblem.ColumnNotInDatabase:
                    case ReportProblem.DbDataTypeNotMatchDefinedSqlType:
                    case ReportProblem.DataTypeNullableButColumnDoesntTakeNulls:
                        file = GetFile(problemColumn.Parent.SqlTableName, problemColumn.Parent.EntityName);
                        segmentObj = file.GetFileSegment(FileSegment.EntityProperties);
                        segmentObj.CorrectionFragments.Add(
                            new EfDiscrepancyCodeCorrectionFragment(problemColumn, problem, problemDetails));
                        break;
                    case ReportProblem.ContextFixedLengthPropertyMissing:
                    case ReportProblem.ContextFixedLengthPropertyToBeRemoved:
                    case ReportProblem.ContextNotUnicodePropertyToBeRemoved:
                    case ReportProblem.ContextNotUnicodePropertyMissing:
                        file = GetFile(CONTEXT_NAME, CONTEXT_NAME);
                        segmentObj = file.GetFileSegment(FileSegment.ContextOnModelCreating, problemColumn.Parent.EntityName);
                        segmentObj.CorrectionFragments.Add(
                                new EfDiscrepancyCodeCorrectionFragment(problemColumn, problem, problemDetails));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(problem), problem, "From EfDiscrepancyReport.RegisterProblem() -- switch didn't cover...");
                }
            }

            internal void RegisterProblem(EfTableAttributes tblObj, string sqlColumnName, string sqlDataType,
                bool sqlNullable, bool sqlKeyColumn, FileSegment fSegment, ReportProblem problem)
            {
                if (problem != ReportProblem.DbColumnMissing)
                    throw new NotImplementedException(
                        "RegisterProblem(EfTableAttributes tblObj,...) not coded for anything but DbColumnMissing.");

                var file = GetFile(tblObj.SqlTableName, tblObj.EntityName);
                var segment = file.GetFileSegment(fSegment);
                segment.CorrectionFragments.Add(new EfDiscrepancyCodeCorrectionFragment(sqlColumnName, sqlDataType,
                    sqlNullable, sqlKeyColumn, problem));
            }

            /// <summary>
            /// Used for SQL Table Not Exists
            /// </summary>
            /// <param name="tblObj">Table object which points to SQL table that doesn't exist</param>
            internal void RegisterProblemTableNotExists(EfTableAttributes tblObj)
            {
                var file = GetFile(tblObj.SqlTableName, tblObj.EntityName);
                var segmentObj = file.GetFileSegment(FileSegment.TableLevel);
                segmentObj.CorrectionFragments.Add(new EfDiscrepancyCodeCorrectionFragment(ReportProblem.DbTableMissing,
                    tblObj.SqlTableName));
            }

            public string CreateOutputText()
            {
                var sb = new StringBuilder();
                bool contextOnModelCreatingPrinted = false;
                foreach (var file in Files)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine($"File:  {file.FileName}");

                    FileSegment lastSegment = FileSegment.None;
                    string lastEntity = "";
                    foreach (var segment in file.FileSegments)
                    {
                        if (lastSegment == FileSegment.ContextOnModelCreating && lastEntity != segment.EntityName)
                        {
                            sb.AppendLine();
                            sb.AppendLine("\t\t});");
                        }
                        lastSegment = segment.Segment;
                        lastEntity = segment.EntityName;

                        string line;
                        if((line = $"{SegmentName(segment, ref contextOnModelCreatingPrinted)}") != "")
                        {
                            sb.AppendLine();
                            sb.AppendLine(line);
                        }

                        foreach (var fragment in segment.CorrectionFragments)
                        {
                            sb.AppendLine();
                            foreach (var ln in fragment.CorrectionCode.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
                            {
                                sb.AppendLine($"\t\t{ln}");
                            }
                        }
                    }
                    if (file.EntityName  == CONTEXT_NAME)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t});");
                    }
                }

                return sb.ToString();
            }

            private string SegmentName(EfDiscrepancyFileSegment segment, ref bool contextOnModelCreatingPrinted)
            {
                switch (segment.Segment)
                {
                    case FileSegment.EntityProperties:
                        return "\tProperties Section:";
                    case FileSegment.ContextOnModelCreating:
                        var ret = 
                        (!contextOnModelCreatingPrinted ? 
                            "\tOnModelCreating() Method:" + Environment.NewLine + Environment.NewLine
                            :"") + 
                        $"\t\tmodelBuilder.Entity<Entities.{segment.EntityName}>(entity =>" + Environment.NewLine + 
                        "\t\t{";

                        contextOnModelCreatingPrinted = true;
                        return ret;
                    case FileSegment.TableLevel:
                        return "";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(segment), segment, "From EfDiscrepancyReport.SegmentName()");
                }
            }
        }

        internal class EfDiscrepancyFile
        {
            internal string SqlTableName { get; private set; }

            internal string EntityName { get; private set; }

            internal string FileName => EntityName + ".cs";

            internal List<EfDiscrepancyFileSegment> FileSegments { get; set; } =
                new List<EfDiscrepancyFileSegment>();

            internal EfDiscrepancyFile(string sqlTableName, string entityName)
            {
                SqlTableName = sqlTableName;
                EntityName = entityName ?? PascalNameFromSqlName(sqlTableName);
            }

            internal EfDiscrepancyFileSegment GetFileSegment(FileSegment segment, string entityName = null)
            {
                EfDiscrepancyFileSegment returnSegment;
                returnSegment = string.IsNullOrWhiteSpace(entityName)
                    ? FileSegments.FirstOrDefault(c => c.Segment == segment)
                    : FileSegments.FirstOrDefault(c => c.Segment == segment && c.EntityName == entityName);
                if (returnSegment == null)
                    FileSegments.Add(returnSegment = new EfDiscrepancyFileSegment
                        {Segment = segment, EntityName = entityName});
                return returnSegment;
            }
        }

        internal class EfDiscrepancyFileSegment
        {
            internal FileSegment Segment { get; set; }

            internal string EntityName { get; set; }

            internal List<EfDiscrepancyCodeCorrectionFragment> CorrectionFragments { get; set; } =
                new List<EfDiscrepancyCodeCorrectionFragment>();
        }

        public class EfDiscrepancyCodeCorrectionFragment
        {
            public string CorrectionCode { get; set; }
            //public string SqlColumnName { get; private set; }
            //public string PropertyName { get; private set; }
            //public FileSegment SectionOfFile { get; set; }
            //internal ReportProblem Error { get; set; }
            //public string ErrorDescription => Error.ToString();
            //public string ErrorDetails { get; set; }

            //internal EfDiscrepancyCodeCorrectionFragment(string sqlColumnName)
            //{
            //    SqlColumnName = sqlColumnName;
            //    PropertyName = PascalNameFromSqlName(sqlColumnName);
            //}

            //internal EfDiscrepancyCodeCorrectionFragment(string sqlColumnName, string propertyName)
            //{
            //    SqlColumnName = sqlColumnName;
            //    PropertyName = propertyName;
            //}

            internal EfDiscrepancyCodeCorrectionFragment(EfColumnAttributes colObj, ReportProblem problem,
                string problemDetails)
            {
                switch (problem)
                {
                    //case ReportProblem.AnnotationNotMatchClrDataType:
                    //    break;
                    case ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype:
                    case ReportProblem.DbDataTypeNotMatchDefinedSqlType:
                    case ReportProblem.DataTypeNullableButColumnDoesntTakeNulls:
                        CorrectionCode =
                            "// Update Property in Entity file" + Environment.NewLine + 
                            (string.IsNullOrWhiteSpace(problemDetails) ? ""
                            : $"// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            colObj.PropertyDefinition();
                        break;
                    case ReportProblem.ColumnNotInDatabase:
                        CorrectionCode =
                            "// This column is no longer in the database" + Environment.NewLine +
                            (string.IsNullOrWhiteSpace(problemDetails) ? ""
                                : $"// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            colObj.PropertyDefinition();
                        break;
                    case ReportProblem.ContextFixedLengthPropertyMissing:
                        CorrectionCode =
                            "\t// Add entity.Property setting described below" + Environment.NewLine +
                            (string.IsNullOrWhiteSpace(problemDetails)
                                ? ""
                                : $"\t// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            $"\tentity.Property(e => e.{colObj.ClrName}).IsFixedLength();";
                        break;
                    case ReportProblem.ContextFixedLengthPropertyToBeRemoved:
                        CorrectionCode =
                            "\t// Remove entity.Property setting described below" + Environment.NewLine +
                            (string.IsNullOrWhiteSpace(problemDetails)
                                ? ""
                                : $"\t// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            $"\tentity.Property(e => e.{colObj.ClrName}).IsFixedLength();";
                        break;
                    case ReportProblem.ContextNotUnicodePropertyMissing:
                        CorrectionCode =
                            "\t// Add entity.Property setting described below" + Environment.NewLine +
                            (string.IsNullOrWhiteSpace(problemDetails)
                                ? ""
                                : $"\t// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            $"\tentity.Property(e => e.{colObj.ClrName}).IsUnicode(false);";
                        break;
                    case ReportProblem.ContextNotUnicodePropertyToBeRemoved:
                        CorrectionCode =
                            "\t// Remove entity.Property setting described below" + Environment.NewLine +
                            (string.IsNullOrWhiteSpace(problemDetails)
                                ? ""
                                : $"\t// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            $"\tentity.Property(e => e.{colObj.ClrName}).IsUnicode(false);";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(problem), problem,
                            "in EfDiscrepancyCodeCorrectionFragment.constructor()");
                }
            }

            public EfDiscrepancyCodeCorrectionFragment(string sqlColumnName, string sqlDataType, in bool sqlNullable,
                in bool sqlKeyColumn, ReportProblem problem)
            {
                if (problem != ReportProblem.DbColumnMissing)
                    throw new NotImplementedException(
                        "EfDiscrepancyCodeCorrectionFragment(string sqlColumnName...) is only coded to handle DbColumnMissing.");

                var propertyName = PascalNameFromSqlName(sqlColumnName);
                var clrType = SqlToClrDataTypeString(sqlDataType, sqlNullable, out bool includeRequired);
                var keyRequiredText = sqlKeyColumn ? ", Key" : (includeRequired ? ", Required" : "");

                CorrectionCode =
                    "// Missing column to be added to the Entity" + Environment.NewLine +
                    $"[Column(\"{sqlColumnName}\", TypeName = \"{sqlDataType}\")" + $"{keyRequiredText}]" +
                    Environment.NewLine +
                    $"public {clrType} {propertyName} {{ get; set; }}";
            }

            public EfDiscrepancyCodeCorrectionFragment(ReportProblem reportProblem, string problemDetails)
            {
                switch (reportProblem)
                {
                    case ReportProblem.DbTableMissing:
                        CorrectionCode =
                            $"The DB Table {problemDetails} doesn't exist.  Remove this file from the project." +
                            Environment.NewLine +
                            $"This will also cause you to have to update the {CONTEXT_NAME} to remove the corresponding DB Set" +
                            Environment.NewLine +
                            $"and possibly an OnModelCreating() \"modelBuilder.Entity<{problemDetails}>(entity => \" section." +
                            Environment.NewLine;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(reportProblem), reportProblem, 
                            "From EfDiscrepancyCodeFragment.ctor (the one with 2 args)");
                }   
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
            DataTypeNotNullableButColumnTakesNulls,
            DataTypeNullableButColumnDoesntTakeNulls,
            ColumnNotInDatabase,
            ContextNotUnicodePropertyMissing,
            ContextNotUnicodePropertyToBeRemoved,
            ContextFixedLengthPropertyMissing,
            ContextFixedLengthPropertyToBeRemoved,
            DbDataTypeNotMatchDefinedSqlType,
            DbTableMissing
        }

        #endregion

        #region Utility Methods

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
        
        private static string SqlToClrDataTypeString(string dataType, bool nullable, out bool includeRequired)
        {
            includeRequired = false;
            var dtParts = dataType.Split(new[] { '(', ')', ',' });

            switch (dtParts[0].ToLower())
            {
                case "varchar":
                case "char":
                case "nvarchar":
                case "nchar":
                    if (!nullable) includeRequired = true;
                    return "string";
                case "time":
                    return nullable ? "TimeSpan?" : "TimeSpan";
                case "date":
                    return nullable ? "DateTime?" : "DateTime";
                case "datetimeoffset":
                    return nullable ? "DateTimeOffset?" : "DateTimeOffset";
                case "bit":
                    return nullable ? "bool?" : "bool";
                case "tinyint":
                    return nullable ? "byte?" : "byte";
                case "smallint":
                    return nullable ? "short?" : "short";
                case "int":
                    return nullable ? "int?" : "int";
                case "bigint":
                    return nullable ? "long?" : "long";
                case "decimal":
                case "numeric":
                    return nullable ? "decimal?" : "decimal";
                case "varbinary":
                    if (!nullable) includeRequired = true;
                    return "byte[]";
                default:
                    throw new NotImplementedException(
                        $"SqlToClrDataTypeString() switch doesn't cover case: '{dtParts[0]}");
            }
        }

        private static string ClrDataTypeToString(Type dataType)
        {
            switch (dataType.ToString())
            {
                case "System.Int64":
                    return "long";
                case "System.Boolean":
                    return "bool";
                case "System.Nullable`1[System.Boolean]":
                    return "bool?";
                case "System.Nullable`1[System.Int64]":
                    return "long?";
                case "System.Int32":
                    return "int";
                case "System.Nullable`1[System.Int32]":
                    return "int?";
                case "System.Int16":
                    return "short";
                case "System.Nullable`1[System.Int16]":
                    return "short?";
                case "System.Byte":
                    return "byte";
                case "System.Nullable`1[System.Byte]":
                    return "byte?";
                case "System.Nullable`1[System.DateTimeOffset]":
                    return "DateTimeOffset?";
                case "System.Nullable`1[System.TimeSpan]":
                    return "TimeSpan?";
                case "System.DateTimeOffset":
                    return "DateTimeOffset";
                case "System.DateTime":
                    return "DateTime";
                case "System.Decimal":
                    return "decimal";
                default:
                    throw new NotImplementedException($"ClrDataTypeToString() switch doesn't cover case: '{dataType.ToString()}'");
            }
        }

        #endregion


        private const string COLUMN_QUERY = "SELECT	c.name, \n\r" +
                                            "TYPE_NAME(system_type_id) + \n\r" +
                                            "CASE\n\r" +
                                            "WHEN TYPE_NAME(system_type_id) LIKE '%char' and max_length = -1 \n\r" +
                                            "THEN '(max)' \n\r" +
                                            "WHEN TYPE_NAME(system_type_id) LIKE 'n%char' \n\r" +
                                            "THEN CONCAT('(', max_length / 2, ')')\n\r" +
                                            "WHEN TYPE_NAME(system_type_id) LIKE '%char' \n\r" +
                                            "OR TYPE_NAME(system_type_id) = 'binary'  \n\r" +
                                            "THEN CONCAT('(', max_length, ')')\n\r" +
                                            "WHEN TYPE_NAME(system_type_id) = 'numeric' \n\r" +
                                            "OR TYPE_NAME(system_type_id) = 'decimal' \n\r" + 
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

        private const string FOREIGN_KEY_QUERY = "";
    }

    public enum FileSegment
    {
        EntityProperties,
        ContextOnModelCreating,
        None,
        TableLevel
    }

#if TestingInternalDatatypeProblems

/**** Go to "C:\Users\bm70142\OneDrive - harriscomputer\Documents\PulseCheck work\Testing Data for EfToDbSynchHelper.sql"
 **** for SQL data to create sample tables */

    // Output from API call with test data (model checking only)
        File:  _ColumnProblemTest.cs

	        Properties Section:

		        // Update Property in Entity file
		        // Problem: NON NULLABLE 'bigint' SQL data type doesn't have a property data type of "long"
		        [Column("bigint_nullable_required", TypeName = "bigint"), Required]
		        public long BigIntNullableRequired { get; set; }

		        // Update Property in Entity file
		        // Problem: [var]binary length doesn't match CLR MaxLength annotation
		        [Column("col_binary12", TypeName = "varbinary(12)")]
		        public byte[] Binary12 { get; set; }

		        // Update Property in Entity file
		        // Problem: NON NULLABLE 'bit' SQL data type doesn't have a property data type of "bool"
		        [Column("bit_nullable_required", TypeName = "bit"), Required]
		        public bool BitNullableRequired { get; set; }

		        // Update Property in Entity file
		        // Problem: NON NULLABLE 'date' SQL data type doesn't have a property data type of "DateTime"
		        [Column("date_nullable_required", TypeName = "date"), Required]
		        public DateTime DateNullableRequired { get; set; }

		        // Update Property in Entity file
		        // Problem: NON NULLABLE 'datetimeoffset' SQL data type doesn't have a property data type of "DateTimeOffset"
		        [Column("datetimeoffset_nullable_required", TypeName = "datetimeoffset"), Required]
		        public DateTimeOffset DateTimeOffsetNullableRequired { get; set; }

		        // Update Property in Entity file
		        // Problem: NON NULLABLE 'decimal' SQL data type doesn't have a property data type of "decimal"
		        [Column("decimal_nullable_required", TypeName = "decimal(11,2)"), Required]
		        public decimal DecimalNullableRequired { get; set; }

		        // Update Property in Entity file
		        // Problem: NON NULLABLE 'decimal' SQL data type doesn't have a property data type of "decimal"
		        [Column("decimal_to_int", TypeName = "decimal"), Required]
		        public decimal DecimalToInt { get; set; }

		        // Update Property in Entity file
		        // Problem: varchar max length doesn't match SQL data type.
		        [Column("description", TypeName = "varchar(90)"), Required]
		        public string Description { get; set; }

		        // Update Property in Entity file
		        // Problem: NON NULLABLE 'numeric' SQL data type doesn't have a property data type of "decimal"
		        [Column("numeric_nullable_required", TypeName = "numeric(11,2)"), Required]
		        public decimal NumericNullableRequired { get; set; }

		        // Update Property in Entity file
		        // Problem: varchar SQL data type not identified as 'String'.
		        [Column("col_varchar_to_byte", TypeName = "varchar(20)")]
		        public string VarcharToByte { get; set; }


        File:  EmarContext.cs

	        OnModelCreating() Method:

		        modelBuilder.Entity<Entities._ColumnProblemTest>(entity =>
		        {

			        // Add entity.Property setting described below
			        // Problem: binary not identified as Fixed Length
			        entity.Property(e => e.Binary).IsFixedLength();

			        // Add entity.Property setting described below
			        // Problem: Properties don't declare char value as Fixed Length.
			        entity.Property(e => e.Char1).IsFixedLength();

			        // Add entity.Property setting described below
			        // Problem: Properties don't declare nchar value as Fixed Length.
			        entity.Property(e => e.Nchar1).IsFixedLength();

			        // Remove entity.Property setting described below
			        // Problem: Properties incorrectly declare nchar value as Non-Unicode.
			        entity.Property(e => e.Nchar2).IsUnicode(false);

			        // Remove entity.Property setting described below
			        // Problem: Properties incorrectly declare 'nvarchar' value as Fixed Length.
			        entity.Property(e => e.Nvarchar2).IsFixedLength();

			        // Remove entity.Property setting described below
			        // Problem: varbinary identified as Fixed Length
			        entity.Property(e => e.VarBinary).IsFixedLength();

			        // Add entity.Property setting described below
			        // Problem: Properties don't declare 'varchar' value as Non-Unicode.
			        entity.Property(e => e.Varchar1).IsUnicode(false);

			        // Remove entity.Property setting described below
			        // Problem: Properties incorrectly declare 'varchar' value as Fixed Length.
			        entity.Property(e => e.Varchar2).IsFixedLength();

		        });

#endif
}