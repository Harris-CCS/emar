print 'create function [dbo].[emar_is_device_pulled_in_from_ibex];';

/*
In visual studio, functions with external references appear to be treated different than procedures with external references
*/
set @template = N'
CREATE OR ALTER FUNCTION [dbo].emar_is_device_pulled_in_from_ibex
(
	-- Add the parameters for the function here
	@medPrn int = NULL
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ret int = 0
	
	-- If @medPrn is in the emar_devices_to_pull_in_from_ibex view, then return it.
	-- Else, return 0.
	-- The API/IDS is setup to handle 0 and use a default value for last used device.
	IF @medPrn IS NOT NULL
	BEGIN
		IF EXISTS (SELECT 1 FROM emar_devices_to_pull_in_from_ibex WHERE [num] = @medPrn)
		BEGIN
			SET @ret = @medPrn
		END
	END

	-- Return the result of the function
	RETURN @ret

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
  , @value = N'Function to determine if the emar device has been pulled in. RETURNS 0=not pulled in 1..n=pulled in device id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'emar_is_device_pulled_in_from_ibex';
*/
