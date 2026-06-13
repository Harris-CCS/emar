import { DoseUnit } from './dose-unit'
import { Route } from './route'

export interface ListOrder {
    siteId: number,
    departmentCode?: string,
    groupName?: string,
    id: number,
    ndc: string,
    drugId: string,
    brandName: string,
    dose: number,
    doseUnit: DoseUnit,
    medicationRoute: Route,
    frequencyId: number,
    pointInTime: boolean,
    orderNotes: string
}
