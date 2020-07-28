create procedure [dbo].[export_ibex_fdb_allergy_name]
as
    begin

        select [source].[MEDID]
             , rtrim(ltrim([source].[med_name])) as       [med_name]
             , [source].[MED_NAME_ID]
             , rtrim(ltrim([source].[PC_MED_NAME_ID])) as [PC_MED_NAME_ID]
             , [source].[HICL_SEQNO]
             , rtrim(ltrim([source].[PC_HICL_SEQNO])) as  [PC_HICL_SEQNO]
             , rtrim(ltrim([source].[allergy_name])) as   [allergy_name]
        from   [ibex].[dbo].[fdb_allergy_name] as [source];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex fdb_allergy_name in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_fdb_allergy_name';
go