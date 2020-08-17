CREATE PROCEDURE [dbo].[pc_fdb_get_drc_info]
  @ndc [varchar](11) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  
  CREATE TABLE #info (
    GCN_SEQNO INT,
    type_description VARCHAR(200),
    age_description VARCHAR(200),
    weight_description VARCHAR(200),
    amount_low VARCHAR(200),
    amount_high VARCHAR(200),
    unit_dose_abbreviation VARCHAR(200),
    max_frequency VARCHAR(200),
    condition1_description VARCHAR(200),
    renal_description VARCHAR(200),
    route_description VARCHAR(200)     
  );
  
  declare @gcn_seqno numeric(6,0);

  select @gcn_seqno = gcn_seqno
  from dbo.fdb_ndc_info
  where ndc=@ndc;

  SELECT
    GCN_SEQNO,    
    CASE
      WHEN ( CAST (DR2_LOAGED AS INT) = 0 ) THEN 
      CASE 
        WHEN ( CAST (DR2_HIAGED AS INT) = 0 ) THEN ''
        WHEN ( CAST (DR2_HIAGED AS INT) > 0 AND CAST (DR2_HIAGED AS INT) <= 29 ) THEN '<= ' + CAST( CAST (DR2_HIAGED AS INT) AS VARCHAR(200)) + ' day(s)'
        WHEN ( CAST (DR2_HIAGED AS INT) > 29 AND CAST (DR2_HIAGED AS INT) <= 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 29 ) AS VARCHAR(200)) + ' month(s)'
        WHEN ( CAST (DR2_HIAGED AS INT) > 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 364 ) AS VARCHAR(200)) + ' years(s)'
        ELSE '<= ' + CAST( CAST (DR2_HIAGED AS INT) AS VARCHAR(200)) + ' days'  
      END 
      WHEN ( CAST (DR2_LOAGED AS INT) > 0 AND  CAST (DR2_LOAGED AS INT) <= 29 ) THEN '>= ' + CAST( CAST (DR2_LOAGED AS INT) AS VARCHAR(200)) + ' days(s)  and ' +
      CASE 
        WHEN ( CAST (DR2_HIAGED AS INT) > 0 AND CAST (DR2_HIAGED AS INT) <= 29 ) THEN '<= ' + CAST( CAST (DR2_HIAGED AS INT) AS VARCHAR(200)) + ' day(s)'
        WHEN ( CAST (DR2_HIAGED AS INT) > 29 AND CAST (DR2_HIAGED AS INT) <= 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 29 ) AS VARCHAR(200)) + ' month(s)'
        WHEN ( CAST (DR2_HIAGED AS INT) > 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 364 ) AS VARCHAR(200)) + ' years(s)'
        ELSE '<= ' + CAST( CAST (DR2_HIAGED AS INT) AS VARCHAR(200)) + ' days'  
      END 
      WHEN ( CAST (DR2_LOAGED AS INT) > 29 AND  CAST (DR2_LOAGED AS INT) <= 364 ) THEN '>= ' + CAST( ( CAST (DR2_LOAGED AS INT) / 29 ) AS VARCHAR(200)) + ' months(s)  and ' +
      CASE 
        WHEN ( CAST (DR2_HIAGED AS INT) > 29 AND CAST (DR2_HIAGED AS INT) <= 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 29 ) AS VARCHAR(200)) + ' month(s)'
        WHEN ( CAST (DR2_HIAGED AS INT) > 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 364 ) AS VARCHAR(200)) + ' years(s)'
        ELSE '<= ' + CAST( CAST (DR2_HIAGED AS INT) AS VARCHAR(200)) + ' days'  
      END 
      WHEN ( CAST (DR2_LOAGED AS INT) > 364 ) THEN '>= ' + CAST( ( CAST (DR2_LOAGED AS INT) /364 ) AS VARCHAR(200)) + ' years(s)  and ' +
      CASE 
        WHEN ( CAST (DR2_HIAGED AS INT) > 29 AND CAST (DR2_HIAGED AS INT) <= 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 29 ) AS VARCHAR(200)) + ' month(s)'
        WHEN ( CAST (DR2_HIAGED AS INT) > 364 ) THEN '<= ' + CAST( ( CAST (DR2_HIAGED AS INT) / 364 ) AS VARCHAR(200)) + ' years(s)'
        ELSE '<= ' + CAST( CAST (DR2_HIAGED AS INT) AS VARCHAR(200)) + ' days'  
      END 
    END AS age_description,
    CASE
      WHEN ( NEOM_WEIGHT_REQ_IND = 1 ) THEN
      CASE
        WHEN ( NEOM_LOW_CURRENT_WEIGHT_GRAMS = 0 )  THEN '<= ' + CAST ( CAST ( NEOM_HIGH_CURRENT_WEIGHT_GRAMS AS FLOAT ) / 1000 AS VARCHAR(200) ) + ' kg'
        WHEN ( NEOM_HIGH_CURRENT_WEIGHT_GRAMS = 0 ) THEN '>= ' + CAST ( CAST ( NEOM_LOW_CURRENT_WEIGHT_GRAMS  AS FLOAT ) / 1000 AS VARCHAR(200) ) + ' kg'
        ELSE '>= ' + CAST ( CAST ( NEOM_LOW_CURRENT_WEIGHT_GRAMS AS FLOAT ) / 1000 AS VARCHAR(200) ) + ' kg and <= ' + CAST ( CAST ( NEOM_HIGH_CURRENT_WEIGHT_GRAMS AS FLOAT ) / 1000 AS VARCHAR(20) ) + ' kg'
      END
      ELSE CAST ( '' AS VARCHAR(200) )
    END AS weight_description,
    DOSTPI_DES AS condition1_description,
    CASE
      WHEN ( DR2_RENIMP = 'Y' ) THEN CAST ( 'Dose needs to be adjusted for renal impairment' AS VARCHAR(200) )
      ELSE CAST ( '' AS VARCHAR(200) )
    END AS renal_description,
    ROUTES_DES AS route_description,
    
    -- For 'daily range'
    DR2_LODOSD, DR2_HIDOSD, DR2_LODOSU,
    
    -- For 'daily max'
    DR2_MXDOSD, DR2_MXDOSU,
    
    -- For 'single dose'
    DR2_MX1DOS, DR2_HIFREQ
    
  INTO #base
  FROM fdb..RDRCNMA1_MSTR
  INNER JOIN fdb..RDRCDTD0_DOSE_TYPE_DESC ON RDRCDTD0_DOSE_TYPE_DESC.DR2_DOSTPI = RDRCNMA1_MSTR.DR2_DOSTPI
  INNER JOIN fdb..RDRCRTD0_ROUTE_DESC ON RDRCRTD0_ROUTE_DESC.DR2_RT = RDRCNMA1_MSTR.DR2_RT
  WHERE RDRCNMA1_MSTR.GCN_SEQNO = @gcn_seqno
  
  INSERT INTO #info
  SELECT
    GCN_SEQNO,
    'daily range' AS type_description,
    age_description, weight_description,
    CAST (DR2_LODOSD AS float) AS amount_low,
    CAST (DR2_HIDOSD AS float) AS amount_high,
    UNITS_DESC AS unit_dose_abbreviation,
    CAST ( '' AS VARCHAR(200) ) AS max_frequency,   
    condition1_description, renal_description, route_description

  FROM #base
  INNER JOIN fdb..RDRCUND0_UNITS_DESC ON RDRCUND0_UNITS_DESC.DR2_UNITS = #base.DR2_LODOSU
  
  INSERT INTO #info
  SELECT
    GCN_SEQNO,
    'daily max' AS type_description,
    age_description, weight_description,
    '' AS amount_low,
    CAST (DR2_MXDOSD AS float) AS amount_high,
    UNITS_DESC AS unit_dose_abbreviation,
    '' AS max_frequency,
    condition1_description, renal_description, route_description
  FROM #base
  INNER JOIN fdb..RDRCUND0_UNITS_DESC ON RDRCUND0_UNITS_DESC.DR2_UNITS = #base.DR2_MXDOSU  
  
  INSERT INTO #info
  SELECT
    GCN_SEQNO,
    'single dose' AS type_description,
    age_description, weight_description, 
    '' AS amount_low,
    CAST (DR2_MX1DOS AS float) AS amount_high,
    RDRCUND0_UNITS_DESC.UNITS_DESC AS unit_dose_abbreviation,
    CASE
      WHEN ( CAST ( DR2_MX1DOS AS INT ) > 0 ) THEN CAST ( CAST ( DR2_HIFREQ AS FLOAT ) AS VARCHAR(20) ) + ' times a day'
      ELSE ''
    END AS max_frequency,
    condition1_description, renal_description, route_description
  FROM #base
  INNER JOIN fdb..RDRCUND0_UNITS_DESC ON RDRCUND0_UNITS_DESC.DR2_UNITS = #base.DR2_MXDOSU  

  INSERT INTO #info
  SELECT
    GCN_SEQNO,
    'adult daily dose range' AS type_description,
    '>= 18 year(s) and < 65 year(s)' AS age_description,
    '' AS weight_description,
    CASE
      WHEN ( MMAR_MND > 0 ) THEN CAST ( CAST ( MMAR_MND AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_low,
    CASE
      WHEN ( MMAR_MXD > 0 ) THEN CAST ( CAST ( MMAR_MXD AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_high,
    MMAR_MNDU AS unit_dose_abbreviation,
    '' AS max_frequency,
    '' AS condition1_description,
    '' AS renal_description,
    '' AS route_description    
  FROM fdb..RMMARMA0_ADULT_RANGE_MSTR
  WHERE RMMARMA0_ADULT_RANGE_MSTR.GCN_SEQNO = @gcn_seqno
  
  INSERT INTO #info
  SELECT
    GCN_SEQNO,
    'adult single dose range' AS type_description,
    '>= 18 year(s) and < 65 year(s)' AS age_description,
    '' AS weight_description,
    CASE
      WHEN ( MMA_MND > 0 ) THEN CAST ( CAST ( MMA_MND AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_low,
    CASE
      WHEN ( MMA_MXD > 0 ) THEN CAST ( CAST ( MMA_MXD AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_high,
    MMA_MNDU AS unit_dose_abbreviation,
    '' AS max_frequency,
    '' AS condition1_description,
    '' AS renal_description,
    '' AS route_description    
  FROM fdb..RMMADMA1_ADULT_DOSE_MSTR
  WHERE RMMADMA1_ADULT_DOSE_MSTR.GCN_SEQNO = @gcn_seqno
  
  INSERT INTO #info
  SELECT
    GCN_SEQNO,
    'geriatric daily dose range' AS type_description,
    '>= 65 year(s)' AS age_description,
    '' AS weight_description,
    CASE
      WHEN ( MMGR_MND = 0 ) THEN CAST ( CAST ( MMGR_MND AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_low,
    CASE
      WHEN ( MMGR_MXD = 0 ) THEN CAST ( CAST ( MMGR_MXD AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_high,
    MMGR_MNDU AS unit_dose_abbreviation,
    '' AS max_frequency,
    '' AS condition1_description,
    '' AS renal_description,
    '' AS route_description    
  FROM fdb..RMMGRMA1_GERI_RANGE_MSTR
  WHERE RMMGRMA1_GERI_RANGE_MSTR.GCN_SEQNO = @gcn_seqno
  
  INSERT INTO #info
  SELECT
    GCN_SEQNO,
    'geriatric single dose range' AS type_description,
    '>= 65 year(s)' AS age_description,
    '' AS weight_description,
    CASE
      WHEN ( MMG_MND = 0 ) THEN CAST ( CAST ( MMG_MND AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_low,
    CASE
      WHEN ( MMG_MXD = 0 ) THEN CAST ( CAST ( MMG_MXD AS FLOAT ) AS VARCHAR(200) )
      ELSE ''
    END AS amount_high,
    MMG_MNDU AS unit_dose_abbreviation,
    '' AS max_frequency,
    '' AS condition1_description,
    '' AS renal_description,
    '' AS route_description    
  FROM fdb..RMMGDMA1_GERI_DOSE_MSTR
  WHERE RMMGDMA1_GERI_DOSE_MSTR.GCN_SEQNO = @gcn_seqno
  
  SELECT DISTINCT * FROM #info
  ORDER BY type_description, age_description
  
  RETURN;
END;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Pulsecheck procdure used to generate "dose range chacking"'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'pc_fdb_get_drc_info';
go