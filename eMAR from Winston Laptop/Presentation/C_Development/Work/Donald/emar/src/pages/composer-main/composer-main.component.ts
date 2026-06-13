import {
  Component,
  OnInit,
  ViewChild,
  AfterViewInit,
  AfterContentChecked,
  ChangeDetectorRef,
  OnDestroy
} from '@angular/core';
import { CartOrder } from '../../app/interfaces/cart-order';
import { Location } from '@angular/common';
import * as moment from 'moment';
import { Router, ActivatedRoute } from '@angular/router';
import { ComposerMedComponent } from '../../pages/composer-med/composer-med.component';
import { MedOrderService } from '../../services/med-order.service';
import { CartStoreService } from '../../services/cart-store.service';
import { Patient } from '../../app/interfaces/patient';
import { PatientStoreService } from '../../services/patient-store.service';
import { ComposerSchedulerService } from '../../services/composer-scheduler.service';
import { UserStoreService } from '../../services/user-store.service';
import { DoseOption } from '../../app/interfaces/doseOption';
import { AntimicrobialIndication } from '../../app/interfaces/antimicrobialIndication';
import { PatientOrderInteraction } from '../../app/interfaces/patient-order-interaction';
import { DurationUnit } from '../../app/interfaces/duration-unit';
import { DateTimePipe } from '../../shared/pipes/dateTime';

import {
  NgbAccordion,
  NgbPanelChangeEvent,
  NgbPanel,
} from '@ng-bootstrap/ng-bootstrap';
import { FrequencyFormComponent } from '../composer-med/frequency-form/frequency-form.component';
import { DetailFormComponent } from '../composer-med/detail-form/detail-form.component';
import { FormStrength } from 'src/app/interfaces/formStrength';
import { Subject, Subscription } from 'rxjs';

import { PatientMedOrderService } from 'src/services/patient-med-order.service'
import { PatientMedOrderStoreService } from 'src/services/patient-med-order-store.service'
import { AdministrationAction } from 'src/app/interfaces/order';

@Component({
  selector: 'composer-main',
  templateUrl: './composer-main.component.html',
  styleUrls: ['./composer-main.component.scss'],
  providers: [DateTimePipe]
})
export class ComposerMainComponent implements OnInit, AfterViewInit, AfterContentChecked, OnDestroy {
  @ViewChild('acc') accordionComponent: NgbAccordion;
  composerMedComponents: Array<ComposerMedComponent>;
  antimicrobialRequiredIndicator: boolean = false;
  addNewMedComponent: boolean = false;
  overallOrderValid: boolean = false;
  gotData: boolean = false;
  userSiteId: number = null;
  doseOptions: Array<DoseOption> = [];
  antimicrobialIndications: Array<AntimicrobialIndication> = [];
  patientOrderInteractions: Array<PatientOrderInteraction> = [];
  durationUnits: Array<DurationUnit> = [];
  activeIds: Array<string> = ['medComponent-0'];
  patient: Patient;
  patientId: number;
  userId: number;
  siteUTCOffset: string;
  medId: number;
  convertedBrandName: string;
  initialComposerData: any;
  formularyPyxisMatchImgPath: string;
  formularyInpatientMatchImgPath: string;
  formularyOutpatientMatchImgPath: string;
  hasAllergyInteractions: boolean = false;
  hasMedicationInteractions: boolean = false;
  subscriptionOrderValidity: Subscription;
  subscriptionAddNewMedComponent: Subscription;
  subscriptionNewFormStrengthSelected: Subscription;
  subscriptionNewMedComponentAdded: Subscription;
  subscriptionAccordionPanelChange: Subscription;

  constructor(
    private datePipe: DateTimePipe,
    private route: ActivatedRoute,
    private composerSchedulerService: ComposerSchedulerService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    public patientStoreService: PatientStoreService,
    private cdref: ChangeDetectorRef,
    private _location: Location,
    private patientMedOrderService: PatientMedOrderService,
    private patientMedOrderStoreService: PatientMedOrderStoreService,
    private router: Router,
  ) {
    this.userId = this.userStoreService.userId;
    this.userSiteId = this.userStoreService.userSiteId;
    this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
  }

