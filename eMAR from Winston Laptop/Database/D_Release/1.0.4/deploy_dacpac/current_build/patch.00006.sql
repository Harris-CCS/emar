use [$(target_database_name)];
set nocount on;

declare
    @export_database_name sysname = 'ibex'
  , @template             nvarchar(max)
  , @sql_cmd              nvarchar(max);

:r $(current_path)\patch_00006\export_ibex_patient_indicators.sql
:r $(current_path)\patch_00006\ddl_create_column_on_patient_indicators.sql

print '~~~~~~~~~ patch.00006.sql complete ~~~~~~~~~';