import { MedicationUnit } from './medication-unit';
import { MedicationRoute } from './medication-route';

export interface HomeMedication {
    id: number;
    patientId: number;
    class: string;
    category: string;
    internalDrugId: string;
    ndc: string;
    drugId: string;
    name: string;
    alternateName: string;
    dose: number;
    medicationUnit: MedicationUnit;
    medicationRoute: MedicationRoute;
    medicationDrugId: string;
    isActive: boolean;
    comment: string;
    schedule: string;
    reaction: string;
    severity: string;
    parentDrugName: string;
}
