To import a Microsoft bacpac file
  1. Target Database must not exist for bacpac import to work. If target database exists use "drop_database.sql" script to remove existing database.
  2. Set appropriate environment variables in the "emar_bacpac.ini" file
  3. Run import_emar_bacpac.cmd script to create the database on the sql server

To create a new export bacpac file
  1. Set appropriate environment variables in the "emar_bacpac.ini" file
  2. Run export_emar_bacpac.cmd script to create the target file 
 

NOTE: For the process to work the full path location to executable file "sqlpackage.exe" must be set in the "emar_bacpac.ini" file
sqlpackage.exe by default uses windows authentication (there is no setting for this). There are settings (if needed) to define an sql authenticated user/password.