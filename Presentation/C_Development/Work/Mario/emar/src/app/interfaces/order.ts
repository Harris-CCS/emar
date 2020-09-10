import { Frequency } from './frequency';
import { Duration } from './duration';

export interface Order {
    id: number,
    patientId: number,
    name: string,
    dose: string,
    route: string,
    frequency?: Frequency,
    duration?: Duration,
    startTime: string,
    endTime?: string,
    triageTime?: string,
    signedOn: string,
    signedBy: string,
    allergies?: any,
    drugs?: any,
    prn?: boolean,
    priority?: string, // STAT, Routine
    orderType?: string, // Scheduled
    orderStatus?: string, // Pending
    orderAdministrations?: OrderAdministration[]
}

export interface OrderAdministration {
    id: number,
    administrationScheduledDatetime?: string,
    administrationInputDatetime?: string,
    administrationDatetime?: string,
    administeringUserId?: number,
    stopScheduledDatetime?: string,
    stopInputDatetime?: string,
    stopDatetime?: string,
    stopUserId?: number,
    acknowledgeUserId?: number,
    acknowledgeDatetime?: string,
    pointInTime?: boolean,
    onHold?: boolean,
    missedDose?: boolean,
    administrationStatus?: string
}
