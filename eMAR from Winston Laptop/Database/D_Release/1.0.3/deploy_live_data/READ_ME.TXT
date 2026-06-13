PURPOSE: Purge all data in the database and reload all data from existing ibex database.
   Files located in folder ......\deploy_live_data\
   If you think you need one always run a database backup first.


1. Edit file: emar_dacpac.ini  (parameters are stored here)
2. Purge all data from existing emar database (not needed if database does not exist)
3. Run the standard client update process. This will populate all the global "EMAR VENDOR" data
4. Build FDB Tables and populate EMAR Medications
5. Run the "GO-Live" Client Load IBEX Data Process


NOTE: For the process to work the full path location to executable file "sqlpackage.exe" must be set in the ".ini" file
sqlpackage.exe by default uses windows authentication (there is no setting for this). There are settings (if needed) to define an sql authenticated user/password.

sqlpackage.exe can be downloaded here if it is not installed locally.
https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download?view=sql-server-ver15

sqlcmd.exe can be downloaded here if it is not installed locally.
https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility?view=sql-server-ver15