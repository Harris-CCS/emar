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
@echo emar_base      = %emar_base%
@echo emar_load      = %emar_load%
@echo emar_deploy    = %emar_deploy%
@echo deploy_version = %deploy_version%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish BACPAC no ibex fdb databases
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Import  /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%"
      "%pgm_sqlpackage%" /Action:Import  /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%"
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%" /Variables:load_data=none /Variables:deploy_version=development /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%" /Variables:load_data=none /Variables:deploy_version=development /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish dacpac SAMPLE data Load, no ibex fdb databases
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_sample" /Variables:load_data=sample /Variables:deploy_version=sample /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_sample" /Variables:load_data=sample /Variables:deploy_version=sample /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish dacpac LIVE data Load, no ibex fdb databases
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_live" /Variables:load_data=live /Variables:deploy_version=live /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_live" /Variables:load_data=live /Variables:deploy_version=live /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlcmd%" -i reset_database.sql -S %server_name%
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
@echo "%pgm_sqlpackage%" /Action:Import  /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%2"
      "%pgm_sqlpackage%" /Action:Import  /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.bacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%2"
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%2" /Variables:load_data=none /Variables:deploy_version=development /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%2" /Variables:load_data=none /Variables:deploy_version=development /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish dacpac SAMPLE data Load 2nd Load
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_sample" /Variables:load_data=sample /Variables:deploy_version=sample2 /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_sample" /Variables:load_data=sample /Variables:deploy_version=sample2 /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo Publish dacpac LIVE data Load 2nd Load
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_live" /Variables:load_data=live /Variables:deploy_version=live2 /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\deploy_bacpac\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_load%_live" /Variables:load_data=live /Variables:deploy_version=live2 /Variables:is_bacpac_build=false /Variables:current_path=%current_path%\..\bin\Debug\ 
title %current_script% **COMPLETE**