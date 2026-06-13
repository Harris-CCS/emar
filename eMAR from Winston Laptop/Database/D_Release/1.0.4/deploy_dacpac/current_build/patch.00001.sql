use [$(target_database_name)];
set nocount on;

begin transaction

declare
    @export_database_name sysname = 'ibex'
  , @template             nvarchar(max)
  , @sql_cmd              nvarchar(max);

:r $(current_path)\patch_00001\export_ibex_medication_routes.sql
:r $(current_path)\patch_00001\export_ibex_medication_units.sql
:r $(current_path)\patch_00001\ddl_create_column_priority.sql
:r $(current_path)\patch_00001\update_column_medication_routes_priority.sql
:r $(current_path)\patch_00001\update_column_medication_units_priority.sql

commit transaction
print '~~~~~~~~~ patch.00001.sql complete ~~~~~~~~~';