  ngOnInit(): void {

    this.medId = this.route.snapshot.params['medId'];
    this.initialComposerData = this.composerSchedulerService.getInitialComposerData();
    // console.log('ComposerMainThis', this);
    this.patient = this.patientStoreService.patient;
    const patientId = this.patientStoreService.patientId;
    this.patientId = this.patientStoreService.patientId;
    this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
    this.convertedBrandName = encodeURIComponent(this.initialComposerData.med.brandName);

    // this.initialComposerData.med.pyxisMatch = 4;
    // this.initialComposerData.med.inpatientMatch = 4;
    // this.initialComposerData.med.outpatientMatch = 4;

    this.formularyPyxisMatchImgPath = this.initialComposerData.med.pyxisMatch ?
      this.getFormularyMatchIconPath('pyxis', this.initialComposerData.med.pyxisMatch) : null;
    this.formularyInpatientMatchImgPath = this.initialComposerData.med.inpatientMatch ?
      this.getFormularyMatchIconPath('inpatient', this.initialComposerData.med.inpatientMatch) : null;
    this.formularyOutpatientMatchImgPath = this.initialComposerData.med.outpatientMatch ?
      this.getFormularyMatchIconPath('outpatient', this.initialComposerData.med.outpatientMatch) : null;

    this.getRequiredAPIData();
    // Setup overall medication validity checking
    this.subscriptionOrderValidity = this.composerSchedulerService.shouldCheckOverallMedOrderValidity.subscribe(
      () => {
        if (
          this.composerSchedulerService.shouldCheckOverallMedOrderValidity.value
        ) {
          this.checkOverallOrdersValidity();
        }
      }
    );

    // Setup first (main) medication component

    if (
      (!this.composerMedComponents ||
        this.composerMedComponents.length === 0) &&
      this.initialComposerData.med
    ) {
      this.composerSchedulerService.addNewComposerMedComponent();
      this.cdref.detectChanges();
      // console.log('componentAddAttempt', this.composerMedComponents);
    }

    this.subscriptionAddNewMedComponent = this.composerSchedulerService.addNewMedComponent.subscribe(() => {
      if (this.composerSchedulerService.addNewMedComponent.value) {
        // console.log('addNewComponentMedEventHeard');
        this.addNewMedComponent = true;
      }
    });

    this.subscriptionNewFormStrengthSelected = this.composerSchedulerService.newFormStrengthSelected.subscribe(() => {
      if (this.composerSchedulerService.newFormStrengthSelected.value !== -1 &&
        this.composerMedComponents[0]) {
        const formStrengthId = this.composerSchedulerService.newFormStrengthSelected.value;
        // alert(`New Form Strength Selected Event: ${formStrengthId}`);
        // console.log('formStrengthOptions', this.composerMedComponents[0].options);
        // const formStrength = this.composerMedComponents[0].options.availableFormStrength.find(fs => fs.id === formStrengthId);
        const formStrength = this.composerMedComponents[0].options.availableFormStrength.find(fs => fs.medicationId === formStrengthId);
        if (formStrength) {
          // alert(`refresh formStrength: ${formStrength.medicationId}`);
          // console.log('refresh formStrength', formStrength);
          this.refreshFormStrengthData(formStrength);
        }
        // console.log('newFormStrengthSelected');
      }
    });

    this.subscriptionNewMedComponentAdded = this.composerSchedulerService.newMedComponentAdded.subscribe(() => {
      if (this.composerSchedulerService.newMedComponentAdded.value && this.composerMedComponents.length !== 0) {
        this.addNewMedComponent = false;
        this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
        // console.log('ComponentAddedMedEventHeard0', this.composerMedComponents);
        if (this.composerMedComponents.length > 1) { this.accordionComponent.collapseAll(); }
        this.activeIds = [`medComponent-${this.composerMedComponents.length - 1}`];
        // console.log('ComponentAddedMedEventHeard1', this.composerMedComponents);
        this.composerSchedulerService.shouldCheckOverallMedOrderValidity.next(
          true
        );
        // console.log('ComponentAddedCheckArray');
      }
    });
  }

