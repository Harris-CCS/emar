create procedure [dbo].[export_ibex_fdb_brand_name]
as
    begin

        select [source].[MEDID]
             , rtrim(ltrim([source].[long_brand_name])) as  [long_brand_name]
             , rtrim(ltrim([source].[active])) as           [active]
             , [source].[MED_NAME_ID]
             , rtrim(ltrim([source].[PC_MED_NAME_ID])) as   [PC_ROUTED_GEN_ID]
             , [source].[ROUTED_GEN_ID]
             , rtrim(ltrim([source].[PC_ROUTED_GEN_ID])) as [PC_ROUTED_GEN_ID]
             , rtrim(ltrim([source].[brand_name])) as       [brand_name]
             , rtrim(ltrim([source].[dea_schedule])) as     [dea_schedule]
             , rtrim(ltrim([source].[rx_otc])) as           [rx_otc]
             , [source].[erx_search]
        from   [ibex].[dbo].[fdb_brand_name] as [source];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex fdb_brand_name in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_fdb_brand_name';
go