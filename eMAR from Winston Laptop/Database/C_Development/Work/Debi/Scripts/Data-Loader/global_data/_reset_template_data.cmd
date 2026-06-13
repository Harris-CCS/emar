setlocal
@echo off
cls
set current_script=%~nx0
set current_path=%cd%
title %current_script% **BEGIN**
for /f "delims=" %%x in (_reset_template_data.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlcmd     = %pgm_sqlcmd%
@echo current_path   = %current_path%
@echo server_name    = %server_name%
@echo emar_base      = %emar_base%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
@echo "%pgm_sqlcmd%" -S %server_name% -i "_reset_template_data.sql" -o "_reset_template_data.txt" -v emar_base=%emar_base%
      "%pgm_sqlcmd%" -S %server_name% -i "_reset_template_data.sql" -o "_reset_template_data.txt" -v emar_base=%emar_base%
title %current_script% **COMPLETE**
pause