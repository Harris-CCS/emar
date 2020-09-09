import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, tap, switchMap } from 'rxjs/operators';


@Injectable({
  providedIn: 'root'
})
export class PatientMedOrderService {

  /* URL to WebAPI */
  private orderUrl = 'api/orders'

  constructor(
    private http: HttpClient
  ) { }

  //API data  
  getPatientCurrentOrders(patientId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json'})
    const patientCurOrderUrl = `${this.orderUrl}?patientId=${patientId}`
    console.log('PatientMedOrderService: getPatientCurrentOrders: patientCurOrderUrl: ', patientCurOrderUrl)

    return this.http
      .get<any>(patientCurOrderUrl, { headers })
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
}
