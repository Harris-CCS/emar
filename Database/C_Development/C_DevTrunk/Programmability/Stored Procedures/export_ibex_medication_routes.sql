create procedure [export_ibex_medication_routes]
as
    begin

        select distinct
               [a].[site]
             , [a].[name]
        from   [ibex].[dbo].[idx] as [a]
        where  [type] in('AC')
        order by [a].[name]
               , [a].[site];
    end;
go
