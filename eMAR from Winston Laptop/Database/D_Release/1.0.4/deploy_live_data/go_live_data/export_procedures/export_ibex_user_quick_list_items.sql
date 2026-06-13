print 'create procedure [dbo].[export_ibex_user_quick_list_items];';
drop procedure if exists [dbo].[export_ibex_user_quick_list_items];

set @template = N'
create or alter procedure [dbo].[export_ibex_user_quick_list_items]
as
    begin
        select 
               [source].[num]                 as [source_id]
             , [source].[site]                as [site]
             , ltrim(rtrim([source].[usr])) as   [user_id]
             , ltrim(rtrim([source].[ndc])) as   [ndc]
             , ltrim(rtrim([ndc].[medid])) as    [drug_id]
             , isnull(ltrim(rtrim([source].[brand])),'''') as [brand_name]
             , case
                   when isnumeric([source].[strength]) = 0
                        or [source].[strength] = ''-''
                       then 0
                   else cast(replace([source].[strength],'','','''') as decimal(12, 3))
               end as                            [dose]
             , isnull([mu].[name], ltrim(rtrim([source].[unit])))  [medication_unit]
             , isnull([mr].[code], ltrim(rtrim([source].[route]))) [medication_route]
             , 0 as                              [frequency_schedule_id]
             , ltrim(rtrim([source].[notes])) as [order_notes]
        from   [<@export_database_name>].[dbo].[rxl] as [source]
               left join [<@export_database_name>].[dbo].[fdb_ndc_info] as [ndc] on [source].[ndc] = [ndc].[ndc]
        outer apply (
                --- duplicate names allowed in ibex, return [sq1].[id] (code / not name value)
                --- even though the sub-query is joined on [sq1].[id]
                 select
                     rtrim(ltrim([sq1].[id])) [code]
                 from [<@export_database_name>].[dbo].[idx] as [sq1]
                     inner join [<@export_database_name>].[dbo].[code_share] [cs1]
                         on [cs1].[cs_site] = [sq1].[site]
                             and [cs1].[cs_name] = ''med_route''
                 where [sq1].[type] in (''AC'')
                       and [sq1].[id] = [source].[route]
                       and [cs1].[site] = [source].[site]
        ) [mr]
        outer apply (
                 select
                     rtrim(ltrim([sq2].[name])) [name]
                 from [<@export_database_name>].[dbo].[idx] as [sq2]
                     inner join [<@export_database_name>].[dbo].[code_share] [cs2]
                         on [cs2].[cs_site] = [sq2].[site]
                             and [cs2].[cs_name] = ''med_unit''
                 where [sq2].[type] in (''BE'')
                       and [sq2].[id] = [source].[unit]
                       and [cs2].[site] = [source].[site]
        ) [mu];
    end;
';

set @sql_cmd = @template;

set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql] 
    @statement = @sql_cmd;