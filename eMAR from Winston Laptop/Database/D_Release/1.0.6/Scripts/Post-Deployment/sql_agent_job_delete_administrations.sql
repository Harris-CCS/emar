select
    @sql_agent_job_name                  = 'EMAR_Delete_Notifications'
  , @sql_agent_category_name             = 'EMAR_Maintenance'
  , @sql_agent_schedule_name             = 'Once an hour'
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
          , @description = ''This is an hourly job that will wait one hour to remove all administrations for an archived patient that are scheduled after archival in EMAR.''
          , @start_step_id = 1
          , @job_id = @sql_agent_job_id output;

        execute [dbo].[sp_add_jobstep] 
            @job_id = @sql_agent_job_id
          , @step_id = 1
          , @step_name = N''Step 1''
          , @subsystem = N''TSQL''
          , @command = N''DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
DECLARE @AnHourAgo DATETIMEOFFSET(7) = DATEADD(hh, -1, @Now);

-- Ensure that all temp tables do not exist.
DROP TABLE IF EXISTS [@action_data];
DROP TABLE IF EXISTS [@give_action_data];
DROP TABLE IF EXISTS [@grouped_data];
DROP TABLE IF EXISTS [@update_orders];
DROP TABLE IF EXISTS [@order_events];
DROP TABLE IF EXISTS [@order_admin_delete];

DECLARE @action_data TABLE (
    patient_order_id BIGINT,
    order_administration_id BIGINT,
    administering_user_id INT
);

DECLARE @give_action_data TABLE (
    patient_order_id BIGINT,
    order_administration_id BIGINT
);

DECLARE @grouped_data TABLE (
    ad_patient_order_id BIGINT,
    gad_patient_order_id BIGINT
);

DECLARE @update_orders TABLE (
    patient_order_id BIGINT
);

DECLARE @order_events TABLE (
    event_id BIGINT
);

DECLARE @order_admin_delete TABLE (
    order_administration_id BIGINT
);

-- get all the administration data that could be affected
-- the patient must be inactive, the deactivation time is more than one hour ago, and the scheduled admin
--   datetimes are after the deactivation datetime
INSERT INTO @action_data
SELECT po.id,oa.id,oa.administering_user_id FROM order_administrations oa
JOIN patient_orders po ON oa.patient_order_id=po.id
JOIN patients p ON po.patient_id=p.id
WHERE p.is_active != 1 AND p.deactivation_datetime < @AnHourAgo /*AND oa.administration_system_datetime IS NULL*/
    AND p.deactivation_datetime < oa.administration_scheduled_datetime ORDER BY po.id,oa.id;

-- get only the gives out of the action_data
INSERT INTO @give_action_data
SELECT patient_order_id,order_administration_id FROM @action_data
WHERE administering_user_id IS NOT NULL;

-- the next two sql statements are designed to determine the list of orders that should have their patient
--   order status set to complete. The first statement gets the unique combinations of action data and give
--   action data. The second statement uses this combination to determine which values in the first column do
--   not have a companion NULL in the second column. This has the net effect of determining which patient
--   orders (from the original @action_data list) have had all of their administrations given.
INSERT INTO @grouped_data
SELECT ad.patient_order_id,gad.patient_order_id FROM @action_data ad
LEFT JOIN @give_action_data gad ON gad.order_administration_id=ad.order_administration_id
GROUP BY ad.patient_order_id,gad.patient_order_id;

;with gd_cte as 
(SELECT gd.ad_patient_order_id FROM @grouped_data gd WHERE gd.gad_patient_order_id IS NULL)
INSERT INTO @update_orders
SELECT gd2.ad_patient_order_id FROM @grouped_data gd2
WHERE gd2.ad_patient_order_id NOT IN (SELECT ad_patient_order_id FROM gd_cte);

-- set the status to completed in the patient orders
UPDATE po SET po.order_status=''''Completed''''
FROM patient_orders po
JOIN @update_orders uo ON po.id = uo.patient_order_id
WHERE po.order_status !=''''Completed'''';

-- load the @order_admin_delete table with the @action_data that also has
-- a null administering_user_id and acknowledge_user_id
INSERT INTO @order_admin_delete
SELECT oa.id FROM order_administrations oa
JOIN @action_data ad ON oa.id=ad.order_administration_id
WHERE oa.administering_user_id IS NULL AND oa.acknowledge_user_id IS NULL; 

-- delete future notifications for inactive patients that have no actions 
DELETE n
FROM notifications n
JOIN @order_admin_delete oad ON n.order_administration_id=oad.order_administration_id;

-- load the @order_events table with the order_events id''''s that will be deleted
INSERT INTO @order_events
SELECT oe.id FROM order_events oe WHERE oe.order_administration_id IN
(SELECT oad.order_administration_id FROM @order_admin_delete oad
)

-- delete future order event details for inactive patients that have no actions 
DELETE oed
FROM order_event_details oed
JOIN @order_events oe ON oe.event_id=oed.order_event_id;

-- delete future order events for inactive patients that have no actions 
DELETE oe
FROM order_events oe
JOIN @order_admin_delete oad ON oe.order_administration_id=oad.order_administration_id;

-- delete future order administrations for inactive patients that have no actions 
DELETE oa
FROM order_administrations oa
JOIN @order_admin_delete oad ON oa.id=oad.order_administration_id;''
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
      , @freq_subday_type = 8
      , @freq_subday_interval = 1
      , @freq_relative_interval = 0
      , @freq_recurrence_factor = 0
      , @active_start_date = 20200101
      , @active_end_date = 99991231
      , @active_start_time = 0
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
  , @sql_agent_attach_schedule           = N'
use msdb;
if not exists
(
    select null
    from   [dbo].[sysjobschedules]
    where  [job_id] = @sql_agent_job_id
)
begin

    execute [dbo].[sp_attach_schedule] 
        @job_name = @sql_agent_job_name
      , @schedule_name = @sql_agent_schedule_name;

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

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

set @sql_agent_cmd = @sql_agent_attach_schedule;

exec [dbo].[sp_executeSQL] 
    @sql_agent_cmd
  , N'@sql_agent_schedule_name nvarchar(128), @sql_agent_job_name nvarchar(128), @sql_agent_job_id uniqueidentifier'
  , @sql_agent_schedule_name
  , @sql_agent_job_name
  , @sql_agent_job_id;

