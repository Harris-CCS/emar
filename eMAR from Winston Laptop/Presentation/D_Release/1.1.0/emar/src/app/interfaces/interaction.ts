import { Medication } from './medication';

export interface Interaction {
  drug_id_1: string;
  drug_id_2: string;
  int_id: number;
  severity_id: string;
  dname1: string;
  dnum2: string;
  dname2: string;
  sourceTable2: string;
  sourceTableId2: number;
  sourceTableMedicationId2: number;
  sourceTableMedication2?: Medication;
  sevtxt: string;
  dnum?: number;
  drug: string;
  type: string;
  interaction: string;
}
