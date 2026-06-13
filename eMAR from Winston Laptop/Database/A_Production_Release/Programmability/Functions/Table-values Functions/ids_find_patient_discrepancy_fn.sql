CREATE FUNCTION dbo.ids_find_patient_discrepancy_fn()
RETURNS @discrepancy TABLE 
(
	-- columns returned by the function
	ExternalPatientId varchar(40) NULL,
	EmarPatientId bigint,
	Discrepancies varchar(500) NOT NULL
)
AS
-- body of the function
BEGIN
	/**** FINAL QUERY ****/
	-- Probably want to lower in the future
	DECLARE @DaysToKeepLookingAtHst int = 30;

	DECLARE	@AllPats TABLE (
		patient_id bigint NULL, 
		ibex char(14) NULL, 
		site tinyint NULL,
		-- 1 Active, 0 Inactive, NULL NotExists
		StatusInIbex bit NULL, 
		StatusInEmar bit NULL);  

	DECLARE	@BlackList TABLE (
		ExternalPatientId varchar(40) NOT NULL,
		EmarPatientId bigint NOT NULL,
		Discrepancies varchar(500) NOT NULL
	)
	INSERT	@BlackList
	SELECT	ExternalPatientId, EmarPatientId, Discrepancies
	FROM	dbo.ids_discrepancy_retries
	WHERE	RetryCount >= 5;

	WITH IbexActivePats AS (
		-- Active Ibex patients mapped to [external_ids]
		SELECT	ex.internal_id as patient_id, ip.ibex, ip.site
		FROM	ibex.dbo.pat ip
		LEFT JOIN dbo.external_ids ex	
			ON CONCAT(ip.site, '|', ip.ibex) = ex.external_id
			AND ex.entity = 'patients'
			AND ex.vendor = 'PulseCheck'
		WHERE	ip.emar_pat = 'Y'
	)
	, IbexRecentlyDepartedPats AS (
		-- Ibex Patients departed in the last 30 days mapped to [external_ids]
		SELECT	ex.internal_id as patient_id, ip.ibex, ip.site
		FROM	ibex.dbo.hst ip
		LEFT JOIN dbo.external_ids ex
			ON CONCAT(ip.site, '|', ip.ibex) = ex.external_id
			AND ex.entity = 'patients'
			AND ex.vendor = 'PulseCheck'
		WHERE	DATEDIFF(day, ip.exitdate_sdt, GETDATE()) < @DaysToKeepLookingAtHst
		AND		ip.emar_pat = 'Y'
	)
	, IbexPats AS (
		SELECT	*, 1 AS StatusInIbex FROM IbexActivePats
		UNION
		SELECT	*, 0 FROM IbexRecentlyDepartedPats
	)
	INSERT	@AllPats
	SELECT	*, NULL
	FROM	IbexPats;

	WITH EmarActivePats AS (
		-- LEFT JOINing to [external_ids] in case we have an orphan active record - want to deactivate it
		SELECT	p.id as patient_id, 
			SUBSTRING(ei.external_id, CHARINDEX('|', ei.external_id) + 1, 30) as ibex, 
			CONVERT(tinyint, SUBSTRING(ei.external_id, 1, CHARINDEX('|', ei.external_id) - 1)) as site,
			p.is_active
		FROM	patients p
		LEFT JOIN external_ids ei
			ON p.id = ei.internal_id
			AND ei.entity = 'patients'
		WHERE	p.is_active = 1
	)
	MERGE INTO @AllPats tar
	USING EmarActivePats as src
		ON tar.ibex = src.ibex
	WHEN MATCHED THEN
		UPDATE SET StatusInEmar = is_active
	WHEN NOT MATCHED THEN
		INSERT (patient_id, ibex, site, StatusInEmar)
		VALUES (patient_id, ibex, site, is_active);

	WITH IbexActiveEmarNot AS (
		SELECT	ap.*
		FROM	@AllPats ap
		WHERE	ISNULL(StatusInIbex, 0) = 1
		AND		ISNULL(StatusInEmar, 0) = 0
	)
	, EmarActiveIbexNot AS (
		SELECT	ap.*
		FROM	@AllPats ap
		WHERE	ISNULL(StatusInIbex, 0) = 0
		AND		ISNULL(StatusInEmar, 0) = 1
	)
	, IbexAllergies AS (
		SELECT	ap.ibex, a.internal_key
		FROM	@AllPats ap
		CROSS APPLY ibex.dbo.emar_patient_allergies_retrieve_fn(ap.ibex) a
		WHERE	ap.StatusInIbex IS NOT NULL
	)
	, EmarAllergies AS ( 
		SELECT	ap.patient_id, pa.internal_key
		FROM	@AllPats ap
		JOIN	patient_allergies pa
			ON ap.patient_id = pa.patient_id
	)
	, AllergiesInIbexNotEmar AS (
		SELECT	i.ibex, COUNT(*) cnt
		FROM	IbexAllergies i
		JOIN	@AllPats ap
			ON i.ibex = ap.ibex
		LEFT JOIN EmarAllergies e
			ON ap.patient_id = e.patient_id
			AND i.internal_key = e.internal_key
		WHERE	e.patient_id IS NULL
		GROUP BY i.ibex
	)
	, AllergiesInEmarNotIbex AS (
		SELECT	e.patient_id, COUNT(*) cnt
		FROM	EmarAllergies e
		JOIN	@AllPats ap
			ON e.patient_id = ap.patient_id
			AND ap.StatusInIbex IS NOT NULL
		LEFT JOIN IbexAllergies i
			ON ap.ibex = i.ibex
			AND i.internal_key = e.internal_key
		WHERE i.ibex IS NULL
		GROUP BY e.patient_id
	)
	, IbexHomeMeds AS (
		SELECT	ap.ibex, a.internal_key
		FROM	@AllPats ap
		CROSS APPLY ibex.dbo.emar_patient_medications_retrieve_fn(ap.ibex) a
		WHERE	ap.StatusInIbex IS NOT NULL
	)
	, EmarHomeMeds AS (
		SELECT	ap.patient_id, pa.internal_key
		FROM	@AllPats ap
		JOIN	patient_home_medications pa
			ON ap.patient_id = pa.patient_id
	)
	, HomeMedsInIbexNotEmar AS (
		SELECT	i.ibex, COUNT(*) cnt
		FROM	IbexHomeMeds i
		JOIN	@AllPats ap
			ON i.ibex = ap.ibex
		LEFT JOIN EmarHomeMeds e
			ON ap.patient_id = e.patient_id
			AND i.internal_key = e.internal_key
		WHERE	e.patient_id IS NULL
		GROUP BY i.ibex
	)
	, HomeMedsInEmarNotIbex AS (
		SELECT	e.patient_id, COUNT(*) cnt
		FROM	EmarHomeMeds e
		JOIN	@AllPats ap
			ON e.patient_id = ap.patient_id
			AND ap.StatusInIbex IS NOT NULL
		LEFT JOIN IbexHomeMeds i
			ON ap.ibex = i.ibex
			AND i.internal_key = e.internal_key
		WHERE i.ibex IS NULL
		GROUP BY e.patient_id
	)
	INSERT @discrepancy
	SELECT	TOP 1 ExternalPatientId = CASE WHEN ap.ibex IS NULL THEN '' ELSE CONCAT(ap.site, '|', ap.ibex) END
		,EmarPatientId = ISNULL(ap.patient_id, -1)
		,Discrepancies = 
			CONCAT(			
				CASE
					WHEN iNotE.ibex IS NOT NULL THEN 'Active in Ibex but not Emar.  '
					WHEN eNotI.patient_id IS NOT NULL THEN 'Active in Emar but not Ibex.  '
					ELSE ''
				END, '//', 
				CASE ISNULL(AinInotE.cnt, 0) + ISNULL(AinEnotI.cnt, 0)
					WHEN 0 THEN ''
						ELSE 
							CONCAT(
								CASE ISNULL(AinInotE.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(AinInotE.cnt, ' Allergies exist in Ibex but not Emar.  ')
								END,
								CASE ISNULL(AinEnotI.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(AinEnotI.cnt, ' Allergies exist in Emar but not Ibex.')
								END
							)
				END, '//', 
				CASE ISNULL(MinInotE.cnt, 0) + ISNULL(MinEnotI.cnt, 0)
					WHEN 0 THEN ''
						ELSE 
							CONCAT(
								CASE ISNULL(MinInotE.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(MinInotE.cnt, ' Home Medications exist in Ibex but not Emar.  ')
								END,
								CASE ISNULL(MinEnotI.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(MinEnotI.cnt, ' Home Medications exist in Emar but not Ibex.')
								END
							)
				END)
	FROM  @AllPats ap
	LEFT JOIN IbexActiveEmarNot iNotE
		ON ap.ibex = iNotE.ibex
	LEFT JOIN EmarActiveIbexNot eNotI
		ON ap.patient_id = eNotI.patient_id
	LEFT JOIN AllergiesInIbexNotEmar AinInotE
		ON ap.ibex = AinInotE.ibex
	LEFT JOIN AllergiesInEmarNotIbex AinEnotI
		ON ap.patient_id = AinEnotI.patient_id
	LEFT JOIN HomeMedsInIbexNotEmar MinInotE
		ON ap.ibex = MinInotE.ibex
	LEFT JOIN HomeMedsInEmarNotIbex MinEnotI
		ON ap.patient_id = MinEnotI.patient_id
	LEFT JOIN @BlackList dl
		ON dl.ExternalPatientId = CASE WHEN ap.ibex IS NULL THEN '' ELSE CONCAT(ap.site, '|', ap.ibex) END
		AND dl.EmarPatientId = ISNULL(ap.patient_id, -1)
		AND dl.Discrepancies = 
			CONCAT(			
				CASE
					WHEN iNotE.ibex IS NOT NULL THEN 'Active in Ibex but not Emar.  '
					WHEN eNotI.patient_id IS NOT NULL THEN 'Active in Emar but not Ibex.  '
					ELSE ''
				END, '//', 
				CASE ISNULL(AinInotE.cnt, 0) + ISNULL(AinEnotI.cnt, 0)
					WHEN 0 THEN ''
						ELSE 
							CONCAT(
								CASE ISNULL(AinInotE.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(AinInotE.cnt, ' Allergies exist in Ibex but not Emar.  ')
								END,
								CASE ISNULL(AinEnotI.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(AinEnotI.cnt, ' Allergies exist in Emar but not Ibex.')
								END
							)
				END, '//', 
				CASE ISNULL(MinInotE.cnt, 0) + ISNULL(MinEnotI.cnt, 0)
					WHEN 0 THEN ''
						ELSE 
							CONCAT(
								CASE ISNULL(MinInotE.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(MinInotE.cnt, ' Home Medications exist in Ibex but not Emar.  ')
								END,
								CASE ISNULL(MinEnotI.cnt, 0)
									WHEN 0 THEN ''
									ELSE CONCAT(MinEnotI.cnt, ' Home Medications exist in Emar but not Ibex.')
								END
							)
				END)
	WHERE	dl.EmarPatientId IS NULL
	AND		(iNotE.ibex IS NOT NULL
		OR		eNotI.patient_id IS NOT NULL
		OR		AinInotE.ibex IS NOT NULL
		OR		AinEnotI.patient_id IS NOT NULL
		OR		MinInotE.ibex IS NOT NULL
		OR		MinEnotI.patient_id IS NOT NULL)
	ORDER BY ap.ibex

	RETURN
END
go

-- Data Dictionary
--    Procedure

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure to find the ids patient discrepancy'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'ids_find_patient_discrepancy_fn';
go