print 'create trigger [ibex].[dbo].[org].[emar_sites_u];'

set @template = N'
create or alter trigger [dbo].[emar_sites_u] on [dbo].[org] after update as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''sites''
         , [i].[site]
         , sysdatetimeoffset()
    from   [inserted] as [i]
    inner join [deleted] as [d]
           on [i].[site] = [d].[site]
    where  isnull([i].[name], char(1)) <> isnull([d].[name], char(1))
           or isnull([i].[status], char(1)) <> isnull([d].[status], char(1))
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;