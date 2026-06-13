import { Interaction } from './interaction';

export interface PatientInteraction {
  siteId: number;
  patientId: number;
  userId: number;
  sourceTable: string;
  sourceTableId: number;
  type: number;
  brandName: string;
  activeName: string;
  activeId: string;
  interactions?: Array<Interaction>;
}
