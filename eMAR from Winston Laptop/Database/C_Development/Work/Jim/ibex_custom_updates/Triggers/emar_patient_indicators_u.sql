print 'create trigger [ibex].[dbo].[pat_indicators].[emar_patient_indicators_u];'

set @template = N'
create or alter trigger [dbo].[emar_patient_indicators_u] on [dbo].[pat_indicators] after update as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''patients''
         , cast([i].[site] as varchar(15))+''|''+[i].[ibex]
         , sysdatetimeoffset()
    from   [inserted] as [i]
    inner join [deleted] as [d]
           on [i].[id] = [d].[id]
    where  isnull([i].[code], char(1)) <> isnull([d].[code], char(1))
           or isnull([i].[type], char(1)) <> isnull([d].[type], char(1))
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;