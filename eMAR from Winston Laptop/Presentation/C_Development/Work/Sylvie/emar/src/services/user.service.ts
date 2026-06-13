import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, retry, tap } from 'rxjs/operators';
import { USER } from '../app/mockup/user';
import { User } from '../app/interfaces/user';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  // url: string = 'http://ros-57c-dx01.picis.com:82/api/';
  private userUrl = 'api/users';

  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    return this.http
      .get<User[]>(this.userUrl, { headers })
      .pipe(catchError(this.handleError<User[]>('getUsers', [])));
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('error', error);
      return of(result as T);
    };
  }

  // getUser(userId: number): User {
  //   const user = USER.find((p) => {
  //     return p.id === userId;
  //   });
  //   return user;
  // }

  getUser(userId: number): Observable<User> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const url = `${this.userUrl}/${userId}?r=${Math.random()}`;
    console.log('userId:', userId);
    return this.http
      .get<User>(url, { headers })
      .pipe(catchError(this.handleError<User>('getUser')));
  }

  getUserByExtId(extId: number): Observable<User> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const url = `${this.userUrl}?extId=${extId}`;
    console.log('extId:', extId);
    return this.http
      .get<User>(url, { headers })
      .pipe(catchError(this.handleError<User>('getUserByExtId')));
  }

  getNotifications(userId: number, siteId: number): Observable<any> {
    const headers = new HttpHeaders({ 
      'Accept': 'application/json',
      'EMAR-User': `${userId}`,
      'EMAR-Site': `${siteId}`
    });
    const url = "/api/usernotifications?r=" + Math.random();
    return this.http
    .get(url, { headers })
    .pipe(catchError(this.handleError<number>('getNbNotifications')));
  }

  getNbNotifications(userId: number, siteId: number): Observable<any> {
    const headers = new HttpHeaders({ 
      'Accept': 'application/json',
      'EMAR-User': `${userId}`,
      'EMAR-Site': `${siteId}`
    });
    const url = "/api/usernotifications/count?r=" + Math.random();
    return this.http
    .get(url, { headers })
    .pipe(catchError(this.handleError<number>('getNbNotifications')));
  }

    // send department filter to database
    setFilterSetting(filter: string, siteId: number, userId: number) {
      const headers = new HttpHeaders({ 
        Accept: 'application/json', 
        'EMAR-Site': `${siteId}`, 
        'EMAR-User': `${userId}` 
      });
      let url = '/api/patients?onlySaveDefaults=true';
      // TODO more cases
      switch (filter) {
        case 'upcomingOrders': url = url + '&upcomingOrdersOnly=true'; break;
        case 'rxVerificationNeeded': url = url + '&pharmacyVerificationStatus=1'; break;
        case 'myPatients': url = url + '&includeMyPatientsOnly=true'; break;
        case 'all': url = url + '&includeMyPatientsOnly=false'; break;
      }
      this.http
        .get(url, { headers })
        .pipe(catchError(this.handleError<any>('setFilterSetting')))
        .subscribe(response => {console.log('RESPONSE:', response, url)});
        // PS: subscribe is needed to have the http get fired
    }
  
}
