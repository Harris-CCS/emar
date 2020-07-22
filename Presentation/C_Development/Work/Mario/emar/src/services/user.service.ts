import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

import { User } from '../app/interfaces/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  url: string = 'http://ros-57c-dx01.picis.com:82/api/';

  constructor(private http: HttpClient) { }

  fetchUser(externalId: number): Observable<User> {
     return this.http.get<User>(
       this.url + '/users/' + externalId,
       {
         headers: new HttpHeaders({ 'Accept' :'application/json'}),
         responseType: 'json'
        }
       ).pipe(
         catchError(error => {
          return throwError(error)
         })
       );
  }
}
