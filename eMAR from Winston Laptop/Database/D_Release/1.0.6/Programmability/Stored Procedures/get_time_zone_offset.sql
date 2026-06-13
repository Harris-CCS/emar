CREATE PROCEDURE [dbo].[get_time_zone_offset]
(
	@time_zone_name varchar(500)
)
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#TimeZoneOffset') IS NOT NULL
	BEGIN
		DROP TABLE #TimeZoneOffset
	END
	
	CREATE TABLE #TimeZoneOffset
	(
	value varchar(500)
	)

	INSERT INTO #TimeZoneOffset (value)
	SELECT current_utc_offset FROM sys.time_zone_info WHERE name = @time_zone_name

	SELECT TOP 1 * FROM #TimeZoneOffset
	RETURN;
END;

go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure Purpose: Get Time Zone Offset
This procedure is designed to run with a timezone name input parameter
and will return its corresponding timezone offset
'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'get_time_zone_offset';
go

