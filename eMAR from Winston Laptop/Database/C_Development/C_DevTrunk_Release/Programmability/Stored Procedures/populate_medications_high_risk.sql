
-- ============================================================================
-- Author:		Jim Hoos
-- Create date: 11/10/2021
-- Description:	To populate the medications_high_risk table
-- 
-- Input Params: None
-- Summary:      This stored procedure will load the medications_high_risk
--               table based upon a predetermined list of active ingredients
--               and routes. Fundamentally, uniqueness is defined at the
--               routed_gen_id level though there are duplicate values since
--               all corresponding drugs to the med_id level are included.
-- Future Fix:   Rewrite this to use TVP (table value parameters) and
--               sp_executesql (build query from strings). 
-- ============================================================================

CREATE PROCEDURE [dbo].[populate_medications_high_risk]
AS
BEGIN
  INSERT INTO [emar].[dbo].[medications_high_risk] (long_brand_name,active,routed_gen_id,pc_routed_gen_id,[route],medication_id)
  SELECT DISTINCT bn.long_brand_name,md.active_list as active,bn.routed_gen_id,bn.pc_routed_gen_id,rrd.GCRT_DESC as [route],md.medication_id
  FROM emar..medication_details md
  JOIN emar..fdb_brand_name bn ON md.drug_id=bn.MEDID
  JOIn fdb..RMINDC1_NDC_MEDID rnm ON bn.MEDID=rnm.MEDID
  JOIN fdb..RMIID1_MED rm ON rm.MEDID = rnm.MEDID
  JOIN fdb..RGCNSEQ4_GCNSEQNO_MSTR rgm ON rgm.GCN_SEQNO = rm.GCN_SEQNO
  JOIN fdb..RROUTED3_ROUTE_DESC rrd ON rgm.GCRT = rrd.GCRT
  WHERE bn.ROUTED_GEN_ID IN (
    SELECT DISTINCT fdb_brand_name.ROUTED_GEN_ID 
	FROM fdb..RMINDC1_NDC_MEDID
    JOIN fdb..RMIID1_MED ON RMIID1_MED.MEDID = RMINDC1_NDC_MEDID.MEDID
    JOIN fdb..RGCNSEQ4_GCNSEQNO_MSTR ON RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO = RMIID1_MED.GCN_SEQNO
    JOIN fdb..RROUTED3_ROUTE_DESC ON RGCNSEQ4_GCNSEQNO_MSTR.GCRT = RROUTED3_ROUTE_DESC.GCRT
    JOIN emar..fdb_brand_name ON fdb_brand_name.MEDID = RMIID1_MED.MEDID
    WHERE(
      fdb_brand_name.active like '%insulin%' OR fdb_brand_name.active like '%Heparin%' OR fdb_brand_name.active like '%Digoxin%' OR fdb_brand_name.active like '%alteplase%' OR fdb_brand_name.active like '%Potassium%' OR fdb_brand_name.active like '%sodium chloride 3 [%]%' OR fdb_brand_name.active like '%Enoxaparin%')
	  AND RROUTED3_ROUTE_DESC.GCRT_DESC IN ('INJECTION','SUBCUTANEOUS','INTRAVENOUS','MISCELLANEOUS'))
	ORDER BY bn.long_brand_name
END;
GO
