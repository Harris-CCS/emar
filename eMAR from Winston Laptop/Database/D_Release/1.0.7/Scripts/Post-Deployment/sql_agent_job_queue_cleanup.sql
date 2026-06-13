select
    @sql_agent_job_name                  = 'EMAR_Queue_Cleanup'
  , @sql_agent_category_name             = 'EMAR_Maintenance'
  , @sql_agent_schedule_name             = 'EMAR_Weekly_Cleanup'
  , @sql_agent_template_job_category     = N'
use msdb;
if not exists
(
    select null
    from   [dbo].[syscategories]
    where  [name] = @sql_agent_category_name
)
begin
  execute [dbo].[sp_add_category] 
      @class = N''JOB''
    , @type = N''LOCAL''
    , @name = @sql_agent_category_name;
end
'
  , @sql_agent_template_job              = N'
use msdb;

set @sql_agent_job_id = null;

select @sql_agent_job_id = job_id 
from [dbo].[sysjobs] 
where name = @sql_agent_job_name;

if @sql_agent_job_id is null
    begin
        exec [dbo].[sp_add_job] 
            @job_name = @sql_agent_job_name
          , @enabled = 1
          , @owner_login_name = ''sa''
          , @category_name = @sql_agent_category_name
          , @description = ''This job deletes any records from the emar_update_queue table that are more than seven days old.  This prevents this table from growing huge over time, which is important since the triggers in ibex are constantly writing to it and since the IDS portion of the eMAR API is constantly reading from it.''
          , @start_step_id = 1
          , @job_id = @sql_agent_job_id output;

        execute [dbo].[sp_add_jobstep] 
            @job_id = @sql_agent_job_id
          , @step_id = 1
          , @step_name = N''Queue Cleanup''
          , @subsystem = N''TSQL''
          , @command = N''-- Delete all rows that are more than seven days old from emar_update_queue.
-- Winston Murdock 05/19/2021.  EMAR-922
DELETE
FROM emar_update_queue
WHERE complete_datetime < dateadd(d, -5, getdate())''
          , @database_name = N''ibex'';
  
    end
'
  , @sql_agent_template_jobschedule      = N'
use msdb;
if not exists
(
    select null
    from   [msdb].[dbo].[sysschedules]
    where  [name] = @sql_agent_schedule_name
)
begin

    execute [msdb].[dbo].[sp_add_jobschedule] 
        @job_id = @sql_agent_job_id
      , @name = @sql_agent_schedule_name
      , @enabled = 1
      , @freq_type = 4
      , @freq_interval = 1
      , @freq_subday_type = 1
      , @freq_subday_interval = 0
      , @freq_relative_interval = 0
      , @freq_recurrence_factor = 0
      , @active_start_date = 20200101
      , @active_end_date = 99991231
      , @active_start_time = 13000
      , @active_end_time = 235959;

end
'
  , @sql_agent_template_jobserver        = N'
use msdb;
if not exists
(
    select null
    from   [dbo].[sysjobservers]
    where  [job_id] = @sql_agent_job_id
)
begin

    execute [dbo].[sp_add_jobserver] 
        @job_id = @sql_agent_job_id;

end
'
  , @sql_agent_template_job_category_delete = N'
use msdb;
if exists
(
    select null
    from   [dbo].[syscategories]
    where  [name] = @sql_agent_category_name
)
begin
    execute [msdb].[dbo].[sp_delete_category] 
        @class = ''JOB''
        , @name = @sql_agent_category_name;
end
'
  , @sql_agent_template_job_delete          = N'
use msdb;
if exists
(
    select null
    from [dbo].[sysjobs] 
    where name = @sql_agent_job_name
)
    begin

        exec [msdb].[dbo].[sp_delete_job] 
            @job_name = @sql_agent_job_name;
  
    end
'
  , @sql_agent_template_jobschedule_delete  = N'
use msdb;
if exists
(
    select null
    from   [msdb].[dbo].[sysschedules]
    where  [name] = @sql_agent_schedule_name
)
begin

    execute [msdb].[dbo].[sp_delete_schedule] 
        @schedule_name = @sql_agent_schedule_name
        , @force_delete = 1;

end
';

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


set @sql_agent_cmd = @sql_agent_template_job_category;

exec [dbo].[sp_executeSQL] 
    @sql_agent_cmd
  , N'@sql_agent_category_name nvarchar(128)'
  , @sql_agent_category_name;

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

set @sql_agent_cmd = @sql_agent_template_job;

exec [dbo].[sp_executeSQL] 
    @sql_agent_cmd
  , N'@sql_agent_job_name nvarchar(128), @sql_agent_category_name nvarchar(128),@sql_agent_job_id uniqueidentifier output'
  , @sql_agent_job_name
  , @sql_agent_category_name
  , @sql_agent_job_id output;

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

set @sql_agent_cmd = @sql_agent_template_jobschedule;

exec [dbo].[sp_executeSQL] 
    @sql_agent_cmd
  , N'@sql_agent_schedule_name nvarchar(128), @sql_agent_job_id uniqueidentifier'
  , @sql_agent_schedule_name
  , @sql_agent_job_id;

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

set @sql_agent_cmd = @sql_agent_template_jobserver;

exec [dbo].[sp_executeSQL] 
    @sql_agent_cmd
  , N'@sql_agent_job_id uniqueidentifier'
  , @sql_agent_job_id;
