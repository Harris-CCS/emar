setlocal
@echo off
cls
rem ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
rem              Set Values Here
rem ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
set /A Counter=0
rem ~~~~~ Load INI Params ~~~~~
for /f "delims=" %%x in (sql_patch_config.ini) do (set "%%x")
@echo %sqlcmd_path%
@echo %release_path%
@echo %sql_user%
@echo %sql_pass%
@echo %sql_server%
@echo %data_folder%
@echo %bcp_log%
@echo %script_count%
@echo %title_val%
set script_count=17
rem ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
echo ~~~~~~~~~~~~~begin~~~~~~~~~~~~~ > %bcp_log%
call :ek "execute emar_clean.dbo.export_ibex_fdb_allergy_name"              ;fdb_allergy_name              ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_fdb_brand_name"                ;fdb_brand_name                ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_fdb_ndc_info"                  ;fdb_ndc_info                  ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_group_list_items"              ;group_list_items              ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_medication_routes"             ;medication_routes             ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_medication_units"              ;medication_units              ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_patient_allergies"             ;patient_allergies             ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_patient_home_medications"      ;patient_home_medications      ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_patient_indicators"            ;patient_indicators            ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_patient_orders"                ;patient_orders                ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_patients"                      ;patients                      ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_site_code_shares"              ;site_code_shares              ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_site_formulary"                ;site_formulary                ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_site_formulary_match"          ;site_formulary_match          ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_sites"                         ;sites                         ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_user_quick_list_items"         ;user_quick_list_items         ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_users"                         ;users                         ;"|~"
title %title_val% *** COMPLETE ***
exit /b
:ek
echo. >> %bcp_log%
set /A Counter+=1
set title_val=Processing %Counter% of %script_count%
title %title_val%
rem  %1 Table to Load
rem  %2 Text File To Import
rem  %3 Delimiter
echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ >> %bcp_log%
echo process: %1
echo process: %1 >> %bcp_log%
echo %1 queryout"%data_folder%%2.txt" -c -t%3 -r \r -S %sql_server% -U %sql_user% -P %sql_pass% >> %bcp_log%
rem bcp  %1 queryout"%data_folder%%2.bcp" -c -t%3 -S %sql_server% -U %sql_user% -P %sql_pass% >> %bcp_log%
bcp  %1 queryout "%data_folder%%2.bcp" -c -t%3 -S %sql_server% -T >> %bcp_log%
exit /b
