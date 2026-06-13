import { Order } from '../interfaces/order';

export const ORDERS: Order[] = [{
	"id": 1,
	"patientId": 1,
	"beginDatetime": "2020-05-13T10:00:00",
	"endDatetime": "2020-05-13T13:00:00",
	"medication": {
		"id":129,
		"displayName": "Motrin"
	},
	"dose": "500",
	"doseUnit": {
		"unitName": "mg"
	},
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2020-05-13T10:08:45",
	"orderingPhysicianUser": {
		"id": 133,
		"displayName": "Merrily Turnbull, MD",
		"firstName": "Merrily",
		"lastName": "Turnbull"
	},
	"allergyReactions": [],
	"prn": false,
	"priority": "STAT",
	"orderStatus": "Ongoing",
	"orderNotes": "Important to give at the scheduled time",
	"addUser": {
		"id": 143,
		"firstName": "Joelle of a lonf first name",
		"lastName": "Doe of a long name"
	},
	"availableActions": [
		{
			"availableAction": "Cancel",
			"buttonText": "Cancel",
			"link": "http://ros-57c-dx01.picis.com:82/api/orders/1/actions/Cancel"	
		},
		{
			"availableAction": "Delete",
			"buttonText": "Delete",
			"link": "http://ros-57c-dx01.picis.com:82/api/orders/1/actions/Delete"
		},
		{
			"availableAction": "OrderDiscontinue",
			"buttonText": "Order Discontinue",
			"link": "http://ros-57c-dx01.picis.com:82/api/orders/1/actions/OrderDiscontinue"
		},
		{
			"availableAction": "Repeat",
            "buttonText": "Repeat",
            "link": "http://ros-57c-dx01.picis.com:82/api/orders/1/actions/Repeat"
		},
		{
			"availableAction": "CoSign",
            "buttonText": "Co-sign",
            "link": "http://ros-57c-dx01.picis.com:82/api/orders/1/actions/CoSign"
		}
	],
	"orderEvents": [
		{
			"id": 23,
			"eventDatetime": "2020-05-13T10:00:00",
			"user": {
				"id": 123,
				"firstName": "Joe",
				"lastName": "Doe"
			},
			"action": {
				"actionId": 5,
				"actionCode": "CoSign",
				"buttonText": "Co-Sign"
			}
		},
		{
			"id": 24,
			"eventDatetime": "2020-05-13T10:00:00",
			"user": {
				"id": 133,
				"firstName": "Peter",
				"lastName": "Smith"
			},
			"action": {
				"actionId": 5,
				"actionCode": "CoSign",
				"buttonText": "Co-Sign"
			}
		}
	],
	"orderAdministrations": [
        {
            "id": 1,
            "administrationScheduledDatetime": "2020-05-13T10:00:00",
            "administrationInputDatetime": "2020-05-13T10:00:00",
            "administrationDatetime": "2020-05-13T10:00:00",
            "administeringUser": {
				"id": 123,
				"firstName": "Joe",
				"lastName": "Doe"
			},
            "stopScheduledDatetime": "2020-05-13T10:05:00",
            "stopInputDatetime": "2020-05-13T10:05:00",
            "stopDatetime": "2020-05-13T10:05:00+05:00",
            "stopUserId": 456,
            "acknowledgeUser": {
				"id": 123,
				"firstName": "Joe",
				"lastName": "Doe"
			},
            "acknowledgeDatetime": "2020-05-13T17:44:35",
            "pointInTime": false,
            "onHold": false,
			"administrationStatus": "Given",
			"administrationEvents": [
				{
					"id": 23,
					"eventDatetime": "2020-05-13T10:02:00",
					"user": {
						"id": 123,
						"firstName": "Joe",
						"lastName": "Doe"
					},
					"action": {
						"actionId": 5,
						"actionCode": "CoSign",
						"buttonText": "Co-Sign"
					}
				},
				{
					"id": 24,
					"eventDatetime": "2020-05-13T10:02:00",
					"user": {
						"id": 124,
						"firstName": "Clarice",
						"lastName": "Doe"
					},
					"action": {
						"actionId": 5,
						"actionCode": "CoSign",
						"buttonText": "Co-Sign"
					}
				}
			],
			"availableActions": [
				{
					"availableAction": "CoSign",
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/administrations/1/actions/CoSign"
				},
				{
					"availableAction": "FollowUp",
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/administrations/1/actions/FollowUp"
				}
			],
        },
        {
            "id": 2,
			"administrationScheduledDatetime": "2020-05-13T11:00:00",
			"administrationDatetime": "",
			"acknowledgeUser":  {
				"id": 124,
				"firstName": "Billy",
				"lastName": "Joe"
			},
            "acknowledgeDatetime": "2020-05-13T10:44:35",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/administrations/2/actions/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/administrations/2/actions/Hold"
				},
				{
					"availableAction": "CoSign",
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/administrations/2/actions/CoSign"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/administrations/2/actions/Reschedule"
				},
				{
					"availableAction": "MissedDose",
					"buttonText": "Missed Dose",
					"link": "http://localhost:51044/api/orders/administrations/2/actions/MissedDose"
				}
			],
		},
		{
            "id": 3,
			"administrationScheduledDatetime": "2020-05-13T12:00:00",
			"administrationDatetime": "",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/administrations/3/actions/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/administrations/3/actions/Hold"
				},
				{
					"availableAction": "Acknowledge",
					"buttonText": "Acknowledge",
					"link": "http://localhost:51044/api/orders/administrations/3/actions/Acknowledge"
				},
				{
					"availableAction": "CoSign",
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/administrations/3/actions/CoSign"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/administrations/3/actions/Reschedule"
				}
			],
		},
		{
            "id": 4,
			"administrationScheduledDatetime": "2020-05-13T13:00:00",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/administrations/4/actions/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/administrations/4/actions/Hold"
				},
				{
					"availableAction": "Acknowledge",
					"buttonText": "Acknowledge",
					"link": "http://localhost:51044/api/orders/administrations/4/actions/Acknowledge"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/administrations/4/actions/Reschedule"
				},
				{
					"availableAction": "FollowUp",
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/administrations/4/actions/FollowUp"
				}
			],
        }
    ]
}, {
	"id": 2,
	"patientId": 1,
	"beginDatetime": "2020-05-13T14:00:00",
	"endDatetime": null,
	"medication": {
		"id": 12,
		"displayName": "Motrin",
		"medicationDetails":[]
	},
	"dose": "600",
	"doseUnit": {
		"unitName": "mg"
	},
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"PRN"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2020-05-13T15:55:12",
	"orderingPhysicianUser": {
		"displayName": "Romel Ursua, MD"
	},
	"priority": "STAT",
	"orderStatus": "Pending",
	"orderNotes": ""
}, {
	"id": 3,
	"patientId": 2,
	"beginDatetime": "2020-05-13T17:00:00",
	"endDatetime": "2020-05-13T19:30:00",
	"medication": {
		"id": 13,
		"displayName": "Asprin",
		"medicationDetails":[]
	},
	"dose": "100",
	"doseUnit": {
		"unitName": "mg"
	},
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2020-05-13T17:12:45",
	"orderingPhysicianUser": {
		"displayName": "Pete Turnbull, MD"
	},
	"allergyReactions": [],
	"drugs": [1],
	"orderStatus": "Held",
	"orderNotes": "",
	"orderAdministrations": [
		{
			"id": 5,
			"administrationScheduledDatetime": "2020-05-13T17:05:00",
			"pointInTime": false,
			"onHold": false,
			"administrationStatus": "Given",
			"availableActions": [
				{
					"availableAction": "CoSign",
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/administrations/5/actions/CoSign"
				},
				{
					"availableAction": "FollowUp",
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/administrations/5/actions/FollowUp"
				}
			],
		},
		{
			"id": 6,
			"administrationScheduledDatetime": "2020-05-13T17:10:00",
			"pointInTime": false,
			"onHold": true,
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/administrations/6/actions/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/administrations/6/actions/Hold"
				},
				{
					"availableAction": "CoSign",
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/administrations/6/actions/CoSign"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/administrations/6/actions/Reschedule"
				}
			],
		},
		{
			"id": 7,
			"administrationScheduledDatetime": "2020-05-13T19:30:00",
			"pointInTime": false,
			"onHold": true,
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/administrations/7/actions/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/administrations/7/actions/Hold"
				},
				{
					"availableAction": "CoSign",
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/administrations/7/actions/CoSign"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/administrations/7/actions/Reschedule"
				}
			],
		}
	]
}, {
	"id": 4,
	"patientId": 1,
	"beginDatetime": "2020-05-13T10:00:00",
	"endDatetime": "2020-05-13T10:30:00",
	"medication": {
		"id": 11,
		"displayName": "Motrin",
		"medicationDetails":[]
	},
	"dose": "400",
	"doseUnit": {
		"unitName": "mg"
	},
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Every hour PRN"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2020-05-13T10:12:45",
	"orderingPhysicianUser": {
		"displayName": "Peter Turnbull, MD"
	},
	"priority": "STAT",
	"prn": true,
	"orderStatus": "Ongoing",
	"orderNotes": "",
	"orderAdministrations": [
	{
		"id": 1,
		"administrationScheduledDatetime": "2020-05-13T10:05:00",
		"administrationInputDatetime": "2020-05-13T10:05:00",
		"administrationDatetime": "2020-05-13T10:10:00",
		"administeringUser":  {
			"id": 126,
			"firstName": "Richard",
			"lastName": "King"
		},
		"stopScheduledDatetime": "2020-05-13T10:10:00",
		"stopInputDatetime": "2020-05-13T10:10:00",
		"stopDatetime": "2020-05-13T10:10:00",
		"stopUserId": 456,
		"acknowledgeUser":  {
			"id": 126,
			"firstName": "Richard",
			"lastName": "King"
		},
		"acknowledgeDatetime": "",
		"pointInTime": false,
		"onHold": false,
		"administrationStatus": "Given",
		"availableActions": [
			{
				"availableAction": "CoSign",
				"buttonText": "Co-sign",
				"link": "http://localhost:51044/api/orders/1/Action/CoSign"
			},
			{
				"availableAction": "FollowUp",
				"buttonText": "Follow-up",
				"link": "http://localhost:51044/api/orders/1/Action/FollowUp"
			}
		],
	}]
}, {
	"id": 5,
	"patientId": 1,
	"beginDatetime": "2020-05-13T14:00:00",
	"endDatetime": "2020-05-13T14:20:00",
	"medication": {
		"id": 18,
		"displayName": "Morphine (PF) injection",
		"medicationDetails":[]
	},
	"dose": "8",
	"doseUnit": {
		"unitName": "mg"
	},
	"route": "injection",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Intramusculaire"
	},
	"signedOn": "2020-05-13T10:15:12",
	"orderingPhysicianUser": {
		"displayName": "Merrily Turnbull, MD"
	},
	"orderNotes": "",
	"allergyReactions": [],
	"drugs": [1],
	"orderStatus": "Pending",
	"orderAdministrations": [
        {
            "id": 1,
			"administrationScheduledDatetime": "2020-05-13T14:00:00",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/1/Action/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/1/Action/Hold"
				},
				{
					"availableAction": "Acknowledge",
					"buttonText": "Acknowledge",
					"link": "http://localhost:51044/api/orders/1/Action/Acknowledge"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/1/Action/Reschedule"
				},
				{
					"availableAction": "FollowUp",
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/1/Action/FollowUp"
				}
			],
		}
	]
}, {
	"id": 6,
	"patientId": 2,
	"beginDatetime": "2020-05-13T12:00:00",
	"endDatetime": "2020-05-13T17:30:00",
	"medication": {
		"id": 18,
		"displayName": "HYDROmorphone (Dilaulid)",
		"medicationDetails":[]
	},
	"dose": "0.2",
	"doseUnit": {
		"unitName": "ml"
	},
	"route": "injection",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "IV"
	},
	"signedOn": "2020-05-13T17:12:45",
	"orderingPhysicianUser": {
		"displayName":  "Peter Turnbull, MD",
	},
	"orderNotes": "",
	"allergyReactions": [],
	"drugs": [],
	"orderStatus": "Pending",
	"pointInTime": false,
	"orderAdministrations": [
		{
			"id": 27,
			"administrationScheduledDatetime": "2020-05-13T12:00:00",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/1/Action/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/1/Action/Hold"
				},
				{
					"availableAction": "CoSign",
					"buttonText": "Co-sign",
					"link": "http://localhost:51044/api/orders/1/Action/CoSign"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/1/Action/Reschedule"
				}
			]
		}
	]
}, {
	"id": 7,
	"patientId": 1,
	"beginDatetime": "2020-05-13T10:00:00",
	"endDatetime": "2020-05-13T10:30:00",
	"medication": {
		"id": 12,
		"displayName": "Ibuprofen (Advil, Motrin) 600MG tablet",
		"medicationDetails":[]
	},
	"dose": "300",
	"doseUnit": {
		"unitName": "mg"
	},
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2020-05-14T11:13:45",
	"orderingPhysicianUser": {
		"displayName": "Romel Ursua, MD"
	},
	"orderNotes": "",
	"orderStatus": "Pending",
	"allergyReactions": [],
	"drugs": [1],
	"orderAdministrations": [
        {
            "id": 1,
			"administrationScheduledDatetime": "2020-05-13T10:00:00",
			"administrationStatus": "Missed",
			"availableActions": []
		}
	]
}, {
	"id": 8,
	"patientId": 1,
	"beginDatetime": "2020-05-13T14:00:00",
	"endDatetime": "2020-05-13T14:30:00",
	"medication": {
		"id": 12,
		"displayName": "GI Cocktail",
		"medicationDetails":[
			{
				"id": 69842,
				"medicationId": 69815,
				"drugId": "174060",
				"brandName": "Lidocaine Viscous",
				"activeList": "lidocaine HCl",
				"dose":	10,
                "doseUnit": {
					"unitName": "ml",
				},
				"medicationUnitId":	50,
				"medicationRouteId": null,
				"isActive":	true
			},
			{
				"id": 69843,
				"medicationId": 69815,
				"drugId": "193741",
				"brandName": "Maalox Plus Extra Strength",
				"activeList": "magnesium hydroxide / aluminum hydroxide / simethicone",
				"dose":	20,
				"doseUnit": {
					"unitName": "mg",
				},
				"medicationUnitId":	50,
				"medicationRouteId": null,
				"isActive":	true
			},
			{
                "id": 69844,
                "medicationId": 69815,
                "drugId": "254593",
                "brandName": "fentaNYL (PF) injection",
                "activeList": "fentanyl citrate / preservative free",
                "dose": 50,
				"doseUnit": {
					"unitName": "mg"
				},
				"medicationUnitId":	50,
				"medicationRouteId": null,
                "isActive": true
              }
		]
	},
	"dose": "500",
	"doseUnit": {
		"unitName": "mg"
	},
	"route": "PO",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "Oral"
	},
	"signedOn": "2020-05-13T10:15:12",
	"orderingPhysicianUser": {
		"displayName": "Merrily Turnbull, MD"
	},
	"orderNotes": "",
	"orderStatus": "Pending",
	"allergyReactions": [],
	"drugs": [1],
	"orderAdministrations": [
		{
            "id": 1,
			"administrationScheduledDatetime": "2020-05-13T14:00:00",
			"administrationStatus": "Pending",
			"availableActions": [
				{
					"availableAction": "Give",
					"buttonText": "Give",
					"link": "http://localhost:51044/api/orders/1/Action/Give"
				},
				{
					"availableAction": "Hold",
					"buttonText": "Hold",
					"link": "http://localhost:51044/api/orders/1/Action/Hold"
				},
				{
					"availableAction": "Acknowledge",
					"buttonText": "Acknowledge",
					"link": "http://localhost:51044/api/orders/1/Action/Acknowledge"
				},
				{
					"availableAction": "Reschedule",
					"buttonText": "Reschedule",
					"link": "http://localhost:51044/api/orders/1/Action/Reschedule"
				},
				{
					"availableAction": "FollowUp",
					"buttonText": "Follow-up",
					"link": "http://localhost:51044/api/orders/1/Action/FollowUp"
				}
			],
		}
	]
}, {
	"id": 9,
	"patientId": 2,
	"beginDatetime": "2020-05-13T17:00:00",
	"endDatetime": "2020-05-13T17:30:00",
	"medication": {
		"id": 12,
		"displayName": "Sodium Chloride 0.9% BOLUS",
		"medicationDetails":[]
	},
	"dose": "20",
	"doseUnit": {
		"unitName": "ml"
	},
	"route": "IV",
	"frequencySchedule": {
		"scheduleName":"Each hour"
	},
	"medicationRoute": {
		"routeName": "IV"
	},
	"signedOn": "2020-05-15T07:12:45",
	"orderingPhysicianUser": {
		"displayName": "Romel Ursua, MD"
	},
	"orderNotes": "",
	"orderStatus": "Completed",
	"allergyReactions": [],
	"drugs": [1],
	"pointInTime": false
}]
