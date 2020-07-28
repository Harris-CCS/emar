import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

import { User } from '../app/interfaces/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient) { }

  fetchUser(externalId: number): Observable<User> {
    // will use the proxy(-qa).conf.json
     return this.http.get<User>(
       '/users/' + externalId,
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
