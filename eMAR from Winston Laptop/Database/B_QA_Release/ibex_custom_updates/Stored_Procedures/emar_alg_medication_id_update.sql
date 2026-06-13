print 'create procedure [ibex].[dbo].[emar_alg_medication_id_update];';

set @template = N'
CREATE OR ALTER   PROCEDURE [dbo].[emar_alg_medication_id_update]
	@ibex  varchar(20) = NULL
	,@DebugOutput bit = 0
AS

/***** For debugging purposes, we want to know what the MERGE statements do, and report on it *****/
DECLARE @output TABLE (
	insertednum int NULL,
	insertedmedication_id int NULL,
	deletednum int NULL,
	deletedmedication_id int NULL
);

/***** figure out the pat''s (ibex''s) we are working with *****/
DECLARE @pat TABLE (ibex varchar(20))
IF @ibex IS NOT NULL
	INSERT	@pat
	VALUES	(@ibex)
ELSE
	INSERT	@pat
	SELECT	ibex
	FROM	pat WHERE ISNULL(emar_pat, ''N'') = ''Y''
	
/***** create a list of alg/hie_alg/hie_meds records for those pat''s that don''t have a cache record *****/
DECLARE	@tbl TABLE (
	num int NOT NULL PRIMARY KEY
	,ndc varchar(30) NULL
	,drug_id varchar(25) NULL
	,[name] varchar(255) NULL
	,medication_id int NOT NULL DEFAULT(0)
	,[match] nvarchar(255) NULL
)

/***** Source = ibex..alg *****/
INSERT	@tbl (num, ndc, [name])
SELECT	a.num, ndc, [name]
FROM	dbo.alg a
JOIN	@pat p
		ON a.ibex = p.ibex
LEFT JOIN emar_alg_medication_id_cache c
		ON a.num = c.num
WHERE	c.num IS NULL
AND		(	(type = ''A'' AND actionstatus <> ''R'')
		OR	(type = ''M'' AND actionstatus = ''C''))
AND		ltrim(rtrim(status)) = ''A''   -- translated to [emar_patient_allergies_retrieve_fn].is_active

