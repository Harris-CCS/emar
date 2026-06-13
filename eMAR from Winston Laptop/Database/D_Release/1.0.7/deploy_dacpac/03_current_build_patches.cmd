setlocal
@echo off
cls

set year=%date:~10,4%
set month=%date:~4,2%
set day=%date:~7,2%

set hour=%time:~0,2%
set minute=%time:~3,2%
set second=%time:~6,2%

if "%month:~0,1%" == " " set month=0%month:~1,1%
if "%day:~0,1%" == " " set day=0%day:~1,1%

if "%hour:~0,1%" == " " set hour=0%hour:~1,1%
if "%minute:~0,1%" == " " set minute=0%minute:~1,1%
if "%second:~0,1%" == " " set second=0%second:~1,1%

set current_date=%year%%month%%day%_%hour%%minute%%second%

set current_script=%~nx0
set current_path=%cd%\current_build
title %current_script% **BEGIN**
for /f "delims=" %%x in (emar_dacpac.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~ >> patch.%current_date%.txt 2>&1
@echo pgm_sqlcmd            = %pgm_sqlcmd% >> patch.%current_date%.txt 2>&1
@echo server_name           = %server_name% >> patch.%current_date%.txt 2>&1
@echo target_database_name  = %target_database_name% >> patch.%current_date%.txt 2>&1
@echo current_path          = %current_path% >> patch.%current_date%.txt 2>&1
@echo current_date          = %current_date% >> patch.%current_date%.txt 2>&1
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ >> patch.%current_date%.txt 2>&1
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlcmd            = %pgm_sqlcmd%
@echo server_name           = %server_name%
@echo target_database_name  = %target_database_name%
@echo current_path          = %current_path%
@echo current_date          = %current_date%
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
rem ***************************************************************
rem ***************************************************************
"%pgm_sqlcmd%" -i "%current_path%"\patch.00001.sql -S %server_name% -v current_path="%current_path%" -v target_database_name="%target_database_name%" >> patch.%current_date%.txt 2>&1
"%pgm_sqlcmd%" -i "%current_path%"\patch.00002.sql -S %server_name% -v current_path="%current_path%" -v target_database_name="%target_database_name%" >> patch.%current_date%.txt 2>&1
"%pgm_sqlcmd%" -i "%current_path%"\patch.00003.sql -S %server_name% -v current_path="%current_path%" -v target_database_name="%target_database_name%" >> patch.%current_date%.txt 2>&1
"%pgm_sqlcmd%" -i "%current_path%"\patch.00004.sql -S %server_name% -v current_path="%current_path%" -v target_database_name="%target_database_name%" >> patch.%current_date%.txt 2>&1
"%pgm_sqlcmd%" -i "%current_path%"\patch.00005.sql -S %server_name% -v current_path="%current_path%" -v target_database_name="%target_database_name%" >> patch.%current_date%.txt 2>&1
"%pgm_sqlcmd%" -i "%current_path%"\patch.00006.sql -S %server_name% -v current_path="%current_path%" -v target_database_name="%target_database_name%" >> patch.%current_date%.txt 2>&1
rem ***************************************************************
rem ***************************************************************
title %current_script% **COMPLETE**
pause