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
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish BACPAC no ibex fdb databases
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Import /Quiet:False /SourceFile:"emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%"
"%pgm_sqlpackage%" /Action:Import /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%"
"%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%"
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo %pgm_sqlcmd% -i reset_database.sql -S %server_name%
"%pgm_sqlcmd%" -i reset_database.sql -S %server_name%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish BACPAC with ibex fdb databases
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Import /Quiet:False /SourceFile:"emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%"
"%pgm_sqlpackage%" /Action:Import /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%2"
"%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database_name%2"
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish dacpac LIVE data Load
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"emar_dacpac_live" /Variables:load_data=live /Variables:is_bacpac_build=true /Variables:current_path=%current_path%\..\bin\Debug\
"%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"emar_dacpac_live"  /Variables:load_data=live /Variables:is_bacpac_build=true /Variables:current_path=%current_path%\..\bin\Debug\
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish dacpac SAMPLE data Load
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"emar_dacpac_sample" /Variables:load_data=live /Variables:is_bacpac_build=true /Variables:current_path=%current_path%\..\bin\Debug\
"%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"emar_dacpac_sample"  /Variables:load_data=sample /Variables:is_bacpac_build=true /Variables:current_path=%current_path%\..\bin\Debug\
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
title %current_script% **COMPLETE**
