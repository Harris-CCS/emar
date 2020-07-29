import {Patient} from './patient';
import {Hateoas} from './hateoas';

export interface PatientResponse {
    patients: Array<Patient>;
    links: Array<Hateoas>;
}
