import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';

import { PatientResponse } from 'src/app/interfaces/patient-response';
import { Patient } from 'src/app/interfaces/patient';
import { Order } from 'src/app/interfaces/order';

import { environment } from '../environments/environment';

import { PATIENTS } from '../app/mockup/patients';
import { ORDERS } from '../app/mockup/orders';
import { of } from 'rxjs';

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

  // getPatient(patientId: number): Observable<Patient> {
  //   const headers = new HttpHeaders({ Accept: 'application/json' });
  //   const url = `${this.patientUrl}/${patientId}`;

  //   // console.log('patient.service: getPatient patientId:', patientId)
  //   // console.log('patient.service: getPatient patientId:', patientId)
  //   console.log('patient.service: getPatient url:', url);
  //   return this.http
  //     .get<Patient>(url, { headers })
  //     .pipe(catchError(this.handleError<Patient>('getPatient')));
  // }

  getPatientOrders(patientId: number): Order[] {
    const orders = ORDERS.filter((o) => {
      return o.patientId === patientId;
    });
    return orders;
  }

  getPatient(patientId: number): Patient {
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
