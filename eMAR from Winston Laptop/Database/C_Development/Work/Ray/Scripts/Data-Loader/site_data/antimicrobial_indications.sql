print 'Loading Table: antimicrobial_indications';

drop table if exists [#antimicrobial_indications];

create table [#antimicrobial_indications]
    (
      [source_id]        [varchar](25) null
    , [site]             [varchar](25) not null
    , [code]             [varchar](10) null
    , [description]      [nvarchar](255) null
    , [status]           [char](1) null
    , [position]         [varchar](25) null
    , [is_active]        [bit] null
    , [site_id]          [int] null
    , [ordinal_position] [int] null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#antimicrobial_indications]
            ([source_id]
           , [site]
           , [code]
           , [description]
           , [status]
           , [position]
            )
        execute ('execute dbo.export_ibex_antimicrobial_indications');
    end;

if(
  select count(*)
  from [#antimicrobial_indications]) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        update [source] set    
            [source_id] = [source].[site] + '|' + [source].[source_id]
          , [is_active] = case [status]
                              when 'A'
                                  then 1
                              else 0
                          end
          , [ordinal_position] = case
                                     when isnumeric([position]) = 1
                                         then [position]
                                     else 0
                                 end
        from   [#antimicrobial_indications] as [source];

        update [source] set    
            [site_id] = [internal_site].[id]
        from   [#antimicrobial_indications] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site]) as [internal_site];

        alter table [#antimicrobial_indications]
        add [id]        [int] identity(1, 1)
          , [target_id] [int];

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

        set identity_insert [dbo].[antimicrobial_indications] on;

        insert into [dbo].[antimicrobial_indications]
            ([id]
           , [site_id]
           , [code]
           , [description]
           , [is_active]
           , [ordinal_position]
            )
        select [target_id]
             , [site_id]
             , [code]
             , [description]
             , [is_active]
             , [ordinal_position]
        from   [#antimicrobial_indications] as [source]
        where  [source].[site_id] > 0;

        set identity_insert [dbo].[antimicrobial_indications] off;

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
             , 'antimicrobial_indications'
             , [source].[source_id]
        from   [#antimicrobial_indications] as [source];

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#antimicrobial_indications];