print 'create trigger [ibex].[dbo].[drs].[emar_users_u];'

set @template = N'
create or alter trigger [dbo].[emar_users_u] on [dbo].[drs] after update as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''users''
         , [i].[num]
         , sysdatetimeoffset()
    from   [inserted] as [i]
    inner join [deleted] as [d]
           on [i].[num] = [d].[num]
    where  isnull([i].[site], 0) <> isnull([d].[site], 0)
           or isnull([i].[type], char(1)) <> isnull([d].[type], char(1))
           or isnull([i].[status], char(1)) <> isnull([d].[status], char(1))
           or isnull([i].[init], char(1)) <> isnull([d].[init], char(1))
           or isnull([i].[first], char(1)) <> isnull([d].[first], char(1))
           or isnull([i].[last], char(1)) <> isnull([d].[last], char(1))
           or isnull([i].[ordonly], char(1)) <> isnull([d].[ordonly], char(1))
           or isnull([i].[loginid], char(1)) <> isnull([d].[loginid], char(1))
           or isnull([i].[password], char(1)) <> isnull([d].[password], char(1))
           or isnull([i].[lastlogin], char(1)) <> isnull([d].[lastlogin], char(1))
           or isnull([i].[medprn], char(1)) <> isnull([d].[medprn], char(1))
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;