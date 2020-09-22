import { Hateoas } from './hateoas'
import { User } from './user'
import { CartOrderAdministration } from './cart-order-administration'

export interface CartOrder {
    patientId: number,
    userId: number,
    addDatetime: string,
    addDate?: string,
    addTime?: string,
    priority: number,
    prn: boolean,
    beginDatetime: string,
    beginDate?: string,
    beginTime?: string,
    endDatetime: string,
    endDate?: string,
    endTime?: string,
    userQuickListItemId: number,
    cartOrderAdministrations: Array<CartOrderAdministration>,
    user?: User,
    id: any, //number | string
    ndc: string,
    drugId: string,
    brandName: string,
    dose: number,
    doseUnit?: string,
    medicationRoute?: string,
    medicationRouteId: number,
    medicationUnitId: number,
    frequencyId: number,
    pointInTime: boolean,
    orderNotes: string,
    links?: Hateoas
}
