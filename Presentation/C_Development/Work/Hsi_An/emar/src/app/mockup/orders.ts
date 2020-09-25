import { Order } from '../interfaces/order';

export const ORDERS: Order[] = [{
	"id": 1,
	"patientId": 1,
	"startTime": "2019-05-13T10:00:00",
	"endTime": "2019-05-13T13:00:00",
	"name": "Motrin",
	"dose": "500mg",
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2019-05-13T10:08:45",
	"signedBy": "Merrily Turnbull, MD",
	"allergies": [1],
	"prn": false,
	"priority": "STAT",
	"orderStatus": "Ongoing",
	"missedDose": true,
	"orderAdministrations": [
        {
            "id": 1,
            "administrationScheduledDatetime": "2020-05-13T10:00:00",
            "administrationInputDatetime": "2020-05-13T10:00:00",
            "administrationDatetime": "2020-05-13T10:00:00",
            "administeringUserId": 123,
            "stopScheduledDatetime": "2020-05-13T10:05:00",
            "stopInputDatetime": "2020-05-13T10:05:00",
            "stopDatetime": "2020-05-13T10:05:00+05:00",
            "stopUserId": 456,
            "acknowledgeUserId": 789,
            "acknowledgeDatetime": "2020-05-13T17:44:35",
            "pointInTime": false,
            "onHold": false,
            "missedDose": false,
			"administrationStatus": "Given",
			"availableActions": [
				{
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/1/Action/CoSign"
				},
				{
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/1/Action/FollowUp"
				}
			],
        },
        {
            "id": 2,
			"administrationScheduledDatetime": "2020-05-13T11:00:00",
			"administrationDatetime": "",
			"missedDose": true,
			"acknowledgeUserId": 789,
            "acknowledgeDatetime": "2020-05-13T10:44:35",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/1/Action/Give"
				},
				{
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/1/Action/Hold"
				},
				{
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/1/Action/CoSign"
				},
				{
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/1/Action/Reschedule"
				}
			],
		},
		{
            "id": 3,
			"administrationScheduledDatetime": "2020-05-13T12:00:00",
			"administrationDatetime": "",
			"missedDose": true,
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/1/Action/Give"
				},
				{
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/1/Action/Hold"
				},
				{
					"buttonText": "Acknowledge",
					"link": "http://localhost:51044/api/orders/1/Action/Acknowledge"
				},
				{
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/1/Action/CoSign"
				},
				{
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/1/Action/Reschedule"
				}
			],
		},
		{
            "id": 4,
			"administrationScheduledDatetime": "2020-05-13T13:00:00",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/1/Action/Give"
				},
				{
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/1/Action/Hold"
				},
				{
					"buttonText": "Acknowledge",
					"link": "http://localhost:51044/api/orders/1/Action/Acknowledge"
				},
				{
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/1/Action/Reschedule"
				},
				{
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/1/Action/FollowUp"
				}
			],
        }
    ]
}, {
	"id": 2,
	"patientId": 1,
	"startTime": "2019-05-13T14:00:00",
	"endTime": "2019-05-13T14:30:00",
	"name": "Motrin",
	"dose": "600mg",
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"PRN"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2019-05-13T15:55:12",
	"signedBy": "Romel Ursua, MD",
	"priority": "STAT",
	"orderStatus": "Pending"
}, {
	"id": 3,
	"patientId": 2,
	"startTime": "2019-05-13T17:00:00",
	"endTime": "2019-05-13T17:30:00",
	"name": "Asprin",
	"dose": "100mg",
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2019-05-13T17:12:45",
	"signedBy": "Pete Turnbull, MD",
	"allergies": [1],
	"drugs": [1],
	"orderStatus": "Held",
	"orderAdministrations": [
		{
			"id": 1,
			"administrationScheduledDatetime": "2020-05-13T17:05:00",
			"pointInTime": false,
			"onHold": false,
			"missedDose": false,
			"administrationStatus": "Given",
			"availableActions": [
				{
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/1/Action/CoSign"
				},
				{
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/1/Action/FollowUp"
				}
			],
		},
		{
			"id": 2,
			"administrationScheduledDatetime": "2020-05-13T17:10:00",
			"pointInTime": false,
			"onHold": true,
			"missedDose": false,
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/1/Action/Give"
				},
				{
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/1/Action/Hold"
				},
				{
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/1/Action/CoSign"
				},
				{
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/1/Action/Reschedule"
				}
			],
		}
	]
}, {
	"id": 4,
	"patientId": 1,
	"startTime": "2019-05-13T10:00:00",
	"endTime": "2019-05-13T10:30:00",
	"name": "Motrin",
	"dose": "500mg",
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Every hour PRN"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2019-05-13T10:12:45",
	"signedBy": "Peter Turnbull, MD",
	"priority": "STAT",
	"prn": true,
	"orderStatus": "Ongoing",
	"orderAdministrations": [
	{
		"id": 1,
		"administrationScheduledDatetime": "2020-05-13T10:05:00",
		"administrationInputDatetime": "2020-05-13T10:05:00",
		"administrationDatetime": "2020-05-13T10:10:00",
		"administeringUserId": 123,
		"stopScheduledDatetime": "2020-05-13T10:10:00",
		"stopInputDatetime": "2020-05-13T10:10:00",
		"stopDatetime": "2020-05-13T10:10:00",
		"stopUserId": 456,
		"acknowledgeUserId": 789,
		"acknowledgeDatetime": "",
		"pointInTime": false,
		"onHold": false,
		"missedDose": false,
		"administrationStatus": "Given",
		"availableActions": [
			{
				"buttonText": "Co-sign",
				"link": "http://localhost:51044/api/orders/1/Action/CoSign"
			},
			{
				"buttonText": "Follow-up",
				"link": "http://localhost:51044/api/orders/1/Action/FollowUp"
			}
		],
	}]
}, {
	"id": 5,
	"patientId": 1,
	"startTime": "2019-05-13T14:00:00",
	"endTime": "2019-05-13T14:20:00",
	"name": "Morphine (PF) injection",
	"dose": "8mg",
	"route": "injection",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "IV"
	},
	"signedOn": "2019-05-13T10:15:12",
	"signedBy": "Merrily Turnbull, MD",
	"allergies": [],
	"drugs": [1]
}, {
	"id": 6,
	"patientId": 2,
	"startTime": "2019-05-13T17:00:00",
	"endTime": "2019-05-13T17:30:00",
	"name": "HYDROmorphone (Dilaudid)",
	"dose": "0.2mg",
	"route": "injection",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "IV"
	},
	"signedOn": "2019-05-13T17:12:45",
	"signedBy": "Peter Turnbull, MD",
	"allergies": [1],
	"drugs": [],
	pointInTime: false
}, {
	"id": 7,
	"patientId": 1,
	"startTime": "2019-05-13T10:00:00",
	"endTime": "2019-05-13T10:30:00",
	"name": "Ibuprofen (Advil, Motrin) 600MG tablet",
	"dose": "300mg",
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2019-05-14T11:13:45",
	"signedBy": "Romel Ursua, MD",
	"allergies": [1],
	"drugs": [1]
}, {
	"id": 8,
	"patientId": 1,
	"startTime": "2019-05-13T14:00:00",
	"endTime": "2019-05-13T14:30:00",
	"name": "Motrin",
	"dose": "500mg",
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2019-05-13T10:15:12",
	"signedBy": "Merrily Turnbull, MD",
	"allergies": [],
	"drugs": [1]
}, {
	"id": 9,
	"patientId": 2,
	"startTime": "2019-05-13T17:00:00",
	"endTime": "2019-05-13T17:30:00",
	"name": "Sodium Chloride 0.9% BOLUS",
	"dose": "20ml",
	"route": "IV",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "IV"
	},
	"signedOn": "2019-05-15T07:12:45",
	"signedBy": "Romel Ursua, MD",
	"allergies": [1],
	"drugs": [1]
}]
