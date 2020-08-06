create procedure [dbo].[export_ibex_antimicrobial_indications]
as
    begin

        select [source].[site] as                      [site_id]
             , ltrim(rtrim([source].[code])) as        [code]
             , ltrim(rtrim([source].[description])) as [description]
             , case
                   when ltrim(rtrim([source].[status])) = 'A'
                       then 1
                   else 0
               end as                                  [is_active]
             , [source].[position] as                  [ordinal_position]
        from   [ibex].[dbo].[medication_indication] as [source]
        order by [source].[description]
               , [source].[site];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex antimicrobial_indications in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_antimicrobial_indications';
go