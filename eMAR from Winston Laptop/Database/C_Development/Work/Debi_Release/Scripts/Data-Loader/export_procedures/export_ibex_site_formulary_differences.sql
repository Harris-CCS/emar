print 'create procedure [dbo].[export_ibex_site_formulary_differences];'
drop procedure if exists [dbo].[export_ibex_site_formulary_differences];

set @template = N'
create or alter procedure [dbo].[export_ibex_site_formulary_differences]
as
    begin
		-- This SP uses the EXCEPT operator to pull in the differences
		-- between the PCED formulary and the eMAR formulary.
		-- The existing export_ibex_site_formulary SP was left alone
		-- because we need the full list to know what to delete.
		-- This guy was added to only pull in the differences so that
		-- we only update the differences.
		-- This way we''re not firing off the update trigger for each
		-- row in the table but only the rows that have actually changed.
		-- Winston Murdock, 07/16/2021.  EMAR-1014.


		-- This table is all of the differences between the PCED
		-- formulary and the eMAR formulary.
		-- It uses the EXCEPT operator to accomplish this.
		declare @temp table
		(
			  [site]               [varchar](25)   null
			, [ndc]                [varchar](32)   null
			, [drug_id]            [varchar](32)   null
			, [hospital_drug_code] [varchar](32)   null
			, [service_code]       [varchar](32)   null
			, [is_inpatient]       [bit]           null
			, [is_outpatient]      [bit]           null
			, [is_pyxis]           [bit]           null
		);

		-- This table is everything in the PCED formulary.
		-- We need this because we have to include columns
		-- that are not in the eMAR formulary (source_id,
		-- brand_name, and dateadd).
		declare @temp2 table
		(
			  [source_id]          [varchar](25)   null
			, [site]               [varchar](25)   null
			, [ndc]                [varchar](32)   null
			, [drug_id]            [varchar](32)   null
			, [brand_name]         [nvarchar](255) null
			, [hospital_drug_code] [varchar](32)   null
			, [service_code]       [varchar](32)   null
			, [is_inpatient]       [bit]           null
			, [is_outpatient]      [bit]           null
			, [is_pyxis]           [bit]           null
			, [dateadd]            [varchar](14)   null
		);

		-- Populate the list of differences between the PCED
		-- formulary and the eMAR formulary.
		INSERT into @temp 
		(
			site, ndc, drug_id,
			hospital_drug_code, service_code,
			is_inpatient, is_outpatient, is_pyxis
		)
		SELECT
			formulary.site as ''site''
			, rtrim(ltrim([formulary].[ndc]))                  as [ndc]
			, isnull(cast([ndc].[medid] as varchar(25)), '''') as [drug_id]
			, isnull(rtrim(ltrim([aliencode])), '''')          as [hospital_drug_code]
			, rtrim(ltrim([svc]))                              as [service_code]
			, case
				when [inpat] = ''Y''
						then 1
				else 0
			end                                                as [is_inpatient]
			, case
				when [outpat] = ''Y''
						then 1
				else 0
			end                                                as [is_outpatient]
			, case
				when [pyxis] = ''Y''
						then 1
				else 0
			end                                                as [is_pyxis]
		from [<@export_database_name>].[dbo].[frm] as [formulary]
				left join [<@export_database_name>].[dbo].[fdb_ndc_info] as [ndc]
					on [ndc].[ndc] = [formulary].[ndc]
		where isnull(cast([ndc].[medid] as varchar(25)), '''') <> ''''
		EXCEPT
		SELECT
			ei.external_id as ''site'', sf.ndc,
			m.drug_id as ''drug_id'', sf.hospital_drug_code,
			sf.service_code, sf.is_inpatient,
			sf.is_outpatient, sf.is_pyxis
		from [dbo].[site_formulary] AS sf
		inner join [dbo].[medications] m on sf.medication_id = m.id
		-- May want to convert following join to cross apply [dbo].[get_external_id] (''Pulsecheck'', ''sites'', [sf].[site_id]) as [ei]
		inner join [dbo].[external_ids] ei on sf.site_id = ei.internal_id
		where ei.entity = ''sites''
		and ei.vendor = ''PulseCheck'';

		-- Populate the list of all rows in the PCED formulary.
		INSERT INTO @temp2
		(
			source_id, site, ndc,
			drug_id, brand_name,
			hospital_drug_code,
			service_code, is_inpatient,
			is_outpatient, is_pyxis, [dateadd]
		)
		SELECT
			  [formulary].[frm_id]                             as [source_id]
			, [formulary].[site]
			, rtrim(ltrim([formulary].[ndc]))                  as [ndc]
			, isnull(cast([ndc].[medid] as varchar(25)), '''') as [drug_id]
			, isnull(rtrim(ltrim([brand])), '''')              as [brand_name]
			, isnull(rtrim(ltrim([aliencode])), '''')          as [hospital_drug_code]
			, rtrim(ltrim([svc]))                              as [service_code]
			, case
				when [inpat] = ''Y''
						then 1
				else 0
			end                                                as [is_inpatient]
			, case
				when [outpat] = ''Y''
						then 1
				else 0
			end                                                as [is_outpatient]
			, case
				when [pyxis] = ''Y''
						then 1
				else 0
			end                                                as [is_pyxis]
			, [formulary].[dateadd]
		from [<@export_database_name>].[dbo].[frm] as [formulary]
			left join [<@export_database_name>].[dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [formulary].[ndc]
		where isnull(cast([ndc].[medid] as varchar(25)), '''') <> '''';
		
		-- Now pull all rows from the PCED formulary that are in the list
		-- of differences between the PCED formulary and the eMAR formulary.
		-- Accomplish this by doing an inner join between the two tables on
		-- every column in the list of differences.
		SELECT
			temp2.source_id, temp2.site, temp2.ndc,
			temp2.drug_id, temp2.brand_name,
			temp2.hospital_drug_code,
			temp2.service_code, temp2.is_inpatient,
			temp2.is_outpatient, temp2.is_pyxis, temp2.[dateadd]
		from @temp2 temp2
		inner join @temp temp on
			temp2.[site] = temp.[site]
			and temp2.ndc = temp.ndc
			and temp2.drug_id = temp.drug_id
			and temp2.hospital_drug_code = temp.hospital_drug_code
			and temp2.service_code = temp.service_code
			and temp2.is_inpatient = temp.is_inpatient
			and temp2.is_outpatient = temp.is_outpatient
			and temp2.is_pyxis = temp.is_pyxis;
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;
