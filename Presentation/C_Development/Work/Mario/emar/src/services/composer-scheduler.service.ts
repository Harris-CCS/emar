import { Injectable } from '@angular/core';
import {
  Form,
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Observable, of, Subject, BehaviorSubject } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  tap,
  switchMap,
} from 'rxjs/operators';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { ComposerOptions } from '../app/interfaces/composerOptions';
import { Site } from '../app/interfaces/site';
import { Frequency } from '../app/interfaces/frequency';
import { Unit } from '../app/interfaces/unit';
import { DoseOption } from '../app/interfaces/doseOption';

import { ComposerMedComponent } from '../pages/composer-med/composer-med.component';
import { ModalService } from '../services/modal.service';
import { MedOrderService } from '../services/med-order.service';
import { CartStoreService } from '../services/cart-store.service';

@Injectable({
  providedIn: 'root',
})
export class ComposerSchedulerService {
  private composerMedComponents: Array<ComposerMedComponent>;
  private composerBrandNameOptions: ComposerOptions;
  private siteData: Array<Site> = [];
  private dosingOptions: Array<DoseOption>;

  private brandNameOptionsUrl: string = 'api/orders/composerOptions';
  private siteMedicationFrequenciesUrl: string =
    'api/orders/composerOptions/frequencies';
  private siteMedicationUnitsUrl: string = 'api/orders/composerOptions/units';
  private doseCheckingBaseUrl: string = 'api/GetDoseRangeCheckingInfo';

