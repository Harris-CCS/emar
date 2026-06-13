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
@echo "%pgm_sqlcmd%" -i drop_database.sql -S %server_name% -v emar_base=%emar_base% -v emar_load=%emar_load%
      "%pgm_sqlcmd%" -i drop_database.sql -S %server_name% -v emar_base=%emar_base% -v emar_load=%emar_load%
title %current_script% **COMPLETE**