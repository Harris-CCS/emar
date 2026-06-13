setlocal
@echo off
set current_path=%cd%
cls
for /f "delims=" %%x in (emar_dacpac.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlpackage = %pgm_sqlpackage%
@echo server_name    = %server_name%
@echo target_database_name = %target_database_name%
@echo current_path         = %current_path%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%" /Variables:deploy_version=%deploy_version%
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%" /Variables:deploy_version=%deploy_version%
pause