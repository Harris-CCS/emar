print 'create procedure [dbo].[export_ibex_fdb_allergy_name];'
drop procedure if exists [dbo].[export_ibex_fdb_allergy_name];

set @template = N'
create or alter procedure [dbo].[export_ibex_fdb_allergy_name]
as
    begin

        select [source].[MEDID]
             , rtrim(ltrim([source].[med_name])) as       [med_name]
             , [source].[MED_NAME_ID]
             , rtrim(ltrim([source].[PC_MED_NAME_ID])) as [PC_MED_NAME_ID]
             , [source].[HICL_SEQNO]
             , rtrim(ltrim([source].[PC_HICL_SEQNO])) as  [PC_HICL_SEQNO]
             , rtrim(ltrim([source].[allergy_name])) as   [allergy_name]
        from   [<@export_database_name>].[dbo].[fdb_allergy_name] as [source];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;