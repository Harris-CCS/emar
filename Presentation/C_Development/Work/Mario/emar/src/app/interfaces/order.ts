import { Frequency } from './frequency';
import { Duration } from './duration';
import { MedicationRoute } from './medication-route';

export interface Order {
    id: number,
    patientId: number,
    name: string,
    dose: string,
    route: string,
    frequency?: Frequency, // TODO delete
    duration?: Duration,
    startTime: string,
    endTime?: string,
    triageTime?: string,
    signedOn: string,
    signedBy: string,
    allergies?: any,
    drugs?: any,
    prn?: boolean,
    priority?: string, // 2 (STAT), Routine
    orderType?: string, // Scheduled
    orderStatus?: string, // Pending, Ongoing, Held, Pending Discontinue, Discontinued, Completed, Cancelled, Deleted
    missedDose?: boolean,
    orderAdministrations?: OrderAdministration[],
    frequencySchedule?: FrequencySchedule,
    medicationRoute?: MedicationRoute,
    pointInTime?: boolean
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
    administrationStatus?: string // Given, OnHold, Missed, Pending, Late, Ongoing
    availableActions?: AdministrationAction[]
}

export interface FrequencySchedule {
    scheduleName?: string;
}

export interface AdministrationAction {
    availableAction?: number,
	buttonText: string,
	link?: string
}