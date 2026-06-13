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
import { MedicationUnit } from '../app/interfaces/medication-unit';
import { Route } from '../app/interfaces/route';
import { DurationUnit } from '../app/interfaces/duration-unit';
import { DoseOption } from '../app/interfaces/doseOption';
import { PatientOrderInteraction } from '../app/interfaces/patient-order-interaction';
import { AntimicrobialIndication } from '../app/interfaces/antimicrobialIndication';
import { UserRememberedListOrder } from '../app/interfaces/user-remembered-list-order';

import { ComposerMedComponent } from '../pages/composer-med/composer-med.component';
import { ModalService } from '../services/modal.service';
import { MedOrderService } from '../services/med-order.service';
import { CartStoreService } from '../services/cart-store.service';
import { ScheduledAdministration } from 'src/app/interfaces/scheduled-administration';

@Injectable({
  providedIn: 'root',
})
export class ComposerSchedulerService {
  private initialComposerData: object = {};
  private composerMedComponents: Array<ComposerMedComponent>;
  private composerBrandNameOptions: ComposerOptions;
  private siteData: Array<Site> = [];
  private durationUnits: Array<DurationUnit> = [];
  private dosingOptions: Array<DoseOption>;
  private siteMedicationAntimicrobialIndications: Array<AntimicrobialIndication>;
  private patientOrderInteractions: Array<PatientOrderInteraction>;

