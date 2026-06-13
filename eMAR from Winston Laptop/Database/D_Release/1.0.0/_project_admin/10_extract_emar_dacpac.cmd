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
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
rem   extract dacpac does not include all scripting options
rem   copy build dacpac from bin folder
@echo copy "%current_path%\..\bin\Debug\emar.dacpac"
@echo   to "%current_path%\..\deploy_dacpac\"
copy "%current_path%\..\bin\Debug\emar.dacpac" "%current_path%\..\deploy_dacpac\"
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo copy "%current_path%\..\bin\Debug\emar.dacpac"
@echo   to "%current_path%\..\deploy_live_data\"
copy "%current_path%\..\bin\Debug\emar.dacpac" "%current_path%\..\deploy_live_data\"
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo %pgm_sqlcmd% -i rename_database.sql -S %server_name%
"%pgm_sqlcmd%" -i rename_database.sql -S %server_name%
title %current_script% **COMPLETE**
