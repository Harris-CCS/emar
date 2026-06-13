declare
    @column_exists  bit = 0

select
    @column_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'templates'
      and [c].[name] = 'prompt_id';

if @column_exists =1
    begin
        print 'rename column: sp_rename ''[dbo].[templates].[prompt_id]'', ''event_datetime_prompt_id'', ''COLUMN''';
        execute sp_rename '[dbo].[templates].[prompt_id]', 'event_datetime_prompt_id', 'COLUMN';
    end;
else
    begin
        print '**column exists: [dbo].[templates].[event_datetime_prompt_id]';
    end;

