print 'create procedure [dbo].[export_ibex_order_instructions];'
drop procedure if exists [dbo].[export_ibex_order_instructions];

set @template = N'
create procedure [dbo].[export_ibex_order_instructions]
as
    begin
        select [idx].[site] as               [site_id]
             , ltrim(rtrim([idx].[name])) as [description]
             , 1 as                          [is_active]
        from   [<@export_database_name>].[dbo].[idx] as [idx]
        where  [idx].[type] = ''SO''
               and [idx].status = ''Y''
        order by 2;
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;