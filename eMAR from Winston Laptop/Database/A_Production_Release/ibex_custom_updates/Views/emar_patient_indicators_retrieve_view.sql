print 'create view [ibex].[dbo].[emar_patient_indicators_retrieve_view];';

set @template = N'
create or alter view [dbo].[emar_patient_indicators_retrieve_view]

as
        select 
				pi.[ibex] as [external_patient_id],
				pi.[site] as [external_site_id],
                isnull([cci].[position], 0) as [ordinal_position]
                , [pi].[code]
                , [pi].[type]
				, [ci].[description] as [type_description]
                , [images].[description]
                , [cii].[name] as [image_name]
        from      [dbo].[pat] as [pat]
                  inner join [dbo].[pat_indicators] as [pi] on [pat].[ibex] = [pi].[ibex]
                                                                      and [pat].[site] = [pi].[site]
                  inner join [dbo].[custom_indicators] as [ci] on [pi].[type] = [ci].[code] and [pi].[cs_site] = [ci].[site]
                  left join [dbo].[current_custom_indicators] as [cci] on [cci].[custom_indicator_id] = [ci].[id]
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
                  inner join [dbo].[custom_indicator_images] as [cii] on [cii].[id] = [images].[image_id]
        where [pi].[admreq] = ''N''
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
