Print 'Loading Table: antoni_data';

INSERT INTO [dbo].[actions] 
	([title], [description], [site_id], [is_active]) 
VALUES 
	('action1', 'description1', 5, 1) 
 

INSERT INTO [dbo].[actions] 
([title], [description], [site_id], [is_active]) 
VALUES 
	('action2', 'description2', 15, 1) 


/***********************************************************************************************************************************************************************/

INSERT INTO [dbo].[medication_routes] 
	([name] ,[site_id]) 
VALUES
	('med_route1', 5)


INSERT INTO [dbo].[medication_routes] 
	([name] ,[site_id]) 
VALUES
	('med_route1', 15)
 

INSERT INTO [dbo].[medication_routes] 
	([name] ,[site_id]) 
VALUES
	('med_route1', 25)
 

/***********************************************************************************************************************************************************************/

INSERT INTO [dbo].[patient_orders]
	([patient_id], [add_datetime], [add_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
VALUES
	(1, '2020-06-19T17:00:00+05:00', 313, N'MED0001', 3, 0, 1, N'Pending', '2020-06-19T17:30:00+05:00', NULL, 456, 3, N'This is a humongous blob of text!!! ?a? ???a ????????!!!', 'tylenol')


INSERT INTO [dbo].[patient_orders]
	([patient_id], [add_datetime], [add_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
VALUES
	(1, '2020-06-19T17:10:00+05:00', 313, N'MED0002', 3, 1, 0, N'Pending', '2020-06-19T17:15:00+05:00', NULL, 444, 2, N'More text!!! ?? ???? ?e?µe??!!!', 'tylenol')


INSERT INTO [dbo].[patient_orders]
	([patient_id], [add_datetime], [add_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
VALUES
	(1, '2020-06-19T17:11:20+05:00', 342, N'MED4571', 2, 1, 1, N'Pending', '2020-06-19T17:11:30+05:00', NULL, 444, 2, N'More more text!!! ?? ???? ???? ?e?µe??!!!', 'tylenol')


INSERT INTO [dbo].[patient_orders]
	([patient_id], [add_datetime], [add_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
VALUES
	(2, '2020-06-20T08:00:00+05:00', 313, N'MED9524', 1, 0, 0, N'Pending', '2020-06-20T08:00:23+05:00', NULL, 852, 3, N'Is this the end? ?d? e??a? t? t????;', 'aspirin')



select * From [dbo].[patient_orders]
/***********************************************************************************************************************************************************************/

INSERT INTO [dbo].[order_administrations]
([patient_order_id], [point_in_time], [on_hold], [missed_dose], [administration_scheduled_datetime], [administration_input_datetime], [administration_datetime], [administering_user_id], [stop_scheduled_datetime], [stop_input_datetime], [stop_datetime], [stop_user_id], [acknowledge_user_id], [acknowledge_datetime])
VALUES
(1,0,0,0,'2020-06-19T17:45:00+05:00','2020-06-19T17:44:28+05:00','2020-06-19T17:44:30+05:00',123,'2020-06-19T17:59:23+05:00','2020-06-19T18:00:00+05:00','2020-06-19T18:00:03+05:00',456,789,'2020-06-19T17:44:35+05:00')


INSERT INTO [dbo].[order_administrations]
([patient_order_id], [point_in_time], [on_hold], [missed_dose], [administration_scheduled_datetime], [administration_input_datetime], [administration_datetime], [administering_user_id], [stop_scheduled_datetime], [stop_input_datetime], [stop_datetime], [stop_user_id], [acknowledge_user_id], [acknowledge_datetime])
VALUES
(1,0,0,0,'2020-06-19T17:45:00+05:00','2020-06-19T17:44:28+05:00','2020-06-19T17:44:30+05:00',123,'2020-06-19T17:59:23+05:00','2020-06-19T18:00:00+05:00','2020-06-19T18:00:03+05:00',456,789,'2020-06-19T17:44:35+05:00')


/***********************************************************************************************************************************************************************/

INSERT INTO [dbo].[order_events]
([patient_order_id], [order_administration_id], [event_datetime], [add_datetime], [add_user_id], [action_id])
VALUES
(1,1,'2020-06-19T17:02:33+05:00','2020-06-19T17:02:30+05:00',1,1)


INSERT INTO [dbo].[order_events]
([patient_order_id], [order_administration_id], [event_datetime], [add_datetime], [add_user_id], [action_id])
VALUES
(1,NULL,'2020-06-19T17:17:17+05:00','2020-06-19T17:17:16+05:00',1,2)


INSERT INTO [dbo].[order_events]
([patient_order_id], [order_administration_id], [event_datetime], [add_datetime], [add_user_id], [action_id])
VALUES
(1,1,'2020-06-19T17:17:23+05:00','2020-06-19T17:17:17+05:00',1,1)


