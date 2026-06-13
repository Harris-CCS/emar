-- =============================================
-- Author:		Winston Murdock
-- Create date: 09/12/2022.
-- Description:	Convert from PulseCheck's "PC Routed Gen ID" (which has an R and zeroes
--					appended to the front) to an FDB "Routed Gen ID" (which does not
--					have an R or any zeroes at the front).  PC-27429
--					R00034619 -> 34619
--					R01048580 -> 1048580
--					R36707478 -> 36707478
-- =============================================
CREATE FUNCTION [dbo].[convert_pc_routed_gen_id_to_routed_gen_id]
(
	@pc_routed_gen_id varchar(9)
)
RETURNS numeric
AS
BEGIN

	-- Return variable.
	DECLARE @routed_gen_id  numeric

	-- Place holder for each time we strip the R or a 0 from the parameter value.
	DECLARE @temp varchar(9)

	-- Length of the place holder.
	-- Using this rather than putting the LEN calculation in each line
	-- leads to slightly cleaner code.
	DECLARE @len int

	-- Default the place holder as the parameter value.
	SET @temp = @pc_routed_gen_id
	SET @len = LEN(@pc_routed_gen_id)

	-- Chop the R off the front.
	IF LEFT(@pc_routed_gen_id, 1) = 'R'
	BEGIN
		--It does start with an R.
		-- Set the place holder to everything from character 2 to the end.
		SET @temp = SUBSTRING(@pc_routed_gen_id, 2, @len - 1)
		SET @len = LEN(@temp)
	END

	-- Remove any zeroes from the front of the value.
	-- Loop while the first character is a 0.
	WHILE LEFT(@temp, 1) = '0'
	BEGIN
		-- The first character is a 0.
		-- Set the place holder to everything from character 2 to the end.
		SET @temp = RIGHT(@temp, @len - 1)
		SET @len = LEN(@temp)
	END

	-- Convert from a varchar to a number.
	SET @routed_gen_id = CONVERT(numeric, @temp)
	
	-- Return.
	RETURN @routed_gen_id
END
GO
/***************
 Data Dictionary
    Function
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This function converts from PulseCheck''s "PC Routed Gen ID" (which has an R and
zeroes appended to the front) to an FDB "Routed Gen ID" (which does not have an
R or any zeroes at the front).'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'convert_pc_routed_gen_id_to_routed_gen_id';
go
