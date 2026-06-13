CREATE PROCEDURE [dbo].[add_missing_med_doc_to_edpc]
	@ibexList VARCHAR(8000) = NULL,			-- Optional comma-delimited list of ibex numbers to process
	@ibexYearMonthDayStart CHAR(8) = NULL,	-- Optional YYYYMMDD string specifying year, month, and day of Triage date to start at
	@ibexYearMonthDayEnd CHAR(8) = NULL,	-- Optional YYYYMMDD string specifying year, month, and day of Triage date to end at

	-- NOTE: Only the list or the range should be provided. If both are provided, the procedure will not run. If 
	-- neither are provided, the procedure defaults to running against the previous day's data.

	@devMode BIT = 1						-- Dev mode flag for additional output and no actual updates to charts
AS
BEGIN

-- CONFIG OPTION: Ignore emar events that come before this date.
DECLARE @eventStartDate DATE = '2022-10-01';

/* CREATE THESE INDEXES BEFORE RUNNING SO THINGS ARE NOT TERRIBLY SLOW!
USE [ibex]
GO
CREATE NONCLUSTERED INDEX med_emar_patient_order_id_IDX
ON [dbo].[med] ([emar_patient_order_id])
INCLUDE ([losecs],[name])
GO

CREATE NONCLUSTERED INDEX emar_med_administrations_med_admin_type_IDX
ON [dbo].[emar_med_administrations] ([med_admin_type])
INCLUDE ([id],[losecs],[med_admin_date],[patient_order_id],[order_administrations_id])
GO

CREATE NONCLUSTERED INDEX hst_emr_pat_IDX
ON [dbo].[hst] ([emar_pat])
INCLUDE ([ibex],[site])
GO

USE [emar]
GO
CREATE NONCLUSTERED INDEX order_events_order_administration_id_IDX
ON [dbo].[order_events] ([order_administration_id],[event_datetime],[action_id])
INCLUDE ([id],[patient_order_id],[add_user_id],[add_datetime],[template_id])
GO

CREATE NONCLUSTERED INDEX order_event_details_order_event_id_IDX
ON [dbo].[order_event_details] ([order_event_id])
INCLUDE ([prompt_id],[prompt_text],[entered_text],[chart_markup])
GO
*/

-- Process starts here
SET NOCOUNT ON;

IF (LEN(ISNULL(@ibexList,'')) > 0 AND (LEN(ISNULL(@ibexYearMonthDayStart, '')) > 0 OR LEN(ISNULL(@ibexYearMonthDayEnd, '')) > 0))
	RETURN;

-- When not provided an ibex list, start date, or end date, default to running on patients triaged any time during
-- the previous day.
IF (LEN(ISNULL(@ibexList,'')) = 0 AND (LEN(ISNULL(@ibexYearMonthDayStart, '')) = 0 OR LEN(ISNULL(@ibexYearMonthDayEnd, '')) = 0))
BEGIN
	SET @ibexYearMonthDayStart = SUBSTRING(CONVERT(varchar, DATEADD(dd, -1, SYSDATETIMEOFFSET()), 112), 1, 8);
	SET @ibexYearMonthDayEnd = @ibexYearMonthDayStart;
END

DECLARE @rangeStart CHAR(14);
DECLARE @rangeEnd CHAR(14);
IF (LEN(ISNULL(@ibexYearMonthDayStart, '')) = 8)
BEGIN
	SET @rangeStart = @ibexYearMonthDayStart + '000000';
	SET @rangeEnd = @ibexYearMonthDayEnd + '235959';
	PRINT 'Running on ibex number range ' + @rangeStart + ' to ' + @rangeEnd + '...';
END

-- This is used to identify records created by this script.
DECLARE @audioFlag VARBINARY(MAX) = CONVERT(VARBINARY, '0x0100110101001101', 1);

DECLARE @InitialResults TABLE (
	ibex char(14),
	site int,
	losecs int,
	orderId int,
	medName varchar(max),
	medRoute varchar(max),
	orderStatus varchar(max),
	eventDateTime datetimeoffset,
	tzDiff int default(0),
	edPcUserId int,
	eventUserName nvarchar(100),
	actionName varchar(max),
	chartActionName varchar(max),
	promptId int,
	promptType varchar(max),
	promptText varchar(max),
	enteredText varchar(max),
	chartMarkup varchar(max),
	chartText varchar(max),
	groupSequence int,
	promptSequence int
);

DECLARE @ibexNumbers TABLE (
	ibex char(14),
	site int
);
IF (LEN(ISNULL(@ibexList, '')) > 0)
BEGIN
	INSERT INTO @ibexNumbers
	SELECT 
		ibex, site
	FROM
		ibex..hst
	WHERE
		ISNULL(emar_pat, 'N') = 'Y'
		AND ibex IN (
			SELECT CAST(item AS char(14)) FROM dbo.delimited_split_8k(@ibexList, ',')
		)
END
ELSE IF (LEN(ISNULL(@ibexYearMonthDayStart, '')) = 8)
BEGIN
	INSERT INTO @ibexNumbers
	SELECT 
		ibex, site
	FROM
		ibex..hst
	WHERE 
		ISNULL(emar_pat, 'N') = 'Y'
		AND (LEN(ISNULL(@ibexYearMonthDayStart, '')) = 8 AND ibex BETWEEN @rangeStart AND @rangeEnd) 
END

