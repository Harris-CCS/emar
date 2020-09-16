print 'Loading Table: antimicrobial_indications';

drop table if exists [#antimicrobial_indications];

create table [#antimicrobial_indications]
    (
      [site_id]          [varchar](25) not null
    , [code]             [varchar](20) not null
    , [description]      [nvarchar](255) not null
    , [is_active]        [bit] not null
    , [ordinal_position] [int] not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#antimicrobial_indications]
            ([site_id]
           , [code]
           , [description]
           , [is_active]
           , [ordinal_position]
            )
        execute ('execute dbo.export_ibex_antimicrobial_indications');
    end;

if
(
    select count(*)
    from   [#antimicrobial_indications]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#antimicrobial_indications]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[antimicrobial_indications];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#antimicrobial_indications] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[antimicrobial_indications] on;

        insert into [dbo].[antimicrobial_indications]
            ([site_id]
           , [code]
           , [description]
           , [is_active]
           , [ordinal_position]
            )
        select isnull([internal_site].[id], -1) as [site_id]
             , [source].[code]
             , [source].[description]
             , [source].[is_active]
             , [source].[ordinal_position]
        from   [#antimicrobial_indications] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

        -- set identity_insert [dbo].[antimicrobial_indications] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#antimicrobial_indications];