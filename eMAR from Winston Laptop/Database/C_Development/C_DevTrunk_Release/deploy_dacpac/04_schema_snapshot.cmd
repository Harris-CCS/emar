setlocal
@echo off
cls
set current_script=%~nx0
set current_path=%cd%
set current_step=%current_script:~0,2%
title %current_script% **BEGIN**
for /f "delims=" %%x in (emar_dacpac.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlpackage       = %pgm_sqlpackage%
@echo pgm_sqlcmd           = %pgm_sqlcmd%
@echo pgm_msbuild          = %pgm_msbuild%
@echo server_name          = %server_name%
@echo target_database_name = %target_database_name%
@echo deploy_version       = %deploy_version%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo  ""%pgm_sqlpackage%" /Action:Extract /Quiet:False /OverwriteFiles:True /SourceDatabaseName:"%target_database_name%" /SourceServerName:"%server_name%" /TargetFile:"%target_database_name%_snapshot_%current_step%.dacpac" /p:CommandTimeout=900 /p:IgnoreUserLoginMappings=TRUE"
cmd /C ""%pgm_sqlpackage%" /Action:Extract /Quiet:False /OverwriteFiles:True /SourceDatabaseName:"%target_database_name%" /SourceServerName:"%server_name%" /TargetFile:"%target_database_name%_snapshot_%current_step%.dacpac" /p:CommandTimeout=900 /p:IgnoreUserLoginMappings=TRUE"
title %current_script% **COMPLETE**
