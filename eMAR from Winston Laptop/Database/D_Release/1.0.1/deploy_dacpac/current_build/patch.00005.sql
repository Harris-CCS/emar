use [$(target_database_name)];
set nocount on;

declare
    @export_database_name sysname = 'ibex'
  , @template             nvarchar(max)
  , @sql_cmd              nvarchar(max);

:r $(current_path)\patch_00005\export_ibex_override_reasons.sql
:r $(current_path)\patch_00005\ddl_create_column_on_override_reasons.sql

print '~~~~~~~~~ patch.00005.sql complete ~~~~~~~~~';