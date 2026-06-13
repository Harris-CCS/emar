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
	DECLARE @combo_drug_ids TABLE
	(
		id int identity(1, 1),
		drug_id varchar(32)
	)
	DECLARE @medication_id int
	DECLARE @drug_id varchar(25)

	-- This takes the PCED routed gen id (padded with an R and zeroes) and returns
	-- the FDB routed gen id (with none of the padding).
	SET @routed_gen_id = dbo.convert_pc_routed_gen_id_to_routed_gen_id(@pc_routed_gen_id)

	-- Get the drug_ids for the combo med into a table variable.
	INSERT INTO @combo_drug_ids
	SELECT drug_id
	FROM medication_details
	WHERE medication_id = @combo_medication_id
	
	-- Get the "normal" medication that matches the pc routed gen id.
	-- and where the FDB med id (medication.drug_id) is in the list of
	-- drug ids for the medication details in this combo med.
	-- This insures we get this exact donnatal and not some other donnatal.
	SELECT TOP 1 @medication_id = m.id, @drug_id = m.drug_id
	FROM medications m
		INNER JOIN fdb_ndc_info fni ON m.drug_id = fni.MEDID_string
		inner join @combo_drug_ids cdi on fni.MEDID_string = cdi.drug_id
	WHERE fni.base_ndc = fni.ndc
		AND fni.ROUTED_GEN_ID = @routed_gen_id

	-- Now that we have the full-blown medication for the specific drug in the combo med
	-- that the normal medication interacts with, get the name of the medication detail
	-- for that medication (using the drug id).
	SELECT TOP 1 @ret = md.brand_name
	FROM medication_details md
	WHERE md.drug_id = @drug_id
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