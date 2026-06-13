import { Frequency } from './frequency';
import { Unit } from './unit';
import { MedicationUnit } from './medication-unit';
import { Route } from './route';
import { AntimicrobialIndication } from './antimicrobialIndication';
import { SiteOptions } from './site-options';

export interface Site {
  id: number;
  active?: boolean;
  name?: string;
  timeZoneName?: string;
  timeZoneOffset?: string;
  medicationFrequencies?: Array<Frequency>;
  medicationUnits?: Array<MedicationUnit>;
  medicationRouteUnits?: Array<Route>;
  antimicrobialIndications?: Array<AntimicrobialIndication>;
  siteOptions?: SiteOptions;
}
