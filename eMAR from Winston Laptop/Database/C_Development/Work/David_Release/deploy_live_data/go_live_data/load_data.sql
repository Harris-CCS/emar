:listvar
use [$(target_database)];
/******************************************
delete all permanent data
    delete performed in hierarchal sequence
******************************************/
set nocount on;
declare
    @max_id          bigint
  , @does_ibex_exist bit = 0
  , @patients        int = 0;

select
    @does_ibex_exist = 1
from sys.databases
where name = 'ibex';

select
    @patients = count(*)
from [dbo].[patients];

if @patients > 0
    begin
        print '';
        print '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~';
        print 'FATAL ERROR: This script has already been run';
        print 'purge the database and start again';
        print '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~';
        print '';
        raiserror ('***** FATAL: Patients already exist *****', 10, 127);
    end;

create table [#medication_items]
    (
        [medication_id] [int]           default 0
      , [site_id]       [int]           not null default -1
      , [ndc]           [varchar](32)   not null
      , [drug_id]       [varchar](32)   not null
      , [brand_name]    [nvarchar](255) not null
      , [match]         [nvarchar](255) null --- Added for testing / debugging
      , primary key clustered ([ndc] asc, [drug_id] asc, [brand_name] asc, [site_id] asc)
    );

execute [dbo].[load_sites];
execute [dbo].[load_antimicrobial_indication_items];
execute [dbo].[load_antimicrobial_indications];
execute [dbo].[load_devices];
execute [dbo].[load_medication_routes];
execute [dbo].[load_medication_units];
:r go_live_data\site_data\order_administration_available_actions.sql
:r go_live_data\site_data\order_available_actions.sql
execute [dbo].[load_order_instructions];
execute [dbo].[load_override_reasons];
:r go_live_data\phi_data\patients.sql
execute [dbo].[load_site_code_shares];
execute [dbo].[load_site_options];
:r go_live_data\user_data\users.sql
:r go_live_data\site_data\department_preferred_list_items.sql
:r go_live_data\site_data\group_list_items.sql
:r go_live_data\phi_data\patient_allergies.sql
:r go_live_data\phi_data\patient_home_medications.sql
:r go_live_data\phi_data\patient_indicators.sql
:r go_live_data\phi_data\patient_problems.sql
execute [dbo].[load_site_formulary];
execute [dbo].[load_site_formulary_match];
:r go_live_data\phi_data\user_patients.sql
execute [dbo].[load_user_quick_list_items];
:r go_live_data\user_data\user_settings.sql
:r go_live_data\phi_data\patient_orders.sql