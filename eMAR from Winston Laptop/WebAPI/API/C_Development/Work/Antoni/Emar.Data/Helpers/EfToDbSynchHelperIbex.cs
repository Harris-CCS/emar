using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Emar.Data.Helpers
{
    public class EfToDbSynchHelperIbex
    {
        private readonly IbexContext _context;
        private const string CONTEXT_NAME = "IbexContext";


        public EfToDbSynchHelperIbex(IbexContext context)
        {
            _context = context;
        }

        public object CompareEfToDb(EfToDbSynchHelperParams parms)
        {
            var tables = new List<EfTableAttributes>(SurveyEfEntities().OrderBy(t => t.EntityName));

#if TestingEfUtility
            var tablesToKeep =
                ",BradSimpleChild,BradSimpleParent,BradTwoKeyChild,BradTwoKeyParent,BradTwoFksChild,BradTwoFksParent,";
                //",OrderEvent,OrderAdministrations,PatientOrders"; //",Action,PatientOrder,BradNameChild,BradNameParent,";
            for (int i = tables.Count - 1; i >= 0; i--)
            {
                if (tablesToKeep.Contains("," + tables[i].EntityName + ","))
                    continue;
                tables.RemoveAt(i);
            }
#endif

            if (ProblemsExitInEfDefinitions(tables, out EfDiscrepancyReport report))
                return report;

            CompareSurveyToDatabase(tables, parms, report);

            if (!report.Files.Any()) return tables;

            return report;
        }

        public EfDiscrepancyReport AddTables(EfToDbSynchHelperParams parms)
        {
            object compareReturn = CompareEfToDb(parms);
            if (compareReturn.GetType() == typeof(EfToDbSynchHelper.EfDiscrepancyReport))
                throw new ApplicationException(
                    "EF Structure is not clean.  Run GET /api/EfConfiguration/Confirm and resolve all returned issues before adding new tables.");

            var tables = new List<EfTableAttributes>(SurveyEfEntities().OrderBy(t => t.EntityName));
            var report = new EfDiscrepancyReport();

            foreach (var table in parms.TablesToAdd)
            {
                if (table != table.Trim())
                {
                    throw new ArgumentException($"The table \"{table}\" has spaces before or after the name");
                }

                var existingEntity = tables.FirstOrDefault(t => t.SqlTableName == table);
                if(existingEntity != null)
                    report.RegisterProblem(existingEntity, ReportProblem.RequestedNewTableAlreadyExists);
                else
                {
                    var tblObj = new EfTableAttributes {SqlTableName = table, CreateNewTable = true};
                    tables.Add(tblObj);
                    // Adding the Report Table here so that we are sure the first declaration of
                    // the report notes it as "CreateNew"
                    report.GetFile(tblObj.SqlTableName, tblObj.EntityName, true);
                }
            }

            CompareSurveyToDatabase(tables, parms, report);

            return report;
        }

        #region Survey Code

        private List<EfTableAttributes> SurveyEfEntities()
        {
            var contextPropertyNames = new Dictionary<string, string>();
            foreach (var propertyInfo in _context.GetType().GetProperties())
            {
                var propTypeSplit = propertyInfo.PropertyType.FullName?
                    .Split('[', ']', StringSplitOptions.RemoveEmptyEntries);
                if (propTypeSplit != null && propTypeSplit.Length == 1)
                    continue;

                var entityName = propTypeSplit?[1].Split(',')[0];
                contextPropertyNames.Add(entityName, propertyInfo.Name);
            }

            var tbls = new List<EfTableAttributes>();
            foreach (IEntityType entity in _context.Model.GetEntityTypes().OrderBy(a => a.Name))
            {
                var table =
                    new EfTableAttributes
                    {
                        EntityName = entity.Name,
                        ContextPropertyName = contextPropertyNames[entity.Name],
                        SqlTableName = entity.GetTableName()
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
                table.Columns = columns;

                tbls.Add(table);
            }

            foreach (IEntityType entity in _context.Model.GetEntityTypes().OrderBy(a => a.Name))
            {
                var table = tbls.FirstOrDefault(t => t.EntityFullName == entity.Name);
                Debug.Assert(table != null, nameof(table) + " != null");

                var foreignKeys = entity.GetForeignKeys()
                    .Select(fk => new EfForeignKeyAttributes(table, tbls)
                    {
                        DeclaringEntityProperties = fk.Properties.Select(property => property.Name).ToList(),
                        DeclaringEntityNavigationProperty = fk.DependentToPrincipal.Name,
                        PrincipalEntityTypeFullName = fk.PrincipalEntityType.Name,
                        PrincipalEntityNavigationProperty = fk.PrincipalToDependent.Name,
                        DeleteBehavior = fk.DeleteBehavior,
                        ConstraintName = fk.GetConstraintName()
                    })
                    .ToList();

                table.ForeignKeys = foreignKeys;
            }

            return tbls;
        }

        public class EfTableAttributes
        {
            private bool _createNewTable;

            public string EntityName
            {
                get =>    EntityFullName.Substring(EntityFullName.LastIndexOf('.') + 1);
                set
                {
                    Debug.Assert(value.Contains('.'), "EntityName must be assigned as the fully-qualified name.");
                    EntityFullName = value;
                }
            }
            public string EntityFullName { get; private set; }

            public List<EfColumnAttributes> Columns { get; set; }
            public List<EfForeignKeyAttributes> ForeignKeys { get; set; } = new List<EfForeignKeyAttributes>();
            public String SqlTableName { get; set; }
            public string ContextPropertyName { get; set; }

            public bool CreateNewTable
            {
                get => _createNewTable;
                set
                {
                    _createNewTable = value;
                    if (value)
                    {
                        EntityFullName = PascalNameFromSqlName(SqlTableName);
                        if (EntityFullName.EndsWith('s'))
                            EntityFullName = EntityFullName.Substring(0, EntityFullName.Length - 1);
                        ContextPropertyName = EntityFullName + "s";
                        EntityFullName = "Emar.Data.Entities." + EntityFullName;
                    }
                }
            }
        }

        public class EfForeignKeyAttributes
        {
            // DeclaringEntityType
            private EfTableAttributes _parentTable;
            private readonly IEnumerable<EfTableAttributes> _tableList;
            internal EfTableAttributes ParentTable => _parentTable;

            internal EfTableAttributes PrincipalTable => _tableList.FirstOrDefault(t =>
                t.EntityName == (SqlPrincipalEntityType?.Substring(SqlPrincipalEntityType.LastIndexOf('.') + 1) ??
                                 PrincipalEntityType?.Substring(PrincipalEntityType.LastIndexOf('.') + 1)));

            // Entity Properties //
            internal List<string> DeclaringEntityProperties { get; set; }
            internal string DeclaringEntityNavigationProperty { get; set; }

            internal string PrincipalEntityType =>
                PrincipalEntityTypeFullName?.Substring(PrincipalEntityTypeFullName.LastIndexOf('.') + 1);
            internal string PrincipalEntityTypeFullName { get; set; }
            internal DeleteBehavior DeleteBehavior { get; set; }
            internal string ConstraintName { get; set; }
            internal string PrincipalEntityNavigationProperty { get; set; }

            // SQL-generated Entity Properties //
            internal List<string> SqlDeclaringEntityProperties { get; set; }
            internal string SqlDeclaringEntityNavigationProperty { get; set; }
            internal string SqlPrincipalEntityType { get; set; }

            internal string SqlPrincipalEntityTypeFullName => SqlPrincipalEntityType == null
                ? null
                : "Emar.Data.Entities." + SqlPrincipalEntityType;
            internal DeleteBehavior SqlDeleteBehavior { get; set; }
            internal string SqlConstraintName { get; set; }
            internal string SqlPrincipalEntityNavigationProperty { get; set; }

            // SQL Fields //
            private string _fkName;
            private string _declaringEntityTable;
            private string _principalEntityTable;
            private string _declaringEntityFields;
            private bool _duplicatePrincipalForeignKeys;

            // Derived Properties //
            internal bool AccountedFor { get; set; } = false;
            internal bool SqlOnly => string.IsNullOrEmpty(DeclaringEntityNavigationProperty);
            internal bool EntityOnly => string.IsNullOrEmpty(SqlDeclaringEntityNavigationProperty);

            // Constructor
            public EfForeignKeyAttributes(SqlDataReader reader, EfTableAttributes parentTable,
                IEnumerable<EfTableAttributes> tableList)
            {
                _tableList = tableList;
                _parentTable = parentTable;
                ReadTheReader(reader);
            }

            public EfForeignKeyAttributes(EfTableAttributes parentTable,
                IEnumerable<EfTableAttributes> tableList)
            {
                _tableList = tableList;
                _parentTable = parentTable;
            }

            // SQL Properties
            internal void RegisterSqlValues(SqlDataReader reader)
            {
                ReadTheReader(reader);
            }

            internal void SetAccountedFor()
            {
                AccountedFor = true;
            }

            private void ReadTheReader(SqlDataReader reader)
            {
                _fkName = reader["name"].ToString();
                _declaringEntityTable = reader["DeclaringEntityTable"].ToString();
                _principalEntityTable = reader["PrincipalEntityTable"].ToString();
                _declaringEntityFields = reader["DeclaringEntityFields"].ToString();
                _duplicatePrincipalForeignKeys = Convert.ToInt16(reader["DuplicatePrincipalForeignKeys"]) == 1;

                SqlConstraintName = _fkName;

                if (_declaringEntityFields != null)
                    SqlDeclaringEntityProperties = new List<string>(
                        _declaringEntityFields
                            .Replace(" ", "")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(field =>
                                _parentTable.Columns.FirstOrDefault(c => c.SqlName == field)?.ClrName)
                    );

                var principalTable = _tableList.FirstOrDefault(t => t.SqlTableName == _principalEntityTable);
                if (principalTable == null)
                    return;

                SqlPrincipalEntityType = principalTable.EntityName;
                
                if (_duplicatePrincipalForeignKeys)
                {
                    SqlDeclaringEntityNavigationProperty = SqlDeclaringEntityProperties[0]
                        .EndsWith("Id", StringComparison.InvariantCultureIgnoreCase)
                        ? SqlDeclaringEntityProperties[0].Substring(0, SqlDeclaringEntityProperties[0].Length - 2)
                        : SqlDeclaringEntityProperties[0];

                    if (!SqlDeclaringEntityNavigationProperty.EndsWith(SqlDeclaringEntityNavigationProperty))
                        throw new NotImplementedException("Didn't account for name of foreign key column not ending with 'foreign_table' + 'Id'");
                }
                else
                    SqlDeclaringEntityNavigationProperty = principalTable.EntityName;

                SqlPrincipalEntityNavigationProperty = (_duplicatePrincipalForeignKeys)
                    ? ParentTable.EntityName + SqlDeclaringEntityNavigationProperty
                    : _parentTable.EntityName + "s";

                SqlDeleteBehavior = DeleteBehavior.ClientSetNull;
            }

            public bool SqlDoesntMatchEntity()
            {
                if (DeclaringEntityProperties.Where((t, i) => t != SqlDeclaringEntityProperties[i]).Any())
                    return true;
                if (PrincipalEntityType != SqlPrincipalEntityType)
                    return true;
                //if (DeleteBehavior != SqlDeleteBehavior)
                //    return true;
                //// Decided that differences in NavigationProperty are not germane enough to constitute a discrepancy ////
                //if (SqlPrincipalEntityNavigationProperty != PrincipalEntityNavigationProperty)
                //{
                //    if(!PrincipalEntityNavigationProperty.StartsWith(ParentTable.EntityName) 
                //    || !PrincipalEntityNavigationProperty.EndsWith(SqlDeclaringEntityNavigationProperty))
                //        return true;
                //}
                //if (DeclaringEntityNavigationProperty != SqlDeclaringEntityNavigationProperty)
                //    return true;

                return false;
            }

            public bool MatchesKeyCharacteristics(EfForeignKeyAttributes entityKey, out bool constNameMatches)
            {
                constNameMatches = SqlConstraintName == entityKey.ConstraintName;

                if (SqlDeclaringEntityProperties.Where((t, i) =>
                        entityKey.DeclaringEntityProperties.Count <= i || t != entityKey.DeclaringEntityProperties[i])
                    .Any())
                {
                    return false;
                }

                //// Name of the Navigation Properties doesn't have to match
                //if (SqlDeclaringEntityNavigationProperty != entityKey.DeclaringEntityNavigationProperty)
                //    return false;
                //if (SqlPrincipalEntityNavigationProperty != entityKey.PrincipalEntityNavigationProperty)
                //    return false;
                
                if (SqlPrincipalEntityType != entityKey.PrincipalEntityType)
                    return false;
                return SqlDeleteBehavior == entityKey.DeleteBehavior;
            }

            public string CorrectionCode(FileSegment segment, string commentLine)
            {
                switch (segment)
                {
                    case FileSegment.EntityForeignKeysDeclaring:
                        var principalEntityType = PrincipalEntityType ?? SqlPrincipalEntityType;
                        if (SqlDeclaringEntityProperties != null)
                        {
                            string propertyNameString;
                            if (SqlDeclaringEntityProperties.Count == 1)
                                propertyNameString = SqlDeclaringEntityProperties[0];
                            else
                                propertyNameString = SqlDeclaringEntityProperties.Aggregate("",
                                    (current, property) => current + ((current == "" ? "" : ", ") + property));

                            return (string.IsNullOrEmpty(commentLine)
                                       ? ""
                                       : $"// {commentLine}" + Environment.NewLine) +
                                   $"// For Foreign Key: {SqlConstraintName}" + Environment.NewLine +
                                   Environment.NewLine +
                                   $"[ForeignKey(nameof({propertyNameString}))]" +
                                   Environment.NewLine +
                                   $"[InverseProperty(nameof(Entities.{SqlPrincipalEntityType}.{SqlPrincipalEntityNavigationProperty}))]" +
                                   Environment.NewLine +
                                   $"public virtual {principalEntityType} {SqlDeclaringEntityNavigationProperty} {{ get; set; }}";
                        }
                        else if (DeclaringEntityProperties != null)
                        {
                            string propertyNameString;
                            if (DeclaringEntityProperties.Count == 1)
                                propertyNameString = DeclaringEntityProperties[0];
                            else
                                propertyNameString = DeclaringEntityProperties.Aggregate("",
                                    (current, property) => current + ((current == "" ? "" : ", ") + property));

                            return (string.IsNullOrEmpty(commentLine)
                                       ? ""
                                       : $"// {commentLine} " + Environment.NewLine) +
                                   $"// For Foreign Key: {ConstraintName} " + Environment.NewLine +
                                   Environment.NewLine +
                                   $"[ForeignKey(nameof({propertyNameString}))]" + 
                                   Environment.NewLine +
                                   $"[InverseProperty(nameof(Entities.{PrincipalEntityType}.{PrincipalEntityNavigationProperty}))] " +
                                   Environment.NewLine +
                                   $"public virtual {principalEntityType} {DeclaringEntityNavigationProperty} {{ get; set; }}";
                        }
                        else
                            throw new ArgumentException("from EfForeignKeyAttributes.CorrectionCode(), shouldn't have gotten here.");
                    case FileSegment.EntityForeignKeysInverse:
                        return (string.IsNullOrEmpty(commentLine)
                                   ? ""
                                   : $"// {commentLine}" + Environment.NewLine) +
                               $"// For Foreign Key: {SqlConstraintName ?? ConstraintName}" + Environment.NewLine +
                               $"[InverseProperty(\"{SqlDeclaringEntityNavigationProperty ?? DeclaringEntityNavigationProperty}\")]" +
                               Environment.NewLine +
                               $"public virtual ICollection<{ParentTable.EntityName}> {SqlPrincipalEntityNavigationProperty ?? PrincipalEntityNavigationProperty} {{ get; set; }}";

                    case FileSegment.EntityForeignKeysConstructor:
                        return (string.IsNullOrEmpty(commentLine)
                                   ? ""
                                   : $"\t// {commentLine}" + Environment.NewLine) +
                               $"\t// For Foreign Key: {SqlConstraintName ?? ConstraintName}" + Environment.NewLine +
                               $"\t{SqlPrincipalEntityNavigationProperty ?? PrincipalEntityNavigationProperty} = new HashSet<{ParentTable.EntityName}>();";

                    case FileSegment.ContextOnModelCreating:
                        if (!EntityOnly)
                            return (string.IsNullOrEmpty(commentLine)
                                       ? ""
                                       : $"\t\t// {commentLine}" + Environment.NewLine) +
                                   $"\t\tentity.HasOne(d => d.{SqlDeclaringEntityNavigationProperty})" +
                                   Environment.NewLine +
                                   $"\t\t\t.WithMany(p => p.{SqlPrincipalEntityNavigationProperty})" +
                                   Environment.NewLine +
                                   ((SqlDeclaringEntityProperties.Count == 1)
                                       ? $"\t\t\t.HasForeignKey(d => d.{SqlDeclaringEntityProperties[0]})"
                                       : "ERROR!!!! (multikey)") + Environment.NewLine +
                                   $"\t\t\t.OnDelete(DeleteBehavior.{SqlDeleteBehavior.ToString()})" +
                                   Environment.NewLine +
                                   $"\t\t\t.HasConstraintName(\"{SqlConstraintName}\");";
                        else
                        {
                            //// Side-lining the logic for intelligently duplicating the scaffolding method of
                            //// Assigning the DeleteBehavior for now...
                            //bool enumerateDeletBehav = false;
                            //foreach (var property in SqlDeclaringEntityProperties)
                            //{
                            //    var col = ParentTable.Columns.FirstOrDefault(c => c.ClrName == property);
                            //    if (col?.Required ?? false)
                            //        enumerateDeletBehav = true;
                            //}

                            //var deleteBehavior = !enumerateDeletBehav
                            //    ? ""
                            //    : $"        .OnDelete(DeleteBehavior.{DeleteBehavior.ToString()})" +
                            //      Environment.NewLine;
                            var deleteBehavior = $"\t\t\t.OnDelete(DeleteBehavior.{DeleteBehavior.ToString()})" +
                                                 Environment.NewLine;

                            return (string.IsNullOrEmpty(commentLine)
                                       ? ""
                                       : $"\t\t// {commentLine} " + Environment.NewLine) +
                                   $"\t\tentity.HasOne(d => d.{DeclaringEntityNavigationProperty})" +
                                   Environment.NewLine +
                                   $"\t\t\t.WithMany(p => p.{PrincipalEntityNavigationProperty})" +
                                   Environment.NewLine +
                                   (DeclaringEntityProperties.Count == 1
                                       ? $"\t\t\t.HasForeignKey(d => d.{DeclaringEntityProperties[0]})"
                                       : "ERROR!!!! (multikey)") + Environment.NewLine +
                                   deleteBehavior +
                                   $"\t\t\t.HasConstraintName(\"{ConstraintName}\");";
                        }

                    default:
                        throw new ArgumentException("from EfForeignKeyAttributes.CorrectionCode()",
                            nameof(segment));
                }
            }
        }

        public class EfColumnAttributes
        {
            internal EfTableAttributes Parent { get; set; }

            // Properties from the Annotations
            public string SqlName { get; set; }
            public bool KeyColumn { get; private set; }
            public bool Required { get; set; }
            private string _sqlDataType;
            public string SqlDataType
            {
                get => _sqlDataType;
                set => _sqlDataType = value.ToLower().Replace(" ", "");
            }

            // ClrProperties
            public string ClrName { get; set; }
            public Type ClrDataType { get; set; }
            public int? MaxStringLength { get; set; }
            public bool? IsUnicode { get; set; } = true;
            public bool? IsFixWidth { get; set; }

            // DB Properties
            private string _dbDataType;
            private bool? _dbNullable;
            private bool? _dbPrimaryKey;
            private bool? _isComputed { get; set; }
            public bool? Computed
            {
                get { return _isComputed; }
                set { _isComputed = value; }
            }

            public bool ExistsInDb => _dbNullable != null;
            
            internal void SetNullableForNewTablesColumns(bool nullable)
            {
                _dbNullable = nullable;
            }

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

                var keyRequiredText = (_dbPrimaryKey ?? KeyColumn) ? ", Key" : (includeRequired ? ", Required" : "");

                return $"[Column(\"{SqlName}\", TypeName = \"{sqlDataType}\"){keyRequiredText}]"
                       + Environment.NewLine +
                       $"public {clrDataType} {ClrName} {{ get; set; }}";
            }

            public void RecordDbPropertiesAndConfirm(string dataType, in bool nullable, in bool primaryKey,
                bool computed, EfDiscrepancyReport report)
            {
                _dbDataType = dataType;
                _dbNullable = nullable;
                _dbPrimaryKey = primaryKey;
                _isComputed = computed;
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
                    else if (Required == nullable && !(_isComputed ?? false))
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
                    case "real":
                        CheckRequiredAssumedType(typeParts[0].Trim().ToLower(), ClrDataType,
                            typeof(Single), typeof(Single?), _dbNullable, out probDetails);
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
                        throw new NotImplementedException($"EntityColumnTypeNotMatchSql() case missing '{typeParts[0]}'");
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

        #endregion

        #region Model Checking

        private void CompareSurveyToDatabase(List<EfTableAttributes> tables, EfToDbSynchHelperParams parms,
            EfDiscrepancyReport report)
        {
            // Create a list of tables that need to be removed from the model because they don't exist in the DB
            var tablesToRemove = new List<EfTableAttributes>();
            using (var conn = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                conn.Open();
                foreach (var tbl in tables)
                {
                    if (tbl.CreateNewTable)
                    {
                        report.RegisterProblem(tbl, ReportProblem.DocumentNewTable);
                        tbl.Columns = new List<EfColumnAttributes>();
                    }
                    if (parms.EntitiesNotMapped.Contains(tbl.EntityName))
                    {
                        // Add to the list of tables to remove below so that we don't claim all columns are missing
                        tablesToRemove.Add(tbl);
                        continue;
                    }

                    using (var comm = new SqlCommand(string.Format(TABLE_VIEW_QUERY, tbl.SqlTableName), conn))
                    {
                        var sqlObjectType = comm.ExecuteScalar().ToString();
                        if (sqlObjectType == "missing")
                        {
                            RegisterTableNotExists(tbl, report);
                            tablesToRemove.Add(tbl);
                        }
                        else
                        {
                            var manKey = parms.ManufacturedKeys.Find(p => p.Table == tbl.SqlTableName);
                            comm.CommandText = manKey == null
                                ? string.Format(COLUMN_QUERY, tbl.SqlTableName)
                                : ManufactureSelectStatement(manKey);
                            using (var reader = comm.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    ReadTheReader(reader, out string colName, out string sqlDataType,
                                        out bool sqlNullable, out bool primaryKey, out bool computed);
                                    // Find the record in the list of columns
                                    var col = tbl.Columns.FirstOrDefault(c => c.SqlName == colName);
                                    if (col?.SqlName == null)
                                    {
                                        tbl.Columns.Add(RegisterMissingColumn(tbl, colName, sqlDataType, sqlNullable, 
                                            primaryKey, computed, report));
                                    }
                                    else
                                        col.RecordDbPropertiesAndConfirm(sqlDataType, sqlNullable, primaryKey, 
                                            computed, report);
                                }
                            }

                            //// Foreign Key Stuff ////
                            comm.CommandText = string.Format(FOREIGN_KEY_QUERY, tbl.SqlTableName);
                            using (var reader = comm.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var fkName = reader["name"].ToString();
                                    var existingKey = tbl.ForeignKeys.FirstOrDefault(k => k.ConstraintName == fkName);
                                    if (existingKey != null)
                                        existingKey.RegisterSqlValues(reader);
                                    else
                                    {
                                        var fk = new EfForeignKeyAttributes(reader, tbl, tables);
                                        if (fk.PrincipalEntityTypeFullName == null
                                            && fk.SqlPrincipalEntityTypeFullName == null)
                                        {
                                            if(parms.ForeignKeysToIgnore == null 
                                               || !parms.ForeignKeysToIgnore.Contains(fk.SqlConstraintName))
                                                report.RegisterProblem(fk, ReportProblem.ForeignTableNotInModel);
                                        }
                                        else 
                                            tbl.ForeignKeys.Add(fk);
                                    }
                                }
                            }

                            var sqlOnlyKeys = new List<EfForeignKeyAttributes>();
                            var entityOnlyKeys = new List<EfForeignKeyAttributes>();
                            foreach (var foreignKey in tbl.ForeignKeys)
                            {
                                if (foreignKey.SqlOnly)
                                    sqlOnlyKeys.Add(foreignKey);
                                else if (foreignKey.EntityOnly)
                                    entityOnlyKeys.Add(foreignKey);
                                else if (foreignKey.SqlDoesntMatchEntity())
                                    report.RegisterProblem(foreignKey, ReportProblem.ForeignKeyDoesntMatch);
                            }

                            for (var i = 0; i < sqlOnlyKeys.Count; i++)
                            {
                                var sqlKey = sqlOnlyKeys[i];
                                for (var index = entityOnlyKeys.Count - 1; index >= 0; index--)
                                {
                                    if (!sqlKey.MatchesKeyCharacteristics(entityOnlyKeys[index], out bool namesMatch))
                                        continue;

                                    Debug.Assert(!namesMatch);

                                    report.RegisterProblem(sqlKey, ReportProblem.ForeignKeyNameChanged);
                                    entityOnlyKeys.RemoveAt(index);
                                    sqlKey.SetAccountedFor();
                                    break;
                                }
                            }

                            foreach (var sqlKey in sqlOnlyKeys.Where(k => !k.AccountedFor))
                                report.RegisterProblem(sqlKey, ReportProblem.ForeignKeySqlOnly);

                            foreach (var entityKey in entityOnlyKeys.Where(entityKey => !parms.ForeignKeysToIgnore.Contains(entityKey.ConstraintName)))
                                report.RegisterProblem(entityKey, ReportProblem.ForeignKeyEntityOnly);
                        }
                    }
                }
            }

            foreach (var table in tablesToRemove) 
                tables.Remove(table);

            foreach (var column in tables.SelectMany(table => table.Columns.Where(column => !column.ExistsInDb)))
            {
                report.RegisterProblem(column, FileSegment.EntityProperties,
                    ReportProblem.ColumnNotInDatabase, null);
            }
        }

        private string ManufactureSelectStatement(ManufacturedKey manKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine("WITH keyColumns AS(");
            sb.AppendLine("    SELECT * FROM(VALUES");
            bool firstLine = true;
            foreach (var col in manKey.KeyColumns.Split(",", StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine($"        {(firstLine ? "" : ",")}('{col}')");
                firstLine = false;
            }

            sb.AppendLine("    ) t (column_name)");
            sb.AppendLine(")");
            sb.AppendLine("SELECT  c.name, ");
            sb.AppendLine("        TYPE_NAME(system_type_id) +");
            sb.AppendLine("        CASE");
            sb.AppendLine("            WHEN TYPE_NAME(system_type_id) LIKE '%char' and max_length = -1");
            sb.AppendLine("                THEN '(max)'");
            sb.AppendLine("            WHEN TYPE_NAME(system_type_id) LIKE 'n%char'");
            sb.AppendLine("                THEN CONCAT('(', max_length / 2, ')')");
            sb.AppendLine("            WHEN TYPE_NAME(system_type_id) LIKE '%char'");
            sb.AppendLine("            OR TYPE_NAME(system_type_id) LIKE '%binary'");
            sb.AppendLine("                THEN CONCAT('(', max_length, ')')");
            sb.AppendLine("            WHEN TYPE_NAME(system_type_id) = 'numeric'");
            sb.AppendLine("            OR TYPE_NAME(system_type_id) = 'decimal'");
            sb.AppendLine("                THEN CONCAT('(', precision, ',', scale, ')')");
            sb.AppendLine("            ELSE ''");
            sb.AppendLine("        END AS datatype");
            sb.AppendLine("        , is_nullable");
            sb.AppendLine("        , CASE WHEN kc.column_name IS NULL THEN 0 ELSE 1 END AS KeyColumn, c.is_computed");
            sb.AppendLine("FROM    sys.columns c");
            sb.AppendLine("LEFT JOIN   keyColumns kc");
            sb.AppendLine("        ON c.name = kc.column_name");
            sb.AppendLine($"WHERE c.object_id = OBJECT_ID('{manKey.Table}')");

            return sb.ToString();
        }


        private void ReadTheReader(SqlDataReader reader, out string colName, out string dataType, out bool nullable,
            out bool primaryKey, out bool computed)
        {
            colName = reader["name"].ToString();
            dataType = reader["datatype"].ToString();
            nullable = Convert.ToByte(reader["is_nullable"]) != 0;
            primaryKey= Convert.ToByte(reader["KeyColumn"]) != 0;
            computed = Convert.ToByte(reader["is_computed"]) != 0;
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
            report.RegisterProblem(tblObj, ReportProblem.DbTableMissing);
        }

        private EfColumnAttributes RegisterMissingColumn(EfTableAttributes tblObj, string colName,
            string sqlDataType, bool nullable, bool keyColumn, bool computed, EfDiscrepancyReport report)
        {
            report.RegisterProblem(tblObj, colName, sqlDataType, nullable, keyColumn, FileSegment.EntityProperties,
                ReportProblem.DbColumnMissing);
            
            // Create the Column Object to return for inclusion in the Tables Columns collection

            var clrType = SqlToClrDataType(sqlDataType, nullable, out bool? isUnicode,
                out bool? isFixWidth, out int? maxStringLength); //, out bool? unicode, out bool noteAsRequired);
            var propertyName = PascalNameFromSqlName(colName);

            var ret = new EfColumnAttributes
            {
                Parent = tblObj,
                SqlName = colName,
                Required = !nullable,
                SqlDataType = sqlDataType,
                ClrName = propertyName,
                ClrDataType = clrType,
                MaxStringLength = maxStringLength,
                IsUnicode = isUnicode,
                IsFixWidth = isFixWidth,
                Computed =  computed
            };
            if (keyColumn) ret.SetKeyColumn();
            ret.SetNullableForNewTablesColumns(nullable);

            return ret;
        }

        public class EfDiscrepancyReport
        {
            internal List<EfDiscrepancyFile> Files { get; set; }= new List<EfDiscrepancyFile>();

            internal EfDiscrepancyFile GetFile(string sqlTableName, string entityName, bool newTable = false)
            {
                var file = Files.FirstOrDefault(t => t.SqlTableName == sqlTableName);
                if (file?.SqlTableName == null)
                    Files.Add(file = new EfDiscrepancyFile(sqlTableName, entityName, newTable));

                return file;
            }

            internal void RegisterProblem(EfColumnAttributes problemColumn, FileSegment segment,
                ReportProblem problem, string problemDetails)
            {
                EfDiscrepancyFile file;
                EfDiscrepancySubSegment segmentObj;
                switch (problem)
                {
                    //case ReportProblem.AnnotationNotMatchClrDataType:
                    //    break;
                    case ReportProblem.AnnotationSqlDatatypeNotMatchClrDatatype:
                    case ReportProblem.ColumnNotInDatabase:
                    case ReportProblem.DbDataTypeNotMatchDefinedSqlType:
                    case ReportProblem.DataTypeNullableButColumnDoesntTakeNulls:
                    case ReportProblem.DataTypeNotNullableButColumnTakesNulls:
                    case ReportProblem.PropertyImproperlyFlaggedAsKey:
                    case ReportProblem.PropertyNotFlaggedAsKey:
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
                    sqlNullable, sqlKeyColumn, problem, tblObj.CreateNewTable));
            }

            /// <summary>
            /// For problems with the Foreign Keys
            /// </summary>
            /// <param name="tbl"></param>
            /// <param name="fk"></param>
            /// <param name="problem"></param>
            internal void RegisterProblem(EfForeignKeyAttributes fk, ReportProblem problem)
            {
                switch (problem)
                {
                    case ReportProblem.ForeignTableNotInModel:
                        GetFile(fk.ParentTable.SqlTableName, fk.ParentTable.EntityName)
                            .GetFileSegment(FileSegment.TableLevel)
                            .CorrectionFragments.Add(
                                new EfDiscrepancyCodeCorrectionFragment(fk.ParentTable, problem, fk.SqlConstraintName)
                            );

                        return;
                    case ReportProblem.ForeignKeyNameChanged:
                        GetFile(CONTEXT_NAME, CONTEXT_NAME)
                            .GetFileSegment(FileSegment.ContextOnModelCreating, fk.ParentTable.EntityName)
                            .CorrectionFragments.Add(
                                new EfDiscrepancyCodeCorrectionFragment(fk, problem, FileSegment.ContextOnModelCreating)
                            );
                        return;
                }

                GetFile(fk.ParentTable.SqlTableName, fk.ParentTable.EntityName)
                    .GetFileSegment(FileSegment.EntityForeignKeysDeclaring)
                    .CorrectionFragments.Add(
                        new EfDiscrepancyCodeCorrectionFragment(fk, problem, FileSegment.EntityForeignKeysDeclaring)
                    );

                GetFile(fk.PrincipalTable.SqlTableName, fk.PrincipalTable.EntityName)
                    .GetFileSegment(FileSegment.EntityForeignKeysInverse)
                    .CorrectionFragments.Add(
                        new EfDiscrepancyCodeCorrectionFragment(fk, problem, FileSegment.EntityForeignKeysInverse)
                    );

                GetFile(fk.PrincipalTable.SqlTableName, fk.PrincipalTable.EntityName)
                    .GetFileSegment(FileSegment.EntityForeignKeysConstructor)
                    .CorrectionFragments.Add(
                        new EfDiscrepancyCodeCorrectionFragment(fk, problem, FileSegment.EntityForeignKeysConstructor)
                    );

                GetFile(CONTEXT_NAME, CONTEXT_NAME)
                    .GetFileSegment(FileSegment.ContextOnModelCreating, fk.ParentTable.EntityName)
                    .CorrectionFragments.Add(
                        new EfDiscrepancyCodeCorrectionFragment(fk, problem, FileSegment.ContextOnModelCreating)
                    );
            }

            /// <summary>
            /// Used for SQL Table-level issues (new table, table doesn't exist, etc.)
            /// </summary>
            /// <param name="tblObj">Table object which has the issue</param>
            /// <param name="problem">Which issue (doesn't exist, or not in model)</param>
            internal void RegisterProblem(EfTableAttributes tblObj, ReportProblem problem)
            {
                if (problem == ReportProblem.DocumentNewTable)
                {
                    GetFile(CONTEXT_NAME, CONTEXT_NAME)
                        .GetFileSegment(FileSegment.ContextPropertySection)
                        .CorrectionFragments.Add(
                            new EfDiscrepancyCodeCorrectionFragment(tblObj, problem, null));

                    GetFile(CONTEXT_NAME, CONTEXT_NAME)
                        .GetFileSegment(FileSegment.ContextOnModelCreating, tblObj.EntityName);
                    //.CorrectionFragments.Add(
                    //    new EfDiscrepancyCodeCorrectionFragment(fk, problem, FileSegment.ContextOnModelCreating)
                    //);
                }
                else
                {
                    GetFile(tblObj.SqlTableName, tblObj.EntityName, problem == ReportProblem.DocumentNewTable)
                        .GetFileSegment(FileSegment.TableLevel)
                        .CorrectionFragments.Add(
                            new EfDiscrepancyCodeCorrectionFragment(tblObj, problem, null)
                        );
                }
            }

            public string CreateOutputText()
            {
                var sb = new StringBuilder();
                foreach (var file in Files.OrderBy(f => f.EntityName))
                {
                    PrintFileHeader(sb, file);

                    foreach (var segment in file.FileSegments.OrderBy(s =>
                    {
                        return s.Segment switch
                        {
                            FileSegment.ContextPropertySection => 1,
                            FileSegment.ContextOnModelCreating => 2,
                            FileSegment.None => 0,
                            FileSegment.TableLevel => 3,
                            FileSegment.EntityForeignKeysConstructor => 4,
                            FileSegment.EntityProperties => 5,
                            FileSegment.EntityForeignKeysDeclaring => 6,
                            FileSegment.EntityForeignKeysInverse => 7,
                            _ => 99
                        };
                    }))
                    {
                        PrintSegmentHeader(sb, segment, file);

                        foreach (var subSeg in segment.SubSegments)
                        {
                            PrintSubSegmentHeader(sb, subSeg);

                            foreach (var fragment in subSeg.CorrectionFragments)
                            {
                                foreach (var ln in fragment.CorrectionCode.Split(new[] {'\r', '\n'},
                                    StringSplitOptions.RemoveEmptyEntries))
                                {
                                    sb.AppendLine($"\t\t{ln}");
                                }

                                sb.AppendLine();
                            }

                            PrintSubSegmentFooter(sb, subSeg);
                        }

                        PrintSegmentFooter(sb, segment);
                    }

                    PrintFileFooter(sb, file);
                }

                return sb.ToString();
            }

            private static void PrintFileHeader(StringBuilder sb, EfDiscrepancyFile file)
            {
                if (file.NewFile)
                {
                    var s =
                        $"File:  {file.FileName} -- (Create the file {file.EntityName}.cs with the following contents)";
                    sb.AppendLine(s);
                    sb.AppendLine(new string('-', s.Length));
                    sb.AppendLine("");
                    sb.AppendLine("using System.Collections.Generic;");
                    sb.AppendLine("using System.ComponentModel.DataAnnotations;");
                    sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                    sb.AppendLine("");
                    sb.AppendLine("namespace Emar.Data.Entities");
                    sb.AppendLine("{");
                    sb.AppendLine($"    [Table(\"{file.SqlTableName}\")]");
                    sb.AppendLine($"    public class {file.EntityName}");
                    sb.AppendLine("    {");
                }
                else
                {
                    sb.AppendLine($"File:  {file.FileName}");
                    sb.AppendLine();
                }
            }

            private static void PrintFileFooter(StringBuilder sb, EfDiscrepancyFile file)
            {
                if (file.NewFile)
                {
                    sb.AppendLine("    }");
                    sb.AppendLine("}");
                }

                sb.AppendLine();
                sb.AppendLine();
            }

            private static void PrintSegmentHeader(StringBuilder sb, EfDiscrepancyFileSegment segment, EfDiscrepancyFile file)
            {
                switch (segment.Segment)
                {
                    case FileSegment.EntityProperties:
                        if(!file.NewFile)
                            sb.AppendLine("\tProperties Section:");
                        return;
                    case FileSegment.ContextPropertySection:
                        sb.AppendLine("\tDbSet Properties Section:");
                        sb.AppendLine();
                        return;
                    case FileSegment.ContextOnModelCreating:
                        sb.AppendLine("\t\tprotected override void OnModelCreating(ModelBuilder modelBuilder)");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine();
                        break;
                    case FileSegment.EntityForeignKeysDeclaring:
                        if(!file.NewFile)
                            sb.AppendLine("\tTop of the Foreign Keys section:");
                        return;
                    case FileSegment.EntityForeignKeysInverse:
                        sb.AppendLine(
                            "\tBelow the 'Foreign Keys' section, where the keys only have the [InverseProperty]:");
                        return;
                    case FileSegment.EntityForeignKeysConstructor:
                        sb.AppendLine("\tIn the Class' Constructor:");
                        sb.AppendLine($"\t\tpublic {file.EntityName}()");
                        sb.AppendLine("\t\t{");
                        break;
                    case FileSegment.TableLevel:
                    case FileSegment.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(segment), segment,
                            "From EfDiscrepancyReport.SegmentName()");
                }
            }

            private static void PrintSegmentFooter(StringBuilder sb, EfDiscrepancyFileSegment segment)
            {
                switch (segment.Segment)
                {
                    case FileSegment.ContextOnModelCreating:
                        break;
                    case FileSegment.None:
                        break;
                    case FileSegment.TableLevel:
                        break;
                    case FileSegment.EntityForeignKeysConstructor:
                        sb.AppendLine("\t\t}");
                        sb.AppendLine();
                        break;
                    case FileSegment.EntityProperties:
                        break;
                    case FileSegment.EntityForeignKeysDeclaring:
                        break;
                    case FileSegment.EntityForeignKeysInverse:
                        break;
                    case FileSegment.ContextPropertySection:
                        sb.AppendLine();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private static void PrintSubSegmentHeader(StringBuilder sb, EfDiscrepancySubSegment subSeg)
            {
                if (subSeg.SubSegmentName != null)
                {
                    sb.AppendLine($"\t\t\tmodelBuilder.Entity<{subSeg.SubSegmentName}>(entity =>");
                    sb.AppendLine("\t\t\t{");
                }
            }

            private static void PrintSubSegmentFooter(StringBuilder sb, EfDiscrepancySubSegment subSeg)
            {
                if (subSeg.SubSegmentName != null)
                {
                    sb.AppendLine("\t\t\t});");
                    sb.AppendLine();
                }
            }
        }

        internal class EfDiscrepancyFile
        {
            internal string SqlTableName { get; private set; }

            internal string EntityName { get; private set; }

            internal string FileName => EntityName + ".cs";

            internal bool NewFile { get; set; }

            internal List<EfDiscrepancyFileSegment> FileSegments { get; set; } =
                new List<EfDiscrepancyFileSegment>();

            internal EfDiscrepancyFile(string sqlTableName, string entityName, bool newFile)
            {
                SqlTableName = sqlTableName;
                EntityName = entityName ?? PascalNameFromSqlName(sqlTableName);
                NewFile = newFile;
            }

            internal EfDiscrepancySubSegment GetFileSegment(FileSegment segment, string entityName = null)
            {
                var retSegment = FileSegments.FirstOrDefault(c => c.Segment == segment);
                if (retSegment == null)
                    FileSegments.Add(retSegment = new EfDiscrepancyFileSegment {Segment = segment});

                EfDiscrepancySubSegment subSegment = retSegment.GetSubSegment(entityName);

                return subSegment;
            }
        }

        internal class EfDiscrepancyFileSegment
        {
            internal FileSegment Segment { get; set; }

            internal List<EfDiscrepancySubSegment> SubSegments { get; set; } = new List<EfDiscrepancySubSegment>();

            public EfDiscrepancySubSegment GetSubSegment(string entityName)
            {
                var ret = SubSegments
                    .FirstOrDefault(s => s.SubSegmentName == entityName);
                if(ret == null)
                    SubSegments.Add(ret = new EfDiscrepancySubSegment {SubSegmentName = entityName});
                return ret;
            }
        }

        internal class EfDiscrepancySubSegment
        {
            internal string SubSegmentName { get; set; }
            
            internal List<EfDiscrepancyCodeCorrectionFragment> CorrectionFragments { get; set; } =
                new List<EfDiscrepancyCodeCorrectionFragment>();
        }

        public class EfDiscrepancyCodeCorrectionFragment
        {
            public string CorrectionCode { get; set; }

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
                    case ReportProblem.DataTypeNotNullableButColumnTakesNulls:
                    case ReportProblem.PropertyNotFlaggedAsKey:
                    case ReportProblem.PropertyImproperlyFlaggedAsKey:
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
                            "\t\t// Add entity.Property setting described below" + Environment.NewLine +
                            (string.IsNullOrWhiteSpace(problemDetails)
                                ? ""
                                : $"\t\t// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            $"\t\tentity.Property(e => e.{colObj.ClrName}).IsUnicode(false);";
                        break;
                    case ReportProblem.ContextNotUnicodePropertyToBeRemoved:
                        CorrectionCode =
                            "\t\t// Remove entity.Property setting described below" + Environment.NewLine +
                            (string.IsNullOrWhiteSpace(problemDetails)
                                ? ""
                                : $"\t\t// Problem: {problemDetails}" + Environment.NewLine + Environment.NewLine) +
                            $"\t\tentity.Property(e => e.{colObj.ClrName}).IsUnicode(false);";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(problem), problem,
                            "in EfDiscrepancyCodeCorrectionFragment.constructor()");
                }
            }

            public EfDiscrepancyCodeCorrectionFragment(string sqlColumnName, string sqlDataType, in bool sqlNullable,
                in bool sqlKeyColumn, ReportProblem problem, bool newTable)
            {
                if (problem != ReportProblem.DbColumnMissing)
                    throw new NotImplementedException(
                        "EfDiscrepancyCodeCorrectionFragment(string sqlColumnName...) is only coded to handle DbColumnMissing.");

                var propertyName = PascalNameFromSqlName(sqlColumnName);
                var clrType = SqlToClrDataTypeString(sqlDataType, sqlNullable, out bool includeRequired);
                var keyRequiredText = sqlKeyColumn ? ", Key" : (includeRequired ? ", Required" : "");

                CorrectionCode =
                    (newTable ? "": "// Missing column to be added to the Entity" + Environment.NewLine) +
                    $"[Column(\"{sqlColumnName}\", TypeName = \"{sqlDataType}\")" + $"{keyRequiredText}]" +
                    Environment.NewLine +
                    $"public {clrType} {propertyName} {{ get; set; }}";
            }

            public EfDiscrepancyCodeCorrectionFragment(EfTableAttributes tblObj, ReportProblem problem,
                string problemDetails)
            {
                switch (problem)
                {
                    case ReportProblem.DbTableMissing:
                        CorrectionCode =
                            $"The DB Table {tblObj.SqlTableName} doesn't exist. " + Environment.NewLine +
                            "Remove this file from the project, or add it to the \"EntitiesNotMapped\" list in the body of the \"Confirm\" call)." +
                            Environment.NewLine +
                            $"This will also cause you to have to update the {CONTEXT_NAME} to remove the corresponding DB Set" +
                            Environment.NewLine +
                            $"and an OnModelCreating() \"modelBuilder.Entity<{tblObj.EntityName}>(entity => \" section." +
                            Environment.NewLine;
                        break;
                    case ReportProblem.ForeignTableNotInModel:
                        CorrectionCode =
                            $"The DB Table {tblObj.SqlTableName} has a foreign key, '{problemDetails}', which points to table not included in the model.";
                        break;
                    case ReportProblem.DocumentNewTable:
                        CorrectionCode = $"public virtual DbSet<{tblObj.EntityName}> {tblObj.EntityName}s {{ get; set; }}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(problem), problem, 
                            "From EfDiscrepancyCodeFragment.ctor (the one with 2 args)");
                }   
            }

            public EfDiscrepancyCodeCorrectionFragment(EfForeignKeyAttributes fk, ReportProblem problem,
                FileSegment segment)
            {
                string commentLine = "";
                switch (problem)
                {
                    case ReportProblem.ForeignKeyDoesntMatch:
                        commentLine =
                            "Found discrepancy in how the foreign key definition between EF and DB - compare to existing code.";
                        break;
                    case ReportProblem.ForeignKeyEntityOnly:
                        commentLine =
                            "Found foreign key defined in EF that doesn't exist in the DB.";
                        break;
                    case ReportProblem.ForeignKeySqlOnly:
                        if (!fk.ParentTable.CreateNewTable)
                            commentLine =
                                "Found foreign key defined in DB that doesn't exist in the EF.";
                        break;
                    case ReportProblem.ForeignKeyNameChanged:
                        commentLine =
                            $"Foreign Key name in the database, \"{fk.SqlConstraintName}\", differs from the declared name (for the same key).";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(problem), problem,
                            "in EfDiscrepancyCodeCorrectionFragment.constructor()");
                }

                CorrectionCode = fk.CorrectionCode(segment, commentLine);
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
            DbTableMissing,
            ForeignKeyDoesntMatch,
            ForeignKeyNameChanged,
            ForeignKeySqlOnly,
            ForeignKeyEntityOnly,
            RequestedNewTableAlreadyExists,
            DocumentNewTable,
            ForeignTableNotInModel
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
                case "real":
                    return nullable ? "Single?" : "Single";
                case "varbinary":
                    if (!nullable) includeRequired = true;
                    return "byte[]";
                default:
                    throw new NotImplementedException(
                        $"SqlToClrDataTypeString() switch doesn't cover case: '{dtParts[0]}");
            }
        }

        private static Type SqlToClrDataType(string dataType, bool nullable, out bool? isUnicode,
            out bool? isFixWidth, out int? maxStringLength)
        {
            var dtParts = dataType.Split(new[] { '(', ')', ',' });

            isFixWidth = null;
            isUnicode = null;
            maxStringLength = null;
            switch (dtParts[0].ToLower())
            {
                case "varchar":
                    if(dtParts[1].ToUpper() != "MAX")
                        maxStringLength =  Convert.ToInt32(dtParts[1]);
                    return typeof(string);
                case "char":
                    if (dtParts[1].ToUpper() != "MAX")
                        maxStringLength = Convert.ToInt32(dtParts[1]);
                    isFixWidth = true;
                    return typeof(string);
                case "nvarchar":
                    if (dtParts[1].ToUpper() != "MAX")
                        maxStringLength = Convert.ToInt32(dtParts[1]);
                    isUnicode = true;
                    return typeof(string);
                case "nchar":
                    if (dtParts[1].ToUpper() != "MAX")
                        maxStringLength = Convert.ToInt32(dtParts[1]);
                    isFixWidth = true;
                    isUnicode = true;
                    return typeof(string);
                case "time":
                    return nullable ? typeof(TimeSpan?) : typeof(TimeSpan);
                case "date":
                    return nullable ? typeof(DateTime?) : typeof(DateTime);
                case "datetimeoffset":
                    return nullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
                case "bit":
                    return nullable ? typeof(bool?) : typeof(bool);
                case "tinyint":
                    return nullable ? typeof(byte?) : typeof(byte);
                case "smallint":
                    return nullable ? typeof(short?) : typeof(short);
                case "int":
                    return nullable ? typeof(int?) : typeof(int);
                case "bigint":
                    return nullable ? typeof(long?) : typeof(long);
                case "decimal":
                case "numeric":
                    return nullable ? typeof(decimal?) : typeof(decimal);
                case "real":
                    return nullable ? typeof(Single?) : typeof(Single);
                case "varbinary":
                    return typeof(byte[]);
                default:
                    throw new NotImplementedException(
                        $"SqlToClrDataType() switch doesn't cover case: '{dtParts[0]}");
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


        private const string COLUMN_QUERY =
            "SELECT  c.name,  \r\n" +
            "        TYPE_NAME(system_type_id) +  \r\n" +
            "            CASE \r\n" +
            "                WHEN TYPE_NAME(system_type_id) LIKE '%char' and max_length = -1  \r\n" +
            "                    THEN '(max)'  \r\n" +
            "                WHEN TYPE_NAME(system_type_id) LIKE 'n%char'  \r\n" +
            "                    THEN CONCAT('(', max_length / 2, ')') \r\n" +
            "                WHEN TYPE_NAME(system_type_id) LIKE '%char'  \r\n" +
            "                OR TYPE_NAME(system_type_id) LIKE '%binary'   \r\n" +
            "                    THEN CONCAT('(', max_length, ')') \r\n" +
            "                WHEN TYPE_NAME(system_type_id) = 'numeric'  \r\n" +
            "                OR TYPE_NAME(system_type_id) = 'decimal'  \r\n" +
            "                    THEN CONCAT('(', precision, ',', scale, ')') \r\n" +
            "                ELSE '' \r\n" +
            "            END AS datatype \r\n" +
            "        , is_nullable \r\n" +
            "        , CASE WHEN ic.index_id IS NULL THEN 0 ELSE 1 END AS KeyColumn,  \r\n" +
            "        c.is_computed \r\n" +
            "FROM    sys.columns c \r\n" +
            "LEFT JOIN sys.indexes i \r\n" +
            "        ON c.object_id = i.object_id \r\n" +
            "        AND i.is_primary_key = 1 \r\n" +
            "LEFT JOIN sys.index_columns ic \r\n" +
            "        ON ic.object_id = i.object_id \r\n" +
            "        AND i.index_id = ic.index_id \r\n" +
            "        AND c.column_id = ic.column_id \r\n" +
            "WHERE c.object_id = OBJECT_ID('{0}') ";

        private const string FOREIGN_KEY_QUERY =
            "WITH dupKeyToTables AS ( \r\n" +
            "	select	principalentitytable = object_name(referenced_object_id) \r\n" +
            "	from	sys.foreign_keys k  \r\n" +
            "	where	parent_object_id = object_id('{0}') \r\n" +
            "	group by object_name(referenced_object_id) \r\n" +
            "	having count(*) > 1 \r\n" +
            ") \r\n" +
            "select	name,   \r\n" +
            "		DeclaringEntityTable = object_name(parent_object_id),  \r\n" +
            "		PrincipalEntityTable = object_name(referenced_object_id),  \r\n" +
            "		substring (  \r\n" +
            "		(  \r\n" +
            "			select ', ' + col_name(parent_object_id, parent_column_id)   \r\n" +
            "			from sys.foreign_key_columns c  \r\n" +
            "			where c.constraint_object_id = k.object_id  \r\n" +
            "			order by constraint_column_id  \r\n" +
            "			for xml path, type  \r\n" +
            "		).value('.[1]', 'nvarchar(max)'),  \r\n" +
            "		3, 8000) as DeclaringEntityFields, \r\n" +
            "		case when d.principalentitytable is not null then 1 else 0 end as DuplicatePrincipalForeignKeys \r\n" +
            "from	sys.foreign_keys k  \r\n" +
            "left join dupkeytotables d \r\n" +
            "		on object_name(referenced_object_id) = d.principalentitytable \r\n" +
            "where	parent_object_id = object_id('{0}')";

        private const string TABLE_VIEW_QUERY =
            "IF EXISTS (SELECT TOP 1 1 FROM sys.tables WHERE name = '{0}') \r\n" +
            "	SELECT 'table'; \r\n" +
            "ELSE IF EXISTS (SELECT TOP 1 1 FROM sys.views WHERE name = '{0}') \r\n" +
            "	SELECT 'view'; \r\n" +
            "ELSE \r\n" +
            "	SELECT 'missing';";
    }

#if TestingEfUtility_bogus

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