print 'create table [ibex].[dbo].[emar_update_queue_errors];'

set @template = N'
if not exists
(
    select null
    from   [sys].[objects]
    where  object_id = object_id(N''[emar_update_queue_errors]'')
           and [type] in(N''U'')
)
    begin
        CREATE TABLE [dbo].[emar_update_queue_errors] (
            [queue_id] bigint NOT NULL,
            [queue_record_error_num] int NOT NULL,
            [error_datetime] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
            [error_location] nvarchar(100) NOT NULL,
            [exception_info] nvarchar(max) NULL,
            PRIMARY KEY ([queue_id], [queue_record_error_num]),
            FOREIGN KEY ([queue_id]) REFERENCES [emar_update_queue] ([id]) ON DELETE CASCADE
        )
    end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
