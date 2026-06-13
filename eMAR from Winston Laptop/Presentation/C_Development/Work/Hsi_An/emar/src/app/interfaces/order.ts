import { Frequency } from './frequency';
import { Duration } from './duration';
import { DurationUnit } from './duration-unit';
import { MedicationRoute } from './medication-route';
import { Medication } from './medication';
import { User } from './user';
import { DoseUnit } from './dose-unit';

export interface Order {
    id: number,
    patientId?: number,
    brandName?: string,
    medication?: Medication,
    dose?: string,
    doseUnit?: DoseUnit,
    route?: string,
    frequency?: Frequency, // TODO delete
    duration?: Duration,
    durationUnit?: DurationUnit,
    beginDatetime?: string,
    endDatetime?: string,
    triageTime?: string,
    signedOn?: string,
    signedBy?: string,
    allergyReactions?: any,
    orderInteractions?: any,
    drugs?: any,
    prn?: boolean,
    priority?: string, // 2 (STAT), Routine
    orderType?: string, // Scheduled
    orderStatus?: string, // Pending, Ongoing, Held, Pending Discontinue, Discontinued, Completed, Cancelled, Deleted
    missedDose?: boolean,
    orderAdministrations?: OrderAdministration[],
    frequencySchedule?: FrequencySchedule,
    medicationRoute?: MedicationRoute,
    pointInTime?: boolean,
    availableActions?: AdministrationAction[],
    orderNotes?: string,
    orderingPhysicianUser?: User,
    addUser?: User,
    addDatetime?: string,
    orderEvents?: Event[],
    nextActionTime?: string,
    applicableFilters?: string[],
    isDisabled?: boolean;
    pharmacyVerificationStatus?: number;
    prnIndication?: string;
}

export interface OrderAdministration {
    id: number,
    administrationScheduledDatetime?: string,
    administrationInputDatetime?: string,
    administrationDatetime?: string,
    administeringUserId?: number
    administeringUser?: User,
    acknowledgeUser?: User,
    stopUser?: User,
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
    availableActions?: AdministrationAction[],
    administrationEvents?: Event[]
}

export interface FrequencySchedule {
    scheduleName?: string;
    prn?: boolean;
}

export interface AdministrationAction {
    availableAction?: string,
	buttonText: string,
	link?: string
}

export interface Event {
    id?: number,
    orderId?: number,
    administrationId?: number,
    eventDatetime: string,
    eventDate?: string,
    eventTime?: string,
    action?: Action,
    user?: User
}

export interface Action {
    actionId: number,
    actionCode: string, // CoSign
    buttonText?: string
}