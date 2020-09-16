print 'Loading Table: antimicrobial_indication_items';

drop table if exists [#antimicrobial_indication_items];

create table [#antimicrobial_indication_items]
    (
      [site_id]      [varchar](25) not null
    , [sub_category] [varchar](25) not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
))
    begin

        insert into [#antimicrobial_indication_items]
            ([site_id]
           , [sub_category]
            )
        execute ('execute dbo.export_ibex_antimicrobial_indication_items');
    end;

if
(
    select count(*)
    from   [#antimicrobial_indication_items]
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

        -- set identity_insert [dbo].[antimicrobial_indication_items] on;

        insert into [dbo].[antimicrobial_indication_items]
            ([site_id]
           , [sub_category]
            )
        select isnull([internal_site].[id], -1) as [site_id]
             , [source].[sub_category]
        from   [#antimicrobial_indication_items] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

        -- set identity_insert [dbo].[antimicrobial_indication_items] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#antimicrobial_indication_items];