-- Find administrations in eMAR that don't seem to have matching documentation in the EDPC chart...
DECLARE @MissingDocumentation TABLE (
	site int,
	ibex char(14),
	emar_patient_id int,
	name varchar(500),
	edpc_administration_id int,
	event_time_1 char(6),
	event_time_2 char(6),
	med_losecs int,
	med_administration_losecs int,
	med_admin_date char(14),
	id int,
	patient_order_id int,
	order_administration_id int,
	event_datetime datetimeoffset,
	add_user_id int,
	add_datetime datetimeoffset,
	action_id int,
	template_id int
);
INSERT INTO @MissingDocumentation
SELECT a.* FROM (
	SELECT DISTINCT
		pat.site,
		pat.ibex,
		p.id as [emar_patient_id],
		med.name,
		ma.id as [edpc_administration_id],
		CASE WHEN ma.id IS NOT NULL AND ma.med_admin_date IS NOT NULL THEN
			SUBSTRING(ma.med_admin_date, 9, 2) + ':' + SUBSTRING(ma.med_admin_date, 11, 2)
		ELSE 
			FORMAT(DATEPART(hour,oe.event_datetime),'00') + ':' + FORMAT(DATEPART(minute,oe.event_datetime),'00')
		END as [event_time_1],
		FORMAT(DATEPART(hour,oe.event_datetime),'00') + ':' + FORMAT(DATEPART(minute,oe.event_datetime),'00') AS [event_time_2],
		med.losecs as [med_losecs],
		ma.losecs as [med_administration_losecs],
		ma.med_admin_date,
		oe.*
	FROM
				  emar..order_events oe
			 join emar..patient_orders po ON oe.patient_order_id = po.id
			 join emar..patients p ON p.id = po.patient_id
			 join emar..external_ids ev ON ev.internal_id = p.id AND ev.vendor = 'pulsecheck' AND ev.entity = 'patients'
			 join @ibexNumbers pat ON ev.external_id = CAST(pat.site AS VARCHAR)+ '|' + pat.ibex
		left join ibex..med med on med.emar_patient_order_id = oe.patient_order_id
		left join ibex..emar_med_administrations ma ON oe.order_administration_id = ma.order_administrations_id AND oe.patient_order_id = ma.patient_order_id 
				AND (
					(oe.action_id = 8 AND ma.med_admin_type = 'Give') OR 
					(oe.action_id = 7 AND ma.med_admin_type = 'FollowUp')
				)
	WHERE 
			oe.action_id IN (7,8)
		AND oe.order_administration_id IS NOT NULL
		AND oe.event_datetime >= @eventStartDate
		AND med.losecs NOT LIKE '%&%'
) a
	 JOIN @ibexNumbers pat ON pat.ibex = a.ibex AND pat.site = a.site
LEFT JOIN charting..archive_charts chart ON 
		chart.ibex = a.ibex 
	AND chart.site = a.site
	AND ISNULL(chart.losecs,'') NOT LIKE '%&%'
	-- Need to match (DISCONTINUE), etc ahead of the name, and F/Us that are writing 'VITAL SIGNS' to the part instead of the med name. WTF?
	-- Uses an "ends with" match on name to avoid matching "sodium chloride" to "sodium chloride intravenous", for exmaple.
	AND (chart.part LIKE '%' + a.name OR chart.part = 'VITAL SIGNS')
	AND (
		(chart.losecs = a.med_losecs) OR 
		(chart.losecs = a.med_administration_losecs) OR 
		(chart.table_xref = a.med_losecs) OR 
		(chart.table_xref = a.med_administration_losecs) OR
		(LEN(ISNULL(chart.losecs, '')) = 0 AND a.action_id = 7 AND chart.chart_xref = a.med_losecs)
	)
	AND chart.nct = 210
	AND (
		(chart.data like '%documented as given%' AND a.action_id = 8) OR 
		(chart.data like '%follow up%by:%' AND a.action_id = 7) OR 
		(chart.data like '%&%' AND a.action_id = 7 AND LEN(ISNULL(chart.losecs,'')) = 0 AND chart.chart_xref = a.med_losecs)
	)
	AND (
		(chart.data like '%' + a.event_time_1 + '%') OR
		(chart.data like '%' + a.event_time_2 + '%') OR 
		(a.med_admin_date IS NOT NULL AND chart.data like '%' + LTRIM(RTRIM(a.med_admin_date)) + '%')
	)
WHERE
		chart.id IS NULL

DECLARE @missingCount INT = (SELECT COUNT(1) FROM @MissingDocumentation);
PRINT 'Found ' + CAST(@missingCount AS VARCHAR(MAX)) + ' administrations in eMAR missing documentation in EDPC...';

-- Using the information stored in the previous table, find the information associated with these administrations to update the charts
insert into @InitialResults
select distinct
	med.ibex,
	med.site,
	med.losecs,
	emar_patient_order_id as [orderId],
	med.name as [medName],
	med.route as [medRoute],
	order_status as [orderStatus],
	oe.event_datetime as [eventDatetime],
	CASE WHEN o.site_use_dst = 'Y' THEN o.services_timezone - o.site_timezone ELSE 0 END,
	CAST(ev.external_id AS INT) as [edPcUserId],
	u.last_name + ', ' + u.first_name as [eventUserName],
	a.name as [actionName],
	CASE
		WHEN a.name = 'Give'				THEN 'Documented as given'
		WHEN a.name = 'Hold'				THEN 'Held'
		WHEN a.name = 'Unhold'				THEN 'Hold Canceled'
		WHEN a.name = 'Acknowledge'			THEN 'Acknowledged'
		WHEN a.name = 'Cancel'				THEN 'Canceled'
		WHEN a.name = 'Delete'				THEN 'Deleted'
		WHEN a.name = 'CoSign'				THEN 'Co-signed'
		WHEN a.name = 'OrderDiscontinue'	THEN 'Discontinue Ordered'
		WHEN a.name = 'CompleteDiscontinue' THEN 'Discontinued'
		WHEN a.name = 'MissedDose'			THEN 'Missed dose noted'
		WHEN a.name = 'Reschedule'			THEN 'Rescheduled'
		WHEN a.name = 'FollowUp'			THEN 'Follow Up'
		WHEN a.name = 'PharmVerification'	THEN 'Pharmacist Verified'
		ELSE ''
	END AS [chartActionName],
	oed.prompt_id as [promptId],
	p.prompt_type AS [promptType],
	oed.prompt_text as [promptText],
	oed.entered_text AS [enteredText],
	oed.chart_markup as [chartMarkup],
	CASE
		-- Text is a datetime offset - turn it into the format we use for writing to the chart
		WHEN TRY_PARSE(oed.entered_text AS datetimeoffset) IS NOT NULL THEN
			-- Abbreviated day of the week
			REPLACE(LEFT(DATENAME(weekday, PARSE(oed.entered_text AS datetimeoffset)), 3) + ' ' + 
			-- Abbreviated month, 2-digit day, 4-digit year
			CONVERT(varchar, PARSE(oed.entered_text AS datetimeoffset), 107), ',', '') + ' ' + 
			-- 2-digit hours:2-digit minutes, 24-hour format
			CONVERT(varchar(5),CONVERT(datetimeoffset, PARSE(oed.entered_text AS datetimeoffset), 0), 108)
		-- Boolean true. We won't add this to the text but we need it so we know to write the markup.
		WHEN oed.entered_text = 'true' THEN ''
		-- Otherwise use the resulting text
		ELSE oed.entered_text
	END AS [chartText],
	ISNULL(tp.sequence, 99) AS groupSequence,
	ISNULL(p.sequence, 99) AS promptSequence
