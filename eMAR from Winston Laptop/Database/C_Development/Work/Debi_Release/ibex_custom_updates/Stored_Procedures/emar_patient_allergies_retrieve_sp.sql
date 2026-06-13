print 'create procedure [ibex].[dbo].[emar_patient_allergies_retrieve_sp];';

set @template = N'
CREATE OR ALTER     PROCEDURE [dbo].[emar_patient_allergies_retrieve_sp]
	@Ibex varchar(20)
AS

-- FROM: export_ibex_patient_allergies in the $/Database/C_Development/C_DevTrunk/emar.sqlproj

-- Pull pat and hst matches here to use against alg table because correcting empty person/acctnum matches on
-- hie_alg in cte_hie_person below results in patients without person or acctnum not having any allergies come 
-- back in results from this sp.
with cte_person as
(
	select ibex, site, person, acctnum from pat where ibex=@Ibex
	union
	select ibex, site, person, acctnum from hst where ibex=@Ibex
),
cte_hie_person as 
(
	-- Match criteria for pat/hst against HIE:
	--	person  = person and acctnum = acctnum and both are not blank
	--	person  = blank  and acctnum = acctnum and acctnum is not blank
	--	acctnum = blank  and person  = person  and person is not blank
	-- NOTE: In different situations these tables have nullable columns and/or values that have been padded with
	-- trailing whitespace in one table but not the other, but should still match!
	-- Hence all the null checks and trimming.
	select src.ibex, src.site, hie_alg.num
	from pat as src
		inner join hie_alg on
				src.site = hie_alg.site
			and ltrim(rtrim(isnull(src.person, ''''))) = ltrim(rtrim(isnull(hie_alg.person, '''')))
			and ltrim(rtrim(isnull(src.acctnum, ''''))) = ltrim(rtrim(isnull(hie_alg.person, '''')))
			and (len(ltrim(rtrim(isnull(src.person, '''')))) > 0 OR len(ltrim(rtrim(isnull(src.acctnum, '''')))) > 0)
	where src.ibex = @Ibex

	union

	select src.ibex, src.site, hie_alg.num
	from hst as src
		inner join hie_alg on
				src.site = hie_alg.site
			and ltrim(rtrim(isnull(src.person, ''''))) = ltrim(rtrim(isnull(hie_alg.person, '''')))
			and ltrim(rtrim(isnull(src.acctnum, ''''))) = ltrim(rtrim(isnull(hie_alg.person, '''')))
			and (len(ltrim(rtrim(isnull(src.person, '''')))) > 0 OR len(ltrim(rtrim(isnull(src.acctnum, '''')))) > 0)
	where src.ibex = @Ibex
)
, cte_hie as (
	select	cast([source].[site] as varchar(5)) + ''|'' + [patients].[ibex] as [patient_id]
			, ltrim(rtrim([source].[class])) as                                [class]
			, ltrim(rtrim([source].[cat])) as                                  [category]
			, ltrim(rtrim([source].[drug])) as                                 [internal_drug_id]
			, ltrim(rtrim([source].[ndc])) as                                  [ndc]
			, isnull(cast([ndc].[medid] as varchar(25)), '''') as                [drug_id]
			, ltrim(rtrim([source].[name])) as                                 [name]
			, ltrim(rtrim('''')) as                                              [alternate_name]
			, ltrim(rtrim([source].[alg_drug_id])) as                          [allergy_drug_id]
			, case
				when ltrim(rtrim([source].[status])) = ''A''
					then CONVERT(bit, 1)
				else CONVERT(bit, 0)
			end as                                                           [is_active]
			, ltrim(rtrim([source].[comment])) as                              [comment]
			, ltrim(rtrim('''')) as                                              [schedule]
			, ltrim(rtrim('''')) as                                              [reaction]
			, ltrim(rtrim([source].[severity])) as                             [severity]
			, ltrim(rtrim([source].[source])) as                               [source]
			, ltrim(rtrim('''')) as                                              [parent_drug_id]
			, ltrim(rtrim('''')) as                                              [parent_drug_name]
			, CONVERT(int, 0) as											   [add_user_id]
			, '''' as                                                            [add_datetime]
			, CONVERT(int, 0) as											   [change_user_id]
			, '''' as                                                            [change_datetime]
			, ltrim(rtrim([source].[actionstatus])) as                         [action_status]
			, ltrim(rtrim(''hie_alg'')) as                                       [information_source]
			, ltrim(rtrim([source].[person])) as                               [person_number]
			, ltrim(rtrim([source].[acctnum])) as                              [account_number]
			, [patients].[ibex]
			, [source].[site]
	from   [dbo].[hie_alg] as [source]
	inner join [cte_hie_person] as [patients] 
			on [patients].[num] = [source].[num]
	left join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[ndc] = [source].[ndc]
)
, cte_pat as (
	select	cast([source].[site] as varchar(5)) + ''|'' + [source].[ibex] as [patient_id]
			, ltrim(rtrim([source].[class])) as                              [class]
			, ltrim(rtrim([source].[cat])) as                                [category]
			, ltrim(rtrim([source].[drug])) as                               [internal_drug_id]
			, ltrim(rtrim([source].[ndc])) as                                [ndc]
			, isnull(cast([ndc].[medid] as varchar(25)), '''') as              [drug_id]
			, ltrim(rtrim([source].[name])) as                               [name]
			, ltrim(rtrim([source].[alt_name])) as                           [alternate_name]
			, ltrim(rtrim([source].[alg_drug_id])) as                        [allergy_drug_id]
			, case
				when ltrim(rtrim([source].[status])) = ''A''
					then CONVERT(bit, 1)
				else CONVERT(bit, 0)
			end as                                                         [is_active]
			, ltrim(rtrim([source].[cmt])) as                                [comment]
			, ltrim(rtrim([source].[sched])) as                              [schedule]
			, ltrim(rtrim([source].[reaction])) as                           [reaction]
			, ltrim(rtrim([source].[severity])) as                           [severity]
			, ltrim(rtrim([source].[source])) as                             [source]
			, ltrim(rtrim([source].[parent_id])) as                          [parent_drug_id]
			, ltrim(rtrim([source].[parent_name])) as                        [parent_drug_name]
			, [source].[usr] as                                              [add_user_id]
			, ltrim(rtrim([source].[dateadd])) as                            [add_datetime]
			,[source].[usrchg] as                                            [change_user_id]
			, ltrim(rtrim([source].[datechg])) as                            [change_datetime]
			, ltrim(rtrim([source].[actionstatus])) as                       [action_status]
			, ltrim(rtrim([source].[provider])) as                           [information_source]
			, ltrim(rtrim([patients].[person])) as                           [person_number]
			, ltrim(rtrim([patients].[acctnum])) as                          [account_number]
			, [source].[ibex]
			, [source].[site]
	from   [dbo].[alg] as [source]
	inner join cte_person as [patients] 
			on [patients].[site] = [source].[site]
            and [patients].[ibex] = [source].[ibex]
	left join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[ndc] = [source].[ndc]
	where  [source].[type] = ''A''
)
, CombinedResult AS
(
    select [hie].[patient_id]
        , [hie].[class]
        , [hie].[category]
        , [hie].[internal_drug_id]
        , [hie].[ndc]
        , [hie].[drug_id]
        , [hie].[name]
        , [hie].[alternate_name]
        , [hie].[allergy_drug_id]
        , [hie].[is_active]
        , [hie].[comment]
        , [hie].[schedule]
        , [hie].[reaction]
        , [hie].[severity]
        , [hie].[source]
        , [hie].[parent_drug_id]
        , [hie].[parent_drug_name]
        , [hie].[add_user_id]
        , [hie].[add_datetime]
        , [hie].[change_user_id]
        , [hie].[change_datetime]
        , [hie].[action_status]
        , [hie].[information_source]
        , [hie].[person_number]
        , [hie].[account_number]
        , [hie].[ibex]
        , [hie].[site]
    from     [cte_hie] as [hie]
    union
    select [pat].[patient_id]
        , [pat].[class]
        , [pat].[category]
        , [pat].[internal_drug_id]
        , [pat].[ndc]
        , [pat].[drug_id]
        , [pat].[name]
        , [pat].[alternate_name]
        , [pat].[allergy_drug_id]
        , [pat].[is_active]
        , [pat].[comment]
        , [pat].[schedule]
        , [pat].[reaction]
        , [pat].[severity]
        , [pat].[source]
        , [pat].[parent_drug_id]
        , [pat].[parent_drug_name]
        , [pat].[add_user_id]
        , [pat].[add_datetime]
        , [pat].[change_user_id]
        , [pat].[change_datetime]
        , [pat].[action_status]
        , [pat].[information_source]
        , [pat].[person_number]
        , [pat].[account_number]
        , [pat].[ibex]
        , [pat].[site]
    from   [cte_pat] as [pat]
)
select [result].[patient_id]
    , [result].[class]
    , [result].[category]
    , [result].[internal_drug_id]
    , [result].[ndc]
    , [result].[drug_id]
    , [result].[name]
    , [result].[alternate_name]
    , [result].[allergy_drug_id]
    , [result].[is_active]
    , [result].[comment]
    , [result].[schedule]
    , [rec].[misc2] [reaction]
    , [sev].[misc2] [severity]
    , [src].[misc2] [source]
    , [result].[parent_drug_id]
    , [result].[parent_drug_name]
    , [result].[add_user_id]
    , [result].[add_datetime]
    , [result].[change_user_id]
    , [result].[change_datetime]
    , [result].[action_status]
    , [result].[information_source]
    , [result].[person_number]
    , [result].[account_number]
from	CombinedResult as [result]
join [dbo].[org] [o] on [result].[site] = [o].[site]
left join [dbo].[idx] [rec] 
		on [result].[reaction]=[rec].[id] and [rec].[type]=''LQ'' and [rec].[site]=[o].[algreactioncs]
left join [dbo].[idx] [sev] 
		on [result].[severity]=[sev].[id] and [sev].[type]=''LR'' and [sev].[site]=[o].[algseveritycs]
left join [dbo].[idx] [src] 
		on [result].[source]=[src].[id] and [src].[type]=''LS'' and [src].[site]=[o].[algsourcecs]
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;