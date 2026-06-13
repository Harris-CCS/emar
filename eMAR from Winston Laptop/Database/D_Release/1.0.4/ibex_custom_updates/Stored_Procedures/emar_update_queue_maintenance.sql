print 'create procedure [ibex].[dbo].[emar_update_queue_maintenance];';

set @template = N'
/*** SP for processing the emar_update_queue ***/
CREATE OR ALTER PROCEDURE [dbo].[emar_update_queue_maintenance]
	@OldMaxId bigint = NULL,
	@Entity varchar(50) = NULL,
	@ExternalId varchar(50) = NULL
AS

DECLARE @MaxRecordsToKeepAfterPurge bigint = 100000

IF @OldMaxId IS NOT NULL
BEGIN
	IF @Entity = ''BogusRecordType'' BEGIN
		DECLARE @id bigint = (
			SELECT	MIN(id)
			FROM	[dbo].[emar_update_queue] q
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
		AND     inprocess_datetime IS NOT NULL
		AND		id <= @OldMaxId;
	END
END

/* Clear any stale InProcess stamps from the queue */
	-- would happen if a process crashed after the queue records were put inprocess, but before completed
UPDATE	emar_update_queue
SET		inprocess_datetime = NULL
WHERE	DATEDIFF(second, inprocess_datetime, SYSDATETIMEOFFSET()) > 60
AND		complete_datetime IS NULL;

/* Get the maximum id for records to process, and to return to the API */
DECLARE @MaxId bigint = (SELECT MAX(id) FROM [dbo].[emar_update_queue]);

/* Check to see if it is time to do queue maintenance (every 1000 records) */
IF @MaxId / 1000 != @OldMaxId / 1000
BEGIN
	DELETE	emar_update_queue
	WHERE	id < (@MaxId - @MaxRecordsToKeepAfterPurge)
END

/* 
 * In one statement, get the entity/external_id to process, and put all of that 
 * record''s queue entries in process
 */
DECLARE @UpdatedRecord TABLE (entity varchar(50) NOT NULL, external_id varchar(50) NOT NULL);
WITH MinId AS (
	SELECT	MIN(id) as id
	FROM	[dbo].[emar_update_queue]
	WHERE	inprocess_datetime IS NULL
)
, RecordToProcess AS (
	SELECT	entity, external_id
	FROM	[dbo].[emar_update_queue]
	WHERE	id = (SELECT id FROM MinId)
)
UPDATE	q
SET		inprocess_datetime = SYSDATETIMEOFFSET()
OUTPUT	inserted.entity, inserted.external_id
INTO	@UpdatedRecord
FROM	[dbo].[emar_update_queue] q
JOIN	RecordToProcess r
		ON q.entity = r.entity
		AND q.external_id = r.external_id
        AND q.inprocess_datetime IS NULL
		AND id >= (SELECT id FROM MinId)
		AND id <= @MaxId

/* If we have processed everything in the queue, we still want to return the MaxId, so that 
 * the listener service will know if there''s a record coming in with an old id on it, that 
 * it doesn''t need to call this SP again */
IF NOT EXISTS (SELECT TOP 1 1 FROM @UpdatedRecord)
BEGIN
	SELECT	@MaxId as id, CONVERT(varchar(50), ''queue_empty'') as entity, CONVERT(varchar(50), ''-1'') as external_id
END ELSE BEGIN
	SELECT	TOP 1 @MaxId AS id, entity as entity, external_id as external_id
	FROM	@UpdatedRecord
END
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;