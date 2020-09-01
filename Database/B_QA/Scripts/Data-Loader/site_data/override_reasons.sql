print 'Loading Table: override_reasons';

drop table if exists [#override_reasons];

create table [#override_reasons]
    (
      [site_id]       [varchar](25) not null
    , [is_medication] [bit] not null
    , [description]   [varchar](80) not null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#override_reasons]
            ([site_id]
           , [is_medication]
           , [description]
            )
        execute ('execute dbo.export_ibex_override_reasons');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#override_reasons] from '$(current_path)Scripts\Data-Loader\sample_data\override_reasons.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#override_reasons]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/
/********************************
        get max id for seed value
********************************/
/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[override_reasons] on;

        insert into [dbo].[override_reasons]
            ([site_id]
           , [is_medication]
           , [description]
            )
        select isnull([internal_site].[id], -1) as [site_id]
             , [source].[is_medication]
             , [source].[description]
        from   [#override_reasons] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

        -- set identity_insert [dbo].[override_reasons] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#override_reasons];