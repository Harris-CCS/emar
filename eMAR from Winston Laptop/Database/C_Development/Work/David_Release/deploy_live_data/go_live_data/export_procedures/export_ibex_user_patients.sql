print 'create procedure [dbo].[export_ibex_user_patients];'
drop procedure if exists [dbo].[export_ibex_user_patients];

set @template = N'
create or alter procedure [dbo].[export_ibex_user_patients]
as
    begin

        select    [source].[ibex] as                              [source_id]
                , ltrim(rtrim([source].[site])) as                [site_id]
                , ltrim(rtrim([source].[doctor])) as              [doctor]
                , ltrim(rtrim([source].[resident])) as            [resident]
                , ltrim(rtrim([source].[drextender])) as          [drextender]
                , ltrim(rtrim([source].[primarynurse])) as        [primarynurse]
                , ltrim(rtrim([source].[extender])) as            [extender]
                , ltrim(rtrim([source].[firstdoctor])) as         [firstdoctor]
        from      [<@export_database_name>].[dbo].[pat] as [source]
                  inner join [<@export_database_name>].[dbo].[org] as [sites] on [sites].[site] = [source].[site];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;