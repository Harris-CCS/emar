import { Hateoas } from './hateoas'
import { User } from './user'
import { CartOrderAdministration } from './cart-order-administration'
import { Medication } from './medication'
import { Frequency } from './frequency'

export interface CartOrder {
    patientId: number,
    userId: number,
    addDatetime: string,
    addDate?: string,
    addTime?: string,
    priority: number,
    prn?: boolean,
    beginDatetime: string,
    beginDate?: string,
    beginTime?: string,
    endDatetime: string,
    endDate?: string,
    endTime?: string,
    userQuickListItemId?: number,
    cartOrderAdministrations?: Array<CartOrderAdministration>,
    user?: User,
    id: any, //number | string
    ndc?: string,
    drugId?: string,
    brandName?: string,
    formStrength?: string;
    dose: number,
    doseUnit?: string,
    medicationId?: number,
    medicationRoute?: string,
    medicationRouteId: number,
    medicationUnitId: number,
    medication?: Medication,
    frequencyId: number,
    frequencySchedule?: Frequency,
    duration?: number,
    durationUnitId?: any,
    pointInTime: boolean,
    orderNotes: string,
    displayDose?: number,
    displayDoseUnit?: string,
    displayRoute?: string,
    displayFrequency?: string,
    links?: Hateoas;
    antimicrobialIndicationId?: number;
    antimicrobialIndicationText?: string;
    patientProblemId?: number;
    isDisabled?: boolean;
}