from
			  ibex..med
		 join @MissingDocumentation m ON med.ibex = m.ibex AND med.site = m.site AND m.patient_order_id = med.emar_patient_order_id
		 join ibex..org o ON med.site = o.site
	left join emar..patient_orders on patient_orders.id=med.emar_patient_order_id
	left join emar..order_administrations oa on patient_orders.id = oa.patient_order_id
	left join emar..order_events oe on oe.order_administration_id = oa.id
	left join emar..order_event_details oed on oed.order_event_id = oe.id
	left join emar..users u on oe.add_user_id = u.id
	left join emar..external_ids ev ON ev.internal_id = u.id AND ev.vendor = 'pulsecheck' AND ev.entity = 'users'
	left join emar..actions a on oe.action_id = a.id
	left join emar..prompts p on oed.prompt_id = p.id
	left join emar..template_prompt_groups tp on p.prompt_group_id = tp.prompt_group_id AND tp.template_id = oe.template_id
where 
	ibex..med.status='a'
	and ibex..med.losecs NOT LIKE '%&%'
	and a.name in ('give', 'followup')
	and order_status in ('completed', 'discontinued', 'ongoing', 'onhold')
	and oe.event_datetime IS NOT NULL
	-- Remove boolean falses (which are not included in documentation)
	and (oed.entered_text IS NULL OR oed.entered_text <> 'false')
order by 
	med.ibex,
	oe.event_datetime,
	isnull(tp.sequence, 99),
	isnull(p.sequence, 99)

-- Generate chart markup for any entries that have prompts and text, but no markup.
-- TODO: This blanks out 'Information' and 'Label' types. But it looks like the C# code would
-- completely skip them anyway? What's up with those types?
UPDATE
	@initialResults
SET
	chartMarkup = (
		CASE 
			WHEN promptType IN ('DropDownListBox', 'threeStateButton')                         THEN '^S'
			WHEN promptType IN ('Checkbox')                                                    THEN '^D'
			WHEN promptType IN ('Date', 'DateTime', 'FreeText', 'MultilineFreeText', 'Notify') THEN '^C'
			ELSE NULL
		END
		) + promptText + '='
	WHERE
		LEN(ISNULL(chartMarkup, '')) = 0
;

-- Generate chart markup for special follow up entries that are build from the promptText.
-- Remove ' ', '(', ')', '~', and '-' from the promptText and use that as markup.
UPDATE
	@initialResults
SET
	chartMarkup = '^S' + REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(promptText, '-', ''), '~', ''), ')', ''), '(', ''), ' ', '') + '='
WHERE
		LEN(ISNULL(chartMarkup, '')) = 0
	AND actionName = 'FollowUp'
	AND promptType = 'DropDownListBox'
;

-- Set user names for any notification actions. Expects to be able to split enteredText on commas to get a list
-- of user IDs, then loook up the names for those IDs and join them back together. enteredText may or may not be
-- comma-delimited with multiple values.
DECLARE @userNameList VARCHAR(MAX);
DECLARE namesCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR 
	SELECT DISTINCT enteredText FROM @initialResults WHERE promptType = 'Notify' AND enteredText IS NOT NULL
OPEN namesCursor
FETCH NEXT FROM namesCursor INTO @userNameList
WHILE @@FETCH_STATUS = 0  
BEGIN
	DECLARE @fullNamesList VARCHAR(MAX) = NULL;
	SELECT 
		@fullNamesList = COALESCE(@fullNamesList + ',', '') + ISNULL(u.last_name + ', ' + u.first_name, '')
	FROM
		emar..users u
	WHERE
		id IN (
			SELECT item FROM dbo.delimited_split_8k(@userNameList, ',')
		);

	UPDATE @initialResults SET enteredText = @fullNamesList WHERE promptType = 'Notify' AND enteredText = @userNameList;

	FETCH NEXT FROM namesCursor INTO @userNameList
END
CLOSE namesCursor
DEALLOCATE namesCursor

-- Clear out any prompts that we don't typically write to the chart.
UPDATE
	@initialResults
SET
	chartMarkup = '', 
	chartText =  ''
WHERE (
	-- These are skipped because they are already included in the entry
	(promptText IN ('At', 'Documented At', 'Given At') AND TRY_PARSE(enteredText AS datetimeoffset) IS NOT NULL)
	OR
	-- These are skipped because the code below still processes them and we don't want to.
	(chartMarkup = '^Con=' AND ISNULL(chartText,'') = '' AND actionName = 'FollowUp')
);

DELETE FROM @initialResults WHERE RIGHT(chartMarkup, 1) = '=' AND chartText = '';

IF (@devMode = 1)
BEGIN
	SELECT * FROM @initialResults ORDER BY ibex, losecs, eventDateTime, groupSequence, promptSequence;
END

-- Start processing the resulting admins 
DECLARE @ibex char(14);
DECLARE @losecs int;
DECLARE @orderId int;
DECLARE @eventDateTime datetimeoffset;
DECLARE @previousLosecs int;
DECLARE @previousAdminData VARCHAR(MAX);

DECLARE @changedEntries TABLE (
	site int,
	ibex char(14),
	med_name varchar(500),
	action_type varchar(100),
	eventDateTime datetimeoffset
);

DECLARE resultCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR 
	SELECT DISTINCT ibex, losecs, orderId, eventDateTime FROM @initialResults ORDER BY ibex, losecs, eventDateTime
