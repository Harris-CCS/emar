import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
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

  getUser(userId: number): User {
    const user = USER.find((p) => {
      return p.id === userId;
    });
    return user;
  }

  // getUser(userId: number): Observable<User> {
  //   const headers = new HttpHeaders({ Accept: 'application/json' });
  //   const url = `${this.userUrl}/${userId}`;
  //   console.log('userId', userId);
  //   return this.http
  //     .get<User>(url, { headers })
  //     .pipe(catchError(this.handleError<User>('getUser')));
  // }
}
