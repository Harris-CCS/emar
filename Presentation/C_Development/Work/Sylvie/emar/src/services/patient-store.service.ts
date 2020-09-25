import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

import { Patient } from 'src/app/interfaces/patient';
import { Allergy } from 'src/app/interfaces/allergy';
import { HomeMedication } from 'src/app/interfaces/home-medication';

import { PatientService } from 'src/services/patient.service';

@Injectable({
  providedIn: 'root',
})
export class PatientStoreService {

  extId1: string  //= '36' //site id
  extId2: string  // = '20190226161557' //patient id - ibex

  emarPatientId: number

  constructor(private patientService: PatientService) {
    if (this.extId1 && this.extId2) {
      this.fetchPatientByExtIds(this.extId1, this.extId2)  
    }
  }

  private readonly _patient = new BehaviorSubject<Patient>(<Patient>{});
  readonly patient$ = this._patient.asObservable();

  get patient(): Patient {
    return this._patient.getValue() || <Patient>{};
  }

  set patient(val: Patient) {
    this._patient.next(val);
  }

  get patientId(): number {
    return this._patient.getValue().id
    // return this._patient.getValue().id || 657
  }

  get patientDeptCode(): string{
    return this._patient.getValue().departmentCode
  }

  get patientAllergies(): Allergy[] {
    return this._patient.getValue().patientAllergies || [];
  }

  get homeMedications(): HomeMedication[] {
    return this._patient.getValue().homeMedications || [];
  }

  async fetchPatient(emarPatientId) {
    this.patient = await this.patientService.getPatient(emarPatientId).toPromise()
    console.log('PatientStore - fetchPatient: ', this.patient)
  }
  
  async fetchPatientByExtIds(extId1, extId2) {
    this.patient = await this.patientService.getPatientByExtIds(extId1, extId2).toPromise()
    console.log('PatientStore - fetchPatientByExtIds: ', this.patient)
  }
}
