import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';

import { PatientResponse } from 'src/app/interfaces/patient-response';
import { Patient } from 'src/app/interfaces/patient';
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

  constructor(private http: HttpClient) {
    //console.log('PATIENT.SERVICE: environment.apiUrl: ', environment.apiUrl)
  }

  /*
  getPatients(): Patient[] {
    return PATIENTS;
  }
  */

  getPatients(): Observable<PatientResponse> {
    const headers = new HttpHeaders({ Accept: 'application/json' });

    return this.http
      .get<PatientResponse>(this.patientUrl, { headers })
      .pipe(catchError(this.handleError<PatientResponse>('getPatients')));
  }

  getPatient(patientId: number): Observable<Patient> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const url = `${this.patientUrl}/${patientId}`;

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
