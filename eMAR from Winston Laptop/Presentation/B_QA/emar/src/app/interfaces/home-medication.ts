import { MedicationUnit } from './medication-unit';
import { MedicationRoute } from './medication-route';

export interface HomeMedication {
  actionStatus: string;
  alternateName: string;
  category: string;
  class: string;
  comment: string;
  dose: number;
  drugId: string;
  id: number;
  internalDrugId: string;
  isActive: boolean;
  lastTakenNote: string;
  medicationDrugId: string;
  medicationRoute: MedicationRoute;
  medicationUnit: MedicationUnit;
  name: string;
  ndc: string;
  parentDrugName: string;
  patientId: number;
  reaction: string;
  schedule: string;
  severity: string;
}
