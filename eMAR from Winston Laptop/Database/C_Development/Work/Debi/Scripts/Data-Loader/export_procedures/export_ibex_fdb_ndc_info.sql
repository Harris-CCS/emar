print 'create procedure [dbo].[export_ibex_fdb_ndc_info];'
drop procedure if exists [dbo].[export_ibex_fdb_ndc_info];

set @template = N'
create or alter procedure [dbo].[export_ibex_fdb_ndc_info]
as
    begin

        select rtrim(ltrim([source].[ndc])) as       [ndc]
             , rtrim(ltrim([source].[base_ndc])) as  [base_ndc]
             , [source].[repackaged]
             , [source].[medid]
             , rtrim(ltrim([source].[packaging])) as [packaging]
             , rtrim(ltrim([source].[strength])) as  [strength]
             , [source].[days_obsolete]
             , [source].[GCN_SEQNO]
             , [source].[HICL_SEQNO]
             , [source].[ROUTED_GEN_ID]
        from   [<@export_database_name>].[dbo].[fdb_ndc_info] as [source]
        order by [source].[ndc];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;