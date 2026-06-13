import { Patient } from './patient';
import { Medication } from './medication';

export interface Notification {
    id: number,
    category: string,
    eventDatetime: string,
    patient?: Patient,
    medication?: Medication,
    action?: NotificationAction
}

export interface NotificationAction {
    url: string
}