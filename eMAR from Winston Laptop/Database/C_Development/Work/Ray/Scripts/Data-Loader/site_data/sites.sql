print 'Loading Table: sites';

drop table if exists [#sites];

create table [#sites]
    (
      [source_id]      [varchar](40) not null
    , [name]           [varchar](40) not null
    , [is_active]      [bit] not null
    , [time_zone_name] [sysname] not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#sites]
            ([source_id]
           , [name]
           , [is_active]
           , [time_zone_name]
            )
        execute ('execute dbo.export_ibex_sites');
    end;

if
(
    select count(*)
    from   [#sites]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#sites]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[sites];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#sites] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        set identity_insert [dbo].[sites] on;

        insert into [dbo].[sites]
            ([id]
           , [name]
           , [is_active]
           , [time_zone_name]
            )
        select [source].[target_id]
             , [source].[name]
             , [source].[is_active]
             , [source].[time_zone_name]
        from   [#sites] as [source]
        order by [name];

        set identity_insert [dbo].[sites] off;

/***************************************
        loading [external_ids] reference
***************************************/

        insert into [dbo].[external_ids]
            ([internal_id]
           , [vendor]
           , [entity]
           , [external_id]
            )
        select [source].[target_id]
             , 'pulsecheck'
             , 'sites'
             , [source].[source_id]
        from   [#sites] as [source];

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#sites];