  ngAfterViewInit() {
    // console.log('ngAfterViewInit', this.accordionComponent);
    if (this.accordionComponent) {
      this.subscriptionAccordionPanelChange = this.accordionComponent.panelChange.subscribe(
        (panelChangeEvent: NgbPanelChangeEvent) => {
          if (panelChangeEvent.panelId) {
            // console.log('panelChangeEvent: ', panelChangeEvent);
            if (panelChangeEvent.nextState) {
              this.activeIds = [panelChangeEvent.panelId];
            }
            this.accordionComponent.panels.forEach((panel) => {
              if (panel.isOpen) {
                const medComponentId: number = parseInt(
                  panel.id.split('-').pop(),
                  10
                );
                this.setMedComponentText(medComponentId);
              }
            });
          }
        }
      );
    }
  }

  ngAfterContentChecked() {
    // console.log('contentChecked');
    this.cdref.detectChanges();
  }

  ngOnDestroy() {
    // this.composerSchedulerService.shouldCheckOverallMedOrderValidity.unsubscribe();
    // this.composerSchedulerService.addNewMedComponent.unsubscribe();
    // this.composerSchedulerService.newFormStrengthSelected.unsubscribe();
    // this.composerSchedulerService.newMedComponentAdded.unsubscribe();
    // this.accordionComponent.panelChange.unsubscribe();
    //
    this.subscriptionOrderValidity.unsubscribe();
    this.subscriptionAddNewMedComponent.unsubscribe();
    this.subscriptionNewFormStrengthSelected.unsubscribe();
    this.subscriptionNewMedComponentAdded.unsubscribe();
    this.subscriptionAccordionPanelChange.unsubscribe();

  }

  async checkOverallOrdersValidity() {
    await this.composerSchedulerService.checkOverallMedOrderValidity()
      .then(res => {
        this.overallOrderValid = res;
      });

  }

  isComboMed(): boolean {
    return (
      this.composerMedComponents[0] &&
      this.composerMedComponents[0].composerMedForm &&
      this.composerMedComponents[0].composerMedForm.value &&
      this.composerMedComponents[0].composerMedForm.value.med &&
      this.composerMedComponents[0].composerMedForm.value.med.formStrengthOptions &&
      this.composerMedComponents[0].composerMedForm.value.med.formStrengthOptions.combo
    ) ? true :
      false;
  }

  getFormularyMatchIconPath(formularyType: string, formularyValue: number): string {
    switch (formularyType) {
      case 'pyxis': {
        if (formularyValue === 3 || formularyValue === 4) {
          return '../../assets/img/formuM.svg';
        } else if (formularyValue === 2) {
          return '../../assets/img/formu1M.svg';
        }
        else if (formularyValue === 1) {
          return '../../assets/img/formu2M.svg';
        } else {
          return null;
        }
      }
      case 'inpatient': {
        if (formularyValue === 3 || formularyValue === 4) {
          return '../../assets/img/formuI.svg';
        } else if (formularyValue === 2) {
          return '../../assets/img/formu1I.svg';
        }
        else if (formularyValue === 1) {
          return '../../assets/img/formu2I.svg';
        } else {
          return null;
        }
      }
      case 'outpatient': {
        if (formularyValue === 3 || formularyValue === 4) {
          return '../../assets/img/formuO.svg';
        } else if (formularyValue === 2) {
          return '../../assets/img/formu1O.svg';
        }
        else if (formularyValue === 1) {
          return '../../assets/img/formu2O.svg';
        } else {
          return null;
        }
      }
      default: {
        return null;
      }
    }
  }

