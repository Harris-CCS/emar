use [$(target_database_name)];
set nocount on;

declare
    @export_database_name sysname = 'ibex'
  , @template             nvarchar(max)
  , @sql_cmd              nvarchar(max);

:r $(current_path)\patch_00004\export_ibex_order_instructions.sql
:r $(current_path)\patch_00004\ddl_create_column_on_order_instructions.sql

print '~~~~~~~~~ patch.00004.sql complete ~~~~~~~~~';
go