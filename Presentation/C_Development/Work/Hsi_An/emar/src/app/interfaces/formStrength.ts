import { Dose } from './dose';
import { Route } from './route';
import { Frequency } from './frequency';
import { AdministrationInstructions } from './administrationInstructions';

export interface FormStrength {
  id?: number;
  formStrengthName?: string;
  availableRoutes?: Route[];
  preferredDoses?: Dose[];
  preferredRoutes?: Route[];
  preferredFrequencies?: Frequency[];
  administrationInstructions?: AdministrationInstructions[];
}
