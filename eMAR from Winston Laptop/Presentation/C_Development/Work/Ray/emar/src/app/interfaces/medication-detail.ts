import { DoseUnit } from './dose-unit'

export interface MedicationDetail {
    id: number,
    medicationId: number,
    drugId: string,
    brandName: string,
    activeList: string,
    dose: number,
    doseUnit?: DoseUnit,
    medicationUnitId?: number,
    medicationRouteId?: number,
    isActive: boolean
}
