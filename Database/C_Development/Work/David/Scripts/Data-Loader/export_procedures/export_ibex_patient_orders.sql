print 'create procedure [dbo].[export_ibex_patient_orders];'
drop procedure if exists [dbo].[export_ibex_patient_orders];

set @template = N'
create procedure [dbo].[export_ibex_patient_orders]
as
    begin

                with cte_med
                     as (select [med].[ibex]
                              , [med].[site]
                              , [med].[losecs]
                              , [med].[status]
                              , [med].[name]
                              , [med].[route]
                              , [med].[unit]
                              , [med].[schedule]
                              , [med].[dose]
                              , [med].[med_notes]
                              , [med].[order_date]
                              , [med].[order_for_usr]
                              , [med].[order_usr]
                         from   [<@export_database_name>].[dbo].[med]
                                inner join [<@export_database_name>].[dbo].[pat] as [pat] on [pat].[ibex] = [med].[ibex]
                         where  [type] = ''M''),
                     cte_med_details
                     as (select [med_details].[ibex]
                              , [med_details].[losecs]
                              , [med_details].[brand_name]
                              , [med_details].[packaging_id]
                         from   [<@export_database_name>].[dbo].[med_details] as [med_details]
                                inner join [<@export_database_name>].[dbo].[pat] as [pat] on [pat].[ibex] = [med_details].[ibex])
                     select cast([med].[site] as varchar(15)) + ''|'' + [med].[ibex] as                     [patient_id]
                          , ltrim(rtrim([med].[order_usr])) as                                            [add_user_id]
                          , ltrim(rtrim([med].[order_date])) as                                           [add_datetime]
                          , ltrim(rtrim([med].[order_for_usr])) as                                        [order_physician_user_id]
                          , ltrim(rtrim([med].[order_date])) as                                           [begin_datetime]
                          , ltrim(rtrim([med].[order_date])) as                                           [end_datetime]
                          , ltrim(rtrim([ndc].[ndc])) as                                                  [ndc]
                          , ltrim(rtrim([ndc].[medid])) as                                                [drug_id]
                          , ltrim(rtrim(isnull(nullif([med].[name], ''''), [med_details].[brand_name]))) as [brand_name]
                          , ltrim(rtrim([med].[dose])) as                                                 [dose]
                          , ltrim(rtrim([med].[unit])) as                                                 [medication_unit_id]
                          , ltrim(rtrim([med].[route])) as                                                [medication_route_id]
                          , 0 as                                                                          [priority]
                          , 0 as                                                                          [frequency_schedule_id]
                          , 0 as                                                                          [prn]
                          , 1 as                                                                          [point_in_time]
                          , ltrim(rtrim([med].[status])) as                                               [order_status]
                          , [med].[med_notes] as                                                          [order_notes]
                     from   [cte_med] as [med]
                            left join [cte_med_details] as [med_details] on [med].[ibex] = [med_details].[ibex]
                                                                            and [med].[losecs] = [med_details].[losecs]
                            left join [<@export_database_name>].[dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [med_details].[packaging_id]
                     order by [med].[ibex]
                            , [ndc].[ndc]
                            , [med].[order_date];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;
