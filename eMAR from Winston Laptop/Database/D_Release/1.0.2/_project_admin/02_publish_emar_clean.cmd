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
@echo "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\bin\Debug\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_base%" /Variables:deploy_version=base
      "%pgm_sqlpackage%" /Action:Publish /Quiet:False /SourceFile:"%current_path%\..\bin\Debug\emar.dacpac" /TargetServerName:"%server_name%" /TargetDatabaseName:"%emar_base%" /Variables:deploy_version=base
title %current_script% **COMPLETE**