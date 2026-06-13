print 'create procedure [dbo].[export_ibex_users];'
drop procedure if exists [dbo].[export_ibex_users];

set @template = N'
create or alter procedure [dbo].[export_ibex_users]
as
    begin

        select [source].[num] as                     [id]
             , [source].[site] as                    [site_id]
             , rtrim(ltrim([source].[type])) as      [type]
             , case
                   when [source].status = ''A''
                       then 1
                   else 0
               end as                                [is_active]
             , rtrim(ltrim([source].[init])) as      [initials_display]
             , rtrim(ltrim([source].[first])) as     [first_name]
             , rtrim(ltrim([source].[last])) as      [last_name]
             , '''' as                                 [middle_name]
             , '''' as                                 [name_suffix]
             , case
                   when [source].[ordonly] = ''Y''
                       then 1
                   else 0
               end as                                [ordering_only_physician]
             , 0 as                                  [name_display_initials]
             , rtrim(ltrim([source].[loginid])) as   [login_name]
             , rtrim(ltrim([source].[password])) as  [login_password]
             , 0x00 as                               [salt]
             , rtrim(ltrim([source].[lastlogin])) as [last_login_time]
             , 0 as                                  [failed_login_attempts]
        from   [<@export_database_name>].[dbo].[drs] as [source]
        order by [source].[last]
               , [source].[first]
               , [source].[site]
               , [source].[num];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;
