import { MedicationUnit } from './medication-unit';

export interface Dose {
  doseName: string;
  dose: number;
  doseUnit: MedicationUnit;
}
