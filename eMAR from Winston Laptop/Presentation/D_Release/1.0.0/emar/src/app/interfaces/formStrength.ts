import { Dose } from './dose';
import { MedicationRoute } from './medication-route';
import { Frequency } from './frequency';
import { AdministrationInstructions } from './administrationInstructions';
import { MedicationDetail } from './medication-detail';

export interface FormStrength {
  administrationInstructions?: AdministrationInstructions[];
  antimicrobialRequiredIndicator?: boolean;
  availableRoutes?: MedicationRoute[];
  combo?: boolean;
  formStrengthName?: string;
  id?: number;
  medicationDetails?: Array<MedicationDetail>;
  medicationId?: number;
  preferredDoses?: Dose[];
  preferredRoutes?: MedicationRoute[];
  preferredFrequencies?: Frequency[];

}
