import { Frequency } from './frequency';
// import { Duration } from './duration';

export interface Order {
  id: number;
  patientId: number;
  name: string;
  dose: string;
  route: string;
  frequency?: Frequency;
  duration?: Duration;
  startTime: string;
  endTime?: string;
  triageTime?: string;
  signedOn: string;
  signedBy: string;
  allergies?: any;
  drugs?: any;
}
