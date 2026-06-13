setlocal
@echo off
cls
set current_script=%~nx0
set current_path=%cd%
set sql_script=\go_live_data\build_local_fdb.sql
title %current_script% **BEGIN**
for /f "delims=" %%x in (parameter_ini_file.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlpackage  = %pgm_sqlpackage%
@echo pgm_sqlcmd      = %pgm_sqlcmd%
@echo current_path    = %current_path%
@echo sql_script      = %sql_script%
@echo server_name     = %server_name%
@echo target_database = %target_database%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
set sql_script=%current_path%%sql_script%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlcmd%" -S %server_name% -i "%sql_script%" -o "log_04.txt" -v target_database=%target_database% -v load_data=live
      "%pgm_sqlcmd%" -S %server_name% -i "%sql_script%" -o "log_04.txt" -v target_database=%target_database% -v load_data=live
title %current_script% **COMPLETE**
