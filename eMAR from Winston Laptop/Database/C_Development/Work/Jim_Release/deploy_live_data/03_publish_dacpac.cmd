setlocal
@echo off
cls
set current_script=%~nx0
set current_path=%cd%
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
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database%" /Variables:deploy_version=initialize_database > log_03.txt
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%target_database%" /Variables:deploy_version=initialize_database > log_03.txt
title %current_script% **COMPLETE**