print 'Loading Table: medication_routes';

drop table if exists [#medication_routes];

create table [#medication_routes]
    (
      [site_id] [varchar](25) not null
    , [name]    [varchar](50) not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin
        insert into [#medication_routes]
            ([site_id]
           , [name]
            )
        execute ('execute dbo.export_ibex_medication_routes');
    end;

if
(
    select count(*)
    from   [#medication_routes]
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

        --set identity_insert [dbo].[medication_routes] on;

        insert into [dbo].[medication_routes]
            ([site_id]
           , [name]
            )
        select isnull([internal_site].[id], -1) as [site_id]
             , [source].[name]
        from   [#medication_routes] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

        --set identity_insert [dbo].[medication_routes] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#medication_routes];