create procedure [dbo].[export_ibex_patient_home_medications]
as
    begin

        select cast([source].[site] as varchar(5)) + '|' + [source].[ibex] as [patient_id]
             , ltrim(rtrim([source].[class])) as                              [class]
             , ltrim(rtrim([source].[cat])) as                                [category]
             , ltrim(rtrim([source].[drug])) as                               [internal_drug_id]
             , ltrim(rtrim([source].[ndc])) as                                [ndc]
             , isnull(cast([ndc].[medid] as varchar(25)), '') as              [drug_id]
             , ltrim(rtrim([source].[name])) as                               [name]
             , ltrim(rtrim([source].[alt_name])) as                           [alternate__name]
             , ltrim(rtrim([source].[dose])) as                               [dose]
             , ltrim(rtrim([source].[unit])) as                               [medication_unit_id]
             , ltrim(rtrim([source].[route])) as                              [medication_route_id]
             , ltrim(rtrim([source].[alg_drug_id])) as                        [medication_drug_id]
             , case
                   when ltrim(rtrim([source].[status])) = 'A'
                       then 1
                   else 0
               end as                                                         [is_active]
             , ltrim(rtrim([source].[cmt])) as                                [comment]
             , ltrim(rtrim([source].[sched])) as                              [schedule]
             , ltrim(rtrim([source].[reaction])) as                           [reaction]
             , ltrim(rtrim([source].[severity])) as                           [severity]
             , ltrim(rtrim([source].[parent_id])) as                          [parent_drug_id]
             , ltrim(rtrim([source].[parent_name])) as                        [parent_drug_name]
             , ltrim(rtrim([source].[usr])) as                                [add_user_id]
             , ltrim(rtrim([source].[dateadd])) as                            [add_datetime]
             , ltrim(rtrim([source].[usrchg])) as                             [change_user_id]
             , ltrim(rtrim([source].[datechg])) as                            [change_datetime]
        from   [ibex].[dbo].[alg] as [source]
               inner join [ibex].[dbo].[org] as [sites] on [sites].[site] = [source].[site]
               inner join [ibex].[dbo].[pat] as [patients] on [patients].[site] = [sites].[site]
                                                              and [patients].[ibex] = [source].[ibex]
               left join [ibex].[dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [source].[ndc]
        where  [source].[type] = 'M'
        order by [source].[ibex]
               , [source].[dateadd]
               , [source].[name]
               , [source].[parent_name];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex patient_home_medications in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_patient_home_medications';
go