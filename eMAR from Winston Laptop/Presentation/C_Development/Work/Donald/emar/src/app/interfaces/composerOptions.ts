import { AdministrationInstructions } from './administrationInstructions';
import { FormStrength } from './formStrength';

export interface ComposerOptions {
  brandName: string;
  administrationInstructions?: Array<AdministrationInstructions>;
  availableFormStrength?: FormStrength[];
}
