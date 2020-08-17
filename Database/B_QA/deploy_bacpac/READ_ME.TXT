To import a Microsoft bacpac file (import now uses both bacpac and dacpac files)
  1. Target Database must not exist for bacpac import to work. If target database exists use "drop_database.sql" script to remove existing database.
  2. Set appropriate environment variables in the "emar_bacpac.ini" file
  3. Run import_emar_bacpac.cmd script to create the database on the sql server
  4. To update site_option DATABASESERVER_ROOT, use the provided script site_options.sql

To create a new export bacpac file 
   export is complicated by stored procedures with exteral references
   this function does not easily work now.
 

NOTE: For the process to work the full path location to executable file "sqlpackage.exe" must be set in the "emar_bacpac.ini" file
sqlpackage.exe by default uses windows authentication (there is no setting for this). There are settings (if needed) to define an sql authenticated user/password.

sqlpackage.exe can be downloaded here if it is not installed locally.
https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download?view=sql-server-ver15