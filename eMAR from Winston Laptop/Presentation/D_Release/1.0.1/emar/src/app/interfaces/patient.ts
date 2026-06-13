import { Allergy } from './allergy';
import { HomeMedication } from './home-medication';
import { PatientIndicator } from './patient-indicator';
import { Diagnosis } from './diagnosis';
import { PatientExternalIdData } from './patient-external-id-data';

export interface Patient {
  id: number;
  active?: boolean; // not in patients table, but in API returned data
  medicalRecordNumber?: string;
  accountNumber?: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  nameSuffix?: string;
  fullName?: string; // not in patients table, but in API returned data
  gender?: string;
  birthDate?: string;
  dateOfBirth?: string;
  deactivationDateTime?: string;
  age?: number;
  ageUnits?: string;
  complaint?: string;
  heightInCm?: number;
  weightInKg?: number;
  siteId?: number;
  departmentCode?: string;
  wardCode?: string;
  roomBedCode?: string;
  urgencyColor?: string;
  urgency?: string;
  nameAlert?: boolean;
  withdrawConsent?: boolean;
  vsDatetime?: string;
  vsDatetimeDate?: string;
  vsDatetimeTime?: string;
  vsBloodPressureIndicator?: string;
  vsSystolic?: string;
  vsDiastolic?: string;
  vsPulseIndicator?: string;
  vsPulse?: string;
  vsMapLevel?: string;
  vsMap?: string;
  vsRespiratoryIndicator?: string;
  vsRespiratory?: string;
  vsTemperatureIndicator?: string;
  vsTemperature?: string;
  vsEndTidalLevel?: string;
  vsEndTidal?: string;
  vsOxygenSaturationIndicator?: string;
  vsOxygenSaturation?: string;
  vsPainScaleIndicator?: string;
  vsPainScale?: string;
  personNumber?: string;
  customNumber?: string;
  site?: { id: number; name: string; active: boolean; timeZoneName: string };
  patientAllergies?: Array<Allergy>;
  homeMedications?: Array<HomeMedication>;
  patientIndicators?: Array<PatientIndicator>;
  patientImageSrc?: string;
  patientProblems?: Array<Diagnosis>;
  visitStartDatetime?: string;
  externalId?: PatientExternalIdData;
  // Missing fields
  // patientImage?: string;
  // customIndicators?: Array<IIndicators>;
  // allergies?: Array<IAllergies>;
  // homeMeds?: Array<IHomeMeds>;
}

interface IIndicators {
  position: number;
  code: string;
  type: string;
  description: string;
  imageName: string;
}

interface IAllergies {
  name: string;
  isActive: number;
  comment: string;
  reaction: string;
  severity: string;
  source?: string;
}

interface IHomeMeds {
  name: string;
  isActive: number;
  dose: string;
  unit: string;
  route: string;
  schedule?: string;
  lastTakenNote?: string;
  comment?: string;
}