IF EXISTS (SELECT TOP 1 1 FROM @tbl)
BEGIN
	-- Populate the drug_id for the subject records
	UPDATE	t
	SET		drug_id = ndc.medid
	FROM	@tbl t
	JOIN	dbo.fdb_ndc_info ndc
			ON t.ndc = ndc.ndc

	-- run the updates to populate the medication_id/match -- Incorporating code from emar.[dbo].[update_medication_id_list] 
	--- ndc match
	update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- ndc match''
	from   @tbl [target]
	inner join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[ndc] = [target].[ndc]
			and LTRIM(ISNULL([target].[ndc], '''')) != ''''
	inner join emar.[dbo].medication_details as [md] 
			on [md].[drug_id] = [ndc].[medid]
	inner join emar.[dbo].medications as [m] 
			on [m].[id] = [md].medication_id
			and  [m].[site_id] = -1
			and [target].[medication_id] = 0;

    --- base ndc match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- base ndc match''
    from   @tbl [target]
    inner join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[base_ndc] = ISNULL([target].[ndc], CHAR(0))
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[drug_id] = [ndc].[medid]
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- drug_id match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- drug_id match''
    from   @tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[drug_id] = ISNULL([target].[drug_id], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- brand_name match
	update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- brand_name match''
    from	@tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[brand_name] = ISNULL([target].[name], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
            and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- long_brand_name match (display_name)
    update	[target] set    
			[target].[medication_id] = [m].[id]
			, [match] = ''--- long_brand_name match (display_name)''
    from   @tbl [target]
	inner join emar.[dbo].[medications] as [m] 
			on [m].[display_name] = ISNULL([target].[name], CHAR(0))
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- long_brand_name wildcard contains match (display_name)
    update	[target] set    
			[target].[medication_id] = [m].[id]
			, [match] = ''--- long_brand_name wildcard contains match (display_name)''
    from   @tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[brand_name] = ISNULL([target].[name], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0
            and [m].[display_name] like ''%'' + ISNULL([target].[name], CHAR(0)) + ''%'';


	/***** merge results into emar_alg_medication_id_cache *****/
		-- Shouldn''t technically have to do a merge, just an insert since the only records we''re working with are the ones that
		-- don''t have records in emar_alg_medication_id_cache, but
		--	- if some other process happened to be running this SP at the same time, they might beat us to the INSERT, so an UPDATE would be good
		--	- an [alg] record might have been dropped during the running of this SP, so what was a valid [alg] at the top might not be valid
		--    by the time we do the merge, so joining back to [alg] in the merge''s CTE will guarantee we aren''t violating the FK
	WITH src AS (
		SELECT t.* 
		FROM @tbl t
		JOIN alg a
				ON t.num = a.num
	)
	MERGE INTO dbo.emar_alg_medication_id_cache tar
	USING src
		ON tar.num = src.num
	WHEN NOT MATCHED THEN
		INSERT (num, medication_id, match)
		VALUES (num, ISNULL(medication_id, 0), match)
	WHEN MATCHED THEN
		UPDATE SET
			medication_id = ISNULL(src.medication_id, 0)
			,[match] = src.[match]
	OUTPUT inserted.num, inserted.medication_id, deleted.num, deleted.medication_id
	INTO	@output;

	IF @DebugOutput = 1 BEGIN
		DECLARE	@InsertedRecords int = (SELECT COUNT(*) FROM @output WHERE deletednum IS NULL)
				,@UpdatedRecords int = (SELECT COUNT(*) FROM @output WHERE deletednum IS NOT NULL)
		PRINT CONCAT(''emar_alg_medication_id_cache: '', @InsertedRecords, '' new records // '', @UpdatedRecords, '' updated records'')
	END
END

/***** Source = ibex..hie_alg *****/
DELETE @tbl;
INSERT	@tbl (num, ndc, [name])
SELECT	src.num, ndc, name
FROM	@pat p
JOIN	dbo.pat as pat
		ON pat.ibex = p.ibex
JOIN	dbo.hie_alg src
		on src.site = pat.site
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.person, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.person, CHAR(1)) 
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.acctnum, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.acctnum, CHAR(1))
LEFT JOIN emar_hie_alg_medication_id_cache c
		ON src.num = c.num
WHERE	c.num IS NULL
AND		(actionstatus <> ''R'')
AND		ltrim(rtrim(status)) = ''A''   -- translated to [emar_patient_allergies_retrieve_fn].is_active
	UNION
SELECT	src.num, ndc, name
FROM	@pat p
JOIN	dbo.hst as pat
		ON pat.ibex = p.ibex
JOIN	dbo.hie_alg src
		on src.site = pat.site
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.person, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.person, CHAR(1)) 
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.acctnum, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.acctnum, CHAR(1))
LEFT JOIN emar_hie_alg_medication_id_cache c
		ON src.num = c.num
WHERE	c.num IS NULL
AND		(actionstatus <> ''R'')
AND		ltrim(rtrim(status)) = ''A''   -- translated to [emar_patient_allergies_retrieve_fn].is_active

IF EXISTS (SELECT TOP 1 1 FROM @tbl)
BEGIN
	-- Populate the drug_id for the subject records
	UPDATE	t
	SET		drug_id = ndc.medid
	FROM	@tbl t
	JOIN	dbo.fdb_ndc_info ndc
			ON t.ndc = ndc.ndc

	-- run the updates to populate the medication_id/match -- Incorporating code from emar.[dbo].[update_medication_id_list] 
	--- ndc match
	update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- ndc match''
	from   @tbl [target]
	inner join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[ndc] = [target].[ndc]
			and LTRIM(ISNULL([target].[ndc], '''')) != ''''
	inner join emar.[dbo].medication_details as [md] 
			on [md].[drug_id] = [ndc].[medid]
	inner join emar.[dbo].medications as [m] 
			on [m].[id] = [md].medication_id
			and  [m].[site_id] = -1
			and [target].[medication_id] = 0;

    --- base ndc match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- base ndc match''
    from   @tbl [target]
    inner join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[base_ndc] = ISNULL([target].[ndc], CHAR(0))
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[drug_id] = [ndc].[medid]
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- drug_id match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- drug_id match''
    from   @tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[drug_id] = ISNULL([target].[drug_id], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- brand_name match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- brand_name match''
    from	@tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[brand_name] = ISNULL([target].[name], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
            and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- long_brand_name match (display_name)
    update	[target] set    
			[target].[medication_id] = [m].[id]
			, [match] = ''--- long_brand_name match (display_name)''
    from   @tbl [target]
	inner join emar.[dbo].[medications] as [m] 
			on [m].[display_name] = ISNULL([target].[name], CHAR(0))
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- long_brand_name wildcard contains match (display_name)
    update	[target] set    
			[target].[medication_id] = [m].[id]
			, [match] = ''--- long_brand_name wildcard contains match (display_name)''
    from   @tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[brand_name] = ISNULL([target].[name], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0
            and [m].[display_name] like ''%'' + ISNULL([target].[name], CHAR(0)) + ''%'';

	/***** merge results into emar_alg_medication_id_cache *****/
		-- Shouldn''t technically have to do a merge, just an insert, but
		--	- if some other process happened to be running this SP at the same time, they might beat us to the INSERT, so an UPDATE would be good
		--	- an [alg] record might have been dropped during the running of this SP, so what was a valid [alg] at the top might not be valid
		--    by the time we do the merge, so joining back to [alg] in the merge''s CTE will guarantee we aren''t violating the FK
	WITH src AS (
		SELECT t.* 
		FROM @tbl t
		JOIN hie_alg a
				ON t.num = a.num
	)
	MERGE INTO emar_hie_alg_medication_id_cache tar
	USING src
		ON tar.num = src.num
	WHEN NOT MATCHED THEN
		INSERT (num, medication_id, match)
		VALUES (num, ISNULL(medication_id, 0), match)
	WHEN MATCHED THEN
		UPDATE SET
			medication_id = ISNULL(src.medication_id, 0)
			,[match] = src.[match]
	OUTPUT inserted.num, inserted.medication_id, deleted.num, deleted.medication_id
	INTO	@output;

	IF @DebugOutput = 1 BEGIN
		SET @InsertedRecords = (SELECT COUNT(*) FROM @output WHERE deletednum IS NULL)
		SET @UpdatedRecords = (SELECT COUNT(*) FROM @output WHERE deletednum IS NOT NULL)
		PRINT CONCAT(''emar_hie_alg_medication_id_cache: '', @InsertedRecords, '' new records // '', @UpdatedRecords, '' updated records'')
	END
END

/***** Source = ibex..hie_meds *****/
DELETE @tbl;
INSERT	@tbl (num, ndc, [name])
SELECT	src.num, ndc, name
FROM	@pat p
JOIN	dbo.pat as pat
		ON pat.ibex = p.ibex
JOIN	dbo.hie_meds src
		on src.site = pat.site
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.person, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.person, CHAR(1)) 
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.acctnum, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.acctnum, CHAR(1))
LEFT JOIN emar_hie_meds_medication_id_cache c
		ON src.num = c.num
WHERE	c.num IS NULL
AND		(actionstatus = ''C'')
AND		ltrim(rtrim(status)) = ''A''   -- translated to [emar_patient_allergies_retrieve_fn].is_active
	UNION
SELECT	src.num, ndc, name
FROM	@pat p
JOIN	dbo.hst as pat
		ON pat.ibex = p.ibex
JOIN	dbo.hie_meds src
		on src.site = pat.site
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.person, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.person, CHAR(1)) 
		and dbo.IfStringIsNullOrWhiteSpaceThen(src.acctnum, CHAR(0)) = dbo.IfStringIsNullOrWhiteSpaceThen(pat.acctnum, CHAR(1))
LEFT JOIN emar_hie_meds_medication_id_cache c
		ON src.num = c.num
WHERE	c.num IS NULL
AND		(actionstatus = ''C'')
AND		ltrim(rtrim(status)) = ''A''   -- translated to [emar_patient_allergies_retrieve_fn].is_active

IF EXISTS (SELECT TOP 1 1 FROM @tbl)
BEGIN
	-- Populate the drug_id for the subject records
	UPDATE	t
	SET		drug_id = ndc.medid
	FROM	@tbl t
	JOIN	dbo.fdb_ndc_info ndc
			ON t.ndc = ndc.ndc

	-- run the updates to populate the medication_id/match -- Incorporating code from emar.[dbo].[update_medication_id_list] 
	--- ndc match
	update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- ndc match''
	from   @tbl [target]
	inner join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[ndc] = [target].[ndc]
			and LTRIM(ISNULL([target].[ndc], '''')) != ''''
	inner join emar.[dbo].medication_details as [md] 
			on [md].[drug_id] = [ndc].[medid]
	inner join emar.[dbo].medications as [m] 
			on [m].[id] = [md].medication_id
			and  [m].[site_id] = -1
			and [target].[medication_id] = 0;

    --- base ndc match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- base ndc match''
    from   @tbl [target]
    inner join [dbo].[fdb_ndc_info] as [ndc] 
			on [ndc].[base_ndc] = ISNULL([target].[ndc], CHAR(0))
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[drug_id] = [ndc].[medid]
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- drug_id match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- drug_id match''
    from   @tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[drug_id] = ISNULL([target].[drug_id], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- brand_name match
    update	[target] set    
			[medication_id] = [md].[medication_id]
			, [match] = ''--- brand_name match''
    from	@tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[brand_name] = ISNULL([target].[name], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
            and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- long_brand_name match (display_name)
    update	[target] set    
			[target].[medication_id] = [m].[id]
			, [match] = ''--- long_brand_name match (display_name)''
    from   @tbl [target]
	inner join emar.[dbo].[medications] as [m] 
			on [m].[display_name] = ISNULL([target].[name], CHAR(0))
			and [m].[site_id] = -1
            and [target].[medication_id] = 0;

    --- long_brand_name wildcard contains match (display_name)
    update	[target] set    
			[target].[medication_id] = [m].[id]
			, [match] = ''--- long_brand_name wildcard contains match (display_name)''
    from   @tbl [target]
    inner join emar.[dbo].[medication_details] as [md] 
			on [md].[brand_name] = ISNULL([target].[name], CHAR(0))
    inner join emar.[dbo].[medications] as [m] 
			on [m].[id] = [md].[medication_id]
			and [m].[site_id] = -1
            and [target].[medication_id] = 0
            and [m].[display_name] like ''%'' + ISNULL([target].[name], CHAR(0)) + ''%'';

	/***** merge results into emar_alg_medication_id_cache *****/
		-- Shouldn''t technically have to do a merge, just an insert, but
		--	- if some other process happened to be running this SP at the same time, they might beat us to the INSERT, so an UPDATE would be good
		--	- an [alg] record might have been dropped during the running of this SP, so what was a valid [alg] at the top might not be valid
		--    by the time we do the merge, so joining back to [alg] in the merge''s CTE will guarantee we aren''t violating the FK
	WITH src AS (
		SELECT t.* 
		FROM @tbl t
		JOIN hie_meds a
				ON t.num = a.num
	)
	MERGE INTO emar_hie_meds_medication_id_cache tar
	USING src
		ON tar.num = src.num
	WHEN NOT MATCHED THEN
		INSERT (num, medication_id, match)
		VALUES (num, ISNULL(medication_id, 0), match)
	WHEN MATCHED THEN
		UPDATE SET
			medication_id = ISNULL(src.medication_id, 0)
			,[match] = src.[match]
	OUTPUT inserted.num, inserted.medication_id, deleted.num, deleted.medication_id
	INTO	@output;

	IF @DebugOutput = 1 BEGIN
		SET @InsertedRecords = (SELECT COUNT(*) FROM @output WHERE deletednum IS NULL)
		SET @UpdatedRecords = (SELECT COUNT(*) FROM @output WHERE deletednum IS NOT NULL)
		PRINT CONCAT(''emar_hie_meds_medication_id_cache: '', @InsertedRecords, '' new records // '', @UpdatedRecords, '' updated records'')
	END
END
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
