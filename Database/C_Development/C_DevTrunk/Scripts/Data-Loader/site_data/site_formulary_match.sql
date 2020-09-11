print 'Loading Table: site_formulary_match';

drop table if exists [#site_formulary_match];

create table [#site_formulary_match]
    (
      [site_id]          [varchar](32) null
    , [ndc]              [varchar](32) null
    , [drug_id]          [varchar](32) not null
    , [brand_name]       [nvarchar](255) not null
    , [inpatient_match]  [tinyint] not null
    , [outpatient_match] [tinyint] not null
    , [pyxis_match]      [tinyint] not null
    , [medication_id]         [int] null default 0);    

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin
        insert into [#site_formulary_match]
            ([site_id]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [inpatient_match]
           , [outpatient_match]
           , [pyxis_match]
            )
        execute ('execute dbo.export_ibex_site_formulary_match');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#site_formulary_match] from '$(current_path)Scripts\Data-Loader\sample_data\site_formulary_match.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#site_formulary_match]
) > 0
    begin

        begin transaction;

        truncate table [#medication_items];
        insert into [#medication_items]
            ([ndc]
           , [drug_id]
           , [brand_name]
            )
        select distinct 
               isnull([ndc],'')
             , isnull([drug_id],'')
             , isnull([brand_name],'')
        from   [#site_formulary_match];

        --set medication id's
        execute [dbo].[update_medication_id_list];

        update [target] set    
            [medication_id] = [source].[medication_id]
        from   [#medication_items] [source]
               inner join [#site_formulary_match] [target] on [source].[ndc] = [target].[ndc]
                                                          and [source].[brand_name] = [target].[brand_name]
                                                          and [source].[drug_id] = [target].[drug_id]
        where  [source].[medication_id] > 0;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#site_formulary_match]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[site_formulary_match];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#site_formulary_match] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        set identity_insert [dbo].[site_formulary_match] on;

        insert into [dbo].[site_formulary_match]
            ([id]
           , [site_id]
           , [inpatient_match]
           , [outpatient_match]
           , [pyxis_match]
           , [medication_id]
            )
        select [source].[target_id]
             , isnull([internal_site].[id], -1) as [site_id]
             , [source].[inpatient_match]
             , [source].[outpatient_match]
             , [source].[pyxis_match]
             , [source].[medication_id]	     
        from   [#site_formulary_match] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site]
        where  [source].[medication_id] > 0
        order by [source].[ndc]
               , [site_id];

        set identity_insert [dbo].[site_formulary_match] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#site_formulary_match];