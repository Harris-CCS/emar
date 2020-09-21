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
    , [days_obsolete] [int] null
    , [GCN_SEQNO]     [numeric](6, 0) null
    , [HICL_SEQNO]    [numeric](6, 0) null
    , [ROUTED_GEN_ID] [numeric](8, 0) null);

/****************************************
        load temporary tables for staging
****************************************/

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#fdb_ndc_info]
            ([ndc]
           , [base_ndc]
           , [repackaged]
           , [medid]
           , [packaging]
           , [strength]
           , [days_obsolete]
           , [GCN_SEQNO]
           , [HICL_SEQNO]
           , [ROUTED_GEN_ID]
            )
        execute ('execute dbo.export_ibex_fdb_ndc_info');
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
           , [GCN_SEQNO]
           , [HICL_SEQNO]
           , [ROUTED_GEN_ID]
            )
        select [source].[ndc]
             , [source].[base_ndc]
             , [source].[repackaged]
             , [source].[medid]
             , [source].[packaging]
             , [source].[strength]
             , [source].[days_obsolete]
             , [source].[GCN_SEQNO]
             , [source].[HICL_SEQNO]
             , [source].[ROUTED_GEN_ID]
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