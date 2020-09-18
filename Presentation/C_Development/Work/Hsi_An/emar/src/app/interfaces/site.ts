import { Frequency } from './frequency';
import { Unit } from './unit';

export interface Site {
  id: number;
  active?: boolean;
  name?: string;
  timeZoneName?: string;
  medicationFrequencies?: Array<Frequency>;
  medicationUnits?: Array<Unit>;
}
