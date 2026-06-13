print 'create view [ibex].[dbo].[emar_personnel_retrieve_view];';

set @template = N'
create or alter view [dbo].[emar_personnel_retrieve_view]

as
	SELECT
		ibex as [external_patient_id],
		site as [external_site_id],
		ISNULL(doctor,0) as [external_user_id],
		''DOCTOR2'' as [role_name]
	FROM
		pat
	UNION
	SELECT ibex, site, ISNULL(resident,0), ''DOCTOR3'' FROM pat
	UNION
	SELECT ibex, site, ISNULL(drextender,0), ''DOCTOR4'' FROM pat
	UNION
	SELECT ibex, site, ISNULL(primarynurse,0), ''NURSE1'' FROM pat
	UNION
	SELECT ibex, site, ISNULL(extender,0), ''NURSE2'' FROM pat
	UNION
	SELECT ibex, site, ISNULL(firstdoctor,0), ''DOCTOR1'' FROM pat
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
