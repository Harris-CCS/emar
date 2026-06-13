/*
	This procedure currently generates notification records the primary nurse associated with a patient 
	when the patient's med orders meet any of the following criteria:

		* Overdue meds (notification_categories.code = 'PO')
			- one hour past scheduled administration time
			- should correspond with overdue icon on MARs
		* Pending (notification_categories.code = 'P')
			- one hour before scheduled admistration time
			- should correspond to Red M on tracking board
		* Follow-ups (notification_categories.code = 'FU')
			- one hour post administration (documented as given user time)
*/
CREATE procedure [dbo].[generate_notifications] 
as

begin

    set nocount on;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @AnHourAgo DATETIMEOFFSET(7) = DATEADD(hh, -1, @Now);
	DECLARE @AnHourFromNow DATETIMEOFFSET(7) = DATEADD(hh, 1, @Now);

	DECLARE @action_orders TABLE (
		patient_order_id BIGINT,
		patient_id BIGINT,
		medication_id INT,
		medication_unit_id INT,
		medication_route_id INT,
		order_administration_id BIGINT,
		action_datetime DATETIMEOFFSET(7),
		category_code VARCHAR(20)
	);

	DECLARE @action_patients TABLE (
		patient_id BIGINT,
		site_id INT,
		last_name NVARCHAR(35),
		first_name NVARCHAR(35),
		middle_name NVARCHAR(35),
		name_suffix NVARCHAR(25),
		recipient_user_id INT
	);

	-- We only send notifications for patients who have orders and assigned primary nurses
	INSERT INTO @action_patients 
	SELECT DISTINCT
		p.id,
		p.site_id,
		CASE WHEN LEN(LTRIM(RTRIM(ISNULL(p.last_name, '')))) = 1 THEN LTRIM(RTRIM(ISNULL(p.last_name, ''))) + '.' ELSE LTRIM(RTRIM(ISNULL(p.last_name, ''))) END,
		CASE WHEN LEN(LTRIM(RTRIM(ISNULL(p.first_name, '')))) = 1 THEN LTRIM(RTRIM(ISNULL(p.first_name, ''))) + '.' ELSE LTRIM(RTRIM(ISNULL(p.first_name, ''))) END,
		CASE WHEN LEN(LTRIM(RTRIM(ISNULL(p.middle_name, '')))) = 1 THEN LTRIM(RTRIM(ISNULL(p.middle_name, ''))) + '.' ELSE LTRIM(RTRIM(ISNULL(p.middle_name, ''))) END,
		LTRIM(RTRIM(ISNULL(p.name_suffix, ''))),
		up.user_id
	FROM 
			 patients p
		JOIN patient_orders po ON P.id = po.patient_id
		JOIN user_patients up ON p.id = up.patient_id AND up.role_name = 'NURSE1';

	-- Find ordered meds with a scheduled administration time at least one hour in the
	-- past, which do not have a documented administration time.
	INSERT INTO
		@action_orders 
	SELECT patient_order_id, patient_id, medication_id, medication_unit_id, medication_route_id, order_administration_id, action_datetime, category_code FROM (
		SELECT
			po.id AS [patient_order_id],
			po.patient_id,
			po.medication_id,
			po.medication_unit_id,
			po.medication_route_id,
			oa.id AS [order_administration_id],
			oa.administration_scheduled_datetime AS [action_datetime],
			'PO' AS [category_code],
			ROW_NUMBER() OVER(PARTITION BY po.id, po.patient_id ORDER BY oa.administration_scheduled_datetime DESC) AS [rn]
		FROM
				 patient_orders po
			JOIN @action_patients p ON po.patient_id = p.patient_id
			JOIN order_administrations oa ON po.id = oa.patient_order_id
		WHERE
				po.order_status NOT IN ('Cancelled','Deleted','PendingDiscontinue','Discontinued')
			AND @Now >= po.begin_datetime
			AND oa.administration_scheduled_datetime <= @AnHourAgo
			AND oa.administration_datetime IS NULL
	) r WHERE rn = 1;

	-- Find ordered meds with a scheduled administration time one hour (or less) in the 
	-- future, which do not have a documented administration time.
	INSERT INTO
		@action_orders
	SELECT patient_order_id, patient_id, medication_id, medication_unit_id, medication_route_id, order_administration_id, action_datetime, category_code FROM (
		SELECT
			po.id AS [patient_order_id],
			po.patient_id,
			po.medication_id,
			po.medication_unit_id,
			po.medication_route_id,
			oa.id AS [order_administration_id],
			oa.administration_scheduled_datetime AS [action_datetime],
			'PENDING' AS [category_code],
			ROW_NUMBER() OVER(PARTITION BY po.id, po.patient_id ORDER BY oa.administration_scheduled_datetime) AS [rn]
		FROM
				 patient_orders po
			JOIN @action_patients p ON po.patient_id = p.patient_id
			JOIN order_administrations oa ON po.id = oa.patient_order_id
		WHERE
				po.order_status NOT IN ('Cancelled','Deleted','PendingDiscontinue','Discontinued')
			AND oa.administration_scheduled_datetime >= @Now
			AND oa.administration_scheduled_datetime <= @AnHourFromNow
			AND oa.administration_datetime IS NULL
	) r WHERE rn = 1;

	-- Find ordered meds of certain statuses, which were given at least an hour ago but do not have a follow up
	-- event associated with them yet. No partitioning here because we want to notify on all of non-FU'd
	-- administrations for a med, not just the first instance we find.
	INSERT INTO
		@action_orders
	SELECT
		po.id AS [patient_order_id],
		po.patient_id,
		po.medication_id,
		po.medication_unit_id,
		po.medication_route_id,
		oa.id AS [order_administration_id],
		oa.administration_datetime AS [action_datetime],
		'FU' AS [category_code]
	FROM
			 patient_orders po
		JOIN @action_patients p ON po.patient_id = p.patient_id
		JOIN order_administrations oa ON po.id = oa.patient_order_id
		LEFT JOIN order_events oe ON oa.id = oe.order_administration_id AND oe.action_id = 7
	WHERE
			po.order_status NOT IN ('Cancelled','Deleted','Held')
		AND @Now >= po.begin_datetime
		AND oa.administration_datetime <= @AnHourAgo
		AND oe.event_datetime IS NULL;

	-- Take the @action_orders results and insert new records.
	-- Notifications that have already been generated but not acknowledged should not be touched, nor should we 
	-- create new notifications for the same entries that have been acknowledged. Although that should never happen.
	-- Only insert records where there is not already a matching 
	-- patient_order_id/order_administration_id/category_code/recipient_user_id.
	INSERT INTO
		notifications (recipient_user_id, patient_order_id, order_administration_id, category_code, event_datetime, generated_datetime, title, body)
	SELECT DISTINCT
		ap.recipient_user_id,
		ao.patient_order_id,
		ao.order_administration_id,
		ao.category_code,
		ao.action_datetime,
		@Now,
		-- Assemble patient name
		ap.first_name + 
			CASE WHEN LEN(ap.middle_name) > 0 THEN ' ' ELSE '' END +
			ap.middle_name + 
			CASE WHEN LEN(ap.last_name) > 0 THEN ' ' ELSE '' END + 
			ap.last_name + 
			CASE WHEN LEN(ap.name_suffix) > 0 THEN ', ' ELSE '' END +
			ap.name_suffix
		AS [patient_name],
		-- Assemble medication display name with medication unit and route
		m.display_name + 
			(CASE WHEN LEN(ISNULL(mu.name, '')) = 0 THEN '' ELSE '; ' + mu.name END) +
			(CASE WHEN LEN(ISNULL(mr.name, '')) = 0 THEN '' ELSE '; ' + mr.name END)
		AS [medication_name]
	FROM
			 @action_orders ao
		JOIN @action_patients ap ON ao.patient_id = ap.patient_id
		JOIN medications m ON ao.medication_id = m.id
		JOIN site_code_shares mus ON ap.site_id = mus.source_site_id AND mus.entity = 'medication_units'
		LEFT JOIN medication_units mu ON mu.site_id = mus.target_site_id AND mu.id = ao.medication_unit_id
		JOIN site_code_shares mrs ON ap.site_id = mrs.source_site_id AND mrs.entity = 'medication_routes'
		LEFT JOIN medication_routes mr ON mr.site_id = mrs.target_site_id AND mr.id = ao.medication_route_id
		LEFT JOIN notifications n ON 
			n.patient_order_id = ao.patient_order_id AND
			n.order_administration_id = ao.order_administration_id AND
			n.category_code = ao.category_code AND
			n.recipient_user_id = ap.recipient_user_id
	WHERE
		n.patient_order_id IS NULL

	-- For now: Send back a count of new notifications for logging purposes.
	DECLARE @NewNotifications INT = @@ROWCOUNT;

	-- In the future: Pull all notifications table entries that have a NULL sent_datetime and send them back as the results
	-- of this SP, so the caller can handle push notification sends in the future.
	-- For now just say all of these were sent now, but in the future probably the service would update these on an individual
	-- basis as the notification is actually sent out.
	UPDATE notifications SET sent_datetime = @Now WHERE sent_datetime IS NULL;

	SELECT @NewNotifications;
end

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Procedure Purpose: Generate eMAR notifications
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'PROCEDURE',@level1name=N'generate_notifications'
GO
