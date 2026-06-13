import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, tap, switchMap } from 'rxjs/operators';

import { PatientStoreService } from './patient-store.service';
import { AdministrationAction, Order, OrderAdministration } from 'src/app/interfaces/order';
import { UserStoreService } from './user-store.service';
import { GivenTemplate } from '../app/interfaces/given-template';
import { FormGroup } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class PatientMedOrderService {
  refreshRequest = new EventEmitter<any>(); //  emit when a refresh is needed
  updateRequest = new EventEmitter<any>(); // emit when an update starts

  /* URL to WebAPI */
  private orderUrl = 'api/orders'

  constructor(
    private http: HttpClient,
    private patientStoreService: PatientStoreService,
    private userStoreService: UserStoreService,
  ) { }

  //API data  
  getPatientCurrentOrders(patientId: number): Observable<any> {
    // const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Patient': `${this.patientStoreService.patientId}`})
    const patId = patientId || this.patientStoreService.patientId
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Patient': `${patId}`})
    // const patientCurOrderUrl = `${this.orderUrl}?patientId=${patientId}`
    // console.log('PatientMedOrderService: getPatientCurrentOrders: this.orderUrl: ', this.orderUrl, headers)

    return this.http
      .get<any>(`${this.orderUrl}?r=${Math.random()}`, { headers })
      .pipe(tap(data =>console.log('ORDERS FROM API',data)))
      .pipe(catchError(this.handleError<any>('getPatientCurrentOrders')))
      
  }

  /* Handle Http failed */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('PatientMedOrderService-handleError: ERROR: ', error);
      console.error('PatientMedOrderService-handleError: STATUS: ', error.status);
      return of(result as T);
    };
  }

  /* post order action  - give back template */
  postOrderAction(action: AdministrationAction, order?, admin?: OrderAdministration) {
    const userId: number = this.userStoreService.userId;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'EMAR-User': `${this.userStoreService.userId}`,
      'EMAR-Site': `${this.userStoreService.userSiteId}`,
      'EMAR-Patient': `${this.patientStoreService.patientId}`
    });
    let url = action.link; // + '?r=' + Math.random();
    // CORS problem: let the proxy do its job in case api returns full url.
    const idx = url.indexOf('/api');
    if (idx > 0) url = url.substr(idx);
    // url = url.replace(':82/', ':8200/')
    console.log('POST ACTION', url, headers);

    return this.http
      .post<any>(url, null, { headers })
      .pipe(
        tap(_ => console.log('ACTION: ' + action.availableAction + ', user: ' + userId)),
      )
  }
  /* post a given or action template */
  postTemplate(template: GivenTemplate, formObj) {
    const userId: number = this.userStoreService.userId;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'EMAR-User': `${this.userStoreService.userId}`,
      'EMAR-Site': `${this.userStoreService.userSiteId}`,
      'EMAR-Patient': `${this.patientStoreService.patientId}`
    });
    let url = template.link.href;
    // CORS problem: let the proxy do its job in case api returns full url.
    const idx = url.indexOf('/api');
    if (idx > 0) url = url.substr(idx);
    // url = url.replace(':82/', ':8200/')

    // let formObj = form.getRawValue();
    let serializedForm = JSON.stringify(formObj);
    console.log('POST TEMPLATE', url, headers, serializedForm);

    return this.http
      .post<any>(url, serializedForm, { headers })
      .pipe(
        tap(_ => console.log('TEMPLATE: ' + template.name + ', user: ' + userId)),
        // map();
      )  
  }
 
}
