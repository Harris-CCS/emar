export interface Patient {
  id: number;
  siteId: number;
  enterprisePatientId: string;
  medicalRecordNumber: string;
  accountNumber: string;
  firstName: string;
  lastName: string;
  middleName: string;
  nameSuffix: string;
  age: number;
  ageUnits: string;
  gender: string;
  complaint: string;
  dateOfBirth: string;
  roomBedCode: string;
  wardCode: string;
  departmentCode: string;
  urgency: string;
  urgencyColor: string;
  nameAlert: string;
  withdrawConsent: string;
  heightInCm: number;
  weightInKg: number;
  vsDateTime: string;
  vsBloodPressureIndicator: string;
  vsSystolic: string;
  vsDiastolic: string;
  vsPulseIndicator: string;
  vsPulse: string;
  vsMapLevel: string;
  vsMap: string;
  vsRespiratoryIndicator: string;
  vsRespiratory: string;
  vsTemperatureIndicator: string;
  vsTemperature: string;
  vsEndTidalLevel: string;
  vsEndTidal: string;
  vsOxygenSaturationIndicator: string;
  vsOxygenSaturation: string;
  vsPainScaleIndicator: string;
  vsPainScale: string;
  customIndicators: Array<IIndicators>;
}

interface IIndicators {
  position: number;
  code: string;
  type: string;
  description: string;
  imageName: string;
}
