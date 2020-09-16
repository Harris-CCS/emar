print 'create procedure [dbo].[export_ibex_group_list_items];';

drop procedure if exists [dbo].[export_ibex_group_list_items];

set @template = N'
create or alter procedure [dbo].[export_ibex_group_list_items]
as
    begin

        create table [#group_list_items_proc]
            (
              [site_id]               [varchar](40) not null
            , [group_name]            [nvarchar](255) not null
            , [group_type]            [varchar](5) not null
            , [ndc]                   [varchar](32) null
            , [drug_id]               [varchar](32) null
            , [brand_name]            [nvarchar](255) not null
            , [dose]                  [varchar](40) null
            , [medication_unit_id]    [varchar](100) null
            , [medication_route_id]   [varchar](40) null
            , [frequency_schedule_id] [int] null
            , [order_notes]           [nvarchar](max) null);

        insert into [#group_list_items_proc]
        --Part 1 combo medications
        select [detail].[site] as                  [site_id]
             , rtrim(ltrim([combo].[name])) as     [combo_med_name]
             , ''CM'' as                             [group_type]
             , rtrim(ltrim([detail].[code])) as    [ndc]
             , rtrim(ltrim([detail].[form_id])) as [drug_id]
             , rtrim(ltrim([detail].[name])) as    [brand_name]
             , rtrim(ltrim([detail].[dose]))
             , rtrim(ltrim([detail].[unit])) as    [medication_unit_id]
             , rtrim(ltrim([detail].[route])) as   [medication_route_id]
             , 0 as                                [frequency_schedule_id]
             , rtrim(ltrim([detail].[notes])) as   [notes]
        from     ibex.dbo.[cde] as [parent]
                 inner join ibex.dbo.[cde] as [combo] on [combo].[grptype] = [parent].[num]
                 inner join ibex.dbo.[grp] as [detail] on [detail].[num] = [combo].[num]
        where   [parent].[type] = ''T''
                and [parent].[altcode] = ''X''--X=Combo
                and [detail].[type] = ''M''--Medication
        --Part 2 Group Meds
        union
        select [detail].[site]
             , rtrim(ltrim([combo].[name])) as     [group_name]
             , case
                   when [detail].[type] = ''X''
                       then ''GX''
                   else ''GM''
               end as                              [group_type]
             , rtrim(ltrim([detail].[code])) as    [ndc]
             , rtrim(ltrim([detail].[form_id])) as [drug_id]
             , rtrim(ltrim([detail].[name])) as    [fdb_brand_name]
             , rtrim(ltrim([detail].[dose]))
             , rtrim(ltrim([detail].[unit])) as    [medication_unit_id]
             , rtrim(ltrim([detail].[route])) as   [medication_route_id]
             , 0 as                                [frequency_schedule_id]
             , rtrim(ltrim([detail].[notes])) as   [notes]
        from   ibex.dbo.[cde] as [parent]
               inner join ibex.dbo.[cde] as [combo] on [combo].[grptype] = [parent].[num]
               inner join ibex.dbo.[grp] as [detail] on [detail].[num] = [combo].[num]
        where  [parent].[type] = ''T''
               and [parent].[altcode] <> ''X''--X=Combo
               and [detail].[type] in(''M'', ''X'');--Medication

        select [site_id]
             , [group_name]
             , [group_type]
             , [ndc]
             , [drug_id]
             , [brand_name]
             , [dose]
             , [medication_unit_id]
             , [medication_route_id]
             , [frequency_schedule_id]
             , [order_notes]
             , rtrim(ltrim([1])) as [brand_name_1]
             , rtrim(ltrim([2])) as [dose_2]
             , rtrim(ltrim([3])) as [medication_unit_id_3]
             , rtrim(ltrim([4])) as [medication_route_id_4]
             , rtrim(ltrim([5])) as [medication_route_id_5]
        into [#group_list_items_parsed]
        from
        (
            select [grp].[site_id]
                 , [grp].[group_name]
                 , [grp].[group_type]
                 , [grp].[ndc]
                 , [grp].[drug_id]
                 , [grp].[brand_name]
                 , [grp].[dose]
                 , [grp].[medication_unit_id]
                 , [grp].[medication_route_id]
                 , [grp].[frequency_schedule_id]
                 , [grp].[order_notes]
                 , [name_part].[ItemNumber]
                 , [name_part].[Item]
            from   [#group_list_items_proc] as [grp]
                   outer apply [dbo].[delimited_split_8k]
                ([brand_name], '':'') as [name_part]
        ) as [t] pivot(max([item]) for [itemnumber] in([1]
                                                     , [2]
                                                     , [3]
                                                     , [4]
                                                     , [5])) as [piviot_table];

        update [#group_list_items_parsed] set
            [medication_route_id] = [medication_route_id_5]
        where  isnull([medication_route_id], '''') = ''''
               and isnull([medication_route_id_5], '''') > '''';

        update [#group_list_items_parsed] set
            [medication_route_id] = [medication_route_id_4]
        where  isnull([medication_route_id], '''') = ''''
               and isnull([medication_route_id_4], '''') > '''';

        --        update [#group_list_items_parsed] set
        --            [medication_unit_id] = [medication_unit_id_3]
        --        where  isnull([medication_unit_id], '''') = ''''
        --               and isnull([medication_unit_id_3], '''') > '''';

        update [#group_list_items_parsed] set
            [dose] = [dose_2]
        where  isnull([dose], '''') = ''''
               and isnull([dose_2], '''') > '''';

        update [#group_list_items_parsed] set
            [drug_id] = isnull(
        (
            select cast([medid] as varchar(32))
            from   [<@export_database_name>].[dbo].[fdb_ndc_info]
            where  [fdb_ndc_info].[ndc] = [#group_list_items_parsed].[ndc]
        ), '''')
        where  isnull([drug_id], '''') = '''';

        update [#group_list_items_parsed] set
            [dose] = ''0''
        where  isnull([dose], '''') = '''';

        update [#group_list_items_parsed] set
            [dose] = ''0''
        where  isnumeric([dose]) = 0;

        update [#group_list_items_parsed] set
            [brand_name] = left([brand_name], charindex('':'', [brand_name]) - 1)
        where  charindex('':'', [brand_name]) > 0;

        select [result].[site_id]
             , [result].[group_name]
             , [result].[group_type]
             , [result].[ndc]
             , [result].[drug_id]
             , rtrim(ltrim([result].[brand_name])) as [brand_name]
             , [result].[dose]
             , [result].[medication_unit_id]
             , [result].[medication_route_id]
             , [result].[frequency_schedule_id]
             , [result].[order_notes]
        from   [#group_list_items_parsed] as [result]
        order by [result].[group_type]
               , [result].[group_name]
               , [result].[ndc]
               , [result].[brand_name]
               , [result].[site_id]
               , [result].[medication_unit_id]
               , cast([result].[order_notes] as varchar(1000));

        drop table if exists [#group_list_items_proc];
        drop table if exists [#group_list_items_parsed];
    end;
';

set @sql_cmd = @template;

set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;