import { Hateoas } from './hateoas'

export interface CartOrder {
    "patientId": number,
    "userId": number,
    "addDatetime": string,
    "addDate"?: string,
    "addTime"?: string,
    "priority": number,
    "prn": boolean,
    "beginDatetime": string,
    "beginDate"?: string,
    "beginTime"?: string,
    "endDatetime": string,
    "endDate"?: string,
    "endTime"?: string,
    "userQuickListItemId": number,
    "cartOrderAdministrations": [
        {
            "id": number,
            "patientCartOrderId": number,
            "administrationScheduledDatetime": string,
            "administrationScheduledDate": string,
            "administrationScheduledTime": string,
            "stopScheduledDatetime": string,
            "stopScheduledDate": string,
            "stopScheduledTime": string,
            "pointInTime": boolean
        }
    ],
    "user"?: {
        "id": number,
        "typeCode": string,
        "typeDescription": string,
        "userInitials": string,
        "firstName": string,
        "middleName": string,
        "lastName": string,
        "nameSuffix": string,
        "displayName": string,
        "orderingOnlyPhysician": boolean,
        "displayInitialsIndicator": boolean,
        "site": number
    },
    "id": any, //number | string
    "ndc": string,
    "drugId": string,
    "brandName": string,
    "dose": number,
    "doseUnit"?: string,
    "medicationRoute"?: string,
    "medicationRouteId": number,
    "medicationUnitId": number,
    "frequencyId": number,
    "pointInTime": boolean,
    "orderNotes": string,
    "links"?: Hateoas
}
