print 'Loading Table: fdb_brand_name';

drop table if exists [#fdb_brand_name];

create table [#fdb_brand_name]
    (
      [MEDID]            [numeric](8, 0) not null
    , [long_brand_name]  [varchar](70) null
    , [active]           [varchar](max) null
    , [MED_NAME_ID]      [numeric](8, 0) null
    , [PC_MED_NAME_ID]   [varchar](9) null
    , [ROUTED_GEN_ID]    [numeric](8, 0) null
    , [PC_ROUTED_GEN_ID] [varchar](9) null
    , [brand_name]       [varchar](70) null
    , [dea_schedule]     [varchar](1) not null
    , [rx_otc]           [varchar](1) null
    , [erx_search]       [int] not null);

/****************************************
        load temporary tables for staging
****************************************/

--if '$(load_data)' = 'live'
--   and exists
--(
--    select null
--    from   [master].[sys].[databases]
--    where  [name] = 'ibex'  
--)
--    begin

--        insert into [#fdb_brand_name]
--            ([MEDID]
--           , [long_brand_name]
--           , [active]
--           , [MED_NAME_ID]
--           , [PC_MED_NAME_ID]
--           , [ROUTED_GEN_ID]
--           , [PC_ROUTED_GEN_ID]
--           , [brand_name]
--           , [dea_schedule]
--           , [rx_otc]
--           , [erx_search]
--            )
--        execute ('execute dbo.export_ibex_fdb_brand_name');
--    end;

if '$(load_data)' = 'live'
or '$(load_data)' = 'sample'
    begin

        bulk insert [#fdb_brand_name] from '$(current_path)Scripts\Data-Loader\sample_data\fdb_brand_name.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#fdb_brand_name]
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

        insert into [dbo].[fdb_brand_name]
            ([MEDID]
           , [long_brand_name]
           , [active]
           , [MED_NAME_ID]
           , [PC_MED_NAME_ID]
           , [ROUTED_GEN_ID]
           , [PC_ROUTED_GEN_ID]
           , [brand_name]
           , [dea_schedule]
           , [rx_otc]
           , [erx_search]
            )
        select [source].[MEDID]
             , [source].[long_brand_name]
             , [source].[active]
             , [source].[MED_NAME_ID]
             , [source].[PC_MED_NAME_ID]
             , [source].[ROUTED_GEN_ID]
             , [source].[PC_ROUTED_GEN_ID]
             , [source].[brand_name]
             , [source].[dea_schedule]
             , [source].[rx_otc]
             , [source].[erx_search]
        from   [#fdb_brand_name] as [source];

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#fdb_brand_name];