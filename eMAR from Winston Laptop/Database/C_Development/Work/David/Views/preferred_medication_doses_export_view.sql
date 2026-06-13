create view [dbo].[preferred_medication_doses_export_view]

as

with cte_raw
    as (
                 select
                     case
                         when [m].[site_id] = -1 then [pmd].[site_id]
                         else [m].[site_id]
                     end                                                                                                                                                                                                    [site_id]
                   , [pmd].[medication_id]
                   , right('00' + cast(row_number() over (partition by [pmd].[site_id], [pmd].[medication_id] order by [pmd].[site_id], [pmd].[medication_id], [pmd].[medication_unit_id], [pmd].[dose]) as varchar(3)), 2) column_name
                   , cast([pmd].[dose] as varchar(50))                                                                                                                                                                      [dose]
                   , [mu].[name]                                                                                                                                                                                            [unit_name]
                 from [dbo].[preferred_medication_doses] [pmd]
                     inner join [dbo].[medications] [m]
                         on [pmd].[medication_id] = [m].[id]
                     inner join [dbo].[medication_units] [mu]
                         on [pmd].[medication_unit_id] = [mu].[id]
        ),
    cte_unpivot
    as (
                 select
                     [site_id]
                   , [medication_id]
                   , columntitle + column_name as columntitle
                   , columndata
                 from cte_raw
                 unpivot (columndata for columntitle in ([dose], [unit_name])) as up
        )
select
    [s].[name] site_name
  , [m].[drug_id]
  , [m].[display_name]
  , isnull([pvt].[dose01]      ,'') [dose01]     
  , isnull([pvt].[unit_name01] ,'') [unit_name01]
  , isnull([pvt].[dose02]      ,'') [dose02]     
  , isnull([pvt].[unit_name02] ,'') [unit_name02]
  , isnull([pvt].[dose03]      ,'') [dose03]     
  , isnull([pvt].[unit_name03] ,'') [unit_name03]
  , isnull([pvt].[dose04]      ,'') [dose04]     
  , isnull([pvt].[unit_name04] ,'') [unit_name04]
  , isnull([pvt].[dose05]      ,'') [dose05]     
  , isnull([pvt].[unit_name05] ,'') [unit_name05]
  , isnull([pvt].[dose06]      ,'') [dose06]     
  , isnull([pvt].[unit_name06] ,'') [unit_name06]
  , isnull([pvt].[dose07]      ,'') [dose07]     
  , isnull([pvt].[unit_name07] ,'') [unit_name07]
  , isnull([pvt].[dose08]      ,'') [dose08]     
  , isnull([pvt].[unit_name08] ,'') [unit_name08]
  , isnull([pvt].[dose09]      ,'') [dose09]     
  , isnull([pvt].[unit_name09] ,'') [unit_name09]
  , isnull([pvt].[dose10]      ,'') [dose10]     
  , isnull([pvt].[unit_name10] ,'') [unit_name10]
from cte_unpivot cu pivot (max(columndata) for columntitle in ([dose01], [unit_name01], [dose02], [unit_name02], [dose03], [unit_name03]
, [dose04], [unit_name04], [dose05], [unit_name05], [dose06], [unit_name06], [dose07], [unit_name07], [dose08], [unit_name08], [dose09], [unit_name09], [dose10], [unit_name10])) pvt
    inner join [dbo].[sites] [s]
        on [s].[id] = [pvt].[site_id]
    inner join [dbo].[medications] [m]
        on [m].[id] = [pvt].[medication_id]
go

-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'View used to generate text to be applied into the Excel Workbook for the purposes of import / edit'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'preferred_medication_doses_export_view';
go
