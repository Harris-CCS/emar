use [$(target_database_name)];
set nocount on;

declare
    @export_database_name sysname = 'ibex'
  , @template             nvarchar(max)
  , @sql_cmd              nvarchar(max);

:r $(current_path)\patch_00003\export_ibex_medication_routes.sql
:r $(current_path)\patch_00003\ddl_create_column_on_medication_routes.sql

print '~~~~~~~~~ patch.00003.sql complete ~~~~~~~~~';