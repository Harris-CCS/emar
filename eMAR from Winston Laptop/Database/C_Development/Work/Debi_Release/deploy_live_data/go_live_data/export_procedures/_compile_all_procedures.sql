use [$(emar_base)];
declare
      @export_database_name sysname = 'ibex'
    , @template nvarchar(max)
    , @sql_cmd nvarchar(max);

        :r export_ibex_antimicrobial_indication_items.sql
        :r export_ibex_antimicrobial_indications.sql
        :r export_ibex_devices.sql
        :r export_ibex_fdb_allergy_name.sql
        :r export_ibex_fdb_brand_name.sql
        :r export_ibex_fdb_ndc_info.sql
        :r export_ibex_group_list_items.sql
        :r export_ibex_medication_routes.sql
        :r export_ibex_medication_units.sql
        :r export_ibex_order_instructions.sql
        :r export_ibex_override_reasons.sql
        :r export_ibex_patient_allergies.sql
        :r export_ibex_patient_home_medications.sql
        :r export_ibex_patient_indicators.sql
        :r export_ibex_patient_orders.sql
        :r export_ibex_patient_problems.sql
        :r export_ibex_patients.sql
        :r export_ibex_site_code_shares.sql
        :r export_ibex_site_formulary.sql
        :r export_ibex_site_formulary_match.sql
        :r export_ibex_site_options.sql
        :r export_ibex_sites.sql
        :r export_ibex_user_patients.sql
        :r export_ibex_user_quick_list_items.sql
        :r export_ibex_user_settings.sql
        :r export_ibex_users.sql