  async getRequiredAPIData() {
    // Get/Set Brand Name Options
    let rememberedListItemType: string = '';

    if (this.initialComposerData.source === 'quick-list') {
      rememberedListItemType = 'UserQuickListItem';
    } else if (this.initialComposerData.source === 'dept-list') {
      rememberedListItemType = 'DepartmentPreferredListItem';
    } else if (this.initialComposerData.source === 'groups') {
      rememberedListItemType = 'GroupRememberedOrder';
    } else if (this.initialComposerData.source === 'cart-order') {
      rememberedListItemType = 'PatientCartOrder';
    } else if (this.initialComposerData.source === 'patient-order') {
      rememberedListItemType = 'PatientOrder';
      this.initialComposerData.med.beginDatetime = ''
      this.initialComposerData.med.endDatetime = ''
    } else {
      rememberedListItemType = '';
    }

    const sourceType = this.initialComposerData.sourceType?.toLowerCase() === 'all' ? 'all' : null 
    const brandNameRequest = (rememberedListItemType) ?
      this.composerSchedulerService
        .getBrandNameOptionsFromAPI(this.userSiteId, '', rememberedListItemType, this.initialComposerData.med.id).toPromise()
      :
      this.composerSchedulerService
        // .getBrandNameOptionsFromAPI(this.userSiteId, '', '', null, this.initialComposerData.med.link.href).toPromise();
        .getBrandNameOptionsFromAPI(this.userSiteId, this.convertedBrandName, null, null, null, sourceType).toPromise();
    // Get/Set Site Frequencies
    const siteFrequenciesRequest = this.composerSchedulerService
      .getSiteMedicationFrequenciesFromAPI(this.userSiteId).toPromise();

    // Get/Set Site Medication Units
    const siteUnitsRequest = this.composerSchedulerService
      .getSiteMedicationUnitsFromAPI(this.userSiteId).toPromise();

    // Get/Set Site Medication Route Units
    const siteRouteUnitRequest = this.composerSchedulerService
      .getSiteMedicationRoutesFromAPI(this.userSiteId).toPromise();

    // const dosingInfoRequest = this.composerSchedulerService
    //   .getDosingOptionsFromAPI(this.initialComposerData.med.medication.id).toPromise();

    const antimicrobialIndicationsRequest = this.composerSchedulerService
      .getSiteMedicationAntimicrobialIndicationsFromAPI(this.userSiteId).toPromise();

    const durationUnitsRequest = this.composerSchedulerService.getDurationUnitsFromAPI(this.userId, this.userSiteId).toPromise();

    const results = await Promise.all([
      brandNameRequest,
      siteFrequenciesRequest,
      siteUnitsRequest,
      siteRouteUnitRequest,
      // dosingInfoRequest,
      antimicrobialIndicationsRequest,
      durationUnitsRequest]);

    // console.log('results', results);

    results.forEach((res, index) => {
      switch (index) {
        case 0: {
          this.composerSchedulerService.setBrandNameOptions(res);
          break;
        }
        case 1: {
          this.composerSchedulerService.setSiteMedicationFrequencies(this.userSiteId, res);
          break;
        }
        case 2: {
          this.composerSchedulerService.setSiteMedicationUnits(this.userSiteId, res);
          break;
        }
        case 3: {
          this.composerSchedulerService.setSiteMedicationRouteUnits(this.userSiteId, res);
          break;
        }
        // case 4: {
        //   this.composerSchedulerService.setDosingOptions(res);
        //   this.doseOptions = this.composerSchedulerService.getDosingOptions();
        //   break;
        // }
        case 4: {
          this.composerSchedulerService.setSiteMedicationAntimicrobialIndications(this.userSiteId, res);
          this.antimicrobialIndications = this.composerSchedulerService.getSiteMedicationAntimicrobialIndications(this.userSiteId);
          break;
        }
        case 5: {
          this.composerSchedulerService.setDurationUnits(res);
          this.durationUnits = this.composerSchedulerService.getDurationUnits();
          break;
        }
        default: {
          break;
        }
      }
    });

    // Make sure all data is collected before moving forward
    if (
      this.composerSchedulerService.getBrandNameOptions() &&
      this.composerSchedulerService.getSiteMedicationFrequencies(
        this.userSiteId
      ) &&
      this.composerSchedulerService.getSiteMedicationUnits(
        this.userSiteId
      ) &&
      this.composerSchedulerService.getSiteMedicationRouteUnits(
        this.userSiteId
      )
    ) {
      this.gotData = true;

    }

    // console.log('gotData', this.gotData, this);

    return this.gotData;
  }

