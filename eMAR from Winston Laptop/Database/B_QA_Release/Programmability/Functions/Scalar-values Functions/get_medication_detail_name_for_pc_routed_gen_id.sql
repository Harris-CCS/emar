-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_medication_detail_name_for_pc_routed_gen_id]
(
	-- Add the parameters for the function here
	@pc_routed_gen_id varchar(9),
	@combo_medication_id int	
)
RETURNS varchar(510)
AS
BEGIN
	DECLARE @ret varchar(510) = ''
	DECLARE @routed_gen_id numeric

	-- This takes the PCED routed gen id (padded with an R and zeroes) and returns
	-- the FDB routed gen id (with none of the padding).
	SET @routed_gen_id = dbo.convert_pc_routed_gen_id_to_routed_gen_id(@pc_routed_gen_id)

	-- Instead of doing multiple selects, do only one select that grabs what we need.
	-- Use inner joins and the where clause to ensure we only get the med name specified for the pc routed gen id.
	SELECT TOP 1 @ret = md.brand_name
	FROM medications m
		INNER JOIN fdb_ndc_info fni ON m.drug_id = fni.MEDID_string
		inner join medication_details md on fni.MEDID_string = md.drug_id
	WHERE fni.base_ndc = fni.ndc
		AND fni.ROUTED_GEN_ID = @routed_gen_id
		AND md.medication_id = @combo_medication_id

	-- Return
	RETURN @ret
END
GO
/***************
 Data Dictionary
    Function
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This function the medication details name for a given PC ROUTED GEN ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'get_medication_detail_name_for_pc_routed_gen_id';
go