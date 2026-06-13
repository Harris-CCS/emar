import { Notification } from '../interfaces/notification';

export const NOTIFICATIONS: Notification[] =[
    {
        "id": 1,
        "eventDatetime": "2020-11-25T06:00:00",
        "category": "overdue",
        "medication": {
            "id": 5755,
            "displayName": "Diovan HCT 160 mg-12.5 mg tablet",

        },
        "patient": {
            "id": 1,
            "lastName": "Stark",
            "firstName": "Tony",
        },
        "action": {
            "url": "external?patientId=993&userId=4751&dest=marpatient"
        }
    },
    {
        "id": 1,
        "eventDatetime": "2020-11-25T06:00:00",
        "category": "overdue",
        "medication": {
            "id": 5756,
            "displayName": "Bactrim DS 160 mg tablet",

        },
        "patient": {
            "id": 1,
            "lastName": "Stark",
            "firstName": "Tony",
        },
        "action": {
            "url": "external?patientId=993&userId=4751&dest=marpatient"
        }
    }
]