  async refreshFormStrengthData(formStrength: FormStrength) {
    // console.log('newFormStrength', formStrength);
    this.antimicrobialRequiredIndicator = formStrength.antimicrobialRequiredIndicator;

    const dosingInfoRequest = (formStrength.medicationId) ?
      this.composerSchedulerService
        .getDosingOptionsFromAPI(formStrength.medicationId).toPromise() :
      null;

    let orderItemType: string = '';
    let orderItemId: number = null;

    switch (this.initialComposerData.source) {
      case 'quick-list': {
        orderItemType = 'UserQuickListItem';
        orderItemId = this.initialComposerData.med.id;
        break;
      }
      case 'dept-list': {
        orderItemType = 'DepartmentPreferredListItem';
        orderItemId = this.initialComposerData.med.id;
        break;
      }
      case 'groups': {
        orderItemType = 'GroupRememberedOrder';
        orderItemId = this.initialComposerData.med.id;
        break;
      }
      case 'med-search': {
        orderItemType = 'MedicationItem';
        orderItemId = formStrength.medicationId;
        break;
      }
      case 'cart-order': {
        orderItemType = 'PatientCartOrder';
        orderItemId = this.initialComposerData.med.id;
        break;
      }
      case 'patient-order': {
        orderItemType = 'PatientOrder';
        orderItemId = this.initialComposerData.med.id;
        break;
      }
      default: {
        alert(`Order Type Item: ${orderItemType}}`);
        orderItemType = this.initialComposerData.med.medication.id;
        break;
      }
    }

    const patientOrderInteractionRequest = orderItemType ?
      this.composerSchedulerService.getPatientOrderInteractionsFromAPI(
        this.userSiteId,
        this.userId,
        this.patientId,
        orderItemType,
        orderItemId,
      ).toPromise() :
      null;

    const results = await Promise.all([
      dosingInfoRequest,
      patientOrderInteractionRequest
    ]);

    results.forEach((res, index) => {
      switch (index) {
        case 0: {
          this.composerSchedulerService.setDosingOptions(res);
          this.doseOptions = this.composerSchedulerService.getDosingOptions();
          // console.log('newDosingOptions', this.doseOptions);
          break;
        }
        case 1: {
          this.composerSchedulerService.setPatientOrderInteractions(res);
          this.patientOrderInteractions = this.composerSchedulerService.getPatientOrderInteractions();
          this.hasAllergyInteractions = this.checkForOrderInteractions('alg');
          this.hasMedicationInteractions = this.checkForOrderInteractions('drug');
          // console.log('newPatientOrderInteractions', this.patientOrderInteractions);
          break;
        }
        default: {
          break;
        }
      }
    });
    // alert('Refresh Res Data');
  }

  checkForOrderInteractions(type: string): boolean {
    let found: boolean = false;
    this.patientOrderInteractions.forEach(node => {
      if (!found && type === 'alg') {
        found = (node.reactions.length > 0) ? true : false;
      } else if (!found && type === 'drug') {
        found = (node.interactions.length > 0) ? true : false;
      }
    });
    return found;
  }

  getMedLabel(): string {
    return (this.initialComposerData.action === 'add'
            || this.initialComposerData.action === 'repeat'
            || this.initialComposerData.action === 'modify') ? 'New Orders: ' : 'Update Order: ';
  }

  getPanelCardClass(): string {
    return (this.composerMedComponents.length === 1) ? 'hide-panel' : '';
  }

  resetComposerMedComponent(id): void {
    this.composerSchedulerService.resetComponentMedFormById(id);
  }

  getMedicationOrderTitle(): string {
    // return `${this.initialComposerData.med.medication.displayName} ${this.composerMedComponents && this.composerMedComponents[0] ? ` - ${this.composerMedComponents[0].composerMedForm.value.med.formStrengthName}` : ''}`;
    return (this.composerMedComponents && this.composerMedComponents[0]) ? `${this.composerMedComponents[0].composerMedForm.value.med.formStrengthName}` : '';
  }

  getLatestMedComponentDOMId(): string {
    // this.addNewMedComponent = false;
    // console.log('addNewMedComponent', this.addNewMedComponent);
    return !this.composerMedComponents ||
      this.composerMedComponents.length === 0
      ? 'medComponent-0'
      : `medComponent-${this.composerMedComponents.length} `;
  }

