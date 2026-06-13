print 'create procedure [dbo].[export_ibex_patient_indicators];'
drop procedure if exists [dbo].[export_ibex_patient_indicators];

set @template = N'
create procedure [dbo].[export_ibex_patient_indicators]
as
    begin

        select    cast([pi].[site] as varchar(15)) + ''|'' + [pi].[ibex] as [patient_id]
                , isnull([cci].[position], 0) as                          [ordinal_position]
                , [pi].[code]
                , [pi].[type]
                , [images].[description]
                , [cii].[name] as                                         [image_name]
				, [ci].[description] as                                   [type_description]
        from      [<@export_database_name>].[dbo].[pat] as [pat]
                  inner join [<@export_database_name>].[dbo].[pat_indicators] as [pi] on [pat].[ibex] = [pi].[ibex]
                                                                      and [pat].[site] = [pi].[site]
                  inner join [<@export_database_name>].[dbo].[custom_indicators] as [ci] on [pi].[type] = [ci].[code] and [pi].[cs_site] = [ci].[site]
                  left join [<@export_database_name>].[dbo].[current_custom_indicators] as [cci] on [cci].[custom_indicator_id] = [ci].[id]
                                                                                 and [cci].[site] = [pi].[site]
                  cross apply
                  (
                    select [js].[code]
                            , [js].[description]
                            , cast([js].[image] as int) as [image_id]
                    from   openjson([ci].[template], ''$.list'') 
                    with(
                            [description] nvarchar(25) ''$.description''
                        , [image] nvarchar(25) ''$.image''
                        , [code] nvarchar(25) ''$.code''
                        ) as [js]
                    where  [js].[code] = [pi].[code]
                  ) as [images]
                  inner join [<@export_database_name>].[dbo].[custom_indicator_images] as [cii] on [cii].[id] = [images].[image_id]
        where [pi].[admreq] = ''N''
        order by [patient_id]
               , [ordinal_position]
               , [type]
               , [image_name];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;