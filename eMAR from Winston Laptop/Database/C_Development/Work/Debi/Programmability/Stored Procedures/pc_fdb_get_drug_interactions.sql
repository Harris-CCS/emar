CREATE PROCEDURE [dbo].[pc_fdb_get_drug_interactions]
  @drugs  VARCHAR(MAX)    = NULL
AS
BEGIN
  SET NOCOUNT ON;
  
  SELECT DISTINCT Item
  INTO #drug_list
  FROM dbo.delimited_split_8k(@drugs, ',')
  
  SELECT ROUTED_GEN_ID AS drug_1_id, DDI_CODEX as int_id
  INTO #drug_1
  FROM fdb..RDDIMRG0_ROUTED_GEN_LINK
  WHERE RDDIMRG0_ROUTED_GEN_LINK.ROUTED_GEN_ID in (select Item from #drug_list)
  
  SELECT ROUTED_GEN_ID AS drug_2_id, DDI_CODEX as ddi_2
  INTO #drug_2
  FROM fdb..RDDIMRG0_ROUTED_GEN_LINK
  WHERE RDDIMRG0_ROUTED_GEN_LINK.ROUTED_GEN_ID in (select Item from #drug_list)
  
  SELECT * 
  INTO #int
  FROM #drug_1
  LEFT JOIN #drug_2 ON #drug_2.ddi_2 = ( 32000 - #drug_1.int_id )
  WHERE #drug_2.ddi_2 = ( 32000 - #drug_1.int_id )
  
  -- The returned 'sev' value (DDI_SEL) requires mapping to values used within PulseCheck
  SELECT
    'R' + RIGHT('0000000' + cast(drug_1_id as varchar), 8) AS drug_id_1,
    'R' + RIGHT('0000000' + cast(drug_2_id as varchar), 8) AS drug_id_2,
    int_id,
    CASE 
      WHEN DDI_SL = 9 THEN 5
      WHEN DDI_SL = 3 THEN 6
      WHEN DDI_SL = 2 THEN 7
      WHEN DDI_SL = 1 THEN 8
    END as severity_id
  FROM #int
  LEFT JOIN fdb..RADIMMA5_MSTR ON RADIMMA5_MSTR.DDI_CODEX = #int.int_id
  
  RETURN;
END;
GO
