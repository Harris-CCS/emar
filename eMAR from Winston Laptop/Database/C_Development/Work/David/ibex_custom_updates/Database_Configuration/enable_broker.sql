print 'Activate: enable_broker';

set @template = N'
if not exists (
             select
                 *
             from sys.[databases] [d]
             where is_broker_enabled = 0
                   and name = ''ibex''
    )
    begin
        alter database ibex set enable_broker with rollback immediate;
    end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;