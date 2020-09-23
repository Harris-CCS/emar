use [master];

set nocount on;

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'ibex_sample'
)
    begin
        alter database [ibex_sample] set single_user with rollback immediate;
    end;

drop database if exists [ibex_sample];

create database [ibex_sample];

alter database [ibex_sample] set compatibility_level = 130;

alter database [ibex_sample] set recovery simple;
go

waitfor delay '00:00:10';

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [antimicrobial_indication_items]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [antimicrobial_indication_items]';

select [site]
     , [sub_cat]
into [ibex_sample].[dbo].[medication_indication_list]
from   [ibex].[dbo].[medication_indication_list];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [antimicrobial_indications]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [antimicrobial_indications]';

select [site]
     , [code]
     , [description]
     , [status]
     , [position]
into [ibex_sample].[dbo].[medication_indication]
from   [ibex].[dbo].[medication_indication]
order by [description]
       , [site];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [fdb_ndc_info]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [fdb_ndc_info]';

select *
into [ibex_sample].[dbo].[fdb_ndc_info]
from   [ibex].[dbo].[fdb_ndc_info];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [fdb_brand_name]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [fdb_brand_name]';

select *
into [ibex_sample].[dbo].[fdb_brand_name]
from   [ibex].[dbo].[fdb_brand_name];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [fdb_allergy_name]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [fdb_allergy_name]';

select *
into [ibex_sample].[dbo].[fdb_allergy_name]
from   [ibex].[dbo].[fdb_allergy_name];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [group_list_items]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

select [num]
     , [type]
       --   , status
     , [site]
     , [name]
       --   , dateadd
       --   , [datechg]
       --   , [usradd]
       --   , [usrchg]
     , [grptype]
     , [altcode]
       --   , [svc]
       --   , [color]
     , [description]
into [ibex_sample].[dbo].[cde]
from   [ibex].[dbo].[cde];

select [num]
     , [type]
     , [code]
       --, [svctype]
     , [site]
     , [name]
       --, [face]
       --, [cpt]
     , [route]
     , [dose]
     , [unit]
       --, [id]
     , [notes]
     , [form_id]
--, [defaulted]
--, [schedule]
--, [indication]
into [ibex_sample].[dbo].[grp]
from   [ibex].[dbo].[grp];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [patient_home_medications] / [patient_allergies]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [patient_home_medications] / [patient_allergies]';

select [site]
     , [class]
     , [cat]
     , [drug]
     , [ndc]
     , [name]
     , [alg_drug_id]
     , [status]
     , [comment]
     , [severity]
     , [actionstatus]
     , [person]
     , [acctnum]
into [ibex_sample].[dbo].[hie_alg]
from   [ibex].[dbo].[hie_alg];

select [site]
     , [class]
     , [cat]
     , [drug]
     , [name]
     , [dose]
     , [unit]
     , [route]
     , [alg_drug_id]
     , [status]
     , [comment]
     , [sched]
     , [actionstatus]
     , [ndc]
     , [person]
     , [acctnum]
into [ibex_sample].[dbo].[hie_meds]
from   [ibex].[dbo].[hie_meds];

select [ibex]
     , [class]
     , [cat]
     , [drug]
     , [ndc]
     , [name]
     , [alt_name]
     , [dose]
     , [unit]
     , [route]
     , [alg_drug_id]
     , [status]
     , [cmt]
     , [sched]
     , [reaction]
     , [severity]
     , [parent_id]
     , [parent_name]
     , [usr]
     , [dateadd]
     , [usrchg]
     , [datechg]
     , [actionstatus]
     , [taken]
     , [type]
     , [site]
     , [provider]
into [ibex_sample].[dbo].[alg]
from   [ibex].[dbo].[alg];

select [type]
     , [site]
     , [id]
     , [status]
     , [name]
     , [misc]
     , [misc2]
--, [misc3]
--, [misc4]
--, [idx_id]
into [ibex_sample].[dbo].[idx]
from   [ibex].[dbo].[idx]
where  [type] in('LQ', 'LR', 'Z', 'BE', 'AC', 'SO');
--  Z = Urgency
-- BE = medication_units
-- AC = medication_routes
-- SO = order_instructions
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [patient_indicators]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

select [id]
     , [code]
     , [site]
     , [template]
into [ibex_sample].[dbo].[custom_indicators]
from   [ibex].[dbo].[custom_indicators];

select [site]
     , [ibex]
     , [code]
     , [type]
into [ibex_sample].[dbo].[pat_indicators]
from   [ibex].[dbo].[pat_indicators];

