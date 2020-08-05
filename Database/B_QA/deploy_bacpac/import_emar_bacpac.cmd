setlocal
@echo off
cls
for /f "delims=" %%x in (emar_bacpac.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlpackage = %pgm_sqlpackage%
@echo server_name    = %server_name%
@echo source_database_name = %source_database_name%
@echo target_database_name = %target_database_name%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Import /Quiet:False /SourceFile:"emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%"
"%pgm_sqlpackage%" /Action:Import /Quiet:False /SourceFile:"emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%"
pause