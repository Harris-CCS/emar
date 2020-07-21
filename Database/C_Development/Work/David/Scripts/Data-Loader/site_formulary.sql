print 'Loading Table: site_formulary';

drop table if exists [#site_formulary];

create table [#site_formulary]
    (
      [site_id]            [varchar](32) null
    , [ndc]                [varchar](32) null
    , [drug_id]            [varchar](32) not null
    , [brand_name]         [nvarchar](255) not null
    , [hospital_drug_code] [varchar](32) null
    , [service_code]       [varchar](32) null
    , [is_inpatient]       [bit] not null
    , [is_outpatient]      [bit] not null
    , [is_pyxis]           [bit] not null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin
        insert into [#site_formulary]
            ([site_id]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [hospital_drug_code]
           , [service_code]
           , [is_inpatient]
           , [is_outpatient]
           , [is_pyxis]
            )
        execute ('execute dbo.export_ibex_site_formulary');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#site_formulary] from '$(current_path)Scripts\Data-Loader\sample_data\site_formulary.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#site_formulary]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#site_formulary]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[site_formulary];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#site_formulary] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        set identity_insert [dbo].[site_formulary] on;

        insert into [dbo].[site_formulary]
            ([id]
           , [site_id]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [hospital_drug_code]
           , [service_code]
           , [is_inpatient]
           , [is_outpatient]
           , [is_pyxis]
            )
        select [source].[target_id]
             , isnull([internal_site].[id], -1) as [site_id]
             , [source].[ndc]
             , [source].[drug_id]
             , [source].[brand_name]
             , [source].[hospital_drug_code]
             , [source].[service_code]
             , [source].[is_inpatient]
             , [source].[is_outpatient]
             , [source].[is_pyxis]
        from   [#site_formulary] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site]
        order by [source].[ndc]
               , [site_id];

        set identity_insert [dbo].[site_formulary] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#site_formulary];