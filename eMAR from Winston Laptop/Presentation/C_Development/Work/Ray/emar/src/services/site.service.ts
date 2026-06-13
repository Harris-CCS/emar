import { Injectable } from '@angular/core';
import { Site } from 'src/app/interfaces/site';
import { SiteOptions } from 'src/app/interfaces/site-options';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class SiteService {

  /* URL to WebAPI */
  private siteOptionsBaseUrl = '/api/siteoptions';
  private siteOptions: SiteOptions = null;

  constructor(private http: HttpClient) { }

  getSiteOptionsFromAPI(userId: number, siteId: number, options: string): Observable<SiteOptions> {

    /*
    NOTES:

    * If 'options' parameter = “all” , then this API Call will return all global options and all site options. 

    * If 'options' parameter = a comma-delimited list of site options, then this API Call will 
      return those specific site options and all global options.

    */

    const headers = new HttpHeaders({
      Accept: 'application/json',
      'EMAR-User': `${userId}`,
      'EMAR-Site': `${siteId}`
    });

    const url = `${this.siteOptionsBaseUrl}/${options}`;

    // console.log('siteOptions.Service: getSiteOptions:', options);
    // console.log('siteOptions.Service: getSiteOptions url:', url);
    return this.http
      .get<SiteOptions>(url, { headers })
      .pipe(catchError(this.handleError<SiteOptions>('getSiteOptionsFromAPI')));

  }

  /* Handle Http failed */

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('SiteService-handleError: ERROR: ', error);
      console.error(
        'SiteService-handleError: STATUS: ',
        error.status
      );
      return of(result as T);
    };
  }

  getSiteOptions(): SiteOptions {
    return this.siteOptions || null;
  }

  setSiteOptions(siteOptions: SiteOptions): void {
    this.siteOptions = siteOptions;
  }

  
  getSiteMedicationFrequenciesFromAPI(siteId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': siteId.toString() });
    const siteMedicationFrequenciesUrl: string = 'api/orders/schedulerOptions/frequencies';

    return this.http
      .get<any>(siteMedicationFrequenciesUrl, { headers })
      .pipe(
        catchError(this.handleError<any>('getSiteMedicationFrequenciesFromAPI'))
      );
  }

}