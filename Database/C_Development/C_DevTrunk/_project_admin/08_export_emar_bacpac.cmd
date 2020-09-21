setlocal
@echo off
cls
set current_script=%~nx0
set current_path=%cd%
title %current_script% **BEGIN**
for /f "delims=" %%x in (emar_dacpac.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlpackage = %pgm_sqlpackage%
@echo pgm_sqlcmd     = %pgm_sqlcmd%
@echo pgm_msbuild    = %pgm_msbuild%
@echo server_name    = %server_name%
@echo source_database_name = %source_database_name%
@echo target_database_name = %target_database_name%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Export /Quiet:False /SourceServerName:"%server_name%" /SourceDatabaseName:"%source_database_name%" /TargetFile:"emar.bacpac" /OverwriteFiles:True
"%pgm_sqlpackage%" /Action:Export /Quiet:False /SourceServerName:"%server_name%" /SourceDatabaseName:"%source_database_name%" /TargetFile:"%current_path%\..\deploy_bacpac\emar.bacpac" /OverwriteFiles:True
title %current_script% **COMPLETE**