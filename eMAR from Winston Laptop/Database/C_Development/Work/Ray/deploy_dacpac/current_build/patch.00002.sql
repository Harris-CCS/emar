use [$(target_database_name)];
set nocount on;

:r $(current_path)\patch_00002\ddl_rename_column_prompt_id.sql

print '~~~~~~~~~ patch.00002.sql complete ~~~~~~~~~';