  private brandNameOptionsUrl: string = 'api/orders/schedulerOptions';
  private siteMedicationFrequenciesUrl: string =
    'api/orders/schedulerOptions/frequencies';
  private siteMedicationUnitsUrl: string = 'api/orders/schedulerOptions/units';
  private siteMedicationRoutesUrl: string = 'api/orders/schedulerOptions/routes';
  private doseCheckingBaseUrl: string = 'api/GetDoseRangeCheckingInfo';
  private siteMedicationAntimicrobialIndicationsBaseUrl: string = 'api/indications';
  private patientOrderInteractionBaseUrl: string = 'api/interactions';
  private userQuickListBaseUrl: string = 'api/userquicklists';
  private durationUnitsBaseUrl: string = 'api/orders/schedulerOptions/durationUnits';
  private frequencyAdministrationsBaseUrl: string = '/api/orders/schedulerOptions/administrations';

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
  newFormStrengthSelected: BehaviorSubject<number> = new BehaviorSubject(-1);
  orderFrequencyChanged: BehaviorSubject<number> = new BehaviorSubject(-1);

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService,
    private http: HttpClient
  ) {
    this.composerMedComponents = [];
  }

  getInitialComposerData() {
    return this.initialComposerData;
  }

  setInitialComposerData(data: object) {
    this.initialComposerData = data;
  }

  addNewComposerMedComponent(): void {
    this.addNewMedComponent.next(true);
  }

  signalNewFormStrengthSelected(componentId: number): void {
    // console.log('signalNewFormStrength', componentId);
    this.newFormStrengthSelected.next(componentId);
  }

  signalOrderFrequencyChanged(componentId: number): void {
    this.orderFrequencyChanged.next(componentId);
  }

  checkComponentFormGroup(componentId: number, formGroupName: string): boolean {
    return this.composerMedComponents[componentId].composerMedForm.value[formGroupName] ? true : false;
  }

  registerComposerMedComponent(newMedComponent: ComposerMedComponent): number {
    // console.log('composerMedComponentsRegisterComponent', newMedComponent);
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
    // console.log('addFormGroupParams', id, name, form, this.composerMedComponents);
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
    this.resetComponentMedFormId.next(-1);
  }

  resetComponentMedFormById(index: number) {
    this.resetComponentMedFormId.next(index);
  }

  removeContinuationMedComponents(): void {
    // When form strength changed, delete any other orders
    // this.composerMedComponents = [];
    // this.composerMedComponents.push(this.composerMedComponents[0]);
    this.composerMedComponents.splice(1);
    // this.composerMedComponents.forEach((component, index) => {
    //   if (index !== 0) {
    //     this.removeMedComponent(index);
    //   }
    // });
  }

  removeMedComponent(id: number): void {
    this.composerMedComponents.splice(id, 1);
    // console.log('removeMedComponent', this.composerMedComponents);
  }

  async checkOverallMedOrderValidity(): Promise<boolean> {
    const invalidMedComponent = this.composerMedComponents.find(
      (medComponent) => medComponent.isMedComposerFormInvalid()
    );
    // console.log('checkOverallValidity', this);
    // console.log('checkOverallMedOrderValidity', this.composerMedComponents, 'ComponentFound', invalidMedComponent);
    return (!this.composerMedComponents || this.composerMedComponents.length === 0 || invalidMedComponent) ? false : true;
  }

  isMedComposerFormInvalid(id: number) {
    // console.log('isMedComposerFormInvalid', id, this.composerMedComponents);
    // return this.composerMedComponents[id].isMedComposerFormInvalid();
    return this.composerMedComponents[id].composerMedForm.invalid;
  }

  getSelectedFormStrengthId(componentId: number): number {
    if (!this.composerMedComponents ||
      !this.composerMedComponents[componentId] ||
      !this.composerMedComponents[componentId].composerMedForm) {
      return null;
    } else {
      const searchFsIndex: number = this.composerMedComponents[componentId].composerMedForm.value.med.formStrengthOptions.medicationId;
      const foundFsIndex: number = this.composerMedComponents[componentId].options.availableFormStrength.findIndex
        // (formStrength => formStrength.medicationId === medicationId);
        (formStrength => formStrength.medicationId === searchFsIndex);
      return foundFsIndex === -1 ? null : foundFsIndex;
    }
  }

  async saveOrderToUserQuickList(medComponentId: number, userId: number, siteId: number): Promise<boolean> {
    const order = this.composerMedComponents[medComponentId];
    // console.log('saveQuickListOrder', order);
    const rememberedListOrder: UserRememberedListOrder = {
      userId,
      siteId,
      medicationId: order.composerMedForm.value.med.formStrengthOptions.medicationId,
      dose: order.composerMedForm.value.med.dose,
      medicationUnitId: order.composerMedForm.value.med.doseUnitData.id,
      medicationRouteId: order.composerMedForm.value.med.routeOfAdministrationData.id,
      priority: order.composerMedForm.value.med.priority,
      frequencyId: order.composerMedForm.value.frequency.frequencyData.id,
      orderNotes: order.composerMedForm.value.med.administrationInstructionsText,
      duration: order.composerMedForm.value.frequency.duration || null,
      durationUnitId: (order.composerMedForm.value.frequency.durationUnit &&
        order.composerMedForm.value.frequency.durationUnit.id) ?
        order.composerMedForm.value.frequency.durationUnit.id :
        null,
      ndc: order.composerMedForm.value.med.formStrengthOptions.baseNdc || null,

    };
    // console.log('saveQuickListRememberedOrder', rememberedListOrder);
    return await this.addOrderToUserQuickList(rememberedListOrder, userId, siteId);
  }

  // HTTP

  getBrandNameOptionsFromAPI(
    siteId: number,
    brandName?: string,
    rememberedListItemType?: string,
    rememberedListItemId?: number,
    hateoasUrl?: string,
  ): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': `${siteId}` });
    const optionsPredicate: string = (rememberedListItemType && rememberedListItemId) ?
      `/${rememberedListItemType}/${rememberedListItemId}` :
      `/${brandName}/site/${siteId}`;
    const fullBrandNameOptionsUrl = hateoasUrl || `${this.brandNameOptionsUrl}${optionsPredicate}`;
    // console.log('headersBrandName', headers);
    // console.log(
    //   'ComposerScheduler: getBrandNameOptionsFromAPI: fullBrandNameUrl: ',
    //   fullBrandNameOptionsUrl
    // );

    return this.http
      .get<any>(fullBrandNameOptionsUrl, { headers })
      .pipe(catchError(this.handleError<any>('getBrandNameOptionsFromAPI')));
  }

  getSiteMedicationFrequenciesFromAPI(siteId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': siteId.toString() });
    const fullSiteMedicationFrequenciesUrl = `${this.siteMedicationFrequenciesUrl}/site/${siteId}`;
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
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': siteId.toString() });
    const fullSiteMedicationUnitsUrl = `${this.siteMedicationUnitsUrl}/site/${siteId}`;
    // console.log(
    //   'ComposerScheduler: getSiteMedicationUnitsFromAPI: fullSiteMedicationsUnitsUrl: ',
    //   fullSiteMedicationUnitsUrl
    // );

    return this.http
      .get<any>(fullSiteMedicationUnitsUrl, { headers })
      .pipe(catchError(this.handleError<any>('getSiteMedicationUnitsFromAPI')));
  }

  getSiteMedicationRoutesFromAPI(siteId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': siteId.toString() });
    const fullSiteMedicationRoutesUrl = `${this.siteMedicationRoutesUrl}/site/${siteId}`;
    // console.log(
    //   'ComposerScheduler: getSiteMedicationRoutesFromAPI: fullSiteMedicationsRoutesUrl: ',
    //   fullSiteMedicationRoutesUrl
    // );

    return this.http
      .get<any>(fullSiteMedicationRoutesUrl, { headers })
      .pipe(
        catchError(this.handleError<any>('getSiteMedicationRoutesFromAPI'))
      );
  }

  getDosingOptionsFromAPI(id: number): Observable<any> {
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

  getSiteMedicationAntimicrobialIndicationsFromAPI(siteId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': `${siteId}` });
    const fullSiteMedicationAntimicrobialIndicationsUrl = `${this.siteMedicationAntimicrobialIndicationsBaseUrl}/site/${siteId}`;
    // console.log(
    //   'ComposerScheduler: getSiteMedicationAntimicrobialIndicationsFromAPI: fullSiteMedicationAntimicrobialIndicationsUrl: ',
    //   fullSiteMedicationAntimicrobialIndicationsUrl
    // );
    // console.log('headers', headers);

    return this.http
      .get<any>(fullSiteMedicationAntimicrobialIndicationsUrl, { headers })
      .pipe(catchError(this.handleError<any>('getAntimicrobialIndicationsFromAPI')));
  }

  getPatientOrderInteractionsFromAPI(
    siteId: number,
    userId: number,
    patientId: number,
    orderItemType: string,
    orderItemNumber?: number
  ): Observable<any> {
    const headers = new HttpHeaders({
      Accept: 'application/json',
      'EMAR-Site': `${siteId}`,
      'EMAR-User': `${userId}`,
      'EMAR-Patient': `${patientId}`,
    });
    const fullPatientOrderInteractionUrl = `${this.patientOrderInteractionBaseUrl}/${orderItemType}/${orderItemNumber}`;
    // console.log(
    //   'ComposerScheduler: getPatientOrderInteractionsFromAPI: : fullPatientOrderInteractionUrl',
    //   fullPatientOrderInteractionUrl
    // );
    // console.log('PatientOrderInteractionUrl headers', headers);
    // console.log('OrderInteractionParams', siteId, userId, patientId, orderItemType, orderItemNumber);

    return this.http
      .get<any>(fullPatientOrderInteractionUrl, { headers })
      .pipe(catchError(this.handleError<any>('getPatientOrderInteractionsFromAPI')));
  }

  async addOrderToUserQuickList(order: UserRememberedListOrder, userId: number, siteId: number): Promise<boolean> {
    const headers = new HttpHeaders({
      Accept: 'application/json',
      'EMAR-Site': `${siteId}`,
      'EMAR-User': `${userId}`,
    });
    // console.log('AddToQuickList Headers: ', headers);
    // console.log('AddToQuickList Order: ', order);

    const response = await this.http
      .post<any>(this.userQuickListBaseUrl, order, { headers })
      .pipe(
        tap(_ => console.log(`Add User QuickList by userID=${userId}`)),
        catchError(this.handleError<any>('postQuickListOrder'))
      ).toPromise();
    // console.log('AddToQuickList Response: ', response);
    return response.medication.id ? true : false;

  }

  getDurationUnitsFromAPI(userId: number, siteId: number): Observable<any> {
    const headers = new HttpHeaders({
      Accept: 'application/json',
      'EMAR-Site': `${siteId}`,
      'EMAR-User': `${userId}`,
    });

    return this.http
      .get<any>(this.durationUnitsBaseUrl, { headers })
      .pipe(catchError(this.handleError<any>('getDurationUnitsFromAPI')));
  }

  getFrequencyAdministrationsFromAPI(
    userId: number,
    siteId: number,
    frequencyId: number,
    beginDateTime?: string,
    endDateTime?: string
  ): Observable<any> {
    const headers = new HttpHeaders({
      Accept: 'application/json',
      'EMAR-Site': `${siteId}`,
      'EMAR-User': `${userId}`,
    });

    let paramString: string = beginDateTime ? `?start=${beginDateTime}` : '';

    if (endDateTime) {
      paramString = paramString ? `${paramString}&end=${endDateTime}` : `?end=${endDateTime}`;
    }

    const frequencyAdministrationsFullUrl: string = paramString ?
      `${this.frequencyAdministrationsBaseUrl}/${frequencyId}${paramString}` :
      `${this.frequencyAdministrationsBaseUrl}/${frequencyId}`;

    return this.http
      .get<any>(frequencyAdministrationsFullUrl, { headers })
      // .pipe(tap(data => console.log('ADMINISTRATIONS',beginDateTime, "Frequency="+frequencyId, data)))
      .pipe(catchError(this.handleError<any>('getFrequencyAdministrationsFromAPI')));

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
    let newAvailableFormStrength = []
    let original = options?.availableFormStrength ? options?.availableFormStrength : []
    
    for (const i in original) {

      console.log(original[i].fdbNdcInfos)
      original[i].fdbNdcInfos = original[i].fdbNdcInfos || []

      for (const j in original[i].fdbNdcInfos) {
        let temp = {...original[i]}
        temp.baseNdc = temp.fdbNdcInfos[j].baseNdc
        temp.formStrengthName = temp.fdbNdcInfos[j].packaging ? temp.formStrengthName + ' [' + temp.fdbNdcInfos[j].packaging + '] ' : temp.formStrengthName
        newAvailableFormStrength.push(temp)
      }
      
      if (original[i].fdbNdcInfos.length === 0) newAvailableFormStrength.push({...original[i]})
    }
      
    // console.log('setBrandNameOptions-new', newAvailableFormStrength );

    // this.composerBrandNameOptions = options;
    options.availableFormStrength = newAvailableFormStrength 
    //console.log('setBrandNameOptions', options);
    
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

  getSiteMedicationUnits(siteId: number): Array<MedicationUnit> {
    const siteIndex: number = this.getSiteData(siteId);
    return siteIndex === 0 || siteIndex
      ? this.siteData[siteIndex].medicationUnits
      : [];
  }

  setSiteMedicationUnits(siteId: number, units: Array<MedicationUnit>): void {
    const siteIndex: number = this.getSiteData(siteId);
    this.siteData[siteIndex].medicationUnits = units;
  }

  getSiteMedicationRouteUnits(siteId: number): Array<Route> {
    const siteIndex: number = this.getSiteData(siteId);
    return siteIndex === 0 || siteIndex
      ? this.siteData[siteIndex].medicationRouteUnits
      : [];
  }

  setSiteMedicationRouteUnits(siteId: number, routes: Array<Route>): void {
    const siteIndex: number = this.getSiteData(siteId);
    this.siteData[siteIndex].medicationRouteUnits = routes;
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

  getSiteMedicationAntimicrobialIndications(siteId: number): Array<AntimicrobialIndication> {
    // console.log('get this.siteMedicationAntimicrobialIndications', this.siteMedicationAntimicrobialIndications);
    const siteIndex: number = this.getSiteData(siteId);
    return siteIndex === 0 || siteIndex
      ? this.siteData[siteIndex].antimicrobialIndications :
      [];
  }

  setSiteMedicationAntimicrobialIndications(siteId: number, indications: AntimicrobialIndication[]) {
    const siteIndex: number = this.getSiteData(siteId);
    this.siteData[siteIndex].antimicrobialIndications = indications;
  }

  getPatientOrderInteractions(): Array<PatientOrderInteraction> {
    // console.log('get this.patientOrderInteractions', this.patientOrderInteractions);
    return this.patientOrderInteractions || [];
  }

  setPatientOrderInteractions(interactions: PatientOrderInteraction[]) {
    // console.log('set this.patientOrderInteractions', this.patientOrderInteractions);
    this.patientOrderInteractions = interactions;
  }

  getDurationUnits(): Array<DurationUnit> {
    return this.durationUnits || [];
  }

  setDurationUnits(durationUnits: DurationUnit[]) {
    this.durationUnits = durationUnits;
  }

  getOrderScheduledAdministrations(medComponentId: number): Array<ScheduledAdministration> {
    // console.log('getOrderScheduledAdministrations', this.composerMedComponents[medComponentId].composerMedForm.value.frequency.scheduledAdministrations);
    return this.composerMedComponents[medComponentId].composerMedForm.value.frequency.scheduledAdministrations || [];
  }

  setOrderScheduledAdministrations(medComponentId: number, scheduledAdministrations: Array<ScheduledAdministration>): void {
    // console.log('getOrderScheduledAdministrations', this.composerMedComponents[medComponentId].composerMedForm.value.frequency.scheduledAdministrations);
    if (this.composerMedComponents[medComponentId].composerMedForm.value.frequency) {
      this.composerMedComponents[medComponentId].composerMedForm.value.frequency.scheduledAdministrations = scheduledAdministrations;
    }
  }

  setOrderScheduledAdministration(medComponentId: number, scheduledId: number, scheduledDateTimeUTC: string) {
    this.composerMedComponents[medComponentId].composerMedForm.value.frequency.scheduledAdministrations[scheduledId].scheduleDateTime = scheduledDateTimeUTC;
  }
}
