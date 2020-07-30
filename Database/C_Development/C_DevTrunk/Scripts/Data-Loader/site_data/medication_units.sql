print 'Loading Table: medication_units';

drop table if exists [#medication_units];

create table [#medication_units]
    (
      [site_id]    [int] not null
    , [code]       [varchar](50) not null
    , [name]       [varchar](50) not null
    , [print_name] [varchar](50) not null
    , [is_active]  [bit] not null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin
        insert into [#medication_units]
            ([site_id]
           , [code]
           , [name]
           , [print_name]
           , [is_active]
            )
        execute ('execute dbo.export_ibex_medication_units');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#medication_units] from '$(current_path)Scripts\Data-Loader\sample_data\medication_units.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#medication_units]
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

        --set identity_insert [dbo].[medication_units] on;
delete [dbo].[medication_units]

        insert into [dbo].[medication_units]
            ([site_id]
           , [code]
           , [name]
           , [print_name]
           , [is_active]
            )
        select isnull([internal_site].[id], -1) as [site_id]
             , [source].[code]
             , [source].[name]
             , [source].[print_name]
             , [source].[is_active]
        from   [#medication_units] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

        --set identity_insert [dbo].[medication_units] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#medication_units];