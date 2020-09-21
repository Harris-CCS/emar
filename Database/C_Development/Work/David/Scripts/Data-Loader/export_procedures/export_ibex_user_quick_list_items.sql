print 'create procedure [dbo].[export_ibex_user_quick_list_items];';
drop procedure if exists [dbo].[export_ibex_user_quick_list_items];

set @template = N'
create or alter procedure [dbo].[export_ibex_user_quick_list_items]
as
    begin
        select distinct 
               [source].[site] as                [site_id]
             , ltrim(rtrim([source].[usr])) as   [user_id]
             , ltrim(rtrim([source].[ndc])) as   [ndc]
             , ltrim(rtrim([ndc].[medid])) as    [drug_id]
             , ltrim(rtrim([source].[brand])) as [brand_name]
             , case
                   when isnumeric([source].[strength]) = 0
                        or [source].[strength] = ''-''
                       then 0
                   else cast([source].[strength] as decimal(11, 2))
               end as                            [dose]
             , ltrim(rtrim([source].[unit])) as  [medication_unit_id]
             , ltrim(rtrim([source].[route])) as [medication_route_id]
             , 0 as                              [frequency_schedule_id]
             , ltrim(rtrim([source].[notes])) as [order_notes]
        from   [<@export_database_name>].[dbo].[rxl] as [source]
               left join [<@export_database_name>].[dbo].[fdb_ndc_info] as [ndc] on [source].[ndc] = [ndc].[ndc]
        order by ltrim(rtrim([source].[brand]))
               , ltrim(rtrim([source].[ndc]))
               , ltrim(rtrim([source].[usr]))
               , [source].[site];
    end;
';

set @sql_cmd = @template;

set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql] 
    @statement = @sql_cmd;