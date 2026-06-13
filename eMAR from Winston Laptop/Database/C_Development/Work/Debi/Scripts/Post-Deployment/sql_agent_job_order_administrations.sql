select
    @sql_agent_job_name                  = 'EMAR_Generate_Order_Administrations'
  , @sql_agent_category_name             = 'EMAR_Maintenance'
  , @sql_agent_schedule_name             = 'EMAR_Daily_Orders'
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
          , @description = ''Daily Job to gernerate future order administrations''
          , @start_step_id = 1
          , @job_id = @sql_agent_job_id output;

        execute [dbo].[sp_add_jobstep] 
            @job_id = @sql_agent_job_id
          , @step_id = 1
          , @step_name = N''Generate Administrations''
          , @subsystem = N''TSQL''
          , @command = N''execute [dbo].[generate_order_administrations] 
      @override_offset           = null
    , @override_patient_order_id = null
    , @is_debug                  = null''
          , @database_name = N''emar'';
  
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
      , @freq_subday_interval = 3
      , @freq_relative_interval = 1
      , @freq_recurrence_factor = 0
      , @active_start_date = 20200101
      , @active_end_date = 99991231
      , @active_start_time = 20000
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

if '$(load_data)' = 'live'
   or '$(load_data)' = 'sample'
--- 
--- If data load is selected delete / rebuild sql agent job
---
    begin

        set @sql_agent_cmd = @sql_agent_template_job_category_delete;

        --do not delete category
        --exec [dbo].[sp_executeSQL] 
        --    @sql_agent_cmd
        --  , N'@sql_agent_category_name nvarchar(128)'
        --  , @sql_agent_category_name;

        set @sql_agent_cmd = @sql_agent_template_job_delete;

        exec [dbo].[sp_executeSQL] 
            @sql_agent_cmd
          , N'@sql_agent_job_name nvarchar(128)'
          , @sql_agent_job_name;

        set @sql_agent_cmd = @sql_agent_template_jobschedule_delete;

        exec [dbo].[sp_executeSQL] 
            @sql_agent_cmd
          , N'@sql_agent_schedule_name nvarchar(128)'
          , @sql_agent_schedule_name;
    end;

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/***********************************************
--  Dev testing to query script results

select '~~~~ syscategories ~~~~'
     , *
from   [msdb].[dbo].[syscategories]
where  [name] = @sql_agent_category_name;

select '~~~~ sysjobs ~~~~'
     , *
from   [msdb].[dbo].[sysjobs]
where  [name] = @sql_agent_job_name;

select '~~~~ sysjobsteps ~~~~'
     , *
from   [msdb].[dbo].[sysjobsteps]
where  [job_id] = @sql_agent_job_id;

select '~~~~ sysschedules ~~~~'
     , *
from   [msdb].[dbo].[sysschedules]
where  [name] = @sql_agent_schedule_name;
--***********************************************/
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