print 'create procedure [ibex].[dbo].[emar_update_queue_maintenance];';

set @template = N'
/*** SP for processing the emar_update_queue ***/
CREATE OR ALTER PROCEDURE [dbo].[emar_update_queue_maintenance]
               @OldMaxId bigint = NULL,
               @Entity varchar(50) = NULL,
               @ExternalId varchar(50) = NULL
AS

DECLARE @MaxRecordsToKeepAfterPurge bigint = 100000
/* Get the maximum id for records to process, and to return to the API */
DECLARE @MaxId bigint = (SELECT MAX(id) FROM [dbo].[emar_update_queue]);

-- DO the maintenance stuff (which if it fails won''t cause problems - we''ll just get it next time) first
BEGIN TRY
	/* Check to see if it is time to do queue maintenance (every 1000 records) */
	IF @MaxId / 1000 != @OldMaxId / 1000
	BEGIN
		DELETE TOP (100000) emar_update_queue WITH (READPAST)
		WHERE id < (@MaxId - @MaxRecordsToKeepAfterPurge)
	END
END TRY
BEGIN CATCH
	-- If the above statement fails, it has nothing to do with stamping the processed records complete, 
	-- or stamping the new records inprocess, so we can just ignore
	-- The next time we cross a 1000 record boundary, we''ll try to delete another 100K records, so we''re fine
END CATCH

-- Loop the real stuff up to 5 times (if there''s failures) and then if still failing, return 
DECLARE	@LoopCount int = 0
		, @Done bit = 0
WHILE @LoopCount < 5 and @Done = 0 BEGIN
	SET @LoopCount += 1;

	BEGIN TRY
		-- Don''t need a transaction here.  If we succeed on stamping the processed records complete,
		-- but fail to mark the next records InProcess, that won''t cause a problem
		IF @OldMaxId IS NOT NULL
		BEGIN
			IF @Entity = ''BogusRecordType'' BEGIN
				DECLARE	@id bigint = (
					SELECT	MIN(id)
					FROM	[dbo].[emar_update_queue] q (NOLOCK)
					WHERE	q.inprocess_datetime IS NOT NULL
					AND		q.complete_datetime IS NULL
					AND		q.entity NOT IN (''users'', ''patients'', ''indicators'', ''heartbeat'')
				)
				IF @id IS NOT NULL
					UPDATE	emar_update_queue
					SET		complete_datetime = SYSDATETIMEOFFSET()
					WHERE	id = @id;
			END ELSE BEGIN
				/* Mark all of the processed records complete */
				UPDATE	emar_update_queue
				SET		complete_datetime = SYSDATETIMEOFFSET()
				WHERE	entity = @Entity
				AND		external_id = @ExternalId
				AND		complete_datetime IS NULL
				AND		inprocess_datetime IS NOT NULL
				AND		id <= @OldMaxId;
			END
		END

		/* Clear any stale InProcess stamps from the queue */
        -- would happen if a process crashed after the queue records were put inprocess, but before completed
		-- Needs to be after current record is marked complete - just in case processing the record takes more than 60 seconds
		-- Gets separate TRY/CATCH because if we fail, it is not a show-stopper
		IF EXISTS (
			SELECT	TOP 1 1 
			FROM	emar_update_queue (NOLOCK) 
			WHERE	inprocess_datetime IS NOT NULL
			AND		complete_datetime IS NULL
			-- 20220623 BRM: Updated the below from GETDATE() to SYSDATETIMEOFFSET()
			AND		DATEDIFF(second, inprocess_datetime, SYSDATETIMEOFFSET()) > 60
		)
		BEGIN
			BEGIN TRY
				UPDATE	emar_update_queue
				SET		inprocess_datetime = NULL
				WHERE	DATEDIFF(second, inprocess_datetime, SYSDATETIMEOFFSET()) > 60
				AND		complete_datetime IS NULL;
			END TRY
			BEGIN CATCH
				-- As above, not a show-stopper
			END CATCH
		END

		/* 
		 * In one statement, get the entity/external_id to process, and put all of that 
		 * record''s queue entries in process
		*/
		DECLARE @UpdatedRecord TABLE (entity varchar(50) NOT NULL, external_id varchar(50) NOT NULL, id bigint);
		WITH MinId AS (
			SELECT  MIN(id) as id
			FROM    [dbo].[emar_update_queue] (NOLOCK)
			WHERE inprocess_datetime IS NULL
		)
		, RecordToProcess AS (
			SELECT  entity, external_id
			FROM    [dbo].[emar_update_queue] (NOLOCK)
			WHERE id = (SELECT id FROM MinId)
		)
		UPDATE	q
		SET		inprocess_datetime = SYSDATETIMEOFFSET()
		OUTPUT	inserted.entity, inserted.external_id, inserted.id
		INTO	@UpdatedRecord
		FROM	[dbo].[emar_update_queue] q
		JOIN	RecordToProcess r
				ON q.entity = r.entity
				AND q.external_id = r.external_id
				AND q.inprocess_datetime IS NULL
				AND id >= (SELECT id FROM MinId)
				AND id <= @MaxId
		
		SET @Done = 1
	END TRY
	BEGIN CATCH
		-- Probably want to drop a message in a message table here about what failed
	END CATCH
END

IF @Done = 0 BEGIN
	-- We failed 5 times trying to do the updating.  Return the dummy row that says the Queue is processed
	INSERT	@UpdatedRecord
	SELECT  @MaxId as id, CONVERT(varchar(50), ''queue_empty'') as entity, CONVERT(varchar(50), ''-1'') as external_id --, CONVERT(varchar(50), ''-1'') as queue_record_id
END ELSE 
	/* If we have processed everything in the queue, we still want to return the MaxId, so that 
	 * the listener service will know if there''s a record coming in with an old id on it, that 
	 * it doesn''t need to call this SP again */
	IF NOT EXISTS (SELECT TOP 1 1 FROM @UpdatedRecord)
	BEGIN
		SELECT  @MaxId as id, CONVERT(varchar(50), ''queue_empty'') as entity, CONVERT(varchar(50), ''-1'') as external_id, CONVERT(varchar(50), ''-1'') as queue_record_id
	END ELSE BEGIN
		SELECT  TOP 1 @MaxId AS id, entity as entity, external_id as external_id, CONVERT(varchar(50), id) as queue_record_id
		FROM    @UpdatedRecord
		ORDER BY Id
	END
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;