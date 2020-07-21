create procedure [dbo].[export_ibex_group_list_items]
as
    begin

        create table [#group_list_items]
            (
              [site_id]             [varchar](40) not null
            , [group_name]          [nvarchar](255) not null
            , [ndc]                 [varchar](32) null
            , [drug_id]             [varchar](32) null
            , [brand_name]          [nvarchar](255) not null
            , [dose]                [varchar](40) null
            , [dose_unit]           [varchar](100) null
            , [medication_route_id] [varchar](40) null
            , [frequency_id]        [int] null
            , [order_notes]         [nvarchar](max) null);

        insert into [#group_list_items]
        select [grp].[site] as                [site_id]
             , rtrim(ltrim([cde].[name])) as  [group_name]
             , rtrim(ltrim([grp].[code])) as  [ndc]
             , '' as                          [drug_id]
             , rtrim(ltrim([grp].[name])) as  [brand_name]
             , rtrim(ltrim([grp].[dose])) as  [dose]
             , rtrim(ltrim([grp].[unit])) as  [dose_unit]
             , rtrim(ltrim([grp].[route])) as [medication_route_id]
             , 0 as                           [frequency_id]
             , [grp].[notes] as               [order_notes]
        from   [ibex].[dbo].[grp]
               inner join [ibex].[dbo].[cde] on [grp].[num] = [cde].[num]
        where  [grp].[type] = 'M';

        select [site_id]
             , [group_name]
             , [ndc]
             , [drug_id]
             , [brand_name]
             , [dose]
             , [dose_unit]
             , [medication_route_id]
             , [frequency_id]
             , [order_notes]
             , rtrim(ltrim([1])) as [brand_name_1]
             , rtrim(ltrim([2])) as [dose_2]
             , rtrim(ltrim([3])) as [dose_unit_3]
             , rtrim(ltrim([4])) as [medication_route_id_4]
             , rtrim(ltrim([5])) as [medication_route_id_5]
        into [#group_list_items_parsed]
        from
        (
            select [grp].[site_id]
                 , [grp].[group_name]
                 , [grp].[ndc]
                 , [grp].[drug_id]
                 , [grp].[brand_name]
                 , [grp].[dose]
                 , [grp].[dose_unit]
                 , [grp].[medication_route_id]
                 , [grp].[frequency_id]
                 , [grp].[order_notes]
                 , [name_part].[ItemNumber]
                 , [name_part].[Item]
            from   [#group_list_items] as [grp]
                   outer apply [dbo].[delimited_split_8k]
                ([brand_name], ':') as [name_part]
        ) as [t] pivot(max([item]) for [itemnumber] in([1]
                                                     , [2]
                                                     , [3]
                                                     , [4]
                                                     , [5])) as [piviot_table];

        update [#group_list_items_parsed] set    
            [medication_route_id] = [medication_route_id_5]
        where  isnull([medication_route_id], '') = ''
               and isnull([medication_route_id_5], '') > '';

        update [#group_list_items_parsed] set    
            [medication_route_id] = [medication_route_id_4]
        where  isnull([medication_route_id], '') = ''
               and isnull([medication_route_id_4], '') > '';

--        update [#group_list_items_parsed] set    
--            [dose_unit] = [dose_unit_3]
--        where  isnull([dose_unit], '') = ''
--               and isnull([dose_unit_3], '') > '';

        update [#group_list_items_parsed] set    
            [dose] = [dose_2]
        where  isnull([dose], '') = ''
               and isnull([dose_2], '') > '';

        update [#group_list_items_parsed] set    
            [drug_id] = isnull(
        (
            select cast([medid] as varchar(32))
            from   [ibex].[dbo].[fdb_ndc_info]
            where  [fdb_ndc_info].[ndc] = [#group_list_items_parsed].[ndc]
        ), '')
        where  isnull([drug_id], '') = '';

        update [#group_list_items_parsed] set    
            [dose] = '0'
        where  isnull([dose], '') = '';

        update [#group_list_items_parsed] set    
            [dose] = '0'
        where  isnumeric([dose]) = 0;

        select [result].[site_id]
             , [result].[group_name]
             , [result].[ndc]
             , [result].[drug_id]
             , [result].[brand_name]
             , [result].[dose]
             , [result].[dose_unit]
             , [result].[medication_route_id]
             , [result].[frequency_id]
             , [result].[order_notes]
        from   [#group_list_items_parsed] as [result]
        order by [result].[group_name]
               , [result].[ndc]
               , [result].[brand_name]
               , [result].[site_id]
               , [result].[dose_unit]
               , cast([result].[order_notes] as varchar(1000));

        drop table if exists [#group_list_items];
        drop table if exists [#group_list_items_parsed];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex group_list_items in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_group_list_items';
go