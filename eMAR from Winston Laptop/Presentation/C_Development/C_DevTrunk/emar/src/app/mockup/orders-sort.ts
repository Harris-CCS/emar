import { Order } from '../interfaces/order';

export const ORDERS_SORT: Order[] = [
{
	"id": 1,
    "beginDatetime": "2020-11-11T10:00:00",
    "addDatetime": "2020-11-11T10:00:00",
    "nextActionTime": "2020-11-11T10:00:00",
    "medication": {
        "id": 1,
        "displayName": "BBBB completed 10"
    },
    "pointInTime": true,
    "orderStatus": "Completed",
    "priority": "STAT",
},
{
    "id": 2,
    "beginDatetime": "2020-11-11T10:00:00",
    "addDatetime": "2020-11-11T10:00:00",
    "nextActionTime": "2020-11-11T10:00:00",
    "medication": {
        "id": 1,
        "displayName": "AAAAAA completed 10"
    },
    "pointInTime": true,
    "orderStatus": "Completed",
    "priority": "STAT",
},
{
"id": 3,
    "beginDatetime": "2020-11-11T10:00:00",
    "addDatetime": "2020-11-11T10:00:00",
    "nextActionTime": "2020-11-11T13:00:00",
    "medication": {
        "id": 1,
        "displayName": "AAAAAA pending start 10 next 13"
    },
    "pointInTime": true,
    "orderStatus": "Pending",
    "priority": "",
},
{
"id": 4,
    "beginDatetime": "2020-11-11T11:00:00",
    "addDatetime": "2020-11-11T11:00:00",
    "nextActionTime": "2020-11-11T12:00:00",
    "medication": {
        "id": 1,
        "displayName": "AAAAAA pending start 11 next 12"
    },
    "pointInTime": true,
    "orderStatus": "Pending",
    "priority": "",
},
{
    "id": 5,
    "beginDatetime": "2020-11-11T10:00:00",
    "addDatetime": "2020-11-11T10:00:00",
    "nextActionTime": "2020-11-11T12:00:00",
    "medication": {
        "id": 1,
        "displayName": "AAAAAA pending stat start 10 next 12"
    },
    "pointInTime": true,
    "orderStatus": "Pending",
    "priority": "STAT",
},
{
    "id": 6,
    "beginDatetime": "2020-11-11T10:00:00",
     "addDatetime": "2020-11-11T10:00:00",
    "nextActionTime": "2020-11-11T13:00:00",
    "medication": {
        "id": 1,
         "displayName": "AAAAAA pending stat start 10 next 13"
    },
    "pointInTime": true,
    "orderStatus": "Pending",
    "priority": "STAT",
},
{
"id": 7,
    "beginDatetime": "2020-11-11T10:00:00",
    "addDatetime": "2020-11-11T10:00:00",
    "nextActionTime": "2020-11-11T13:00:00",
    "medication": {
        "id": 1,
    "displayName": "AAAAAA pending start 10 next 13 IV"
    },
    "pointInTime": false,
    "orderStatus": "Pending",
    "priority": "",
},
]