print 'create procedure [dbo].[export_ibex_fdb_brand_name];'
drop procedure if exists [dbo].[export_ibex_fdb_brand_name];

set @template = N'
create or alter procedure [dbo].[export_ibex_fdb_brand_name]
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
        from   [<@export_database_name>].[dbo].[fdb_brand_name] as [source];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;