  // Behavior Subjects to trigger actions based on UI Interaction
  resetComponentMedFormId: BehaviorSubject<number> = new BehaviorSubject(-1);
  resetAllComponentMedFormIds: BehaviorSubject<boolean> = new BehaviorSubject(
    false
  );
  addNewMedComponent: BehaviorSubject<boolean> = new BehaviorSubject(false);
  newMedComponentAdded: BehaviorSubject<boolean> = new BehaviorSubject(false);
  changeIndication: BehaviorSubject<boolean> = new BehaviorSubject(false);
  changeDiagnosis: BehaviorSubject<boolean> = new BehaviorSubject(false);
  shouldCheckOverallMedOrderValidity: BehaviorSubject<
    boolean
  > = new BehaviorSubject(false);

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService,
    private http: HttpClient
  ) {
    this.composerMedComponents = [];
  }

  addNewComposerMedComponent(): void {
    this.addNewMedComponent.next(true);
  }

  registerComposerMedComponent(newMedComponent: ComposerMedComponent): number {

    this.composerMedComponents.push(newMedComponent);
    this.newMedComponentAdded.next(true);
    // console.log('composerMedComponentsRegister', this.composerMedComponents);
    return this.composerMedComponents.length - 1;
  }

  getComposerMedComponents() {
    return !this.composerMedComponents ? [] : this.composerMedComponents;
  }

  // addFormGroup(name: string, form: FormGroup) {
  addFormGroup(id: number, name: string, form: FormGroup) {
    // console.log('addFormGroupParams', id, name, form);
    // if (this.composerMedComponents[id]) {
    this.composerMedComponents[id].composerMedForm.setControl(name, form);
    // console.log('addFormGroup', id, name, form);
    /// console.log('composerMedComponentsThis', this);
    // }
  }

  resetAllComponentMedForms(): void {
    // this.composerMedComponents[id].composerMedForm.reset();
    // this.composerMedComponents[id].performFormReset.next(true);
    this.composerMedComponents.forEach((medComponent, index) => {
      this.resetComponentMedFormById(index);
    });
    this.composerMedComponents = [];
  }

  resetComponentMedFormById(index: number) {
    this.resetComponentMedFormId.next(index);
  }

  removeMedComponent(id: number): void {
    this.composerMedComponents.splice(id, 1);
    // console.log('removeMedComponent', this.composerMedComponents);
  }

  checkOverallMedOrderValidity(): boolean {
    const invalidMedComponent = this.composerMedComponents.find(
      (medComponent) => medComponent.isMedComposerFormInvalid()
    );
    // console.log('checkOverallValidity', this.composerMedComponents.length);
    // console.log('checkOverallMedOrderValidity', this.composerMedComponents, 'ComponentFound', invalidMedComponent);
    return invalidMedComponent ? false : true;
  }

  // HTTP

  getBrandNameOptionsFromAPI(brandName: string): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const fullBrandNameOptionsUrl = `${this.brandNameOptionsUrl}/${brandName}`;
    // console.log(
    //   'ComposerScheduler: getBrandNameOptionsFromAPI: fullBrandNameUrl: ',
    //   fullBrandNameOptionsUrl
    // );

    return this.http
      .get<any>(fullBrandNameOptionsUrl, { headers })
      .pipe(catchError(this.handleError<any>('getBrandNameOptionsFromAPI')));
  }

  getSiteMedicationFrequenciesFromAPI(siteId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const fullSiteMedicationFrequenciesUrl = `${this.siteMedicationFrequenciesUrl}/${siteId}`;
    // console.log(
    //   'ComposerScheduler: getSiteMedicationFrequenciesFromAPI: fullSiteMedicationsFrequenciesUrl: ',
    //   fullSiteMedicationFrequenciesUrl
    // );

    return this.http
      .get<any>(fullSiteMedicationFrequenciesUrl, { headers })
      .pipe(
        catchError(this.handleError<any>('getSiteMedicationFrequenciesFromAPI'))
      );
  }

  getSiteMedicationUnitsFromAPI(siteId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const fullSiteMedicationUnitsUrl = `${this.siteMedicationUnitsUrl}/${siteId}`;
    // console.log(
    //   'ComposerScheduler: getSiteMedicationUnitsFromAPI: fullSiteMedicationsUnitsUrl: ',
    //   fullSiteMedicationUnitsUrl
    // );

    return this.http
      .get<any>(fullSiteMedicationUnitsUrl, { headers })
      .pipe(catchError(this.handleError<any>('getSiteMedicationUnitsFromAPI')));
  }

  getDosingOptionsFromAPI(id: string): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json' });
    const fullDosingOptionsUrl = `${this.doseCheckingBaseUrl}/${id}`;
    // console.log(
    //   'ComposerScheduler: getDosingOptionsFromAPI: fullDosingUrl: ',
    //   fullDosingOptionsUrl
    // );

    return this.http
      .get<any>(fullDosingOptionsUrl, { headers })
      .pipe(catchError(this.handleError<any>('getDosingOptionsFromAPI')));
  }

  /* Handle Http Request failed */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('ComposerSchedulerService-handleError: ERROR: ', error);
      console.error(
        'ComposerSchedulerService-handleError: STATUS: ',
        error.status
      );
      return of(result as T);
    };
  }

  // Get and Set API Data

  getBrandNameOptions(): ComposerOptions {
    // console.log('getBrandNameOptions', this.composerBrandNameOptions);
    return this.composerBrandNameOptions;
  }

  setBrandNameOptions(options: ComposerOptions): void {
    this.composerBrandNameOptions = options;
  }

  getSiteMedicationFrequencies(siteId: number): Array<Frequency> {
    const siteIndex: number = this.getSiteData(siteId);
    return siteIndex === 0 || siteIndex
      ? this.siteData[siteIndex].medicationFrequencies
      : [];
  }

  setSiteMedicationFrequencies(
    siteId: number,
    frequencies: Array<Frequency>
  ): void {
    const siteIndex: number = this.getSiteData(siteId);
    this.siteData[siteIndex].medicationFrequencies = frequencies;
  }

  getSiteMedicationUnits(siteId: number): Array<Unit> {
    const siteIndex: number = this.getSiteData(siteId);
    return siteIndex === 0 || siteIndex
      ? this.siteData[siteIndex].medicationUnits
      : [];
  }

  setSiteMedicationUnits(siteId: number, units: Array<Unit>): void {
    const siteIndex: number = this.getSiteData(siteId);
    this.siteData[siteIndex].medicationUnits = units;
  }

  getSiteData(siteId: number): number {
    let siteIndex: number = null;
    const site: Site = this.siteData.find((siteToFind, index) => {
      if (siteToFind.id === siteId) {
        siteIndex = index;
        return siteToFind;
      }
    });
    if (siteIndex === null) {
      this.siteData.push({
        id: siteId,
        active: true,
      });
      return this.siteData.length - 1;
    } else {
      return siteIndex;
    }
  }

  getDosingOptions(): Array<DoseOption> {
    // console.log('get this.dosingOptions', this.dosingOptions);
    return this.dosingOptions || [];
  }

  setDosingOptions(options: DoseOption[]) {
    // console.log('set this.dosingOptions', this.dosingOptions);
    this.dosingOptions = options;
  }
}
