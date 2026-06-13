-- =============================================
-- Author:		Winston Murdock
-- Create date: 04/21/2023
-- Description:	Map from the ibex time zone name to the emar time zone name.
--					We need this to be able to dynamically pull in the site's
--					time zone and not have to hard code it to central time.
--					At Emerus, we have the sites pull in commented out in the
--					hourly job so that we don't set the mountain time sites
--					to central time.  This will allow us to uncomment that.
-- Jira Ticket: PC-27916
-- =============================================
CREATE FUNCTION [dbo].[map_ibex_time_zone_name_to_emar_time_zone_name]
(
	-- Add the parameters for the function here
	@ibex_time_zone_name varchar(255)
)
RETURNS varchar(256)
AS
BEGIN
	DECLARE @ret varchar(256)
	-- Default this to central time, since we need to have something as a default.
	SET @ret = 'Central Standard Time'

	-- If the string from PulseCheck has no length, then we'll return the default of central standard time.
	if (LEN(@ibex_time_zone_name) > 0 AND @ibex_time_zone_name IS NOT NULL)
	BEGIN
		-- Actually do the mapping.
		--'hawaii' = 'Hawaiian Standard Time'
		--'alaska' = 'Alaskan Standard Time'
		--'pacific' = 'Pacific Standard Time'
		--'baja' = 'US Mountain Standard Time' (year round MST and don't observe DST)
		--'arizona' = 'US Mountain Standard Time' (year round MST and don't observe DST)
		--'mountain' = 'Mountain Standard Time'
		--'central' = 'Central Standard Time'
		--'eastern' = 'Eastern Standard Time'
		--'indiana' = 'Eastern Standard Time' (any cities that are in cental time should be listed as central time in PCED)
		if (PATINDEX('%hawaii%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'Hawaiian Standard Time'
		END
		ELSE if (PATINDEX('%alaska%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'Alaskan Standard Time'
		END
		ELSE if (PATINDEX('%pacific%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'Pacific Standard Time'
		END
		ELSE if (PATINDEX('%baja%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'US Mountain Standard Time'
		END
		ELSE if (PATINDEX('%arizona%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'US Mountain Standard Time'
		END
		ELSE if (PATINDEX('%mountain%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'Mountain Standard Time'
		END
		ELSE if (PATINDEX('%central%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'Central Standard Time'
		END
		ELSE if (PATINDEX('%eastern%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'Eastern Standard Time'
		END
		ELSE if (PATINDEX('%indiana%', @ibex_time_zone_name) > 0)
		BEGIN
			SET @ret = 'Eastern Standard Time'
		END
		ELSE
		BEGIN
			-- Default case should PulseCheck have some wacky time zone saved.
			SET @ret = 'Central Standard Time'
		END
	END

	-- Return.
	RETURN @ret
END

go
/***************
 Data Dictionary
    Function
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This function maps the ibex time zone name to the emar time zone name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'map_ibex_time_zone_name_to_emar_time_zone_name';
go