  setMedComponentText(id: number): string {
    // console.log('setMedComponentText0', this.composerMedComponents, id);
    if (this.composerMedComponents && this.composerMedComponents[id]) {
      const data = this.composerMedComponents[id].composerMedForm.value;
      // console.log('setMedComponentData', data);
      let textLine1: string;
      let textLine2: string;

      // Check Text Line #1
      textLine1 = `${this.getCompData(data, 'dose')} ${this.getCompData(
        data,
        'duration'
      )
        } ${this.getCompData(data, 'route')} ${this.getCompData(
          data,
          'priority'
        )
        } ${this.getCompData(data, 'frequency')} `
        .split('  ')
        .join(' ');
      textLine1 = new RegExp(`^ [\s]$`).test(textLine1) ? '' : textLine1;
      // Check Text Line #2
      textLine2 = `${this.getCompData(data, 'startTime')} ${this.getCompData(
        data,
        'endTime'
      )
        } `
        .split('  ')
        .join(' ');
      textLine2 = new RegExp(`^ [\s]$`).test(textLine2) ? '' : textLine2;
      // console.log('textline1', textLine1);
      // console.log('textline2', textLine2);

      this.composerMedComponents[id].title =
        textLine1 || textLine2 ? `${textLine1} \n${textLine2} ` : `Order #${id} `;
      return this.composerMedComponents[id].title;
    } else {
      return `Order #${id} `;
    }
  }

  getCompData(data: any, name: string): string {
    // console.log('getCompData', data);
    if (Object.keys(data).length > 0) {
      switch (name) {
        case 'dose': {
          if (!data.med.dose || !data.med.doseUnitName) {
            return '';
          } else {
            return `${data.med.dose} ${data.med.doseUnitName} `;
          }
        }
        case 'route': {
          if (!data.med.routeName) {
            return '';
          } else {
            return `${data.med.routeName} `;
          }
        }
        case 'priority': {
          if (!data.med.priority) {
            return '';
          } else {
            return `${data.med.priority} `;
          }
        }
        case 'diagnosis': {
          if (!data.detail.diagnosis) {
            return '';
          } else {
            return `${data.detail.diagnosis} `;
          }
        }
        case 'indication': {
          if (!data.detail.antimicrobialIndication) {
            return '';
          } else {
            return `${data.detail.antimicrobialIndication} `;
          }
        }
        case 'frequency': {
          if (!data.frequency.frequency) {
            return '';
          } else {
            return `${data.frequency.frequency} `;
          }
        }
        case 'duration': {
          if (!data.frequency.duration || !data.frequency.durationUnit) {
            return '';
          } else {
            return `[${data.frequency.duration} ${data.frequency.durationUnit.name}]`;
          }
        }
        case 'startTime': {
          if (!data.frequency.startTime) {
            return '';
          } else {
            return `Start On: ${data.frequency.startTime} `;
          }
        }
        case 'endTime': {
          if (!data.frequency.endTime) {
            return '';
          } else {
            return `End On: ${data.frequency.endTime} `;
          }
        }
        default: {
          return '';
        }
      }
    }
  }

  isMedComponentPanelSelected(id: string): boolean {
    const panel = this.accordionComponent.panels.find((pn) => pn.id === id);
    // console.log('isPanelOpen', panel && panel.isOpen);
    return (panel && panel.isOpen) || false;
  }

  // addOrderToQuickList(): void {
  //   alert('Add To Quick List!');
  // }

  continueOrder(): void {
    // console.log('continueThis', this);
    this.composerSchedulerService.addNewComposerMedComponent();
    this.cdref.detectChanges();
    // console.log('componentAddAttemptFromContinueButton');
  }

  cancelOrder(): void {
    this.composerSchedulerService.resetAllComponentMedForms();
    this._location.back();
  }

  async submitOrder() {
    // console.log('submitOrders', this.composerMedComponents);
    await this.submitCartOrders();
    this.composerSchedulerService.resetAllComponentMedForms();
    // console.log('processCartOrder');

    if (this.initialComposerData.action === 'repeat'
        || this.initialComposerData.action === 'modify') {

      console.log('^^^^^IN SCHEDULER^^submitOrder^^^^DONE action: ', this.initialComposerData.action)
      console.log('^^^^^IN SCHEDULER^^submitOrder^^^^about to navigate to Med Svc patientId: ', this.patientId)
      this.router.navigate([`patients/${this.patientId}/medservice`])

    } else {
      this._location.back();
    }
  }

