CREATE PROCEDURE [dbo].[create_FDB_search]
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements.
  SET NOCOUNT ON;

  IF OBJECT_ID('fdb_brand_name', 'U') IS NOT NULL
    DROP TABLE fdb_brand_name
  IF OBJECT_ID('fdb_allergy_name', 'U') IS NOT NULL
    DROP TABLE fdb_allergy_name
  IF OBJECT_ID('fdb_ndc_info', 'U') IS NOT NULL
    DROP TABLE fdb_ndc_info

  -- get all of the name AND id information, using the tall-man lettering (alt name) WHEN available
  SELECT
    coalesce(RTMMID1_TM_MED.TM_ALT_MEDID_DESC, RMIID1_MED.MED_MEDID_DESC) AS long_name,

    RMINMID1_MED_NAME.MED_NAME_ID,
    coalesce(RTMNMID1_TM_MED_NAME.TM_ALT_MED_NAME_DESC, RMINMID1_MED_NAME.MED_NAME) AS med_name,

    RMIRMID1_ROUTED_MED.ROUTED_MED_ID,
    coalesce(RTMRMID1_TM_ROUTED_MED.TM_ALT_ROUTED_MED_ID_DESC, RMIRMID1_ROUTED_MED.MED_ROUTED_MED_ID_DESC) AS routed_med_name,

    RMIDFID1_ROUTED_DOSE_FORM_MED.ROUTED_DOSAGE_FORM_MED_ID,
    coalesce(RTMDFID1_TM_ROUTED_DF_MED.TM_ALT_ROUTED_DF_MED_ID_DESC,RMIDFID1_ROUTED_DOSE_FORM_MED.MED_ROUTED_DF_MED_ID_DESC) AS routed_dose_form_name,

    RMIID1_MED.MEDID,
    coalesce(RTMMID1_TM_MED.TM_ALT_MEDID_DESC, RMIID1_MED.MED_MEDID_DESC) AS long_brand_name,

    RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.ROUTED_GEN_ID,
    RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO,

    MED_REF_DEA_CD AS dea_schedule,

    RMIID1_MED.MED_STATUS_CD,
    CASE
      WHEN RMIID1_MED.MED_STATUS_CD = 0 THEN 1
      ELSE 0
    END AS erx_search
  INTO
    #drug_info
  FROM
    fdb..RMIID1_MED
    LEFT JOIN fdb..RRTGNGC0_RTD_GEN_GCNSEQNO_LNK ON RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.GCN_SEQNO = RMIID1_MED.GCN_SEQNO
    LEFT JOIN fdb..RGCNSEQ4_GCNSEQNO_MSTR ON RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO = RMIID1_MED.GCN_SEQNO
    LEFT JOIN fdb..RTMMID1_TM_MED ON RTMMID1_TM_MED.MEDID = RMIID1_MED.MEDID
    LEFT JOIN fdb..RMIDFID1_ROUTED_DOSE_FORM_MED ON RMIDFID1_ROUTED_DOSE_FORM_MED.ROUTED_DOSAGE_FORM_MED_ID = RMIID1_MED.ROUTED_DOSAGE_FORM_MED_ID
    LEFT JOIN fdb..RTMDFID1_TM_ROUTED_DF_MED ON RTMDFID1_TM_ROUTED_DF_MED.ROUTED_DOSAGE_FORM_MED_ID = RMIDFID1_ROUTED_DOSE_FORM_MED.ROUTED_DOSAGE_FORM_MED_ID
    LEFT JOIN fdb..RMIRMID1_ROUTED_MED ON RMIRMID1_ROUTED_MED.ROUTED_MED_ID = RMIDFID1_ROUTED_DOSE_FORM_MED.ROUTED_MED_ID
    LEFT JOIN fdb..RTMRMID1_TM_ROUTED_MED ON RTMRMID1_TM_ROUTED_MED.ROUTED_MED_ID = RMIRMID1_ROUTED_MED.ROUTED_MED_ID
    LEFT JOIN fdb..RMINMID1_MED_NAME ON RMINMID1_MED_NAME.MED_NAME_ID = RMIRMID1_ROUTED_MED.MED_NAME_ID
    LEFT JOIN fdb..RTMNMID1_TM_MED_NAME ON RTMNMID1_TM_MED_NAME.MED_NAME_ID = RMINMID1_MED_NAME.MED_NAME_ID
    LEFT JOIN fdb..RMINDC1_NDC_MEDID ON RMINDC1_NDC_MEDID.MEDID = RMIID1_MED.MEDID
  WHERE
    RMIID1_MED.MED_STATUS_CD IN (0,3) AND RMINDC1_NDC_MEDID.NDC IS NOT NULL;

  -- Gather the OTC / Prescription Only ordering identifiers
  SELECT DISTINCT
    RMINDC1_NDC_MEDID.MEDID,
    'R' AS rx_otc
  INTO
    #rx_otc_info
  FROM
    fdb..RNDC14_NDC_MSTR
    LEFT JOIN fdb..RMINDC1_NDC_MEDID ON RMINDC1_NDC_MEDID.NDC = RNDC14_NDC_MSTR.NDC
  WHERE
    RNDC14_NDC_MSTR.HCFA_TYP = 1;

  INSERT INTO #rx_otc_info
  SELECT DISTINCT
    RMINDC1_NDC_MEDID.MEDID,
    'O' AS rx_otc
  FROM
    fdb..RNDC14_NDC_MSTR
    LEFT JOIN fdb..RMINDC1_NDC_MEDID ON RMINDC1_NDC_MEDID.NDC = RNDC14_NDC_MSTR.NDC
  WHERE
    RNDC14_NDC_MSTR.HCFA_TYP = 2 AND RMINDC1_NDC_MEDID.MEDID NOT IN (SELECT MEDID FROM  #rx_otc_info);

  -- Create the active ingredient list for all hicl codes
  CREATE TABLE #active_base (
    hicl numeric(6,0) NULL,
    active VARCHAR(MAX) NULL,
  );
  INSERT INTO #active_base
  SELECT DISTINCT
    HICL_SEQNO,
    STUFF((
      SELECT ' / ' + hd.HIC_DESC
      FROM fdb..RHICL1_HIC_HICLSEQNO_LINK hhl_2
      LEFT JOIN fdb..RHICD5_HIC_DESC hd ON hd.HIC_SEQN = hhl_2.HIC_SEQN
      WHERE hhl_1.HICL_SEQNO=hhl_2.HICL_SEQNO
      ORDER BY hhl_2.HIC_REL_NO
      FOR XML PATH('')
  ), 1, 3, '') as active
  FROM fdb..RHICL1_HIC_HICLSEQNO_LINK hhl_1

  -- Get the counts of med names
  SELECT med_name, COUNT(MED_NAME_ID) AS cnt
  INTO #med_name_counts
  FROM #drug_info
  GROUP BY med_name;

  -- Get the counts of routed med names
  SELECT routed_med_name, COUNT(ROUTED_MED_ID) AS cnt
  INTO #routed_med_name_counts
  FROM #drug_info
  GROUP BY routed_med_name;

  -- Get the counts of routed dose form med names
  SELECT routed_dose_form_name, count(ROUTED_DOSAGE_FORM_MED_ID) AS cnt
  INTO #routed_dose_form_name_counts
  FROM #drug_info
  GROUP BY routed_dose_form_name;

  -- Get the counts of med name/HICL combinations
  SELECT MED_NAME_ID,med_name,HICL_SEQNO, COUNT(hicl_seqno) AS cnt
  INTO #hicl_seqno_med_name_counts
  FROM #drug_info
  GROUP BY HICL_SEQNO, MED_NAME_ID,med_name;

  -- Get the counts of routed med name/HICL combinations
  SELECT MED_NAME_ID,routed_med_name,HICL_SEQNO, COUNT(hicl_seqno) AS cnt
  INTO #hicl_seqno_routed_med_name_counts
  FROM #drug_info
  GROUP BY HICL_SEQNO, MED_NAME_ID,routed_med_name;

  -- Get the counts of routed dose form med name/HICL combinations
  SELECT MED_NAME_ID,routed_dose_form_name,HICL_SEQNO, COUNT(hicl_seqno) AS cnt
  INTO #hicl_seqno_routed_dose_form_name_counts
  FROM #drug_info
  GROUP BY HICL_SEQNO, MED_NAME_ID,routed_dose_form_name;

  -- Get the counts of med name/routed generic ID combinations
  SELECT MED_NAME_ID,med_name,ROUTED_GEN_ID, COUNT(ROUTED_GEN_ID) AS cnt
  INTO #routed_gen_id_med_name_counts
  FROM #drug_info
  GROUP BY ROUTED_GEN_ID, MED_NAME_ID,med_name;

  -- Get the counts of routed med name/routed generic ID combinations
  SELECT MED_NAME_ID,routed_med_name,ROUTED_GEN_ID, COUNT(ROUTED_GEN_ID) AS cnt
  INTO #routed_gen_id_routed_med_name_counts
  FROM #drug_info
  GROUP BY ROUTED_GEN_ID, MED_NAME_ID,routed_med_name;

  -- Get the counts of routed dose form med name/routed generic ID combinations
  SELECT MED_NAME_ID,routed_dose_form_name,ROUTED_GEN_ID, COUNT(ROUTED_GEN_ID) AS cnt
  INTO #routed_gen_id_routed_dose_form_name_counts
  FROM #drug_info
  GROUP BY ROUTED_GEN_ID, MED_NAME_ID,routed_dose_form_name;

  -- Gather the MEDID codes that are in the Durable Medical Equipment categories
  SELECT
    ETC_ID
  INTO #dme_ultimate_parent
  FROM fdb..RETCTBL0_ETC_ID
  WHERE ETC_NAME = 'Medical Supplies and Durable Medical Equipment (DME)'

  SELECT
    RETCMED0_ETC_MEDID.MEDID,
    RETCMED0_ETC_MEDID.ETC_ID
  INTO #dme_medid
  FROM #dme_ultimate_parent
  INNER JOIN fdb..RETCTBL0_ETC_ID ON RETCTBL0_ETC_ID.ETC_ULTIMATE_PARENT_ETC_ID = #dme_ultimate_parent.ETC_ID
  INNER JOIN fdb..RETCMED0_ETC_MEDID ON RETCMED0_ETC_MEDID.ETC_ID = RETCTBL0_ETC_ID.ETC_ID

  -- Build the fdb_brand_name table
  SELECT DISTINCT
    #drug_info.MEDID,
    #drug_info.long_brand_name AS long_brand_name,
    #active_base.active,
    #drug_info.MED_NAME_ID,
    'G' + RIGHT('0000000' + cast(#drug_info.MED_NAME_ID AS VARCHAR), 8) AS 'PC_MED_NAME_ID', -- PulseCheck adds a G AND zero-pads to 8 characters, so do that here to save time later
    #drug_info.ROUTED_GEN_ID,
    'R' + RIGHT('0000000' + cast(#drug_info.ROUTED_GEN_ID AS VARCHAR), 8) AS 'PC_ROUTED_GEN_ID', -- PulseCheck adds a R AND zero-pads to 8 characters, so do that here to save time later
    CASE
      WHEN #dme_medid.ETC_ID IS NOT NULL THEN #med_name_counts.med_name -- DME entries are not dependent on counts
      WHEN #med_name_counts.cnt = #routed_gen_id_med_name_counts.cnt THEN #med_name_counts.med_name -- WHEN we have the same number of med names AS med name/routed gen ID combos, use that name
      WHEN #routed_med_name_counts.cnt = #routed_gen_id_routed_med_name_counts.cnt THEN #routed_med_name_counts.routed_med_name -- WHEN we have the same number of routed med names AS routed med name/routed gen ID combos, use that name
      WHEN #routed_dose_form_name_counts.cnt = #routed_gen_id_routed_dose_form_name_counts.cnt THEN #routed_dose_form_name_counts.routed_dose_form_name -- WHEN we have the same number of routed dose form med names AS routed dose form med name/routed gen ID combos, use that name
      ELSE #drug_info.long_brand_name
    END AS brand_name,
    dea_schedule,
    #rx_otc_info.rx_otc,
    erx_search
  INTO
    fdb_brand_name
  FROM
    #drug_info
    LEFT JOIN #med_name_counts ON #drug_info.med_name = #med_name_counts.med_name
    LEFT JOIN #routed_med_name_counts ON #drug_info.routed_med_name = #routed_med_name_counts.routed_med_name
    LEFT JOIN #routed_dose_form_name_counts ON #drug_info.routed_dose_form_name = #routed_dose_form_name_counts.routed_dose_form_name
    LEFT JOIN #routed_gen_id_med_name_counts ON #drug_info.MED_NAME_ID = #routed_gen_id_med_name_counts.MED_NAME_ID AND #drug_info.ROUTED_GEN_ID = #routed_gen_id_med_name_counts.ROUTED_GEN_ID AND #routed_gen_id_med_name_counts.med_name = #drug_info.med_name
    LEFT JOIN #routed_gen_id_routed_med_name_counts ON #drug_info.MED_NAME_ID = #routed_gen_id_routed_med_name_counts.MED_NAME_ID AND #drug_info.ROUTED_GEN_ID = #routed_gen_id_routed_med_name_counts.ROUTED_GEN_ID AND #routed_gen_id_routed_med_name_counts.routed_med_name = #drug_info.routed_med_name
    LEFT JOIN #routed_gen_id_routed_dose_form_name_counts ON #drug_info.MED_NAME_ID = #routed_gen_id_routed_dose_form_name_counts.MED_NAME_ID AND #drug_info.ROUTED_GEN_ID = #routed_gen_id_routed_dose_form_name_counts.ROUTED_GEN_ID AND #routed_gen_id_routed_dose_form_name_counts.routed_dose_form_name = #drug_info.routed_dose_form_name
    LEFT JOIN #rx_otc_info ON #drug_info.MEDID = #rx_otc_info.MEDID
    LEFT JOIN #dme_medid ON #drug_info.MEDID = #dme_medid.MEDID
    LEFT JOIN #active_base ON #active_base.hicl = #drug_info.HICL_SEQNO;

  SELECT DISTINCT
    #drug_info.MEDID,
    #drug_info.med_name,
    #drug_info.MED_NAME_ID,
    'G' + RIGHT('0000000' + cast(#drug_info.MED_NAME_ID AS VARCHAR), 8) AS 'PC_MED_NAME_ID', -- PulseCheck adds a G AND zero-pads to 8 characters, so do that here to save time later
    #drug_info.HICL_SEQNO,
    'L' + RIGHT('00000' + cast(#drug_info.HICL_SEQNO AS VARCHAR), 6) AS 'PC_HICL_SEQNO',-- PulseCheck adds a L AND zero-pads to 6 characters, so do that here to save time later
    CASE
      WHEN #med_name_counts.cnt = #hicl_seqno_med_name_counts.cnt THEN #med_name_counts.med_name -- WHEN we have the same number of med names AS med name/HICL combos, use that name
      WHEN #routed_med_name_counts.cnt = #hicl_seqno_routed_med_name_counts.cnt THEN #routed_med_name_counts.routed_med_name -- WHEN we have the same number of routed med names AS routed med name/HICL combos, use that name
      WHEN #routed_dose_form_name_counts.cnt = #hicl_seqno_routed_dose_form_name_counts.cnt THEN #routed_dose_form_name_counts.routed_dose_form_name -- WHEN we have the same number of routed dose form med names AS routed dose form med name/HICL combos, use that name
      ELSE #drug_info.long_brand_name
    END AS allergy_name
  INTO
    fdb_allergy_name
  FROM #drug_info
    LEFT JOIN #med_name_counts ON #drug_info.med_name = #med_name_counts.med_name
    LEFT JOIN #routed_med_name_counts ON #drug_info.routed_med_name = #routed_med_name_counts.routed_med_name
    LEFT JOIN #routed_dose_form_name_counts ON #drug_info.routed_dose_form_name = #routed_dose_form_name_counts.routed_dose_form_name
    LEFT JOIN #hicl_seqno_med_name_counts ON #drug_info.MED_NAME_ID = #hicl_seqno_med_name_counts.MED_NAME_ID AND #drug_info.HICL_SEQNO = #hicl_seqno_med_name_counts.HICL_SEQNO AND #hicl_seqno_med_name_counts.med_name = #drug_info.med_name
    LEFT JOIN #hicl_seqno_routed_med_name_counts ON #drug_info.MED_NAME_ID = #hicl_seqno_routed_med_name_counts.MED_NAME_ID AND #drug_info.HICL_SEQNO = #hicl_seqno_routed_med_name_counts.HICL_SEQNO AND #hicl_seqno_routed_med_name_counts.routed_med_name = #drug_info.routed_med_name
    LEFT JOIN #hicl_seqno_routed_dose_form_name_counts ON #drug_info.MED_NAME_ID = #hicl_seqno_routed_dose_form_name_counts.MED_NAME_ID AND #drug_info.HICL_SEQNO = #hicl_seqno_routed_dose_form_name_counts.HICL_SEQNO AND #hicl_seqno_routed_dose_form_name_counts.routed_dose_form_name = #drug_info.routed_dose_form_name;

  -- fdb_allergy_name indexes
  CREATE CLUSTERED INDEX [ClusteredIndex-20140611-084822] ON [dbo].[fdb_allergy_name]
  (
    [MEDID] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140611-102020] ON [dbo].[fdb_allergy_name]
  (
    [MED_NAME_ID] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140611-103242] ON [dbo].[fdb_allergy_name]
  (
    [med_name] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140611-103253] ON [dbo].[fdb_allergy_name]
  (
    [allergy_name] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  -- fdb_brand_name indexes
  CREATE CLUSTERED INDEX [ClusteredIndex-20140611-085119] ON [dbo].[fdb_brand_name]
  (
    [MEDID] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140611-101732] ON [dbo].[fdb_brand_name]
  (
    [PC_ROUTED_GEN_ID] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140611-101716] ON [dbo].[fdb_brand_name]
  (
    [brand_name] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  -- Create a table with info generated from other tables info for improved performance
  SELECT
    -- fields associated with the NDC that are to be stored
    RMINDC1_NDC_MEDID.NDC,
    case
      when RNDC14_NDC_MSTR.REPACK = 1
        then 1
      else 0
    end AS repackaged,
    case
      when (RNDC14_NDC_MSTR.PD = 'BAG' OR  RNDC14_NDC_MSTR.PD = 'SYRINGE') AND RNDC14_NDC_MSTR.DF = 2 AND RNDC14_NDC_MSTR.PS > 0
        then cast(cast(cast(RNDC14_NDC_MSTR.PS as DECIMAL(9,3)) as float) as varchar(20)) + ' mL(s)'
      when ( RNDC14_NDC_MSTR.PD = 'SYRINGE' OR RNDC14_NDC_MSTR.PD = 'BOX' ) AND RNDC14_NDC_MSTR.DF = 1 AND RNDC14_NDC_MSTR.SYR_CPCTY > 0
        then cast(cast(cast(RNDC14_NDC_MSTR.SYR_CPCTY as DECIMAL(9,3)) as float) as varchar(20)) + ' mL(s)'
      else ''
    end AS packaging,
    case
      when (RNDC14_NDC_MSTR.PD = 'BAG' OR  RNDC14_NDC_MSTR.PD = 'SYRINGE') AND RNDC14_NDC_MSTR.DF = 2 AND RNDC14_NDC_MSTR.PS > 0
        then RGCNSEQ4_GCNSEQNO_MSTR.STR60 + ' : [' + cast(cast(cast(RNDC14_NDC_MSTR.PS as DECIMAL(9,3)) as float) as varchar(20)) + ' mL(s)]'
      when ( RNDC14_NDC_MSTR.PD = 'SYRINGE' OR RNDC14_NDC_MSTR.PD = 'BOX' ) AND RNDC14_NDC_MSTR.DF = 1 AND RNDC14_NDC_MSTR.SYR_CPCTY > 0
        then RGCNSEQ4_GCNSEQNO_MSTR.STR60 + ' : [' + cast(cast(cast(RNDC14_NDC_MSTR.SYR_CPCTY as DECIMAL(9,3)) as float) as varchar(20)) + ' mL(s)]'
      else RGCNSEQ4_GCNSEQNO_MSTR.STR60
    end AS strength,

    -- fields used to identify the NDC to identidy as the base NDC
    RMINDC1_NDC_MEDID.MEDID,
    case
      when DateDiff(day,RNDC14_NDC_MSTR.OBSDTEC,GETDATE()) > 0 then DateDiff(day,RNDC14_NDC_MSTR.OBSDTEC,GETDATE())
      when RNDC14_NDC_MSTR.PD IS NULL then 999999999
      else 0
    end AS days_obsolete,
    RMIID1_MED.GCN_SEQNO,
    RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO,
    RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.ROUTED_GEN_ID
  INTO
    #base
  FROM
    fdb..RMINDC1_NDC_MEDID
    LEFT JOIN fdb..RNDC14_NDC_MSTR ON RNDC14_NDC_MSTR.NDC = RMINDC1_NDC_MEDID.NDC
    LEFT JOIN fdb..RMIID1_MED ON RMIID1_MED.MEDID = RMINDC1_NDC_MEDID.MEDID
    LEFT JOIN fdb..RGCNSEQ4_GCNSEQNO_MSTR ON RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO = RMIID1_MED.GCN_SEQNO
    LEFT JOIN fdb.dbo.RRTGNGC0_RTD_GEN_GCNSEQNO_LNK ON RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.GCN_SEQNO = RMIID1_MED.GCN_SEQNO
  WHERE
    RMIID1_MED.MED_STATUS_CD IN (0,3) AND RMINDC1_NDC_MEDID.NDC IS NOT NULL;


  -- Make a copy with info needed for 'base_ndc' creation
  SELECT
    MEDID,
    packaging,
    NDC as ndc,
    repackaged,
    days_obsolete
  INTO #sorter
  FROM #base

  -- Create a table with info generated from other info for improved performance
  SELECT DISTINCT
    NDC as ndc,
    ( SELECT TOP 1 ndc FROM #sorter WHERE MEDID = #base.MEDID AND packaging = #base.packaging ORDER BY repackaged, days_obsolete, ndc ) AS base_ndc,
    repackaged,
    MEDID as medid,
    packaging,
    strength,
    days_obsolete,
    GCN_SEQNO,
    HICL_SEQNO,
    ROUTED_GEN_ID
  INTO fdb_ndc_info
  FROM #base
  ORDER BY base_ndc, ndc, days_obsolete, repackaged


  CREATE CLUSTERED INDEX [ndc-base_ndc] ON [dbo].[fdb_ndc_info]
  (
      [ndc] ASC,
      [base_ndc] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  CREATE NONCLUSTERED INDEX [ndc] ON [dbo].[fdb_ndc_info]
  (
      [ndc] ASC
  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

  -- drop all of the temp tables
  DROP TABLE #drug_info;
  DROP TABLE #med_name_counts;
  DROP TABLE #routed_med_name_counts;
  DROP TABLE #routed_dose_form_name_counts;
  DROP TABLE #hicl_seqno_med_name_counts;
  DROP TABLE #hicl_seqno_routed_med_name_counts;
  DROP TABLE #hicl_seqno_routed_dose_form_name_counts;
  DROP TABLE #routed_gen_id_med_name_counts;
  DROP TABLE #routed_gen_id_routed_med_name_counts;
  DROP TABLE #routed_gen_id_routed_dose_form_name_counts;
  DROP TABLE #rx_otc_info;
  DROP TABLE #dme_medid;
  DROP TABLE #dme_ultimate_parent;
  DROP TABLE #sorter;
  DROP TABLE #base;

END;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Pulsecheck procdure used to create 3 fdb tables
  fdb_allergy
  fdb_brand_name
  fdb_ndc_info
this was modified to add 3 new columns to fdb_ndc_info (GCN_SEQNO,HICL_SEQNO,ROUTED_GEN_ID)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'create_FDB_search';
go