import { Allergy } from './allergy';
import { HomeMedication } from './home-medication';

export interface Patient {
  id: number;
  active: boolean; // not in patients table, but in API returned data
  medicalRecordNumber?: string;
  accountNumber?: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  nameSuffix?: string;
  fullName?: string; // not in patients table, but in API returned data
  gender: string;
  dateOfBirth?: string;
  age?: number;
  ageUnits?: string;
  chiefComplaint?: string;
  heightInCm?: number;
  weightInKg?: number;
  siteId: number;
  departmentCode?: string;
  wardCode?: string;
  roomBedCode?: string;
  urgencyColor?: string;
  urgency?: string;
  nameAlert: boolean;
  withdrawConsent: boolean;
  vsDateTime?: string;
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
  site: { id: number; name: string; active: boolean; timeZoneName: string };
  // Missing fields
  patientImage?: string;
  customIndicators?: Array<IIndicators>;
  allergies?: Array<IAllergies>;
  homeMeds?: Array<IHomeMeds>;
  patientAllergies?: Array<Allergy>;
  homeMedications?: Array<HomeMedication>;
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
  lastTaken?: string;
  comment?: string;
}
