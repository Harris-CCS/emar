import { DrugInteraction } from './drug-interaction'

export interface OrderInteraction {
    id: number,
    medicationInteractionId: number,
    drugNum: number
    patientOrderId: number,
    patientCartOrderId: number,
    patientHomeMedicationId: number,
    drugInteraction: DrugInteraction
}
