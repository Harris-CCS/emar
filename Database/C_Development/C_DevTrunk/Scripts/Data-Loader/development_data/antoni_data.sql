if '$(load_data)' in('sample', 'live')
begin

print 'Loading Table: antoni_data';

insert into [dbo].[actions]([title], [description], [site_id], [is_active])
values('action1', 'description1', 5, 1);

insert into [dbo].[actions]([title], [description], [site_id], [is_active])
values('action2', 'description2', 15, 1); 

/***********************************************************************************************************************************************************************/

insert into [dbo].[medication_routes]([name], [site_id])
values('med_route1', 5);

insert into [dbo].[medication_routes]([name], [site_id])
values('med_route1', 15);

insert into [dbo].[medication_routes]([name], [site_id])
values('med_route1', 25);

/***********************************************************************************************************************************************************************/

insert into [dbo].[patient_orders]([patient_id], [add_datetime], [add_user_id], [order_physician_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
values(1, '2020-06-19T17:00:00+05:00', 313, 313, N'MED0001', 3, 0, 1, N'Pending', '2020-06-19T17:30:00+05:00', null, 456, 3, N'This is a humongous blob of text!!! ?a? ???a ????????!!!', 'tylenol');


insert into [dbo].[patient_orders]([patient_id], [add_datetime], [add_user_id], [order_physician_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
values(1, '2020-06-19T17:10:00+05:00', 313, 342, N'MED0002', 3, 1, 0, N'Pending', '2020-06-19T17:15:00+05:00', null, 444, 2, N'More text!!! ?? ???? ?e?µe??!!!', 'tylenol');


insert into [dbo].[patient_orders]([patient_id], [add_datetime], [add_user_id], [order_physician_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
values(1, '2020-06-19T17:11:20+05:00', 342, 313, N'MED4571', 2, 1, 1, N'Pending', '2020-06-19T17:11:30+05:00', null, 444, 2, N'More more text!!! ?? ???? ???? ?e?µe??!!!', 'tylenol');


insert into [dbo].[patient_orders]([patient_id], [add_datetime], [add_user_id], [order_physician_user_id], [drug_id], [priority], [prn], [point_in_time], [order_status], [begin_datetime], [end_datetime], [frequency_id], [medication_route_id], [order_notes], [brand_name])
values(2, '2020-06-20T08:00:00+05:00', 313, 313, N'MED9524', 1, 0, 0, N'Pending', '2020-06-20T08:00:23+05:00', null, 852, 3, N'Is this the end? ?d? e??a? t? t????;', 'aspirin');

/***********************************************************************************************************************************************************************/

insert into [dbo].[order_administrations]([patient_order_id], [point_in_time], [on_hold], [missed_dose], [administration_scheduled_datetime], [administration_input_datetime], [administration_datetime], [administering_user_id], [stop_scheduled_datetime], [stop_input_datetime], [stop_datetime], [stop_user_id], [acknowledge_user_id], [acknowledge_datetime])
values(1, 0, 0, 0, '2020-06-19T17:45:00+05:00', '2020-06-19T17:44:28+05:00', '2020-06-19T17:44:30+05:00', 123, '2020-06-19T17:59:23+05:00', '2020-06-19T18:00:00+05:00', '2020-06-19T18:00:03+05:00', 456, 789, '2020-06-19T17:44:35+05:00');


insert into [dbo].[order_administrations]([patient_order_id], [point_in_time], [on_hold], [missed_dose], [administration_scheduled_datetime], [administration_input_datetime], [administration_datetime], [administering_user_id], [stop_scheduled_datetime], [stop_input_datetime], [stop_datetime], [stop_user_id], [acknowledge_user_id], [acknowledge_datetime])
values(1, 0, 0, 0, '2020-06-19T17:45:00+05:00', '2020-06-19T17:44:28+05:00', '2020-06-19T17:44:30+05:00', 123, '2020-06-19T17:59:23+05:00', '2020-06-19T18:00:00+05:00', '2020-06-19T18:00:03+05:00', 456, 789, '2020-06-19T17:44:35+05:00');


/***********************************************************************************************************************************************************************/

insert into [dbo].[order_events]([patient_order_id], [order_administration_id], [event_datetime], [add_datetime], [add_user_id], [action_id])
values(1, 1, '2020-06-19T17:02:33+05:00', '2020-06-19T17:02:30+05:00', 1, 1);


insert into [dbo].[order_events]([patient_order_id], [order_administration_id], [event_datetime], [add_datetime], [add_user_id], [action_id])
values(1, null, '2020-06-19T17:17:17+05:00', '2020-06-19T17:17:16+05:00', 1, 2);


insert into [dbo].[order_events]([patient_order_id], [order_administration_id], [event_datetime], [add_datetime], [add_user_id], [action_id])
values(1, 1, '2020-06-19T17:17:23+05:00', '2020-06-19T17:17:17+05:00', 1, 1);



print 'Loading Table: antoni_data part II';

DECLARE @IdentityOutput TABLE (ID INT) 

--------------------------------------------------------------------------------------------------------------------

INSERT INTO [dbo].[patient_cart_orders]
([patient_id],[user_id],[add_datetime],[ndc],[drug_id],[brand_name],[dose],[medication_unit_id],[medication_route_id],[priority],[frequency_id],[prn],[point_in_time],[begin_datetime],[end_datetime],[order_notes],[user_quick_list_item_id])
OUTPUT inserted.id INTO @IdentityOutput
VALUES 
(56,1,'2020-09-22',NULL,'drug01','brandname01',NULL,NULL,NULL,3,NULL,1,1,'2020-09-22',NULL,NULL,NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 16:45:00',NULL)

DELETE @IdentityOutput

--------------------------------------------------------------------------------------------------------------------

INSERT INTO [dbo].[patient_cart_orders]
([patient_id],[user_id],[add_datetime],[ndc],[drug_id],[brand_name],[dose],[medication_unit_id],[medication_route_id],[priority],[frequency_id],[prn],[point_in_time],[begin_datetime],[end_datetime],[order_notes],[user_quick_list_item_id])
OUTPUT inserted.id INTO @IdentityOutput
VALUES
(56,1,'2020-09-22',NULL,'drug01','brandname01',NULL,NULL,NULL,3,NULL,1,1,'2020-09-22',NULL,NULL,NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 00:00:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 02:00:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 04:00:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 06:00:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 08:00:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 10:00:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 12:00:00',NULL)

DELETE @IdentityOutput

--------------------------------------------------------------------------------------------------------------------

INSERT INTO [dbo].[patient_cart_orders]
([patient_id],[user_id],[add_datetime],[ndc],[drug_id],[brand_name],[dose],[medication_unit_id],[medication_route_id],[priority],[frequency_id],[prn],[point_in_time],[begin_datetime],[end_datetime],[order_notes],[user_quick_list_item_id])
OUTPUT inserted.id INTO @IdentityOutput
VALUES
(56,1,'2020-09-22',NULL,'drug02','brandname02',NULL,NULL,NULL,3,NULL,1,1,'2020-09-22',NULL,NULL,NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 17:00:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),1,'2020-09-22 17:15:00',NULL)

DELETE @IdentityOutput

--------------------------------------------------------------------------------------------------------------------

INSERT INTO [dbo].[patient_cart_orders]
([patient_id],[user_id],[add_datetime],[ndc],[drug_id],[brand_name],[dose],[medication_unit_id],[medication_route_id],[priority],[frequency_id],[prn],[point_in_time],[begin_datetime],[end_datetime],[order_notes],[user_quick_list_item_id])
VALUES
(163,240,'2020-09-23',NULL,'drug03','brandname03',NULL,NULL,NULL,3,NULL,1,0,'2020-09-23',NULL,NULL,NULL)

--------------------------------------------------------------------------------------------------------------------

INSERT INTO [dbo].[patient_cart_orders]
([patient_id],[user_id],[add_datetime],[ndc],[drug_id],[brand_name],[dose],[medication_unit_id],[medication_route_id],[priority],[frequency_id],[prn],[point_in_time],[begin_datetime],[end_datetime],[order_notes],[user_quick_list_item_id])
OUTPUT inserted.id INTO @IdentityOutput
VALUES
(56,1,'2020-09-21',NULL,'drug04','brandname04',NULL,NULL,NULL,2,NULL,0,1,'2020-09-21',NULL,NULL,NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),0,'2020-09-20 12:30:00',NULL)

INSERT INTO [dbo].[cart_order_administrations]
([patient_cart_order_id],[point_in_time],[administration_scheduled_datetime],[stop_scheduled_datetime])
VALUES
((SELECT ID FROM @IdentityOutput),0,'2020-09-20 13:00:00',NULL)

DELETE @IdentityOutput

--------------------------------------------------------------------------------------------------------------------

INSERT INTO [dbo].[patient_cart_orders]
([patient_id],[user_id],[add_datetime],[ndc],[drug_id],[brand_name],[dose],[medication_unit_id],[medication_route_id],[priority],[frequency_id],[prn],[point_in_time],[begin_datetime],[end_datetime],[order_notes],[user_quick_list_item_id])
VALUES
(163,240,'2020-09-20',NULL,'drug05','brandname05',NULL,NULL,NULL,1,NULL,0,0,'2020-09-20',NULL,NULL,NULL)



/************************************************
Testing : name_display_initials
Default Values for Site Options Table
************************************************/

update [users] set    
    [name_display_initials] = 1
where  [id] in(5, 10, 15, 20, 25, 30, 35, 40, 45, 50);

/*********************************************************
Testing : PATIENT_IMAGE_PATH, CUSTOM_INDICATORS_IMAGE_PATH
Default Values for Site Options Table
*********************************************************/

update [target] set    
    [option_value] = '\\ros-57c-dx01.picis.com\E$\ibex\inc'
from   [dbo].[site_options] as [target]
       inner join [dbo].[options] [options] on [target].[option_id] = [options].[id]
       cross join
(
    select [id] from [get_internal_id] ('pulsecheck', 'sites', 19) union all
    select [id] from [get_internal_id] ('pulsecheck', 'sites', 23) union all
    select [id] from [get_internal_id] ('pulsecheck', 'sites',  1) union all
    select [id] from [get_internal_id] ('pulsecheck', 'sites', 36)
) [site]
where  [options].[name] = 'PATIENT_IMAGE_PATH'
       and [target].[site_id] = [site].[ID];

update [target] set    
    [option_value] = '\\ros-57c-dx01.picis.com\E$\git\pulsecheck\root\images\custom_indicators'
from   [dbo].[site_options] as [target]
       inner join [dbo].[options] [options] on [target].[option_id] = [options].[id]
       cross join
(
    select [id] from [get_internal_id] ('pulsecheck', 'sites', 19) union all
    select [id] from [get_internal_id] ('pulsecheck', 'sites', 23) union all
    select [id] from [get_internal_id] ('pulsecheck', 'sites',  1) union all
    select [id] from [get_internal_id] ('pulsecheck', 'sites', 36)
) [site]
where  [options].[name] = 'CUSTOM_INDICATORS_IMAGE_PATH'
       and [target].[site_id] = [site].[ID];


end;