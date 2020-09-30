export interface MedicationDetail {
    id: number,
    medicationId: number,
    drugId: string,
    brandName: string,
    activeList: string,
    dose: number,
    medicationUnitId: number,
    medicationRouteId: number,
    isActive: boolean
}
