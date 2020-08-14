create procedure [dbo].[export_ibex_user_quick_list_items]
as
    begin
        select distinct 
               [source].[site] as                [site_id]
             , ltrim(rtrim([source].[usr])) as   [user_id]
             , ltrim(rtrim([source].[ndc])) as   [ndc]
             , ltrim(rtrim([ndc].[medid])) as    [drug_id]
             , ltrim(rtrim([source].[brand])) as [brand_name]
             , case
                   when isnumeric([source].[strength]) = 0
                        or [source].[strength] = '-'
                       then 0
                   else cast([source].[strength] as decimal(11, 2))
               end as                            [dose]
             , ltrim(rtrim([source].[unit])) as  [medication_unit_id]
             , ltrim(rtrim([source].[route])) as [medication_route_id]
             , 0 as                              [frequency_schedule_id]
             , ltrim(rtrim([source].[notes])) as [order_notes]
        from   [ibex].[dbo].[rxl] as [source]
               left join [ibex].[dbo].[fdb_ndc_info] as [ndc] on [source].[ndc] = [ndc].[ndc]
        order by ltrim(rtrim([source].[brand]))
               , ltrim(rtrim([source].[ndc]))
               , ltrim(rtrim([source].[usr]))
               , [source].[site];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex export_ibex_user_quick_list_items in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_user_quick_list_items';
go