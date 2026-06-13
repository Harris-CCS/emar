import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { ActivatedRoute, ParamMap } from '@angular/router';

import { Patient } from 'src/app/interfaces/patient';
import { PatientExternalIdData } from 'src/app/interfaces/patient-external-id-data';
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

  constructor(
    private patientService: PatientService,
    private route: ActivatedRoute,
  ) {
    console.log('PatientStoreService constructor', this.route.snapshot.paramMap.get('patientId'))
    if (this.extId1 && this.extId2) {
      this.fetchPatientByExtIds(this.extId1, this.extId2)
    }

    this.route.paramMap.subscribe((params: ParamMap) => {
      const patientId = params.get('patientId')

      if (patientId) {
        this.fetchPatient(patientId)
      }
    })
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

  get patientExternalIdData(): PatientExternalIdData {
    return this._patient.getValue().externalId || {};
  }

  get patientDeptCode(): string {
    return this._patient.getValue().departmentCode
  }

  get patientAllergies(): Allergy[] {
    return this._patient.getValue().patientAllergies || [];
  }

  get homeMedications(): HomeMedication[] {
    return this._patient.getValue().homeMedications || [];
  }

  get visitStartDateTime(): string {
    return this._patient.getValue().visitStartDatetime;
  }

  async fetchPatient(emarPatientId) {
    const p = await this.patientService.getPatient(emarPatientId).toPromise()
    console.log('fetchPatient', p)
    this.patient = p
    console.log('PatientStore - fetchPatient: ', this.patient)
  }

  async fetchPatientByExtIds(extId1, extId2) {
    this.patient = await this.patientService.getPatientByExtIds(extId1, extId2).toPromise()
    console.log('PatientStore - fetchPatientByExtIds: ', this.patient)
  }
}
