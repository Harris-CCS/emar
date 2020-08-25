create procedure [dbo].[export_ibex_patient_problems]
as
    begin
        select [trx].[site] as                                           [site_id]
             , cast([pat].[site] as varchar(15)) + '|' + [trx].[ibex] as [patient_id]
             , isnull([cs].[display], '') as                             [code_set_name]
             , isnull([trx].[alienkey], '') as                           [code_set_value]
             , [trx].[name]
             , case
                   when isnull([trx].[riskgreen], '') = ''
                       then ''
                   when [trx].[service] = 201
                       then 'Primary'
                   when [trx].[service] = 200
                       then 'Secondary'
                   when [trx].[service] = 203
                       then 'Admitting'
                   else ''
               end as                                                    [diagnosis_type]
        from     [ibex].[dbo].[trx]
                 inner join [ibex].[dbo].[pat] as [pat] on [pat].[ibex] = [trx].[ibex]
                 left join [ibex].[dbo].[code_systems] as [cs] on [cs].[oid] = [trx].[riskgreen]
                                                                  and [trx].[riskgreen] > ''
        where   [trx].[type] = 'Q'
                and [trx].status = 'A'
                and [trx].[service] in(203, 200, 201)
        union
        select [pat].[site] as                                          [site_id]
             , cast([pat].[site] as varchar(15)) + '|' + [pe].[ibex] as [patient_id]
             , isnull([cs].[display], '') as                            [code_set_name]
             , isnull([pe].[problem_code], '') as                       [code_set_value]
             , [pe].[problem_name]
             , case
                   when isnull([pe].[problem_code], '') = ''
                       then ''
                   else 'Secondary'
               end as                                                   [diagnosis_type]
        from   [ibex].[dbo].[problem_episode] as [pe]
               inner join [ibex].[dbo].[pat] as [pat] on [pat].[ibex] = [pe].[ibex]
               left join [ibex].[dbo].[code_systems] as [cs] on [cs].[oid] = [pe].[problem_code_system]
        where  [pe].[internal_status] = 'A'
        order by 1
               , 2;
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex patient_problems in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_patient_problems';
go