select [position]
     , [custom_indicator_id]
     , [site]
into [ibex_sample].[dbo].[current_custom_indicators]
from   [ibex].[dbo].[current_custom_indicators];

select [id]
     , [name]
into [ibex_sample].[dbo].[custom_indicator_images]
from   [ibex].[dbo].[custom_indicator_images];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [patient_orders]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

select [ibex]
     , [site]
     , [losecs]
     , [status]
     , [name]
     , [route]
     , [unit]
     , [schedule]
     , [dose]
     , [med_notes]
     , [order_date]
     , [order_for_usr]
     , [order_usr]
     , [type]
into [ibex_sample].[dbo].[med]
from   [ibex].[dbo].[med];

select [ibex]
     , [losecs]
     , [brand_name]
     , [packaging_id]
into [ibex_sample].[dbo].[med_details]
from   [ibex].[dbo].[med_details];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [patients_problems]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

select [site]
     , [ibex]
     , [alienkey]
     , [name]
     , [riskgreen]
     , [service]
     , [type]
     , [status]
into [ibex_sample].[dbo].[trx]
from   [ibex].[dbo].[trx];

select [display]
     , [oid]
into [ibex_sample].[dbo].[code_systems]
from   [ibex].[dbo].[code_systems];

select [ibex]
     , [problem_code]
     , [problem_name]
     , [problem_code_system]
     , [internal_status]
into [ibex_sample].[dbo].[problem_episode]
from   [ibex].[dbo].[problem_episode];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [patients]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [patients]';

select [ibex]
     , [person]
     , [acctnum]
     , [site]
     , [medrec]
     , [lname]
     , [fname]
     , [mname]
     , [suffix]
     , [gender]
     , [dob]
     , [age]
     , [ageunits]
     , [complaint]
     , [height]
     , [weight]
     , [bed]
     , [ward]
     , [dept]
     , [ord42]
     , [naalert]
     , [withdraw]
     , [vsdate]
     , [ord11]
     , [vssys]
     , [vsdia]
     , [ord12]
     , [vspulse]
     , [vsmaplevel]
     , [vsmap]
     , [ord13]
     , [vsresp]
     , [ord14]
     , [vstemp]
     , [vsendtidallevel]
     , [vsendtidal]
     , [ord23]
     , [vso2]
     , [ord15]
     , [vspain]
     , [custom_insurance_id]
     , [eun]
     , [gender_system]
     , [doctor]
     , [resident]
     , [drextender]
     , [primarynurse]
     , [extender]
     , [firstdoctor]
into [ibex_sample].[dbo].[pat]
from   [ibex].[dbo].[pat];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [site_code_shares]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [site_code_shares]';

select [site]
     , [cs_site]
     , [cs_name]
into [ibex_sample].[dbo].[code_share]
from   [ibex].[dbo].[code_share];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [site_formulary]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [site_formulary]';

select [site]
     , [ndc]
     , [brand]
     , [aliencode]
     , [svc]
     , [inpat]
     , [outpat]
     , [pyxis]
     , [dateadd]
into [ibex_sample].[dbo].[frm]
from   [ibex].[dbo].[frm] as [formulary]
order by [ndc]
       , [site];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [site_formulary_match]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [site_formulary_match]';

select [site]
     , [ndc]
     , [inpat]
     , [outpat]
     , [pyxis]
into [ibex_sample].[dbo].[formulary_match]
from   [ibex].[dbo].[formulary_match];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [sites]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [sites]';

select [site]
     , [name]
     , [status]
into [ibex_sample].[dbo].[org]
from   [ibex].[dbo].[org];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [user_quick_list_items]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [user_quick_list_items]';

select distinct 
       [site]
     , [usr]
     , [ndc]
     , [brand]
     , [strength]
     , [unit]
     , [route]
     , [notes]
into [ibex_sample].[dbo].[rxl]
from   [ibex].[dbo].[rxl];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [users]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [users]';

select [num]
     , [site]
     , [type]
     , [status]
     , [init]
     , [first]
     , [last]
     , [ordonly]
     , [loginid]
     , [password]
     , [lastlogin]
into [ibex_sample].[dbo].[drs]
from   [ibex].[dbo].[drs];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [backup]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- shrinkdatabase';

dbcc shrinkdatabase([ibex_sample]);

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- Backup';

backup database [ibex_sample] to disk = N'$(current_path)ibex_sample.bak' with blocksize = 65536, maxtransfersize = 1048576, init, compression, stats = 9, copy_only;