  async submitCartOrders() {
    // console.log('submitCartOrders', this.composerMedComponents);
    console.log('********************submitCartOrders', this.composerMedComponents);
    console.log('********************submitCartOrders**initialComposerData', this.initialComposerData);
    this.composerMedComponents.forEach((medComponent, index) => {
      // **************************************
      const orderForm = medComponent.composerMedForm;
      const cartOrder: CartOrder = {
        medication: this.initialComposerData.med.medication,
        addDatetime: this.datePipe.transform(Date.now().toString(), 'UTC', this.siteUTCOffset),
        // addDatetime: null,
        patientId: this.patientId,
        id: (this.initialComposerData.action === 'add' 
              || this.initialComposerData.action === 'repeat'
              || this.initialComposerData.action === 'modify') ? 0 : this.initialComposerData.med.id,
        medicationId: orderForm.value.med.formStrengthOptions.medicationId,
        // Med Form
        dose: orderForm.value.med.dose,
        medicationUnitId: orderForm.value.med.doseUnitData.id,
        medicationRouteId: orderForm.value.med.routeOfAdministrationData.id,
        orderNotes: orderForm.value.med.administrationInstructionsText,
        priority: orderForm.value.med.priority,
        // Detail Form
        antimicrobialIndicationId: this.composerMedComponents[0].composerMedForm.value.detail &&
          this.composerMedComponents[0].composerMedForm.value.detail.antimicrobialIndication ?
          this.composerMedComponents[0].composerMedForm.value.detail.antimicrobialIndication.id
          : null,
        antimicrobialIndicationText: this.composerMedComponents[0].composerMedForm.value.detail &&
          this.composerMedComponents[0].composerMedForm.value.detail.antimicrobialIndicationFreeText ?
          this.composerMedComponents[0].composerMedForm.value.detail.antimicrobialIndicationFreeText
          : null,
        patientProblemId: this.composerMedComponents[0].composerMedForm.value.detail &&
          this.composerMedComponents[0].composerMedForm.value.detail.diagnosis ?
          this.composerMedComponents[0].composerMedForm.value.detail.diagnosis.id
          : null,
        //Frequency Form
        frequencyId: orderForm.value.frequency.frequencyData.id,
        duration: orderForm.value.frequency.duration,
        durationUnitId: orderForm.value.frequency.durationUnit ? orderForm.value.frequency.durationUnit.id : null,
        beginDatetime: orderForm.value.frequency.startTimeUTC || null,
        endDatetime: orderForm.value.frequency.endTimeUTC || null,
        userId: this.userId,
        // TODO: Fill out these values in the future when the API is ready
        // userQuickListItemId: this.initialComposerData.source === 'quick-list' ? this.initialComposerData.med.id : null,
        userQuickListItemId: this.initialComposerData.source === 'quick-list' || this.initialComposerData.med.userQuickListItemId ?
        this.initialComposerData.med.userQuickListItemId || this.initialComposerData.med.id :
        null,
        // PRN will be determined by the API based on frequencyId selected
        prn: !orderForm.value.frequency.frequencyData.prn ? false : true,
        prnIndication: !orderForm.value.frequency.frequencyData.prn ? '' : orderForm.value.frequency.prnIndicationDescription,
        pointInTime: orderForm.value.frequency.frequencyData.pointInTime,
        // cartOrderAdministrations: !orderForm.value.frequency.scheduledAdministrations ? [] :
        //   [...orderForm.value.frequency.scheduledAdministrations],
        cartOrderAdministrations: !orderForm.value.frequency.scheduledAdministrations ||
          !orderForm.value.frequency.scheduledAdministrations.length ? [] :
          orderForm.value.frequency.scheduledAdministrations.map(schAdmin => {
            return {
              pointInTime: schAdmin.pointInTime,
              administrationScheduledDatetime: schAdmin.scheduleDateTime || null,
              stopScheduledDatetime: schAdmin.stopDateTime || null,
            }
          }),
        ndc: orderForm.value.med.formStrengthOptions.baseNdc || null,
      };
      // **************************************
      if (this.initialComposerData.action === 'update') {
        // console.log('cartOrderUpdate', this.patientId, this.userId, cartOrder);
        this.cartStoreService.updateCartOrder(cartOrder, this.patientId, this.userId);
      } else {
        // console.log('cartOrderPost', this.patientId, this.userId, cartOrder);
        this.cartStoreService.postCartOrder(cartOrder, this.patientId, this.userId);

        console.log('+++++++++++++++++order action: ', this.initialComposerData.action)
        console.log('+++++++++++++++++initialComposerData: ', this.initialComposerData)
        // if (this.initialComposerData.action === 'repeat') {
        if (this.initialComposerData.action === 'modify') {

        console.log('+++++++++++++++++it is MODIFY')

          // cancel the original order on behalf of the business logic
          // 1. retrieve cancel order template
          let cancelOrderAction: AdministrationAction;
          cancelOrderAction = this.initialComposerData.med.availableActions?.find( action => action.availableAction === 'Cancel' )
          console.log('+++++++++++++++++it is MODIFY...cancelOrderAction: ', cancelOrderAction)

          // const cancelOrderAction: AdministrationAction = {
          //   // actionId: 2,
          //   availableAction: "Cancel",
          //   buttonText: "Cancel",
          //   link: "http://localhost:4200/api/orders/933/actions/2"
          // }

          // 2. construct cancel payload
          let cancelOrderTemplate = {template: {}}
          let cancelOrderResponse = this.patientMedOrderService
            .postOrderAction(cancelOrderAction)
            .subscribe(data => {
              cancelOrderTemplate = data
              console.log('^^^^^IN SCHEDULER^^^^^^RESULT POST ACTION: cancelOrderTemplate: ', cancelOrderTemplate);
              // const cancelOrderUrl = data.template.link.href
              // console.log('^^^^^IN SCHEDULER^^^^^^CANCEL cancelOrderUrl: ', cancelOrderUrl)

              let cancelOrderPayload = {}
              cancelOrderPayload = data.template.promptGroups.reduce( (prev, promptGroup) => {
                console.log('^^^^^IN SCHEDULER^^^^^^^^^^^CANCEL promptGroup: ', promptGroup)

                promptGroup.prompts?.forEach( (prompt) => {
                  console.log('^^^^^IN SCHEDULER^^^^^^^^^^^CANCEL forEach prompt: ', prompt)
                  if (prompt.prompt === 'Notes' && prompt.type === 'MultiLineFreeText') 
                    prev[prompt.id] = `This Order has been modified by User ${this.userId}. System Cancelled`
                  else if (prompt.prompt === 'At' && prompt.type === 'DateTime') {
                    prev[prompt.id] = prompt.default // API generated current date/time
                  } else {
                    prev[prompt.id] = null
                  }
                })

                return prev
              }, {})


              console.log('^^^^^IN SCHEDULER^^^^^^CANCEL cancelOrderTemplate: ', cancelOrderTemplate)
              console.log('^^^^^IN SCHEDULER^^^^^^CANCEL cancelOrderPayload: ', cancelOrderPayload)

              // 3. send cancel order request to API
              this.patientMedOrderService.postTemplate(cancelOrderTemplate.template, cancelOrderPayload)
                .subscribe(data => {
                    console.log('^^^^^IN SCHEDULER^^^^^^RESULT POST TEMPLATE', data);
                    console.log('^^^^^IN SCHEDULER^^^^^^SUBSCRIBE TEMPLATE');
                    
                    // update patient current order
                    this.patientMedOrderStoreService.fetchPatientMedOrder(this.patientStoreService.patientId)
                    console.log('^^^^^IN SCHEDULER^^^^^^POST ACTION completed: patientMedOrderStoreService: fetchPatientMedOrder')
                });

            });

        }
      }
    });
  }

  validOrder(): string {
    // alert('Valid Order!');
    // console.log('overallOrderValid', this.overallOrderValid);
    return this.overallOrderValid ? 'VALID' : 'INVALID';
  }

  isMedComposerFormInvalid(id: number) {
    // console.log('isMedComposerFormInvalid', id, this.composerMedComponents);
    return this.composerMedComponents[id].isMedComposerFormInvalid();
  }

  getMedComponentValidityText(id: number): string {
    return this.isMedComposerFormInvalid(id) ? 'INVALID' : 'VALID';
  }

  removeMedComponent(id: number): void {
    // console.log('removeComposerMedComponentsStart', this.composerMedComponents);
    this.composerSchedulerService.removeMedComponent(id);
    this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
    if (this.composerMedComponents.length === 1) {
      this.activeIds = [`medComponent-${this.composerMedComponents.length - 1}`];
    }
    this.composerSchedulerService.shouldCheckOverallMedOrderValidity.next(true);
    // console.log('removeComposerMedComponentsDone', this.composerMedComponents);
  }

  getActionText() {
    let action = this.initialComposerData.action || 'add';
    return action === 'update' ? 'Update Order' : 'Add Order';
  }

}
