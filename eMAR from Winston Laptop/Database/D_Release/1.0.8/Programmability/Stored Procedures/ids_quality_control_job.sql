CREATE PROCEDURE dbo.ids_quality_control_job
	@DoMedIdUpdate bit = 1
	,@PreviewOnly bit = 0
	,@Records tinyint = NULL	-- # of records to process, NULL means all
AS

-- Normalize potentially NULL bit values
SET @DoMedIdUpdate = ISNULL(@DoMedIdUpdate, 1)
SET	@PreviewOnly = ISNULL(@PreviewOnly, 0)
SET @Records = CASE WHEN ISNULL(@Records, 1) < 1 THEN 1 ELSE @Records END

IF @DoMedIdUpdate = 1
BEGIN
	-- Make sure the medication_id cache is up-to-date
	EXEC ibex.dbo.emar_alg_medication_id_update --@DebugOutput = 1;

	-- Piggybacking on this key to do an orphaned records check 
	IF EXISTS (
		SELECT *
		FROM	dbo.patients o
		LEFT JOIN dbo.external_ids ex
				ON o.id = ex.internal_id
				AND ex.entity = 'patients'
		WHERE	ex.internal_id IS NULL
		AND		is_active = 1
	)
	UPDATE	o
	SET		is_active = 0
	FROM	dbo.patients o
	LEFT JOIN dbo.external_ids ex
			ON o.id = ex.internal_id
			AND ex.entity = 'patients'
	WHERE	ex.internal_id IS NULL
	AND		is_active = 1;
END

DECLARE @discrepancy TABLE 
(
    -- columns returned by the function
    ExternalPatientId varchar(40) NOT NULL,
	EmarPatientId bigint NOT NULL,
    Discrepancies varchar(500) NOT NULL
)

-- Monitor emar_update_queue for complete processed (don't want to burden it if it has work to catch up on)
DECLARE	@UnprocessedRecCnt int
		,@LowestUnprocessed varchar(20)
SELECT	@UnprocessedRecCnt = COUNT(*)
		,@LowestUnprocessed = CONVERT(varchar(20), MIN(id))
FROM ibex.dbo.emar_update_queue WHERE complete_datetime IS NULL
WHILE @UnprocessedRecCnt > 0 
BEGIN
	RAISERROR('%d unprocessed records in queue, Oldest Id: %s.  Waiting 2 seconds', 10, 1, @UnprocessedRecCnt, @LowestUnprocessed) WITH NOWAIT
	WAITFOR DELAY '00:00:02';

	SELECT	@UnprocessedRecCnt = COUNT(*)
			,@LowestUnprocessed = MIN(id)
	FROM ibex.dbo.emar_update_queue WHERE complete_datetime IS NULL
END

-- Get the next discrepancy
INSERT @discrepancy
SELECT * FROM dbo.ids_find_patient_discrepancy_fn()

-- Loop as long as discrepancies exist
WHILE EXISTS (SELECT * FROM @discrepancy) AND ISNULL(@Records, 1) > 0
BEGIN
	-- Take care of the Test Record Count if it exists
	IF @Records IS NOT NULL SET @Records -= 1;

	-- Merge into the retry table
	MERGE INTO dbo.ids_discrepancy_retries tar
	USING @discrepancy src
		ON tar.ExternalPatientId = src.ExternalPatientId
		AND tar.EmarPatientId = src.EmarPatientId
		AND tar.Discrepancies = src.Discrepancies
	WHEN NOT MATCHED THEN 
		INSERT (ExternalPatientId, EmarPatientId, Discrepancies)
		VALUES (ExternalPatientId, EmarPatientId, Discrepancies)
	WHEN MATCHED THEN 
		UPDATE SET 
			LatestRetryTime = GETDATE(),
			RetryCount += 1;
	
	IF @PreviewOnly = 1 BEGIN
		-- If we are previewing, setting RetryCount to a ridiculous (Douglas Adams would agree) number
		-- so that it will cause the record not to be considered again, and so it is easily recognizable as a
		-- test record
		UPDATE	l
		SET		RetryCount = 42
		FROM	ids_discrepancy_retries l
		JOIN	@discrepancy d
				ON l.ExternalPatientId = d.ExternalPatientId
				AND l.EmarPatientId = d.EmarPatientId
				AND l.Discrepancies = d.Discrepancies
	END
	ELSE 
	BEGIN
		DECLARE	@ExternalPatientId varchar(40) = (SELECT TOP 1 ExternalPatientId FROM @discrepancy)
				,@CurrentTime varchar(50) = CONVERT(varchar(50), GETDATE(), 121)
		RAISERROR('Inserting %s in emar_update_queue at %s', 10, 1, @ExternalPatientId, @CurrentTime) WITH NOWAIT

		INSERT	ibex.dbo.emar_update_queue
				(entity, external_id)
		SELEcT	'patients', ExternalPatientId
		FROM	@discrepancy
	END

	-- Monitor emar_update_queue for complete processed
	SELECT	@UnprocessedRecCnt = COUNT(*)
			,@LowestUnprocessed = CONVERT(varchar(20), MIN(id))
	FROM ibex.dbo.emar_update_queue WHERE complete_datetime IS NULL
	WHILE @UnprocessedRecCnt > 0 
	BEGIN
		RAISERROR('%d unprocessed records in queue, Oldest Id: %s.  Waiting 2 seconds', 10, 1, @UnprocessedRecCnt, @LowestUnprocessed) WITH NOWAIT
		WAITFOR DELAY '00:00:02';

		SELECT	@UnprocessedRecCnt = COUNT(*)
				,@LowestUnprocessed = MIN(id)
		FROM ibex.dbo.emar_update_queue WHERE complete_datetime IS NULL
	END

	DELETE @discrepancy
	INSERT @discrepancy
	SELECT * FROM dbo.ids_find_patient_discrepancy_fn()
END
IF @PreviewOnly = 1
BEGIN
	SELECT * FROM ids_discrepancy_retries
	WHERE	RetryCount = 42
	ORDER BY 1;

	DELETE	ids_discrepancy_retries
	WHERE	RetryCount = 42;
END
