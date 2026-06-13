print 'create procedure [ibex].[dbo].[emar_patient_medications_retrieve_sp];';

set @template = N'
CREATE OR ALTER      PROCEDURE [dbo].[emar_patient_medications_retrieve_sp]
	@Ibex varchar(20)
AS

-- Pull pat and hst matches here to use against alg table because correcting empty person/acctnum matches on
-- hie_meds in cte_hie_person below results in patients without person or acctnum not having any home medications come 
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
	select src.ibex, src.site, hie_meds.num
	from pat as src
		inner join hie_meds on
				src.site = hie_meds.site
			and ltrim(rtrim(isnull(src.person, ''''))) = ltrim(rtrim(isnull(hie_meds.person, '''')))
			and ltrim(rtrim(isnull(src.acctnum, ''''))) = ltrim(rtrim(isnull(hie_meds.person, '''')))
			and (len(ltrim(rtrim(isnull(src.person, '''')))) > 0 OR len(ltrim(rtrim(isnull(src.acctnum, '''')))) > 0)
	where src.ibex = @Ibex

	union

	select src.ibex, src.site, hie_meds.num
	from hst as src
		inner join hie_meds on
				src.site = hie_meds.site
			and ltrim(rtrim(isnull(src.person, ''''))) = ltrim(rtrim(isnull(hie_meds.person, '''')))
			and ltrim(rtrim(isnull(src.acctnum, ''''))) = ltrim(rtrim(isnull(hie_meds.person, '''')))
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
			, '''' as															   [alternate_name]
			, ltrim(rtrim([source].[alg_drug_id])) as                          [medication_drug_id]
			, case
				when ltrim(rtrim([source].[status])) = ''A''
					then CONVERT(bit, 1)
				else CONVERT(bit, 0)
			end as                                                             [is_active]
			, ltrim(rtrim([source].[comment])) as                              [comment]
			, '''' as															   [last_taken_note]
			, ltrim(rtrim(isnull([source].[dose], ''''))) as                     [dose]
			, ltrim(rtrim(isnull([source].[route], ''''))) as                    [route]
			, ltrim(rtrim(isnull([source].[sched], ''''))) as                    [schedule]
			, ltrim(rtrim(isnull([source].[strength], ''''))) as                 [strength]
			, ltrim(rtrim(isnull([source].[unit], ''''))) as                     [unit]
			, ltrim(rtrim('''')) as                                              [reaction]
			, ltrim(rtrim([source].[source])) as                               [source]
			, ltrim(rtrim('''')) as                                              [parent_drug_id]
			, ltrim(rtrim('''')) as                                              [parent_drug_name]
			, CONVERT(int, 0) as											   [add_user_id]
			, '''' as                                                            [add_datetime]
			, CONVERT(int, 0) as											   [change_user_id]
			, '''' as                                                            [change_datetime]
			, ltrim(rtrim([source].[actionstatus])) as                         [action_status]
			, ltrim(rtrim(''hie_meds'')) as                                      [information_source]
			, ltrim(rtrim([source].[person])) as                               [person_number]
			, ltrim(rtrim([source].[acctnum])) as                              [account_number]
			, [patients].[ibex]
			, [source].[site]
	from   [dbo].[hie_meds] as [source]
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
			, ltrim(rtrim([source].[alg_drug_id])) as                        [medication_drug_id]
			, case
				when ltrim(rtrim([source].[status])) = ''A''
					then CONVERT(bit, 1)
				else CONVERT(bit, 0)
			end as                                                           [is_active]
			, ltrim(rtrim([source].[cmt])) as                                [comment]
			, ltrim(rtrim([source].[taken])) as                              [last_taken_note]
			, ltrim(rtrim(isnull([source].[dose], ''''))) as                   [dose]
			, ltrim(rtrim(isnull([source].[route], ''''))) as                  [route]
			, ltrim(rtrim(isnull([source].[sched], ''''))) as                  [schedule]
			, '''' as                                                          [strength]
			, ltrim(rtrim(isnull([source].[unit], ''''))) as                   [unit]
			, ltrim(rtrim([source].[reaction])) as                           [reaction]
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
	where  [source].[type] = ''M''
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
        , [hie].[medication_drug_id]
        , [hie].[is_active]
        , [hie].[comment]
		, [hie].[last_taken_note]
		, [hie].[dose]
		, [hie].[route]
		, [hie].[schedule]
		, [hie].[strength]
		, [hie].[unit]
        , [hie].[reaction]
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
        , [pat].[medication_drug_id]
        , [pat].[is_active]
        , [pat].[comment]
		, [pat].[last_taken_note]
		, [pat].[dose]
		, [pat].[route]
		, [pat].[schedule]
		, [pat].[strength]
		, [pat].[unit]
        , [pat].[reaction]
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
    , [result].[medication_drug_id]
    , [result].[is_active]
    , [result].[comment]
	, [result].[last_taken_note]
	, [result].[dose]
	, [result].[route]
    , [result].[schedule]
	, [result].[strength]
	, [result].[unit]
    , [rec].[misc2] [reaction]
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
left join [dbo].[idx] [rec] 
		on [result].[reaction]=[rec].[id] and [rec].type=''LQ'' and [rec].[site]=[result].[site]
left join [dbo].[idx] [src] 
		on [result].[source]=[src].[id] and [src].type=''LS'' and [src].[site]=[result].[site]
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;