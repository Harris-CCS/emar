print 'Loading Table: fdb_ndc_info';

drop table if exists [#fdb_ndc_info];

create table [#fdb_ndc_info]
    (
      [ndc]           [varchar](11) not null
    , [base_ndc]      [varchar](11) null
    , [repackaged]    [int] not null
    , [medid]         [numeric](8, 0) not null
    , [packaging]     [varchar](26) null
    , [strength]      [varchar](91) null
    , [days_obsolete] [int] null);

/****************************************
        load temporary tables for staging
****************************************/

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#fdb_ndc_info]
            ([ndc]
           , [base_ndc]
           , [repackaged]
           , [medid]
           , [packaging]
           , [strength]
           , [days_obsolete]
            )
        execute ('execute dbo.export_ibex_fdb_ndc_info');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#fdb_ndc_info] from '$(current_path)Scripts\Data-Loader\sample_data\fdb_ndc_info.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#fdb_ndc_info]
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

        insert into [dbo].[fdb_ndc_info]
            ([ndc]
           , [base_ndc]
           , [repackaged]
           , [medid]
           , [packaging]
           , [strength]
           , [days_obsolete]
            )
        select [source].[ndc]
             , [source].[base_ndc]
             , [source].[repackaged]
             , [source].[medid]
             , [source].[packaging]
             , [source].[strength]
             , [source].[days_obsolete]
        from   [#fdb_ndc_info] as [source];

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#fdb_ndc_info];