print 'create function [dbo].[emar_patient_allergies_retrieve_fn];';

/*
In visual studio, functions with external references appear to be treated different than procedures with external references
*/
set @template = N'
CREATE OR ALTER FUNCTION [dbo].[emar_patient_allergies_retrieve_fn] (@Ibex varchar(20))
RETURNS @ret TABLE (
	patient_id varchar(20)	-- ibex varchar(14)  --  only
	,internal_key varchar(500)
	,class varchar(12)
	,category varchar(12)
	,internal_drug_id varchar(9)
	,ndc varchar(32)
	,drug_id varchar(25)
	,name varchar(255)
	,alternate_name varchar(255)
	,allergy_drug_id varchar(9)
	,is_active bit
	,comment varchar(255)
	,schedule varchar(40)
	,reaction varchar(80)
	,severity varchar(80)
	,[source] varchar(80)
	,parent_drug_id varchar(255)
	,parent_drug_name varchar(255)
	,add_user_id int
	,add_datetime varchar(12)
	,change_user_id int
	,change_datetime varchar(12)
	,action_status char(1)
	,information_source varchar(25)
	,person_number varchar(20)
	,account_number varchar(14)
	,medication_id int NULL DEFAULT(0)
	,match nvarchar(255) NULL
)
AS
BEGIN
	DECLARE	 @ret_work TABLE (
		id int NOT NULL Primary Key
		,patient_id varchar(20)	 -- ibex varchar(14)  --  only
		,internal_key varchar(500)
		,class varchar(12)
		,category varchar(12)
		,internal_drug_id varchar(9)
		,ndc varchar(32)
		,drug_id varchar(25)
		,name varchar(255)
		,alternate_name varchar(255)
		,allergy_drug_id varchar(9)
		,is_active bit
		,comment varchar(255)
		,schedule varchar(40)
		,reaction varchar(80)
		,severity varchar(80)
		,[source] varchar(80)
		,parent_drug_id varchar(255)
		,parent_drug_name varchar(255)
		,add_user_id int
		,add_datetime varchar(12)
		,change_user_id int
		,change_datetime varchar(12)
		,action_status char(1)
		,information_source varchar(25)
		,person_number varchar(20)
		,account_number varchar(14)
		,medication_id int NOT NULL DEFAULT(0)
		,match nvarchar(255) NULL
		,dup_num tinyint NULL
	)
	-- FROM: export_ibex_patient_allergies in the $/Database/C_Development/C_DevTrunk/emar.sqlproj

	-- Pull pat and hst matches here to use against alg table because correcting empty person/acctnum matches on
	-- hie_alg in cte_hie_person below results in patients without person or acctnum not having any allergies come 
	-- back in results from this sp.
	;with cte_person as
	(
		select ibex, site, person, acctnum from pat where ibex=@Ibex
		union
		select ibex, site, person, acctnum from hst where ibex=@Ibex
	)
	,cte_hie_person as 
	(
		-- Match criteria for pat/hst against HIE:
		--	person  = person and acctnum = acctnum and both are not blank
		--	person  = blank  and acctnum = acctnum and acctnum is not blank
		--	acctnum = blank  and person  = person  and person is not blank
		-- NOTE: In different situations these tables have nullable columns and/or values that have been padded with
		-- trailing whitespace in one table but not the other, but should still match!
		-- Hence all the null checks and trimming.
		-- 20220722 BRM: Updating the JOIN per discussions with Romel and Debi.  
		--          Also using cte_person so we only need one SELECT.
		select	pt.ibex, pt.site, src.num
		from	cte_person as pt
		inner join hie_alg src 
				on src.site = pt.site
				and dbo.IfStringIsNullOrWhiteSpaceThen(src.person, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pt.person, CHAR(1)) 
				and dbo.IfStringIsNullOrWhiteSpaceThen(src.acctnum, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pt.acctnum, CHAR(1))
	)
	, cte_hie as (
		select	source.num * -1 as id
				, ltrim(rtrim([patients].[ibex])) as							   [patient_id]
				, ltrim(rtrim([source].[class])) as                                [class]
				, ltrim(rtrim([source].[cat])) as                                  [category]
				, ltrim(rtrim([source].[drug])) as                                 [internal_drug_id]
				, ltrim(rtrim([source].[ndc])) as                                  [ndc]
				, isnull(cast([ndc].[medid] as varchar(25)), '''') as              [drug_id]
				, ltrim(rtrim([source].[name])) as                                 [name]
				, ltrim(rtrim('''')) as                                            [alternate_name]
				, ltrim(rtrim([source].[alg_drug_id])) as                          [allergy_drug_id]
				, case
					when ltrim(rtrim([source].[status])) = ''A''
						then CONVERT(bit, 1)
					else CONVERT(bit, 0)
				end as															   [is_active]
				, ltrim(rtrim([source].[comment])) as                              [comment]
				, ltrim(rtrim('''')) as                                            [schedule]
				, ltrim(rtrim('''')) as                                            [reaction]
				, ltrim(rtrim([source].[severity])) as                             [severity]
				, ltrim(rtrim([source].[source])) as                               [source]
				, ltrim(rtrim('''')) as                                            [parent_drug_id]
				, ltrim(rtrim('''')) as                                            [parent_drug_name]
				, CONVERT(int, 0) as											   [add_user_id]
				, '''' as                                                          [add_datetime]
				, CONVERT(int, 0) as											   [change_user_id]
				, '''' as                                                          [change_datetime]
				, ltrim(rtrim([source].[actionstatus])) as                         [action_status]
				, ltrim(rtrim(''hie_alg'')) as                                     [information_source]
				, ltrim(rtrim([source].[person])) as                               [person_number]
				, ltrim(rtrim([source].[acctnum])) as                              [account_number]
				, [patients].[ibex]
				, [source].[site]
				, c.medication_id
				, c.[match]
		from   [dbo].[hie_alg] as [source]
		JOIN	dbo.emar_hie_alg_medication_id_cache c
				ON [source].num = c.num
		inner join [cte_hie_person] as [patients] 
				on [patients].[num] = [source].[num]
		left join [dbo].[fdb_ndc_info] as [ndc] 
				on [ndc].[ndc] = [source].[ndc]
	)
	, cte_pat as (
		select	source.num as id
				, ltrim(rtrim([source].[ibex])) as                               [patient_id]
				, ltrim(rtrim([source].[class])) as                              [class]
				, ltrim(rtrim([source].[cat])) as                                [category]
				, ltrim(rtrim([source].[drug])) as                               [internal_drug_id]
				, ltrim(rtrim([source].[ndc])) as                                [ndc]
				, isnull(cast([ndc].[medid] as varchar(25)), '''') as            [drug_id]
				, ltrim(rtrim([source].[name])) as                               [name]
				, ltrim(rtrim([source].[alt_name])) as                           [alternate_name]
				, ltrim(rtrim([source].[alg_drug_id])) as                        [allergy_drug_id]
				, case
					when ltrim(rtrim([source].[status])) = ''A''
						then CONVERT(bit, 1)
					else CONVERT(bit, 0)
				end as                                                           [is_active]
				, ltrim(rtrim([source].[cmt])) as                                [comment]
				, ltrim(rtrim([source].[sched])) as                              [schedule]
				, ltrim(rtrim([source].[reaction])) as                           [reaction]
				, ltrim(rtrim([source].[severity])) as                           [severity]
				, ltrim(rtrim([source].[source])) as                             [source]
				, ltrim(rtrim([source].[parent_id])) as                          [parent_drug_id]
				, ltrim(rtrim([source].[parent_name])) as                        [parent_drug_name]
				, [source].[usr] as                                              [add_user_id]
				, ltrim(rtrim([source].[dateadd])) as                            [add_datetime]
				, [source].[usrchg] as                                           [change_user_id]
				, ltrim(rtrim([source].[datechg])) as                            [change_datetime]
				, ltrim(rtrim([source].[actionstatus])) as                       [action_status]
				, ltrim(rtrim([source].[provider])) as                           [information_source]
				, ltrim(rtrim([patients].[person])) as                           [person_number]
				, ltrim(rtrim([patients].[acctnum])) as                          [account_number]
				, [source].[ibex]
				, [source].[site]
				, c.medication_id
				, c.[match]
		from   [dbo].[alg] as [source]
		JOIN	dbo.emar_alg_medication_id_cache c
				ON [source].num = c.num
		inner join cte_person as [patients] 
				on [patients].[site] = [source].[site]
				and [patients].[ibex] = [source].[ibex]
		left join [dbo].[fdb_ndc_info] as [ndc] 
				on [ndc].[ndc] = [source].[ndc]
		where  [source].[type] = ''A''
	)
	, CombinedResult AS
	(
		select id		
			, [hie].[patient_id]
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
			, [hie].[medication_id]
			, [hie].[match]
		from     [cte_hie] as [hie]
		union
		select id		
			, [pat].[patient_id]
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
			, [pat].[medication_id]
			, [pat].[match]
		from   [cte_pat] as [pat]
	)
	INSERT	@ret_work
			(id, internal_key, patient_id, class, category, internal_drug_id, ndc, drug_id, [name], alternate_name, allergy_drug_id, is_active, 
			 comment, schedule, reaction, severity, [source], parent_drug_id, parent_drug_name, add_user_id, add_datetime, 
			 change_user_id, change_datetime, action_status, information_source, person_number, account_number,
			 medication_id, [match])
	select result.id
		-- 20220818 BRM:  We don''t have an ELSE, and don''t want one.  If this CASE statement is updated, then
		-- be sure to update the "WHERE ... AND (<make sure at least one internal key WHEN has a value>)" below
		, internal_key = CASE
				WHEN medication_id != 0 
					THEN CONCAT(''MedId:'', medication_id)
				WHEN LTRIM(ISNULL(allergy_drug_id, '''')) NOT IN (''0'', '''')
					THEN ''AlgDrugId:'' + allergy_drug_id + CASE WHEN LTRIM(ISNULL(parent_drug_id, '''')) = '''' THEN '''' ELSE '':'' + parent_drug_id END
				WHEN LTRIM(ISNULL(ndc, '''')) != ''''
					THEN ''NDC:'' + ndc
				WHEN ISNULL(LTRIM(internal_drug_id), ''ft'') NOT IN (''ft'', '''')
					THEN ''IntDrugId:'' + internal_drug_id
				WHEN ISNULL(LTRIM(result.name), '''') != ''''
					THEN ''FT:'' + result.name
			END
		, [result].[patient_id]
		, CONVERT(varchar(12), [result].[class]) as [class]
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
		, [result].[medication_id]
		, [result].[match]
	from	CombinedResult as [result]
	join [dbo].[org] [o] on [result].[site] = [o].[site]	
	left join [dbo].[idx] [rec] 
			on [result].[reaction]=[rec].[id] and [rec].type=''LQ'' and [rec].[site]=[o].[algreactioncs]
	left join [dbo].[idx] [sev] 
			on [result].[severity]=[sev].[id] and [sev].type=''LR'' and [sev].[site]=[o].[algseveritycs]
	left join [dbo].[idx] [src] 
			on [result].[source]=[src].[id] and [src].type=''LS'' and [src].[site]=[o].[algsourcecs]
	-- Also filter the result list to only include allergies where the status is not R
	-- Winston Murdock, 07/05/2022.  PC-27381
	WHERE result.action_status <> ''R''
	AND	result.is_active = 1	
	-- Added 20220623 BRM - if it is NULL, it will crash the IDS
	-- 20220818 BRM:  Upgrading the name filter.  NULL name will no longer crash the IDS, but we get weird
	-- results if we don''t make sure that at least one of the WHEN''s from the internal_key returns a value
	-- If not, the record is of no use, so filter it out...
	AND	(medication_id != 0 
		OR LTRIM(ISNULL(allergy_drug_id, '''')) NOT IN (''0'', '''')
		OR LTRIM(ISNULL(ndc, '''')) != ''''
		OR ISNULL(LTRIM(internal_drug_id), ''ft'') NOT IN (''ft'', '''')
		OR ISNULL(LTRIM(result.name), '''') != '''');

	WITH DuplicateRanking AS (
		-- In this CTE, the ORDER BY clause can be made as complex as necessary in order to properly
		-- sort the best duplicate candidate to survive (by making it sort to the top)
		SELECT	id, ROW_NUMBER() OVER (PARTITION BY patient_id, internal_key
				ORDER BY
					-- Give priority to ''C'' over ''U''
					CASE action_status WHEN ''C'' THEN 1 ELSE 2 END
					-- Give priority to stuff with reactions and severities
					,LEN(CONCAT(severity, reaction)) DESC
				) priority
		FROM	@ret_work
	)
	UPDATE	r
	SET		dup_num = d.priority
	FROM	@ret_work r
	JOIN	DuplicateRanking d
			ON r.id = d.id

	INSERT	@ret
			(patient_id, internal_key, class, category, internal_drug_id, ndc, drug_id, name, alternate_name, allergy_drug_id, is_active, 
			comment, schedule, reaction, severity, [source], parent_drug_id, parent_drug_name, add_user_id, add_datetime, change_user_id, 
			change_datetime, action_status, information_source, person_number, account_number, medication_id, match)
	SELECT	patient_id, internal_key, class, category, internal_drug_id, ndc, drug_id, name, alternate_name, allergy_drug_id, is_active, 
			comment, schedule, reaction, severity, [source], parent_drug_id, parent_drug_name, add_user_id, add_datetime, change_user_id, 
			change_datetime, action_status, information_source, person_number, account_number, 
			CASE medication_id WHEN 0 THEN NULL ELSE medication_id END as medication_id, match
	FROM	@ret_work
	WHERE	dup_num = 1

	RETURN 
END
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] @statement = @sql_cmd;

/***************
 Data Dictionary
    Function
***************/
/*
execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Function to retrieve a table of patient allergy data'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'emar_patient_allergies_retrieve_fn';
*/