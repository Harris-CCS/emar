create procedure [dbo].[export_ibex_patient_indicators]
as
    begin

        select    cast([pi].[site] as varchar(15)) + '|' + [pi].[ibex] as [patient_id]
                , isnull([cci].[position], 0) as                          [ordinal_position]
                , [pi].[code]
                , [pi].[type]
                , [images].[description]
                , [cii].[name] as                                         [image_name]
        from      [ibex].[dbo].[pat] as [pat]
                  inner join [ibex].[dbo].[pat_indicators] as [pi] on [pat].[ibex] = [pi].[ibex]
                                                                      and [pat].[site] = [pi].[site]
                  inner join [ibex].[dbo].[custom_indicators] as [ci] on [pi].[type] = [ci].[code] and [pi].[site] = [ci].[site]
                  left join [ibex].[dbo].[current_custom_indicators] as [cci] on [cci].[custom_indicator_id] = [ci].[id]
                                                                                 and [cci].[site] = [pi].[site]
                  cross apply
                  (
                    select [js].[code]
                            , [js].[description]
                            , cast([js].[image] as int) as [image_id]
                    from   openjson([ci].[template], '$.list') 
                    with(
                            [description] nvarchar(25) '$.description'
                        , [image] nvarchar(25) '$.image'
                        , [code] nvarchar(25) '$.code'
                        ) as [js]
                    where  [js].[code] = [pi].[code]
                  ) as [images]
                  inner join [ibex].[dbo].[custom_indicator_images] as [cii] on [cii].[id] = [images].[image_id]
        order by [patient_id]
               , [ordinal_position]
               , [type]
               , [image_name];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex patient_indicators in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_patient_indicators';
go
