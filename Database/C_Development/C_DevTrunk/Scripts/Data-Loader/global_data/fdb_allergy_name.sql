print 'Loading Table: fdb_allergy_name';

drop table if exists [#fdb_allergy_name];

create table [#fdb_allergy_name]
    (
      [MEDID]          [numeric](8, 0) not null
    , [med_name]       [varchar](70) null
    , [MED_NAME_ID]    [numeric](8, 0) null
    , [PC_MED_NAME_ID] [varchar](9) null
    , [HICL_SEQNO]     [numeric](6, 0) null
    , [PC_HICL_SEQNO]  [varchar](7) null
    , [allergy_name]   [varchar](70) null);

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

--        insert into [#fdb_allergy_name]
--            ([MEDID]
--           , [med_name]
--           , [MED_NAME_ID]
--           , [PC_MED_NAME_ID]
--           , [HICL_SEQNO]
--           , [PC_HICL_SEQNO]
--           , [allergy_name]
--            )
--        execute ('execute dbo.export_ibex_fdb_allergy_name');
--    end;

if '$(load_data)' = 'live'
or '$(load_data)' = 'sample'
    begin

        bulk insert [#fdb_allergy_name] from '$(current_path)Scripts\Data-Loader\sample_data\fdb_allergy_name.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#fdb_allergy_name]
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

        insert into [dbo].[fdb_allergy_name]
            ([MEDID]
           , [med_name]
           , [MED_NAME_ID]
           , [PC_MED_NAME_ID]
           , [HICL_SEQNO]
           , [PC_HICL_SEQNO]
           , [allergy_name]
            )
        select [source].[MEDID]
             , [source].[med_name]
             , [source].[MED_NAME_ID]
             , [source].[PC_MED_NAME_ID]
             , [source].[HICL_SEQNO]
             , [source].[PC_HICL_SEQNO]
             , [source].[allergy_name]
        from   [#fdb_allergy_name] as [source];

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#fdb_allergy_name];