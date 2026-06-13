import { Interaction } from './interaction';

export interface PatientOrderInteraction {
  siteId: number;
  patientId: number;
  userId: number;
  sourceTable: string;
  sourceTableId: number;
  type: number;
  brandName: string;
  activeName: string;
  activeId: string;
  interactions?: Array<Interaction>; // For Medication Interactions
  reactions?: Array<Interaction>;   // For Allergy Interactions
}
