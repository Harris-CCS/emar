setlocal
@echo off
cls
set current_script=%~nx0
set current_path=%cd%
title %current_script% **BEGIN**
set /A Counter=0
for /f "delims=" %%x in (emar_dacpac.ini) do (set "%%x")
@echo ~~~~~~~~ Active Parameters ~~~~~~~~
@echo pgm_sqlpackage = %pgm_sqlpackage%
@echo pgm_sqlcmd     = %pgm_sqlcmd%
@echo pgm_msbuild    = %pgm_msbuild%
@echo server_name    = %server_name%
@echo source_database_name = %source_database_name%
@echo target_database_name = %target_database_name%
set script_count=19
set bcp_log=bcp_log.txt
@echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

echo ~~~~~~~~~~~~~begin~~~~~~~~~~~~~ > %bcp_log%
call :ek "execute emar_clean.dbo.export_ibex_antimicrobial_indication_items";antimicrobial_indication_items;"|~"
call :ek "execute emar_clean.dbo.export_ibex_antimicrobial_indications"     ;antimicrobial_indications     ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_fdb_allergy_name"              ;fdb_allergy_name              ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_fdb_brand_name"                ;fdb_brand_name                ;"|~"
rem there are problems with the procedure and deployment, so this file needs to be created manually
rem call :ek "execute emar_clean.dbo.export_ibex_fdb_ndc_info"                  ;fdb_ndc_info                  ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_group_list_items"              ;group_list_items              ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_medication_routes"             ;medication_routes             ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_medication_units"              ;medication_units              ;"|~"
call :ek "execute emar_clean.dbo.export_ibex_override_reasons"              ;override_reasons              ;"|~"                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
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
set title_val= %current_script% :: Processing %Counter% of %script_count%
title %title_val%
rem  %1 Table to Load
rem  %2 Text File To Import
rem  %3 Delimiter
echo ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ >> %bcp_log%
echo process: %1
echo process: %1 >> %bcp_log%
echo %1 queryout "%bcp_data_folder%%2.txt" -c -t%3 -S %server_name% -T >> %bcp_log%
bcp  %1 queryout "%bcp_data_folder%%2.bcp" -c -t%3 -S %server_name% -T >> %bcp_log%
exit /b