import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';

// import 'rxjs/add/operator/catch';
// import 'rxjs/add/observable/throw';
import { observable } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { USER } from '../app/mockup/user';
import { User } from '../app/interfaces/user';
import { PrinterInformation } from '../app/interfaces/printer-information'
import { analyzeAndValidateNgModules } from '@angular/compiler';

@Injectable({
  providedIn: 'root',
})
export class PrinterService {
  // url: string = 'http://ros-57c-dx01.picis.com:82/api/';
  private userUrl = '/api/devices/devices/';

  constructor(private httpClient: HttpClient) { }
  id: number = 0;

  // private handleError<T>(operation = 'operation', result?: T) {
  //   return (error: any): Observable<T> => {
  //     console.error('error', error);
  //     return of(result as T);
  //   };
  // }

  // getUser(userId: number): User {
  //   const user = USER.find((p) => {
  //     return p.id === userId;
  //   });
  //   return user;
  // }

  // getPrinterInfo(id: number): Observable<PrinterInformation> {
  //   const headers = new HttpHeaders({ Accept: 'application/json' });
  //   const url = `${id}/${id}`;
  //   console.log('xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx:',id);
  //   return this.http
  //     .get<any>(url, { headers })
  //     .pipe(catchError(this.handleError<any>('getPrinterInfo')));
  // }

  getPrinterInfo(
    site: number,
    userId: number
  ): Observable<PrinterInformation[]> {
    var emarSite;
    var emarUserId;
    if (!site) { emarSite = `${emarSite}`; } else { emarSite = site; }
    if (!userId) { emarUserId = `${emarUserId}`; } else { emarUserId = userId; }
    var userUrlSite = this.userUrl + 'site/' + emarSite;
    // let eHeaders = new HttpHeaders();
    //  return this.httpClient.get<UserHeader[]>("http://localhost:3000/user", { headers: eHeaders })        .catch(this.handleError);
    //   }
    // let headers = new HttpHeaders();

    // const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': emarSite, 'EMAR-User': emarUserId })
    const headers = new HttpHeaders(
      { Accept: 'application/json', 'EMAR-Site': `${emarSite}`, 'EMAR-User': `${emarUserId}` })
    //const headers = new HttpHeaders({ Accept: 'application/json' })
    // headers = headers.append('Content-Type', 'application/json');
    // headers = headers.append('Accept' , 'application/json');
    // headers = headers.append('Authorization', "someKey");

    // headers.set('siteId',  '31');
    // headers.set("userId" , "8404");
    return this.httpClient
      .get<PrinterInformation[]>(userUrlSite, { headers })
      .pipe(catchError(this.handleError<PrinterInformation[]>('getPrinterInfo', [])));
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('error', error);
      return of(result as T);
    };
  }



  postPdfBase64(

    user_id_printing: number,
    device_id: number,
    patient_id: number,
    description: string,
    document_type: string,
    file_name: string,
    printerAddressType: string,
    printAddress: string,
    page_count: number,
    date_time: string,
    expiration_documentation: string,
    content: string
  ) {

    var postId: any;
    var poPdDescription: string;
    var poPdFile_name: string;
    var poPdprinterAddressType: string;
    var poPdPrintAddress: string;
    var poPdDate_time: string;
    var poPdExpiration_documentation: string;
    var rightNow = new Date;

    if (!user_id_printing) { console.log("Error!, missing user_id_printing"); return (404); }
    if (!device_id) { console.log("Error!, missing device_id"); return (404); }
    if (!patient_id) { console.log("Error!, missing patient_id"); return (404); }
    if (!document_type) { console.log("Error!, missing document_type"); return (404); }
    if (!page_count) { console.log("Error!, missing page_count"); return (404); }
    if (!content) { console.log("Error!, missing content"); return (404); }
    var rptFileName = "eMarU" + user_id_printing + "y" + rightNow.getUTCFullYear()
      + "m" + (rightNow.getUTCMonth() + 1) + "d" + rightNow.getUTCDate()
      + "h" + rightNow.getUTCHours() + "m" + rightNow.getUTCMinutes()
      + "s" + rightNow.getUTCSeconds() + "c" + rightNow.getUTCMilliseconds()
      + ".pdf";

    var utc_offset = rightNow.getTimezoneOffset();
    var utc_diff = 0;
    var utc_diffStr = "";
    if (utc_offset >= 61) { utc_diff = utc_offset / 60; utc_offset = Math.floor(utc_diff); }
    if (utc_offset <= 9) { utc_diffStr = "0" + utc_offset + ":00" } else { utc_diffStr = utc_offset + ":00" }
    // var rptDateTimes = rightNow.getUTCFullYear()
    // + "-"+(rightNow.getUTCMonth() +1) + "-" + rightNow.getUTCDate()
    // + " " + rightNow.getUTCHours() + ":" + rightNow.getUTCMinutes()
    // + ":" + rightNow.getUTCSeconds() + ":" +rightNow.getUTCMilliseconds()
    // + " -" + utc_diffStr;
    let rptDateTimes = rightNow.toISOString();

    if (!description) {
      console.log("Warning!, missing description");
      poPdDescription = "MAR Report";
    }
    else {
      poPdDescription = description;
    }
    if (!file_name) {
      console.log("Warning!, missing file_name");
      poPdFile_name = rptFileName;
    }
    else {
      poPdFile_name = file_name;
    }
    if (!printerAddressType) {
      console.log("Warning!, missing printerAddressType");
      poPdprinterAddressType = "PDF Printer";
    }
    else {
      poPdprinterAddressType = printerAddressType;
    }
    if (!date_time) {
      console.log("Warning!, missing date_time");
      poPdDate_time = rptDateTimes;
    }
    else {
      poPdDate_time = date_time;
    }
    if (!expiration_documentation) {
      console.log("Warning!, missing expiration_documentation");
      poPdExpiration_documentation = rptDateTimes;
    }
    else {
      poPdExpiration_documentation = expiration_documentation;
    }
    if (printAddress) {
      poPdPrintAddress = printAddress;
    }
    else {
      poPdPrintAddress = " ";
    }

    let postRptUrl = '/api/devices/print/';
    let headers = new HttpHeaders();
    headers = headers.append('Content-Type', 'application/json');

    let body = {
      user_id_printing: user_id_printing,
      device_id: device_id,
      patient_id: patient_id,
      description: poPdDescription,
      document_type: document_type,
      file_name: poPdFile_name,
      printerAddressType: poPdprinterAddressType,
      printAddress: poPdPrintAddress,
      page_count: page_count,
      date_time: poPdDate_time,
      expiration_documentation: poPdExpiration_documentation,
      content: "data:application/pdf;base64," + content


    };
    this.httpClient.post<any>(postRptUrl, body, { headers }).subscribe(data => {
      postId = data.id;
      console.log(headers);
      console.log(data);
      return data;
    }, err => {
      throw err;
    });

  }
}
