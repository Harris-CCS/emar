export interface CartOrderAdministration {
    id?: number,
    patientCartOrderId?: number,
    administrationScheduledDatetime?: string,
    administrationScheduledDate?: string,
    administrationScheduledTime?: string,
    stopScheduledDatetime?: string,
    stopScheduledDate?: string,
    stopScheduledTime?: string,
    pointInTime?: boolean
}
