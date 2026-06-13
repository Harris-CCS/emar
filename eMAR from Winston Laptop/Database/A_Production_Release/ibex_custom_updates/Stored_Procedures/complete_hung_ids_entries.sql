print 'create procedure [ibex].[dbo].[complete_hung_ids_entries];';

set @template = N'
CREATE OR ALTER PROCEDURE [dbo].[complete_hung_ids_entries]
AS
BEGIN
	-- Since marking a row as complete and moving on after five
	-- attempts is not currently working on prod, Romel and I
	-- discussed making this temporary SQL SP (which will be
	-- called by a scheduled job) to help out.
	-- If we have an entry that was entered more than x minutes ago,
	-- is in process, and is not yet completed, mark it as completed.
	-- That lets the IDS continue processing records and doesn''t
	-- force it to get stuck at this one entry (with all other
	-- entries, piling up behind it).
	-- The time we''ve seen it on prod was with a patient who
	-- has been in the system for more than eight days and has
	-- a large number of home medications.  We''re not sure what
	-- the cause of the error is.  But this will keep things moving.
	-- And we probably have all of the patient''s data in eMAR from
	-- a prior IDS pull in, so I''m not worried about data loss.
	-- Winston Murdock, 06/27/2022.

	DECLARE	@StaleEntry bigint = 
		(SELECT	TOP 1 id
		FROM	emar_update_queue
		WHERE	inprocess_datetime IS NOT NULL
		AND		complete_datetime IS NULL
		AND		DATEDIFF(minute, inprocess_datetime, SYSDATETIMEOFFSET()) > 5
		ORDER BY id)

	WHILE @StaleEntry IS NOT NULL
	BEGIN
		UPDATE emar_update_queue
		SET complete_datetime = SYSDATETIMEOFFSET()
		WHERE id = @StaleEntry

		-- Write to a log somewhere that we just completed a record that had been "InProcess" for
		-- DATEDIFF(second, inprocess_datetime, SYSDATETIMEOFFSET()) seconds
		INSERT INTO ids_entries_stamped_as_complete (timestamp, entity, external_id)
		SELECT SYSDATETIME(), entity, external_id
		FROM emar_update_queue
		WHERE id = @StaleEntry

		SET @StaleEntry = 
			(SELECT	TOP 1 id
			FROM	emar_update_queue
			WHERE	inprocess_datetime IS NOT NULL
			AND		complete_datetime IS NULL
			AND		DATEDIFF(minute, inprocess_datetime, SYSDATETIMEOFFSET()) > 5
			ORDER BY id)
	END
END
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
