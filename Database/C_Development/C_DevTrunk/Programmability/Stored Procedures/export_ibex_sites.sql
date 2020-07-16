create procedure [dbo].[export_ibex_sites]
as
    begin

        select [source].[site]
             , [source].[name]
             , case
                   when [source].[status] = 'A'
                       then 1
                        else 0
               end
             , 'Central Standard Time'
        from   [ibex].[dbo].[org] as [source]
        order by [source].[name]
               , [source].[site];
    end;
go
