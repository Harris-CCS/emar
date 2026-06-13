export interface Allergy {
  actionStatus: string;
  allergyDrugId: string;
  alternateName: string;
  comment: string;
  drugId: string;
  id: number;
  informationSource: string;  // such as PulseCheck or Interface
  informationSourceCode: string;
  internalDrugId?: string;
  isActive: boolean;
  name: string;
  ndc: string;
  parentDrugId: string;
  parentDrugName: string;
  patientId: number;
  reaction: string;
  schedule: string;
  severity: string;
  source?: string; // such as Patient, Parent, etcetera
}
