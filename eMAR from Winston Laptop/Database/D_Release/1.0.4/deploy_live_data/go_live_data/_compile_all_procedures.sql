:listvar
use [$(target_database)];
/******************************************
delete all permanent data
    delete performed in hierarchal sequence
******************************************/
set nocount on;
declare
      @export_database_name sysname = 'ibex'
    , @template nvarchar(max)
    , @sql_cmd nvarchar(max);

:r go_live_data\export_procedures\export_ibex_antimicrobial_indication_items.sql
:r go_live_data\export_procedures\export_ibex_antimicrobial_indications.sql
:r go_live_data\export_procedures\export_ibex_devices.sql
:r go_live_data\export_procedures\export_ibex_fdb_allergy_name.sql
:r go_live_data\export_procedures\export_ibex_fdb_brand_name.sql
:r go_live_data\export_procedures\export_ibex_fdb_ndc_info.sql
:r go_live_data\export_procedures\export_ibex_group_list_items.sql
:r go_live_data\export_procedures\export_ibex_medication_routes.sql
:r go_live_data\export_procedures\export_ibex_medication_units.sql
:r go_live_data\export_procedures\export_ibex_order_instructions.sql
:r go_live_data\export_procedures\export_ibex_override_reasons.sql
:r go_live_data\export_procedures\export_ibex_patient_allergies.sql
:r go_live_data\export_procedures\export_ibex_patient_home_medications.sql
:r go_live_data\export_procedures\export_ibex_patient_indicators.sql
:r go_live_data\export_procedures\export_ibex_patient_orders.sql
:r go_live_data\export_procedures\export_ibex_patient_problems.sql
:r go_live_data\export_procedures\export_ibex_patients.sql
:r go_live_data\export_procedures\export_ibex_site_code_shares.sql
:r go_live_data\export_procedures\export_ibex_site_formulary.sql
:r go_live_data\export_procedures\export_ibex_site_formulary_match.sql
:r go_live_data\export_procedures\export_ibex_site_options.sql
:r go_live_data\export_procedures\export_ibex_sites.sql
:r go_live_data\export_procedures\export_ibex_user_patients.sql
:r go_live_data\export_procedures\export_ibex_user_quick_list_items.sql
:r go_live_data\export_procedures\export_ibex_user_settings.sql
:r go_live_data\export_procedures\export_ibex_users.sql
