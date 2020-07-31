print 'Loading Table: site_code_shares';

drop table if exists [#site_code_shares];

create table [#site_code_shares]
    (
      [source_site_id] [int] not null
    , [target_site_id] [int] not null
    , [entity]         sysname not null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin
        insert into [#site_code_shares]
            ([source_site_id]
           , [target_site_id]
           , [entity]        
            )
        execute ('execute dbo.export_ibex_site_code_shares');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#site_code_shares] from '$(current_path)Scripts\Data-Loader\sample_data\site_code_shares.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#site_code_shares]
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

        --set identity_insert [dbo].[site_code_shares] on;

        insert into [dbo].[site_code_shares]
            ([source_site_id]
           , [target_site_id]
           , [entity]        
            )
        select isnull([source_site].[id], -1) as [source_site_id]
             , isnull([target_site].[id], -1) as [target_site_id]
             , [source].[entity]
        from   [#site_code_shares] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[source_site_id]) as [source_site]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[target_site_id]) as [target_site];

        --set identity_insert [dbo].[site_code_shares] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#site_code_shares];