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

waitfor delay '00:00:30'

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [antimicrobial_indication_items]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [antimicrobial_indication_items]';

select [source].[site]
     , [source].[sub_cat]
into [ibex_sample].[dbo].[medication_indication_list]
from   [ibex].[dbo].[medication_indication_list] as [source]
order by [source].[sub_cat]
       , [source].[site];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [antimicrobial_indications]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [antimicrobial_indications]';

select [source].[site]
     , [source].[code]
     , [source].[description]
     , [source].[status]
     , [source].[position]
into [ibex_sample].[dbo].[medication_indication]
from   [ibex].[dbo].[medication_indication] as [source]
order by [source].[description]
       , [source].[site];

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
---- [patients]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [patients]';

select [patients].[ibex]
     , [patients].[person]
     , [patients].[acctnum]
     , [patients].[site]
into [ibex_sample].[dbo].[pat]
from   [ibex].[dbo].[pat] as [patients];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [patient_home_medications] / [patient_allergies]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [patient_home_medications] / [patient_allergies]';

select [source].[site]
     , [source].[class]
     , [source].[cat]
     , [source].[drug]
     , [source].[ndc]
     , [source].[name]
     , [source].[alg_drug_id]
     , [source].[status]
     , [source].[comment]
     , [source].[severity]
     , [source].[actionstatus]
     , [source].[person]
     , [source].[acctnum]
into [ibex_sample].[dbo].[hie_alg]
from   [ibex].[dbo].[hie_alg] as [source];

select [source].[site]
     , [source].[class]
     , [source].[cat]
     , [source].[drug]
     , [source].[name]
     , [source].[dose]
     , [source].[unit]
     , [source].[route]
     , [source].[alg_drug_id]
     , [source].[status]
     , [source].[comment]
     , [source].[sched]
     , [source].[actionstatus]
     , [source].[ndc]
     , [source].[person]
     , [source].[acctnum]
into [ibex_sample].[dbo].[hie_meds]
from   [ibex].[dbo].[hie_meds] as [source];

select [source].[ibex]
     , [source].[class]
     , [source].[cat]
     , [source].[drug]
     , [source].[ndc]
     , [source].[name]
     , [source].[alt_name]
     , [source].[dose]
     , [source].[unit]
     , [source].[route]
     , [source].[alg_drug_id]
     , [source].[status]
     , [source].[cmt]
     , [source].[sched]
     , [source].[reaction]
     , [source].[severity]
     , [source].[parent_id]
     , [source].[parent_name]
     , [source].[usr]
     , [source].[dateadd]
     , [source].[usrchg]
     , [source].[datechg]
     , [source].[actionstatus]
     , [source].[taken]
     , [source].[type]
     , [source].[site]
     , [source].[provider]
into [ibex_sample].[dbo].[alg]
from   [ibex].[dbo].[alg] as [source];

select [type]
     , [site]
     , [id]
       --, [status]
       --, [name]
       --, [misc]
     , [misc2]
--, [misc3]
--, [misc4]
--, [idx_id]
into [ibex_sample].[dbo].[idx]
from   [ibex].[dbo].[idx] as [source]
where  [type] in('LQ', 'LR');

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
into [ibex_sample].[dbo].[med]
from   [ibex].[dbo].[med];

select [ibex]
     , [losecs]
     , [brand_name]
     , [packaging_id]
into [ibex_sample].[dbo].[med_details]
from   [ibex].[dbo].[med_details];

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
---- [formulary]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [formulary]';

select [formulary].[site]
     , [formulary].[ndc]
     , [brand]
     , [aliencode]
     , [svc]
     , [inpat]
     , [outpat]
     , [pyxis]
     , [formulary].[dateadd]
into [ibex_sample].[dbo].[frm]
from   [ibex].[dbo].[frm] as [formulary]
order by [formulary].[ndc]
       , [formulary].[site];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [formulary_match]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- [formulary_match]';

select [formulary].[site]
     , [formulary].[ndc]
     , [formulary].[inpat]
     , [formulary].[outpat]
     , [formulary].[pyxis]
into [ibex_sample].[dbo].[formulary_match]
from   [ibex].[dbo].[formulary_match] as [formulary];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [group_list_items]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

select [source].[num]
     , [source].[type]
       --, [source].status
       --, [source].[site]
     , [source].[name]
       --, [source].dateadd
       --, [source].[datechg]
       --, [source].[usradd]
       --, [source].[usrchg]
     , [source].[grptype]
     , [source].[altcode]
--, [source].[svc]
--, [source].[color]
--, [source].[description]
into [ibex_sample].[dbo].[cde]
from   [ibex].[dbo].[cde] as [source];

select [source].[num]
     , [source].[type]
     , [source].[code]
       --, [source].[svctype]
     , [source].[site]
     , [source].[name]
       --, [source].[face]
       --, [source].[cpt]
     , [source].[route]
     , [source].[dose]
     , [source].[unit]
       --, [source].[id]
     , [source].[notes]
     , [source].[form_id]
--, [source].[defaulted]
--, [source].[schedule]
--, [source].[indication]
into [ibex_sample].[dbo].[grp]
from   [ibex].[dbo].[grp] as [source];

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- [backup]
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- shrinkdatabase';

dbcc shrinkdatabase([ibex_sample]);

---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
---- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

print '---- Backup';

backup database [ibex_sample] to disk = N'$(current_path)\ibex_sample.bak' with blocksize = 65536, maxtransfersize = 1048576, init, compression, stats = 20, copy_only;