OPEN resultCursor
FETCH NEXT FROM resultCursor INTO @ibex, @losecs, @orderId, @eventDateTime
WHILE @@FETCH_STATUS = 0  
BEGIN
	-- When we see a new losecs, reset the admin data we are keeping track of.
	-- THIS IS DEV-ONLY BECAUSE WE PULL THIS FROM THE DATABASE IN PROD (SEE BELOW)
	IF (@devMode = 1)
	BEGIN
		IF (ISNULL(@previousLosecs,0) <> @losecs)
			SET @previousAdminData = null;
	END

	DECLARE @userName VARCHAR(100);
	DECLARE @chartActionName VARCHAR(100);
	DECLARE @actionName VARCHAR(100);
	DECLARE @data VARCHAR(MAX) = '';
	DECLARE @chartTime VARCHAR(50);
	DECLARE @edPcUserId INT;
	DECLARE @entrySystemTime CHAR(14);
	DECLARE @entryUserTime CHAR(14);
	DECLARE @currentSystemTime CHAR(14) = 
		CONVERT(varchar, SYSDATETIMEOFFSET(), 112) + REPLACE(CONVERT(varchar, SYSDATETIMEOFFSET(), 108), ':', '');

	-- With the results of the previous query and manipulations, escape and join together the chartMarkup and chartText 
	-- values for storing in the chart.
	SELECT
		@userName = eventUserName,
		@edPcUserId = edPcUserId,
		@actionName = actionName,
		@chartActionName = chartActionName,
		@chartTime = 
			REPLACE(LEFT(DATENAME(weekday, DATEADD(hh, tzDiff, eventDateTime)), 3) + ' ' + 
			-- Abbreviated month, 2-digit day, 4-digit year
			CONVERT(varchar, eventDateTime, 107), ',', '') + ' ' + 
			-- 2-digit hours:2-digit minutes, 24-hour format
			CONVERT(varchar(5),CONVERT(datetimeoffset, DATEADD(hh, tzDiff, eventDateTime), 0), 108),
		@entrySystemTime =  CONVERT(varchar, eventDateTime, 112) + REPLACE(CONVERT(varchar, eventDateTime, 108), ':', ''),
		@data = COALESCE(@data + '&' + CASE WHEN LEN(ISNULL(chartText,'')) > 0 THEN chartMarkup + 
		CASE
			-- When chart markup appears to expect to be followed by user input,
			-- perform the following escapes on the user input before appending it:
			-- < -> <LT>
			-- & -> <AMP>
			-- | -> <PIPE>
			-- ^ -> CARET
			-- newline + linefeed -> <LF>
			-- newline -> <LF>
			-- linefeed -> <LF>
			WHEN 
				RIGHT(chartMarkup, 1) = '=' AND
				PromptText NOT IN ('BP (Systolic)', 'BP (Diastolic)', 'MAP', 'Pulse', 'Temperature', 'O2 SAT', 'Respiratory', 'Pain', 'End-Tidal CO2') THEN 
				REPLACE(
					REPLACE(
						REPLACE(
							REPLACE(
								REPLACE(
									REPLACE(
										REPLACE(
											chartText, '<', '<LT>'
										),
										'&', '<AMP>'
									), 
									'|', '<PIPE>'
								), 
								'^', '<CARET>'
							),
							CHAR(13) + CHAR(10), '<LF>'
						), 
						CHAR(13), '<LF>'
					), 
					CHAR(10), '<LF>'
				)
			ELSE ''
		END WHEN LEN(ISNULL(chartMarkup, '')) > 0 AND chartText IS NOT NULL THEN chartMarkup ELSE '' END, '')
	FROM 
		@initialResults
	WHERE
			ibex = @ibex
		AND losecs = @losecs
		AND eventDateTime = @eventDateTime
	ORDER BY
		groupSequence,
		promptSequence

	-- Fix errors and clear out empty data
	WHILE(CHARINDEX('&&', @data) > 0)
	BEGIN
		SET @data = REPLACE(@data, '&&', '&');
	END

	IF (@data = '&')
		SET @data = '';

	IF (RIGHT(@data, 1) = '&')
		SET @data = LEFT(@data, LEN(@data) - 1);

	DECLARE @alternateChartTimeString VARCHAR(50);
	-- Get user-entered time from event.
	-- eMAR seems to use the last value it sees, so sort our data in reverse to find what eMAR would use.
	SELECT TOP 1
		@alternateChartTimeString = CASE 
			WHEN LEN(ISNULL(chartText, '')) = 0 AND TRY_PARSE(enteredText AS datetimeoffset) IS NOT NULL THEN 
				REPLACE(LEFT(DATENAME(weekday, PARSE(enteredText AS datetimeoffset)), 3) + ' ' + 
				-- Abbreviated month, 2-digit day, 4-digit year
				CONVERT(varchar, PARSE(enteredText AS datetimeoffset), 107), ',', '') + ' ' + 
				-- 2-digit hours:2-digit minutes, 24-hour format
				CONVERT(varchar(5), CONVERT(datetimeoffset, enteredText, 0), 108)				
			ELSE 
				chartText 
			END,
		@entryUserTime = CONVERT(varchar, PARSE(enteredText as datetimeoffset), 112) + REPLACE(CONVERT(varchar, PARSE(enteredText as datetimeoffset), 108), ':', '')
	FROM
		@initialResults
	WHERE
			ibex = @ibex
		AND losecs = @losecs
		AND eventDateTime = @eventDateTime
		AND ISNULL(enteredText, '') <> ''
		AND ((promptType = 'DateTime') OR promptType IN('At', 'Documented At', 'Given At'))
	ORDER BY
		groupSequence DESC, promptSequence DESC
		
	IF (@entryUserTime IS NULL)
		SET @entryUserTime = @entrySystemTime;

	PRINT '** Processing ''' + CAST(@actionName AS VARCHAR(MAX)) + ''' admin on ' + @ibex + ' / ' + CAST(@losecs AS VARCHAR(MAX)) + ' / ' + CONVERT(varchar, @eventDateTime, 0) + ' **';

	-- Look up most recent previous admin data so we have something to build from.
	-- THIS SHOULD NOT RUN EVERY LOOP IN DEV. ONLY PRODUCTION
	DECLARE @previousPart VARCHAR(256);
	DECLARE @newPart VARCHAR(256);
	DECLARE @previousEntrySystemTime CHAR(14);
	DECLARE @site INT = NULL;
	DECLARE @sourceLineId INT = NULL;
	IF (@devMode = 0 OR (@devMode = 1 AND @previousAdminData IS NULL))
	BEGIN
		IF (@devMode = 0)
		BEGIN
			SET @previousAdminData = '';
		END
		SELECT TOP 1
			@sourceLineId = id,
			@site = [site],
			@previousAdminData = [data],
			@previousPart = part,
			@previousEntrySystemTime = sys_time
		FROM
			charting..archive_charts
		WHERE
				ibex = @ibex
			AND nct = 210
			AND losecs = CAST(@losecs AS VARCHAR(MAX))
			AND inactive_time IS NULL
			AND data LIKE '%by:%'
			AND ISNULL(user_time, sys_time) <= ISNULL(@entryUserTime, @entrySystemTime)
		ORDER BY
			ISNULL(user_time, sys_time) DESC, id DESC;
	END

	-- Find site and med name from med entry in case we didn't find it yet.
	IF (@site IS NULL OR @previousPart IS NULL)
	BEGIN
		SELECT TOP 1 @site = site, @previousPart = [name] FROM ibex..med WHERE ibex = @ibex AND losecs = @losecs;
	END

	IF (@previousEntrySystemTime IS NULL)
	BEGIN
		SET @previousEntrySystemTime = @entrySystemTime;
	END

	--DECLARE @inactivationLineNum INT;
	--SELECT @inactivationLineNum = COUNT(1) FROM charting..archive_charts WHERE ibex=@ibex AND nct <> -1 AND id < @sourceLineId AND ISNULL([status],'A') <> 'I';

	IF (@devMode = 1)
	BEGIN
		PRINT '  EMAR order ID: ' + CAST(@orderId AS VARCHAR(MAX));
		PRINT '  Source line ID: ' + CAST(@sourceLineId AS VARCHAR(MAX));
		--PRINT '  Inactivation line: ' + CAST(@inactivationLineNum AS VARCHAR(MAX));
	END

	-- Change part prefix if necessary, based on action
	DECLARE @inactiveUser INT;
	DECLARE @inactiveTime CHAR(14);

	SET @newPart = @previousPart;
	IF (@actionName IN ('Cancel', 'CompleteDiscontinue', 'Delete', 'OrderDiscontinue'))
	BEGIN
		SET @newPart = REPLACE(REPLACE(REPLACE(REPLACE(@previousPart, '(DISCONTINUE) ', ''), '(DELETED) ', ''), '(DISCONTINUED) ', ''), '(CANCELED) ', '')
		IF (@actionName = 'Cancel')
		BEGIN
			SET @newPart = '(CANCELED) ' + @newPart;
		END
		ELSE IF (@actionName = 'CompleteDiscontinue')
		BEGIN
			SET @newPart = '(DISCONTINUED) ' + @newPart;
		END
		ELSE IF (@actionName = 'Delete')
		BEGIN
			SET @newPart = '(DELETED) ' + @newPart;
		END
		ELSE IF (@actionName = 'OrderDiscontinue')
		BEGIN
			SET @newPart = '(DISCONTINUE) ' + @newPart;
		END

		SET @inactiveUser = @edpcUserId;
		SET @inactiveTime = @entrySystemTime;
	END
	
	-- We bold follow up for some reason...
	IF (@chartActionName LIKE '%follow%')
	BEGIN
		SET @chartActionName = '<LT>b>Follow Up<LT>/b>';
	END

	IF (@devMode = 1)
	BEGIN
		PRINT '  Alternate chart time string: ' + ISNULL(@alternateChartTimeString, '');
		PRINT '  Current chart time string: ' + @chartTime;
	END

	IF (LEN(ISNULL(@alternateChartTimeString, '')) > 0)
		SET @chartTime = @alternateChartTimeString;

	DECLARE @userAttributionAndTime VARCHAR(MAX) = @chartActionName + ' by: ' + @userName + ' ' + @chartTime;
	DECLARE @entry VARCHAR(MAX) = @userAttributionAndTime + '&^s=' + @data;
	DECLARE @newData VARCHAR(MAX) = '&^s<LF>=' + @entry;

	IF (@previousAdminData IS NULL)
		SET @previousAdminData = '';

	PRINT '  Previous admin data: ' + @previousAdminData;
	--PRINT 'New entry: ' + @entry;
	DECLARE @isDuplicated BIT = 0;
	
	DECLARE @insertData VARCHAR(MAX);
	SET @insertData = CASE WHEN @newData LIKE '%follow up%' THEN @newData ELSE @previousAdminData + @newData END;

	DECLARE @multipleGives BIT = 0;
	IF (@entry LIKE 'Documented as given%' AND @previousAdminData LIKE '%Documented as given%')
	BEGIN
		SET @multipleGives = 1;
	END

	DECLARE @duplicateLine VARCHAR(MAX);
	IF (@insertData LIKE '^s=Order%')
	BEGIN
		IF (@entry LIKE 'Documented as given%')
		BEGIN
			-- Find 'Documented as given' in previous admin and remove everything before it.
			DECLARE @comparisonData VARCHAR(MAX) = @previousAdminData;
			DECLARE @docIndex INT = CHARINDEX('Documented as given', @comparisonData);
			IF (@docIndex >= 1)
			BEGIN
				SET @comparisonData = SUBSTRING(@comparisonData, @docIndex, LEN(@comparisonData) - (@docIndex - 1));
			END
			ELSE
			BEGIN
				SELECT TOP 1 @comparisonData = [data] FROM charting..archive_charts WHERE ibex = @ibex AND site = @site AND nct = 210 AND losecs = CAST(@losecs AS VARCHAR(MAX)) AND inactive_time IS NULL AND [data] LIKE '%Documented as given%' ORDER BY sys_time DESC, id DESC;
				IF (@comparisonData IS NOT NULL)
				BEGIN
					SET @docIndex = CHARINDEX('Documented as given', @comparisonData);
					IF (@docIndex >= 1)
					BEGIN
						SET @comparisonData = SUBSTRING(@comparisonData, @docIndex, LEN(@comparisonData) - @docIndex);
					END
				END
			END

			-- Find any other sorts of admins tacked on to this one and strip them out.
			-- This handles most <LF> admins as well as the bolded Follow Up.
			SET @docIndex = CHARINDEX('&^s=<', @comparisonData);
			WHILE(@docIndex > 1)
			BEGIN
				SET @comparisonData = SUBSTRING(@comparisonData, 1, @docIndex - 1);
				SET @docIndex = CHARINDEX('&^s=<', @comparisonData);
			END

			-- Remove co-signs.
			SET @docIndex = CHARINDEX('&^s=Co-signed', @comparisonData);
			WHILE(@docIndex > 1)
			BEGIN
				SET @comparisonData = SUBSTRING(@comparisonData, 1, @docIndex - 1);
				SET @docIndex = CHARINDEX('&^s=Co-signed', @comparisonData);
			END

			--TODO: Remove other types of actions?

			DECLARE @str1 VARCHAR(8000) = dbo.strip_chart_markup_from_string(@entry, 1);
			DECLARE @str2 VARCHAR(8000) = dbo.strip_chart_markup_from_string(@comparisonData, 1);
			IF (@str1 = @str2)
			BEGIN
				SET @isDuplicated = 1;
				SET @duplicateLine = @previousAdminData;
			END
			ELSE
			BEGIN
				IF (@devMode = 1)
				BEGIN
					PRINT 'Comparison strings did not match:';
					PRINT @entry;
					PRINT @str1;
					PRINT '--vs--'
					PRINT @comparisonData;
					PRINT @str2;
				END
			END
		END
		ELSE
		BEGIN
			IF (LEN(ISNULL(@previousAdminData, '')) > 0 AND CHARINDEX(@entry, @previousAdminData) > 0)
			BEGIN
				SET @isDuplicated = 1;
				SET @duplicateLine = @previousAdminData;
			END
			ELSE
			BEGIN
				DECLARE @previousFollowup INT = CHARINDEX('<LT>b>Follow Up', @previousAdminData);
				IF (@previousFollowup >= 1)
				BEGIN
					PRINT '  New entry:        ' + @entry;
					DECLARE @followupCompare VARCHAR(8000) = SUBSTRING(@previousAdminData, @previousFollowup, LEN(@previousAdminData) - @previousFollowup);
					SET @docIndex = CHARINDEX('&^s=<LF>', @followupCompare);
					IF (@docIndex > 1)
					BEGIN
						SET @followupCompare = SUBSTRING(@followupCompare, 1, @docIndex - 1);
					END
					ELSE
					BEGIN
						SET @docIndex = CHARINDEX('&^s=Co-signed', @comparisonData);
						IF (@docIndex > 1)
							SET @comparisonData = SUBSTRING(@comparisonData, 1, @docIndex - 1);
					END
					IF (@devMode = 1)
					BEGIN
						PRINT 'Followup compare: ' + @followupCompare;
					END
					SET @str1 = dbo.strip_chart_markup_from_string(@entry, 1);
					SET @str2 = dbo.strip_chart_markup_from_string(@followupCompare, 1);
					IF (@str1 = @str2)
					BEGIN
						SET @isDuplicated = 1;
						SET @duplicateLine = @previousAdminData;
					END
					ELSE
					BEGIN
						IF (@devMode = 1)
						BEGIN
							PRINT 'Comparison strings did not match';
							PRINT @entry;
							PRINT @str1;
							PRINT '--vs--';
							PRINT @followupCompare;
							PRINT @str2;
						END
					END
				END
			END
		END
	END
	ELSE
	BEGIN
		-- Special handling for multiple ways we've been writing "Follow Up" to the chart. Plain, bolded, and bolded with escaping...
		IF (@chartActionName LIKE '%follow%')
		BEGIN
			IF (EXISTS(SELECT 1 FROM charting..archive_charts WHERE ibex = @ibex AND site = @site AND nct=210 AND (losecs = CAST(@losecs AS VARCHAR(MAX)) OR table_xref = @losecs) AND part = @newPart AND data LIKE '%follow up%by: ' + @userName + ' ' + @chartTime + '%'))
			BEGIN
				SET @duplicateLine = (SELECT TOP 1 data FROM charting..archive_charts WHERE ibex = @ibex AND site = @site AND nct=210 AND (losecs = CAST(@losecs AS VARCHAR(MAX)) OR table_xref = @losecs) AND part = @newPart AND data LIKE '%follow up%by: ' + @userName + ' ' + @chartTime + '%')
				SET @isDuplicated = 1;
			END			
		END
		ELSE
		BEGIN
			IF (EXISTS(SELECT 1 FROM charting..archive_charts WHERE ibex = @ibex AND site = @site AND nct=210 AND table_xref = @losecs AND part = @newPart AND data LIKE '%' + @userAttributionAndTime + '%'))
			BEGIN
				SET @duplicateLine = (SELECT TOP 1 data FROM charting..archive_charts WHERE ibex = @ibex AND site = @site AND nct=210 AND table_xref = CAST(@losecs AS INT) AND part = @newPart AND data LIKE '%' + @userAttributionAndTime + '%')
				SET @isDuplicated = 1;
			END
		END
	END

	IF (@multipleGives = 1)
	BEGIN
		PRINT '  This medication has been given multiple times';
		SET @newData = REPLACE(@newData, '&^s<LF>=', '^s=');
		SET @insertData = @newData;
		IF (EXISTS(SELECT 1 FROM charting..archive_charts WHERE ibex = @ibex AND site = @site AND nct=210 AND (losecs = CAST(@losecs AS VARCHAR(MAX)) OR table_xref = CAST(@losecs AS INT)) AND part = @newPart AND data LIKE '%' + @newData + '%'))
		BEGIN
			SET @duplicateLine = (SELECT TOP 1 data FROM charting..archive_charts WHERE ibex = @ibex AND site = @site AND nct=210 AND (losecs = CAST(@losecs AS VARCHAR(MAX)) OR table_xref = CAST(@losecs AS INT)) AND part = @newPart AND data LIKE '%' + @newData + '%');
			SET @isDuplicated = 1;
		END
	END

	-- Try to make sure we have not generated an entry that already exists or is missing data.
	IF (@isDuplicated = 0 AND (LEN(ISNULL(@data, '')) > 0 OR @entry NOT LIKE '^s=Order%'))
	BEGIN
		PRINT '  Generated documentation:';
		PRINT ' ' + @insertData;
		--PRINT '  Entry system time: ' + @entrySystemTime;

		-- Store this change in the table for output later
		INSERT INTO @changedEntries (site, ibex, med_name, action_type, eventDateTime) VALUES (
			@site, @ibex, @newPart, @actionName, @eventDateTime
		);

		-- ONLY IN DEV MODE: Update previous admin data to use in next iteration with matching identifiers.
		IF (@devMode = 1)
		BEGIN
			SET @previousAdminData = @insertData
		END
		ELSE
		-- ONLY IN PRODUCTION MODE: Store the new entry in the chart.
		BEGIN
			-- Normal behavior, order-level change
			IF (@insertData LIKE '^s=Order%')
			BEGIN
				-- Inactivate previous line
				IF (ISNULL(@sourceLineId, 0) > 0)
				BEGIN
					UPDATE charting..archive_charts SET [status] = 'I' WHERE id = @sourceLineId;
					-- Kept having issues doing inactivations the old "file" way, then realized "Oh! I'm using a real database. Let's make this easy."
					/*
					INSERT INTO charting..archive_charts
						(ibex, [site], sys_time, usr, audio, nct, section, part, [data], [data_source])
					VALUES (@ibex, @site, @currentSystemTime, @edPcUserId, @audioFlag, -1, '', '', @inactivationLineNum, 'E');
					*/
				END

				-- Create new line
				INSERT INTO charting..archive_charts
					(ibex, [site], sys_time, usr, losecs, audio, user_time, nct, section, part, [data], [data_source])
				VALUES (
					@ibex, @site, @previousEntrySystemTime, @edPcUserId, @losecs, @audioFlag, @entryUserTime, 210, 'MEDICATION SERVICE', @newPart, @insertData, 'E'
				)
			END
			-- Alternate behavior, admin-level change or multiple give, linking back to order.
			ELSE
			BEGIN
				DECLARE @newXLosecs INT = (SELECT MAX(CAST(losecs AS INT)) FROM charting..archive_charts WHERE ibex=@ibex AND site=@site AND losecs NOT LIKE '%&%');
				SET @newXLosecs = @newXLosecs + 1;
				INSERT INTO charting..archive_charts
					(ibex, [site], sys_time, usr, losecs, audio, user_time, table_xref, nct, section, part, [data], [data_source])
				VALUES (
					@ibex, @site, @previousEntrySystemTime, @edPcUserId, @newXLosecs, @audioFlag, @entryUserTime, @losecs, 210, 'MEDICATION SERVICE', @newPart, @newData, 'E'
				)
			END
		END
	END
	ELSE
	BEGIN
		IF (LEN(ISNULL(@data, '')) > 0) 
		BEGIN
			PRINT '  Skipping. This action seems to already exist in the chart:';
		END
		ELSE
		BEGIN
			PRINT '  Skipping. Generated data is empty:';
		END
		PRINT '  ' + @duplicateLine;
	END

	-- Now make sure med admin summary information is correct
	DECLARE @lastAdminLosecs INT;
	SELECT TOP 1 @lastAdminLosecs = losecs FROM ibex..emar_med_administrations WHERE ibex = @ibex AND site = @site AND patient_order_id = @orderId AND losecs NOT LIKE '%&%' ORDER BY losecs DESC;

	DECLARE @giveDate CHAR(14);
	SELECT TOP 1 @giveDate = give_date FROM ibex..med WHERE ibex = @ibex AND site = @site AND emar_patient_order_id = @orderId ORDER BY losecs DESC;

	-- Entries that meet this criteria need to exist in the emar_med_administrations table.
	DECLARE @giveCount INT = (SELECT COUNT(1) FROM emar..order_events oe JOIN emar..actions a ON oe.action_id = a.id AND a.name = 'Give' AND oe.patient_order_id = @orderId);
	--PRINT '  Give count from eMAR: ' + CAST(@giveCount AS VARCHAR(MAX));

	IF (((@giveDate IS NOT NULL AND @giveCount > 1) OR @actionName <> 'Give') OR (@giveCount > 1 AND @actionName = 'Give' AND ISNULL(@lastAdminLosecs, 0) > 0))
	BEGIN
		--PRINT '  Look for entry in emar_med_administrations table';
		DECLARE @currLosecs INT = CASE WHEN (ISNULL(@lastAdminLosecs, 0) > 0) THEN @lastAdminLosecs ELSE @losecs END;
		DECLARE @newLosecs INT = @currLosecs;
		
		-- TODO: Need to figure out @adminId.
		DECLARE @adminId INT;

		-- If this is a new order admin, get a new losecs value. Else use currLosecs
		DECLARE @newOrderAdmin BIT = 0;
		IF (@adminId IS NULL AND (@actionName = 'CoSign' OR @actionName = 'Cancel' OR @actionName = 'Delete' OR @actionName = 'OrderDiscontinue' OR @actionName = 'CompleteDiscontinue' OR @actionName = 'Hold' OR @actionName = 'UnHold'))
		BEGIN
			SET @newOrderAdmin = 0;
		END
		ELSE
		BEGIN
			IF (@adminId IS NULL)
			BEGIN
				SET @newOrderAdmin = 1;
			END
			ELSE
			BEGIN
				DECLARE @orderAdminId INT;
				SELECT 
					TOP 1 @orderAdminId = order_administration_id 
				FROM 
						 emar..order_events oe 
					JOIN emar..order_administrations oa ON oe.order_administration_id = oa.id
				WHERE 
					oe.patient_order_id = @orderId
				ORDER BY
					oa.administration_scheduled_datetime DESC;

				DECLARE @orderEventPresent BIT = 0;
				IF (ISNULL(@orderAdminId, 0) > 0)
					SET @orderEventPresent = 1;

				DECLARE @foundAdminId INT;
				SELECT 
					TOP 1 @foundAdminId = order_administration_id 
				FROM 
						 emar..order_events oe 
					JOIN emar..order_administrations oa ON oe.order_administration_id = oa.id
				WHERE 
					oe.patient_order_id = @adminId

				IF (ISNULL(@foundAdminId, 0) > 0)
					SET @foundAdminId = 1;

				IF (@orderEventPresent = 1 AND @foundAdminId <> 1)
					SET @newOrderAdmin = 1;
			END
		END

		IF (@newOrderAdmin = 1)
		BEGIN
			DECLARE @adminMatchCount INT = 0;
			-- Try to find an entry in the table that shows this action
			SELECT @adminMatchCount = COUNT(1) FROM ibex..emar_med_administrations WHERE patient_order_id = @orderId AND ibex = @ibex AND site = @site AND med_admin_type = @actionName;
			IF (ISNULL(@adminMatchCount, 0) < 1)
			BEGIN
				PRINT '    Need to create an emar_med_administrations record for this entry!';
			END
		END
	END

	-- Entries that meet this criteria exist in the med table, which is a given based on the previous queries. Here we just
	-- need to make sure the relevant fields are set properly.
	ELSE
	BEGIN
		IF (@actionName = 'Give')
		BEGIN
			IF (@giveDate IS NULL)
			BEGIN
				-- Try to find the give in the emar_med_administrations table first...
				SELECT TOP 1 @giveDate = med_admin_date FROM ibex..emar_med_administrations WHERE ibex = @ibex AND site = @site AND patient_order_id = @orderId AND med_admin_type = 'Give' AND med_admin_date IN(@entryUserTime, @entrySystemTime) ORDER BY losecs DESC;
				IF (LEN(ISNULL(@giveDate,'')) = 0)
				BEGIN
					IF (@devMode = 1)
					BEGIN
						PRINT '  Need to set give-related fields in med table for this entry!';
					END 
					ELSE
					BEGIN
						UPDATE
							ibex..med
						SET
							give_date = @entryUserTime,
							give_sysdate = @entrySystemTime,
							give_usr = @edPcUserId
						WHERE
								ibex = @ibex
							AND site = @site
							AND losecs = @losecs
							AND emar_patient_order_id = @orderId;
					END
				END
			END
		END
		ELSE
		BEGIN
			PRINT '  Need to set action-related fields in med table for this entry!';
		END
	END

	PRINT '';

	SET @previousLosecs = @losecs;

	FETCH NEXT FROM resultCursor INTO @ibex, @losecs, @orderId, @eventDateTime
END
CLOSE resultCursor
DEALLOCATE resultCursor

-- Now make sure all med give dates are unique for the purposes of charge calculation...
DECLARE @medUpdates TABLE (
	id INT,
	ibex CHAR(14),
	site TINYINT,
	old_give_date VARCHAR(14),
	new_give_date VARCHAR(14)
);

-- First find the list of patients who had at least one Hydration/Infusion/Injection given,
-- grouping ALL their given meds together by the YYYYMMDDHHMM portion of the give date (some meds only have these 12 digits)
-- and then filtering down to only those that have 2 or more meds with the same resulting give date
WITH initialResults AS (
SELECT 
	ibex, site, SUBSTRING(give_date,1,12) AS [give_date]
FROM 
	ibex..med 
WHERE 
	ibex IN (
		SELECT DISTINCT 
			ibex
		FROM
			ibex..med
		WHERE 
				[status] = 'A' 
			AND LEN(ISNULL(iv_type,'')) > 0 
			AND (
				-- Process all ibex numbers in a particular range
				(LEN(ISNULL(@ibexYearMonthDayStart, '')) = 8 AND ibex BETWEEN @rangeStart AND @rangeEnd) 
				OR
				-- Process all ibex numbers passed in
				(LEN(ISNULL(@ibexList, '')) > 0 AND ibex IN (SELECT ibex FROM @ibexNumbers))
			)
	)
	AND [status] = 'A'
	AND give_date IS NOT NULL
GROUP BY
	ibex, site, SUBSTRING(give_date,1,12)
HAVING 
	COUNT(1) > 1
)

-- Using the results from CTE, make a new give date for each of grouped entries by appending a new 2-digit seconds string
-- to the give_date. The new string is based on partitioning of records in the groups, so all give dates across all meds
-- for a particular patient will now be unique.
INSERT INTO @medUpdates (id, ibex, site, old_give_date, new_give_date)
SELECT
	m.id,
	m.ibex,
	m.site,
	m.give_date AS [old_give_date],
	SUBSTRING(m.give_date, 1, 12) + 
		RIGHT('0' + CAST(ROW_NUMBER() OVER(PARTITION BY m.ibex, m.site, SUBSTRING(m.give_date, 1, 12) ORDER BY m.give_date) AS VARCHAR), 2)
	AS [new_give_date]
FROM
		 ibex..med m
	JOIN initialResults i ON i.ibex = m.ibex AND i.site = m.site AND SUBSTRING(m.give_date, 1, 12) = i.give_date


-- Now run the resulting give_date updates on the med table for these patients.
IF (@devMode = 0)
BEGIN
	UPDATE
		m
	SET
		m.give_date = u.new_give_date
	FROM
			 ibex..med m
		JOIN @medUpdates u ON m.id = u.id
END

-- Show all admin-level changes made
SELECT * FROM @changedEntries;

-- Show distinct site/ibex combo for changes made
IF (@devMode = 1)
BEGIN
	SELECT DISTINCT site, ibex FROM @changedEntries;
END
ELSE
BEGIN
	-- This trigger often causes these update statements to fail, so disable it.
	ALTER TABLE ibex..hst DISABLE TRIGGER [emar_patients__hst_u]

	-- First clear out all patients in this range that were already flagged
	UPDATE 
		ibex..hst 
	SET 
		bilstatus2 = '' 
	WHERE 
			bilstatus2 = 'emar'
		AND (
			(LEN(ISNULL(@ibexYearMonthDayStart, '')) = 8 AND ibex BETWEEN @rangeStart AND @rangeEnd) 
			OR
			-- Process all ibex numbers passed in
			(LEN(ISNULL(@ibexList, '')) > 0 AND ibex IN (SELECT ibex FROM @ibexNumbers))
		);

	-- Then set the flag on the new patients we just determined.
	UPDATE
		h
	SET
		bilstatus2 = 'emar'
	FROM
			 @changedEntries c
		JOIN ibex..hst h ON c.site = h.site AND c.ibex = h.ibex;

	ALTER TABLE ibex..hst ENABLE TRIGGER [emar_patients__hst_u]
END

-- Show changes made to med give times
--IF (@devMode = 1)
--BEGIN
	SELECT * FROM @medUpdates;
--END

END;
go
