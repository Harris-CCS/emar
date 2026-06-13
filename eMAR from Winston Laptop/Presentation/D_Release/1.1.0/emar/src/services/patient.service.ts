import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';

import { UserStoreService } from 'src/services/user-store.service'

import { PatientResponse } from 'src/app/interfaces/patient-response';
import { Patient } from 'src/app/interfaces/patient';
// import { PatientsList } from 'src/app/interfaces/patients-list';
import { Order } from 'src/app/interfaces/order';

import { environment } from '../environments/environment';

import { PATIENTS } from '../app/mockup/patients';
import { ORDERS } from '../app/mockup/orders';

@Injectable({
  providedIn: 'root',
})
export class PatientService {
  /* URL to WebAPI */
  private patientUrl = 'api/patients';

  constructor(
    private http: HttpClient,
    private userStoreService: UserStoreService) {
    //console.log('PATIENT.SERVICE: environment.apiUrl: ', environment.apiUrl)
  }

  /*
  getPatients(): Patient[] {
    return PATIENTS;
  }
  */

  getPatients(siteId: number, userId: number, departmentCode?: string, wardCodes?: string): Observable<PatientResponse> {
    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-Site': `${siteId}`, 
      'EMAR-User': `${userId}` 
    });
    
    departmentCode = departmentCode || ''
    wardCodes = wardCodes || ''

    const url: string = `${this.patientUrl}?departmentCode=${departmentCode}&wardCodes=${wardCodes}&includeOrders=true&r=${Math.random()}`
    // const url: string = `${this.patientUrl}?departmentCode=${departmentCode}&wardCodes=${wardCodes}&r=${Math.random()}`

    return this.http
      .get<PatientResponse>(url, { headers })
      .pipe(catchError(this.handleError<PatientResponse>('getPatients')));
  }

  getMyPatients(siteId: number, userId: number, departmentCode?: string, wardCodes?: string): Observable<PatientResponse> {
    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-Site': `${siteId}`, 
      'EMAR-User': `${userId}` 
    });

    const url: string = `${this.patientUrl}?includeMyPatientsOnly=true&departmentCode=${departmentCode}&wardCodes=${wardCodes}&includeOrders=true&r=${Math.random()}`

    return this.http
      .get<PatientResponse>(url, { headers })
      .pipe(catchError(this.handleError<PatientResponse>('getMyPatients')));
  }

  getPharmVerificationPatients(siteId: number, userId: number, departmentCode?: string, wardCodes?: string): Observable<PatientResponse> {
    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-Site': `${siteId}`, 
      'EMAR-User': `${userId}` 
    });

    const url: string = `${this.patientUrl}?pharmacyVerificationStatus=1&includeOrders=true&departmentCode=${departmentCode}&wardCodes=${wardCodes}&r=${Math.random()}`
    // const url: string = `${this.patientUrl}?pharmacyVerificationStatus=1&departmentCode=${departmentCode}&wardCodes=${wardCodes}&r=${Math.random()}`

    return this.http
      .get<PatientResponse>(url, { headers })
      .pipe(catchError(this.handleError<PatientResponse>('getPharmVerificationPatients')));
  }

  getPatient(patientId: number): Observable<Patient> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const url = `${this.patientUrl}/${patientId}?r=${Math.random()}`;

    console.log('patient.service: getPatient patientId:', patientId);
    console.log('patient.service: getPatient url:', url);
    return this.http
      .get<Patient>(url, { headers })
      .pipe(catchError(this.handleError<Patient>('getPatient')));
  }

  /* Initial - Get patient JSON by extId1 (site id) and extId2 (patient id - PCED ibex) */
  getPatientByExtIds(extId1: string, extId2: string): Observable<Patient> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const url = `${this.patientUrl}?extId1=${extId1}&extId2=${extId2}`;

    console.log(
      'patient.service: getPatientAPIByExtIds extId1:',
      extId1,
      ' extId2: ',
      extId2
    );
    console.log('patient.service: getPatientAPIByExtIds url:', url);
    return this.http
      .get<Patient>(url, { headers })
      .pipe(catchError(this.handleError<Patient>('getPatientAPIByExtIds')));
  }

  getDepartmentPatients(siteId: number, userId: number, departmentCode?: string, wardCodes?: Array<string>, hateaosLink?: string): Observable<PatientResponse> {
    // async getDepartmentPatients(siteId: number, userId: number, departmentCode?: string, wardCodes?: Array<string>, hateaosLink?: string) {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': `${siteId}`, 'EMAR-User': `${userId}` });
    let url: string = (hateaosLink && hateaosLink.includes('http')) ? hateaosLink : '';

    if (!url) {
      let urlParameters: string = (departmentCode) ? `?departmentCode=${departmentCode}` : '';
      if (wardCodes && wardCodes.length > 0) {
        urlParameters = `${urlParameters}&wardCodes=${wardCodes}`;
      }
      // console.log('wardCodes', wardCodes);
      url = `${this.patientUrl}${urlParameters}`;
    }
    // console.log(
    //   'patient.service: getDepartmentPatientsAPI'
    // );
    // console.log('patient.service: getDepartmentPatientsAPI url:', url);

    return this.http
      .get<PatientResponse>(url, { headers })
      .pipe(catchError(this.handleError<PatientResponse>('getDepartmentPatientsAPI')));
  }

  getPatientOrders(patientId: number): Order[] {
    const orders = ORDERS.filter((o) => {
      return o.patientId === patientId;
    });
    return orders;
  }

  getPatientByMock(patientId: number): Patient {
    const patient = PATIENTS.find((p) => {
      return p.id === patientId;
    });
    return patient;
  }

  /*
  getPatientOrders(patientId: number): Order[] {
    const orders = ORDERS.filter( (o) => {
        return o.patientId === patientId;
      }
    );
    return orders;
  }
  */

  /* Handle Http failed */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error);
      return of(result as T);
    };
  }
}
