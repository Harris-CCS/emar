print 'create procedure [ibex].[dbo].[emar_update_queue_log_error];';

set @template = N'
CREATE OR ALTER PROCEDURE dbo.emar_update_queue_log_error
	@QueueId bigint,
	@ErrorLocation nvarchar(100),
	@Exception nvarchar(max)
AS

IF NOT EXISTS (SELECT TOP 1 1 FROM emar_update_queue WHERE id = @QueueId)
	RETURN;

WITH ExistingHighestErrorNum AS (
	SELECT ISNULL(
		(SELECT	MAX(queue_record_error_num) 
		FROM	dbo.emar_update_queue_errors
		WHERE	queue_id = @QueueId)
		, 0) as maxNum
)
INSERT	dbo.emar_update_queue_errors
		(queue_id, queue_record_error_num, error_location, exception_info)
SELECT	@QueueId, maxNum + 1, @ErrorLocation, @Exception
FROM	ExistingHighestErrorNum
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
