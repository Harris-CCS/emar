import { Component, OnInit, AfterViewInit, OnDestroy, ViewEncapsulation, ViewChild, ElementRef, ComponentFactoryResolver, HostListener } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import * as moment from 'moment';
import { FormBuilder, FormGroup } from '@angular/forms';

import { Patient } from '../../app/interfaces/patient';
import { PatientBriefComponent } from '../../shared/component/patient-brief/patient-brief.component';
import { PatientService } from '../../services/patient.service';
import { MedOrderService } from '../../services/med-order.service';
import { PatientMedOrderStoreService } from '../../services/patient-med-order-store.service';
import { Order, OrderAdministration } from 'src/app/interfaces/order';
import { ModalService } from 'src/services/modal.service';
import { PatientStoreService } from '../../services/patient-store.service';
import { SiteStoreService } from 'src/services/site-store.service';
import { MyPatientsStoreService } from 'src/services/my-patients-store.service'
import { AllPatientsStoreService } from 'src/services/all-patients-store.service'
import { PharmVerificationStoreService } from 'src/services/pharm-verification-store.service'
 
const INTERVAL_MINUTES = 5; // length in minutes of an interval
const NB_HOURS = 8; // default number of hours displayed (will be reduce on  smaller screen)
const PRE_HOURS_DISPLAY = 2; // hours before current time display begins
const RELOAD_SECONDS = 60; // reload time
const STAT_ID = 2;
const NB_CHAR_ORDER_NAME = 60; /* order displayed name will be truncated to this size to have a fix width for sticky header for IE11 */
const MOCKUP = false; // true: mockup orders

import { ORDERS } from '../../app/mockup/orders';
// import { ORDERS_SORT } from '../../app/mockup/orders-sort';
import { PatientMedOrderService } from 'src/services/patient-med-order.service';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
import { UserStoreService } from 'src/services/user-store.service';
import { DatePipe } from '@angular/common';
import { PatientResponse } from 'src/app/interfaces/patient-response';

import { Observable, of, Subject, Subscription } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  tap,
  switchMap,
  takeUntil,
} from 'rxjs/operators';

interface Interval {
  event: string;
  isHour: boolean;
  isNow: boolean;
  time: string;
}
interface OrderInterval {
  orderId: number;
  intervals: Interval[];
}
interface TypeCount {
  upcomingOrders?: number;
  myPatients?: number
  all?: number;
  prn?: number;
  stat?: number;
  scheduled?: number;
  continuous?: number;
  timed?: number;
  iv?: number;
  ancilliary?: number;
  rxVerificationComplete?: number;
  rxVerificationNeeded?: number;
}


@Component({
  selector: 'department-dashboard',
  templateUrl: './department-dashboard.component.html',
  styleUrls: ['./department-dashboard.component.scss', '../../assets/css/site.css'], // TODO remove site.css
  // encapsulation: ViewEncapsulation.None
})

export class DepartmentDashboardComponent implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild('gridEl') gridEl: ElementRef;
    // @ViewChild('thead', {static: true}) thead: ElementRef;
    patient: Patient;
    patientsList: PatientResponse = {};  // display
    upcomingOrders: PatientResponse = {};  // Upcoming Orders patient list
    myPatients: PatientResponse = {};  // My patients list
    all: PatientResponse = {};  // All patients list
    rxVerificationNeeded: PatientResponse = {};  // RX Verification patients list
    filter: string = 'upcomingOrders';
    // filter: string = 'all';
    
    sort: string = 'Entry time';
    orders: Order[] = [];

    patientOrders: object = {};  // display
    upcomingOrdersPatientOrders: object = {};
    myPatientsPatientOrders: object = {};
    allPatientOrders: object = {};
    rxVerificationNeededPatientOrders: object = {}

    patientOrderCounts: object = {};
    
    nbPatients: TypeCount = { upcomingOrders: 0, myPatients: 0, all: 0, rxVerificationNeeded: 0};

    patientConsolidatedOrderAdministrations: object = {};
    patientConsolidatedOrderAdministrationOrderInfo: object = {};
    consolidatedOrderAdministrationGroup: object = {};  // not used
    minutesPerGroup: number = 15

    times: string[] = [];
    currentTime: string; // 2020-09-04T13:46:59-04:00
    intervals: OrderInterval[] = [];
    fillerIntervals: Interval[] = []; //for patients who do not have orders

    nbHours: number = NB_HOURS;
    displayDose: boolean = true; // TODO Api
    displayStrength: boolean = true; // TODO Api
    reload: number = null; // reload  var
    currentIntervalTime: string = null;
    timeHasBeenMoved: boolean = false;
    fetching: boolean = false; /* we have no data, we are calling the api */
    fetchingMsg: string = ''
    updating: boolean = false; /* the data are perhaps out of sync, we are calling the api */
    siteUTCOffset: string; // -06:00
    oldestDue: string; // 2020-09-04T13:46:59-04:00 if due adminstration or ''
    nbCharOrderName: number = NB_CHAR_ORDER_NAME;
    orderNameColumnWidth: string = (NB_CHAR_ORDER_NAME/2).toString()+'rem';
    administrationsColumnWidth: string = '100%';
    startBeforeWidth: number = 25;
    userId: number;
    userSiteId: number = null;
    departmentCode: string = null;
    wardCode: string = null;
    timeLineWidth: number = 0;
    showCurrentTimeLine: boolean = true;

    // status update
    myPatientsIsLoading: boolean = false
    rxVerificationNeededIsLoading: boolean = false
    allIsLoading: boolean = false

    // patient super search
    model: any;
    // searching: boolean = false;
    // searchFailed: boolean = false;
    ngUnsubscribe = new Subject<void>();

  constructor (
    private route: ActivatedRoute,
    private patientService: PatientService,
    private medOrderService: MedOrderService,
    private formBuilder: FormBuilder,
    private modalService: ModalService,
    private patientMedOrderStoreService: PatientMedOrderStoreService,
    private patientMedOrderService: PatientMedOrderService,
    public patientStoreService: PatientStoreService,
    private userStoreService: UserStoreService,
    private siteStoreService: SiteStoreService,
    private datePipe: DatePipe,
    private myPatientsStoreService: MyPatientsStoreService,
    private allPatientsStoreService: AllPatientsStoreService,
    private pharmVerificationStoreService: PharmVerificationStoreService,
    private router: Router,
  ) {
    this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
    // console.log('SiteStore: MAR Patient: site: ', this.siteStoreService.site)
    this.userId = this.userStoreService.userId;
    this.userSiteId = this.userStoreService.userSiteId;
    // console.log('userSiteId in Department', this.userSiteId);
    this.departmentCode = this.userStoreService.departmentCode;
    this.wardCode = this.userStoreService.wardCode.toLowerCase() === 'all' ? null : this.userStoreService.wardCode

    this.myPatientsIsLoading = true
    this.rxVerificationNeededIsLoading = true
    this.allIsLoading = true
    this.fetching = true;

    console.log('MAR DEPT: (constructor) departmentCode: ', this.departmentCode)
    console.log('MAR DEPT: (constructor) wardCode: ', this.wardCode)
  }
  
  // async ngOnInit() {
  ngOnInit(): void {
      // this.sort = 'Administration time'; console.log('TESTSORT', ORDERS_SORT.sort(this.compareOrders.bind(this)));
      // const patientId: number = +this.route.snapshot.params['id'];
      // this.patient = this.patientService.getPatient(patientId);
      // const patientId = this.patientStoreService.patientId;
      // this.patient = this.patientStoreService.patient;
      console.log('~~~~~~~~~~MAR Dept ngOnInit at ', new Date().toUTCString())
      this.patientConsolidatedOrderAdministrations = {}
      this.patientConsolidatedOrderAdministrationOrderInfo = {}
      this.consolidatedOrderAdministrationGroup = {}

      // const mp = this.departmentStoreService.myPatientsResp
      // console.log('MAR DEPT: (ngOnInit) mmmmmmmmp: ', mp)


      //My Patients
      this.myPatientsStoreService.myPatients$
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(async (resp) => {
        
          this.fetching = true;
          // this.myPatientsIsLoading = true
          // this.fetchingMsg = '... Preparing My Patients Patients/Orders...'
          console.log('MAR DEPT: (ngOnInit)(subscribe myPatient$) mmmmmmmmp2: ', resp)
          this.myPatients.patients = resp
          console.log('MAR DEPT: (ngOnInit)(subscribe myPatient$) myPatients: ', this.myPatients)
          // this.prepareList()
          console.log('MAR DEPT: (ngOnInit)(subscribe myPatient$) myPatients prepareList DONE')

          // get orders from patient by includeOrders=true in API request
          this.myPatientsPatientOrders = this.myPatients.patients.reduce((prev, patient) => {
            prev[patient.id] = patient.orders
    
            return prev
          }, {})

          // change the filter to My Patients by default at initial load
          if (this.myPatients?.patients.length) {
            // this.countPatients()
            // this.nbPatients.myPatients = this.myPatients?.patients?.length;
            this.filter = 'myPatients'
            console.log('MAR DEPT: (ngOnInit)(subscribe NEW myPatient$): you have "My Patients"')
            console.log('MAR DEPT: (ngOnInit)(subscribe NEW myPatient$) going to prepare()')
            this.nbPatients.myPatients = this.myPatients.patients.length;
            this.prepare()

            console.log('MAR DEPT: (ngOnInit)(subscribe NEW myPatient$): going to refresh()')
            this.refresh();
          }
          this.myPatientsIsLoading = false
          // this.fetchingMsg = '... Preparing Pharmacy Verification Patients/Orders...'
        })

      /* get orders from patient by includeOrders=true in API request
      this.myPatientsStoreService.myPatientsOrders$
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(async (resp) => {
          this.fetchingMsg = '... Preparing My Patients orders...'
          console.log('MAR DEPT: (ngOnInit)(subscribe myPatientsOrders$) mmmmmmmmpooooooooo2: ', resp)
          this.myPatientsPatientOrders = resp
          console.log('MAR DEPT: (ngOnInit)(subscribe myPatientsOrders$) myPatientsPatientOrders: ', this.myPatientsPatientOrders)

          console.log('MAR DEPT: (ngOnInit)(subscribe myPatientsOrders$) has MY PATIENTS: ', !!this.myPatients?.patients.length)
          // change the filter to My Patients by default at initial load
          if (this.myPatients?.patients.length) {
            // this.countPatients()
            // this.nbPatients.myPatients = this.myPatients?.patients?.length;
            this.filter = 'myPatients'
            console.log('MAR DEPT: (ngOnInit)(subscribe myPatientsOrders$): you have "My Patients"')
            console.log('MAR DEPT: (ngOnInit)(subscribe myPatientsOrders$) going to prepare()')
            this.nbPatients.myPatients = this.myPatients.patients.length;
            this.prepare()

            console.log('MAR DEPT: (ngOnInit)(subscribe myPatientsOrders$): going to refresh()')
            this.refresh();
          }
        })
      */


      //Pharmacy Verification
      this.pharmVerificationStoreService.pharmVerificationPatients$
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(async (resp) => {
          // this.fetchingMsg = '... Preparing Pharmacy Verification Patients/Orders...'
          // this.rxVerificationNeededIsLoading = true
          console.log('MAR DEPT: (ngOnInit)(subscribe myPatient$) mmmmmmmmp2: ', resp)
          this.rxVerificationNeeded.patients = resp
          console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatient$) pharmVerificationPatients(rxVerificationNeeded): ', this.rxVerificationNeeded)
          // this.prepareList()
          console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatient$) pharmVerificationPatients prepareList DONE')


          // get orders from patient by includeOrders=true in API request
          this.rxVerificationNeededPatientOrders = this.rxVerificationNeeded.patients.reduce((prev, patient) => {
            prev[patient.id] = patient.orders
    
            return prev
          }, {})


          this.nbPatients.rxVerificationNeeded = this.rxVerificationNeeded.patients.length;
          console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatient$) pharmVerificationPatientsPatientOrders: ', this.rxVerificationNeededPatientOrders)
          // this.fetchingMsg = '... Preparing All & Upcoming Orders Patients/Orders...11111'
          this.rxVerificationNeededIsLoading = false
        })

/* get orders from patient by includeOrders=true in API request
      this.pharmVerificationStoreService.pharmVerificationPatientsOrders$
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(async (resp) => {
          this.fetchingMsg = '... Preparing Pharmacy Verification Patients orders...'
          console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatientsOrders$) mmmmmmmmpooooooooo2: ', resp)
          this.rxVerificationNeededPatientOrders = resp
          console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatientsOrders$) pharmVerificationPatientsPatientOrders: ', this.rxVerificationNeededPatientOrders)

          console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatientsOrders$) has pharmVerification PATIENTS: ', !!this.rxVerificationNeeded?.patients.length)
          this.nbPatients.rxVerificationNeeded = this.rxVerificationNeeded.patients.length;
          if (this.rxVerificationNeeded?.patients.length) {
            // this.filter = 'rxVerificationNeeded'
            console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatientsOrders$): you have "pharmVerification Patients"')
            console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatientsOrders$) going to prepare()')
            // this.prepare()
            
            console.log('MAR DEPT: (ngOnInit)(subscribe pharmVerificationPatientsOrders$): going to refresh()')
            // this.refresh();
          }
        })
*/

      //All & Upcoming Orders
      this.allPatientsStoreService.allPatients$
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(async (resp) => {
          // console.log('MAR DEPT: (ngOnInit)(subscribe allPatient$) allPatients resp: ', resp)
          // this.fetchingMsg = '... Preparing All & Upcoming Orders Patients/Orders...'
          // this.allIsLoading = true
          this.all.patients = resp
          console.log('MAR DEPT: (ngOnInit)(subscribe allPatient$) allPatients: ', this.all)
          // this.prepareList()
          // console.log('MAR DEPT: (ngOnInit)(subscribe allPatient$) allPatients prepareList DONE')



          // get orders from patient by includeOrders=true in API request
          this.allPatientOrders = this.all.patients.reduce((prev, patient) => {
            prev[patient.id] = patient.orders
    
            return prev
          }, {})

          console.log('MAR DEPT: (ngOnInit)(subscribe NEW) allPatientOrders: ', this.allPatientOrders)

          // this.upcomingOrders.patients = this.all.patients.filter((patient) => !!this.allPatientOrders[patient.id])
          this.upcomingOrders.patients = this.all.patients.filter((patient) => !!this.allPatientOrders[patient.id].length )
          console.log('MAR DEPT: (ngOnInit)(subscribe NEW) upComingOrdersPatients: ', this.upcomingOrders)

          this.upcomingOrdersPatientOrders = this.upcomingOrders.patients.reduce((prev, patient) => {
            prev[patient.id] = this.allPatientOrders[patient.id]
      
            return prev
          }, {})

          console.log('MAR DEPT: (ngOnInit)(subscribe NEW) upcomingOrdersPatientOrders: ', this.upcomingOrdersPatientOrders)
          this.fetching = false
          this.fetchingMsg = ''
          // this.allIsLoading = false

          console.log('MAR DEPT: (ngOnInit)(subscribe NEW) going to PREPARE')
          // this.patientConsolidatedOrderAdministrations = {}
          // this.patientConsolidatedOrderAdministrationOrderInfo = {}

          this.nbPatients.upcomingOrders = this.upcomingOrders.patients.length;
          this.nbPatients.all = this.all.patients.length;
          // if filter has changed to My Patients at initial load, do NOT run prepare() again
          if (this.myPatients?.patients.length === 0) {
            console.log('MAR DEPT: (ngOnInit)(subscribe NEW) NO MY PATIENTS going to prepare() for UpcomingOrders')
            this.prepare()
            
            console.log('MAR DEPT: (ngOnInit)(subscribe NEW) NO MY PATIENTS going to refresh() for UpcomingOrders')
            this.refresh();
          }
        })

/* get orders from patient by includeOrders=true in API request
      this.allPatientsStoreService.allPatientsOrders$
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(async (resp) => {
          // console.log('MAR DEPT: (ngOnInit)(subscribe allPatientsOrders$) allPatientsOrders resp: ', resp)
          this.fetchingMsg = '... Preparing Upcoming Orders patient orders...'
          this.allPatientOrders = resp
          console.log('MAR DEPT: (ngOnInit)(subscribe allPatientsOrders$) allPatientOrders: ', this.allPatientOrders)

          this.upcomingOrders.patients = this.all.patients.filter((patient) => !!this.allPatientOrders[patient.id])
          console.log('MAR DEPT: (ngOnInit)(subscribe allPatientsOrders$) upComingOrdersPatients: ', this.upcomingOrders)

          this.upcomingOrdersPatientOrders = this.upcomingOrders.patients.reduce((prev, patient) => {
            prev[patient.id] = this.allPatientOrders[patient.id]
      
            return prev
          }, {})

          console.log('MAR DEPT: (ngOnInit)(subscribe allPatientsOrders$) upcomingOrdersPatientOrders: ', this.upcomingOrdersPatientOrders)
          this.fetching = false
          this.fetchingMsg = ''

          console.log('MAR DEPT: (ngOnInit)(subscribe allPatientsOrders$) going to PREPARE')
          // this.patientConsolidatedOrderAdministrations = {}
          // this.patientConsolidatedOrderAdministrationOrderInfo = {}

          this.nbPatients.upcomingOrders = this.upcomingOrders.patients.length;
          this.nbPatients.all = this.all.patients.length;
          // if filter has changed to My Patients at initial load, do NOT run prepare() again
          if (this.myPatients?.patients.length === 0) {
            console.log('MAR DEPT: (ngOnInit)(subscribe allPatientsOrders$): going to prepare()')
            this.prepare()
            
            console.log('MAR DEPT: (ngOnInit)(subscribe allPatientsOrders$): going to refresh()')
            this.refresh();
          }

          // this.countPatients()
        })
 */       

    // this.refresh();
    // this.patientMedOrderService.refreshRequest.subscribe(order => {
    //   // console.log('REFRESH ORDER', order);
    //   this.refreshOrder(order);
    // });

    // this.patientMedOrderService.updateRequest.subscribe(data => {
    //   this.updating = true;
    // });

    // console.log('MAR DEPT: (ngOnInit): going to getRequiredAPIData()')
    // this.prepare()
    // await this.getRequiredAPIData();
    // console.log('departmentPatientsThis', this);
    
    // console.log('MAR DEPT: (ngOnInit): going to refresh()')

    // this.refresh();
  }

  getLoadingStatus() {
    
    // console.log('getLoadingStatus -----', this.myPatientsIsLoading, this.rxVerificationNeededIsLoading, this.allIsLoading, this.fetching)
    if (this.myPatientsIsLoading) return '... Preparing My Patients Patients/Orders...'
    if (this.rxVerificationNeededIsLoading) return '... Preparing Pharmacy Verification Patients/Orders...'
    if (this.allIsLoading) return '... Preparing All & Upcoming Orders Patients/Orders...'

    return ''
  }

  ngAfterViewInit() {

    // console.log('**********ngAfterViewInit')          

    // console.log('***********gridEl: ', this.gridEl)
    // console.log('***********gridEl.nativeElement: ', this.gridEl.nativeElement)

    window.addEventListener('resize', () => this.onResize())
    
    this.onResize()
  }

  onResize() {
    const {width} = this.gridEl.nativeElement.getBoundingClientRect()
    const firstColumnWidth = parseInt(this.orderNameColumnWidth) * 12 // patient-brief. convert rem to px.  font size 12
    console.log('***********', width, firstColumnWidth, width - firstColumnWidth - (3 * 42) - 2)
    this.timeLineWidth = width - firstColumnWidth - (3 * 42) - 2 // STAT, PRN, and a blank. scollbar 2px
  }
  
  onGridWidthResize() {
    return `calc(100% / ${this.nbHours})`
  }

  onGridHourWidthCalc() {
    return `calc(100% - ${this.startBeforeWidth}px)`
  }

  onCurrentTimeLineTranslate() {
    const widthFromStartDec = (parseFloat(this.widthFromStart(this.currentTime)) / 100)

    if (this.widthFromStart(this.currentTime) === '0') {
      this.showCurrentTimeLine = false
    } else {
      this.showCurrentTimeLine = true
    }

    return 'translate(' + ((parseFloat(this.widthFromStart(this.currentTime)) / 100) * this.timeLineWidth + this.startBeforeWidth) + ', 0)'
  }

  onGridHourTranslate() {
    return `translate(${this.startBeforeWidth}, 0)`
  }

  trackByFn(index, item) {
    if (!item) return null
    return item.id
  }

  // @HostListener('window:beforeunload')
  ngOnDestroy() {
    console.log('~~~~~~~~~~MAR Dept ngOnDestroy at ', new Date().toUTCString())

    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();

    console.log('~~~~~~~~~~MAR Dept ngOnDestroy FINSIHED unsubscribe at ', new Date().toUTCString())
  }

  // not in used anymore
  async getRequiredAPIData() {
    // console.log('MAR DEPT: in getRequiredAPIData')
    
    // Tab/filter - My Patients
    // this.myPatients = await this.patientService.getMyPatients(this.userSiteId, this.userId).toPromise()
    
    // if (this.myPatients) {
    //   for (const patient of this.myPatients.patients) {
    //     this.myPatientsPatientOrders[patient.id] = await this.getPatientOrders(patient.id)
    //     // this.countPatientOrders(patient.id)
    //   }
    // }
    
    // this.myPatients = await this.patientService.getMyPatients(this.userSiteId, this.userId).toPromise()
    // await this.patientService.getMyPatients(this.userSiteId, 10110).toPromise()
    // .then(res => this.myPatients = res);
    

    if (this.myPatients) {
      this.filter = 'myPatients'
      console.log('you have "My Patients"')
    }


    
    // Tab/filter - All
    // await this.patientService.getDepartmentPatients(this.userSiteId, this.userId, this.departmentCode, wardCodes, '').toPromise()
    //   .then(res => this.patientsList = res);
    
    this.all = await this.patientService.getPatients(this.userSiteId, this.userId, this.departmentCode, this.wardCode).toPromise()
    // console.log('MAR DEPT: all: ', this.all)

    if (this.all) {
      for (const patient of this.all.patients) {
        this.allPatientOrders[patient.id] = await this.getPatientOrders(patient.id)
        console.log('MAR DEPT: allPatientOrders: ', this.allPatientOrders)

        if (this.allPatientOrders[patient.id]) {
          // this.countPatientOrders(patient.id)
        }
      }
    }

    // Tab/filter - Upcoming Orders - build upcomingOrders from allPatientOrders
    this.upcomingOrders.patients = this.all.patients.filter((patient) => !!this.allPatientOrders[patient.id])
    this.upcomingOrdersPatientOrders = this.upcomingOrders.patients.reduce((prev, patient) => {
      prev[patient.id] = this.allPatientOrders[patient.id]

      return prev
    }, {})

    // console.log('MAR DEPT: upcomingOrders: ', this.upcomingOrders)
    // console.log('MAR DEPT: upcomingOrdersPatientOrders: ', this.upcomingOrdersPatientOrders)
    
    
    // console.log('initialMyPatientsListFilter', this.patientListFilter);
    

    this.prepare()
    // this.patientsList = {...this[this.filter]}
    // this.patientOrders = { ...this[`${this.filter + 'PatientOrders'}`]}
    // this.patientOrderCounts = { ...this[`${this.filter + 'PatientOrderCounts'}`]}
    
    // this.countPatients()

    // for (const patient of this.patientsList.patients) {
    //   if (this.patientOrders[patient.id]) {
    //     this.countPatientOrders(patient.id)
    //   }
    // }

    // for (const patientId in this.patientOrders) {
    //   this.consolidateOrderAdministrationsForPatient(patientId, this.patientOrders[patientId])
    // }
    
    // console.log('MAR DEPT: patientsList: ', this.patientsList);
    // console.log('MAR DEPT: patientOrders: ', this.patientOrders);
    // console.log('MAR DEPT: patientOrderCounts: ', this.patientOrderCounts);
    // console.log('MAR DEPT: patientConsolidatedOrderAdministrations: ', this.patientConsolidatedOrderAdministrations)
    // console.log('MAR DEPT: patientConsolidatedOrderAdministrationOrderInfo: ', this.patientConsolidatedOrderAdministrationOrderInfo)
    




    // this.patientsList = this.patientListFilter[this.filter]

    // if (this.patientsList && this.patientsList.links && this.patientsList.links.length > 1) {
    //   let counter: number = 0;
    //   for (const link of this.patientsList.links) {
    //     if (counter > 0 && link.href && link.href.includes('http')) {
    //       this.patientService.getDepartmentPatients(this.userSiteId, this.userId, this.departmentCode, wardCodes, link.href).toPromise()
    //         .then(res => {
    //           if (res.patients && res.patients.length > 0) {
    //             // combine existing patient list with the new entries and sort them.
    //             const combinedPatientsList = [...this.patientsList.patients, ...res.patients];
    //             // const updatedPatientsList = [];
    //             // console.log('combinedPatientsList', combinedPatientsList);
    //             // combinedPatientsList.forEach(cmbPatient => {
    //             //   const index: number = updatedPatientsList.findIndex(updPatient => updPatient.wardCode > cmbPatient.wardCode && updPatient.roomBedCode > cmbPatient.roomBedCode);
    //             //   if (index === -1) {
    //             //     updatedPatientsList.push(cmbPatient);
    //             //   } else {
    //             //     updatedPatientsList.splice(index, 0, cmbPatient);
    //             //   }

    //             // });
    //             // this.patientsList.patients = updatedPatientsList;
    //             this.patientsList.patients = combinedPatientsList.sort(this.compare);
    //             // console.log('this.patientsList', this.patientsList);

    //           }
    //         });
    //     }
    //     counter++;
    //   }
    // }
  }

  prepareList() {
    console.log('MAR DEPT (prepareList): filter: ', this.filter) 

    if (this.myPatients?.patients.length) {
      this.filter = 'myPatients'
      console.log('MAR DEPT (prepareList): you have "My Patients"')
    }

    console.log('MAR DEPT (prepareList): filter: ', this.filter)
    this.patientsList = {...this[this.filter]}

    console.log('MAR DEPT (prepareList): patientsList: ', this.patientsList); 
  }

  prepare() {

    console.log('MAR DEPT (prepare): filter: ', this.filter)

    // if (this.myPatients?.patients.length) {
    //   this.filter = 'myPatients'
    //   console.log('MAR DEPT (prepare): you have "My Patients"')
    // }

    this.patientConsolidatedOrderAdministrations = {}
    this.patientConsolidatedOrderAdministrationOrderInfo = {}
    this.consolidatedOrderAdministrationGroup = {}

    console.log('MAR DEPT (prepare): filter: ', this.filter)
    this.patientsList = {...this[this.filter]}
    this.patientOrders = { ...this[`${this.filter + 'PatientOrders'}`]}
    // this.patientOrderCounts = { ...this[`${this.filter + 'PatientOrderCounts'}`]}
    this.patientOrderCounts = { ...this['PatientOrderCounts']}
    
    // this.countPatients()

    if (this.patientsList.patients) {
      for (const patient of this.patientsList.patients) {
        if (this.patientOrders[patient.id]) {
          this.countPatientOrders(patient.id)
        }
      }
    }

    for (const patientId in this.patientOrders) {
      this.consolidateOrderAdministrationsForPatient(patientId, this.patientOrders[patientId])
    }
    
    console.log('MAR DEPT (prepare): patientsList: ', this.patientsList);
    console.log('MAR DEPT (prepare): patientOrders: ', this.patientOrders);
    console.log('MAR DEPT (prepare): patientOrderCounts: ', this.patientOrderCounts);
    console.log('MAR DEPT (prepare): patientConsolidatedOrderAdministrations: ', this.patientConsolidatedOrderAdministrations)
    console.log('MAR DEPT (prepare): patientConsolidatedOrderAdministrationOrderInfo: ', this.patientConsolidatedOrderAdministrationOrderInfo)
    console.log('MAR DEPT (prepare): consolidatedOrderAdministrationGroup: ', this.consolidatedOrderAdministrationGroup)
  }

  async getPatientOrders(patientId: number) : Promise<Order[]> {
    
    let orders = await this.patientMedOrderService.getPatientCurrentOrders(patientId).toPromise()
    
    orders = orders?.map((order) => ({
      ...order,
      displayName: order.medication?.displayName,
      displayRoute: order.medicationRoute ? order.medicationRoute.routeName : '',
      displayFrequency: order.frequencySchedule ? order.frequencySchedule.scheduleName : '',
      displayDose: order.dose,
      displayDoseUnit: order.doseUnit ? order.doseUnit.printName : '',
      isComboMed: order.medication?.medicationDetails.length > 1 ? true : false,  // TODO: check the drugId &&
      comboMedDetails: order.medication?.medicationDetails.length > 1 ? order.medication.medicationDetails.map((m) => ({
        brandName: m.brandName,
        dose: m.dose,
        doseUnit: m.doseUnit ? m.doseUnit.printName : ''
      })) : [],
      // allergyReactionsText: order.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
      // drugInteractionsText: order.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
    }));

    return orders
  }

  consolidateOrderAdministrationsForPatient(patientId, patientOrders = []) {
    let pcoa = this.patientConsolidatedOrderAdministrations[patientId] || {}
    let pcoa_orderInfo = this.patientConsolidatedOrderAdministrationOrderInfo[patientId] || {}

    for (const order of patientOrders) {
      for (const admin of order.orderAdministrations) {

        const adminTimeHour = moment(admin.administrationScheduledDatetime).hour()
        const minuteGroup = Math.floor(moment(admin.administrationScheduledDatetime).minutes() / this.minutesPerGroup) * this.minutesPerGroup
        const groupTime = moment(admin.administrationScheduledDatetime).set('hour', adminTimeHour).set('minute', minuteGroup).set('second', 0).format()

        pcoa[groupTime] = pcoa[groupTime] || {
          adminCount: 0, 
          mostSignificantStatusAdmin: {}, 
          admins: [],
        }
        
        if (pcoa[groupTime].adminCount === 0) {
          
          pcoa[groupTime].mostSignificantStatusAdmin = admin

        } else {
          // compare the administration status for the "least/most significant"
          // Late / Pending / OnHold / Ongoing (given but not completed) / Missed (not going to give) / Given
          const currentMostSignificantStatusAdmin = pcoa[groupTime].mostSignificantStatusAdmin

          if (currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'late' && admin.administrationStatus.toLowerCase() === 'late') {
            pcoa[groupTime].mostSignificantStatusAdmin = admin

          } else if (admin.administrationStatus.toLowerCase() === 'pending') {
            if (currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'late') {
              pcoa[groupTime].mostSignificantStatusAdmin = admin
            }

          } else if (admin.administrationStatus.toLowerCase() === 'onhold') {
            if (currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'late' 
                 && currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'pending') {
              pcoa[groupTime].mostSignificantStatusAdmin = admin
            }

          } else if (admin.administrationStatus.toLowerCase() === 'ongoing') {
            if (currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'late' 
                && currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'pending' 
                && currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'onhold') {
              pcoa[groupTime].mostSignificantStatusAdmin = admin
            }

          } else if (admin.administrationStatus.toLowerCase() === 'missed' || admin.administrationStatus.toLowerCase() === 'given') {
            if (currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'late' 
                && currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'pending' 
                && currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'onhold'
                && currentMostSignificantStatusAdmin.administrationStatus.toLowerCase() !== 'ongoing') {
              pcoa[groupTime].mostSignificantStatusAdmin = admin
            }
          }
        }
        pcoa[groupTime].adminCount = pcoa[groupTime].adminCount + 1
        pcoa[groupTime].admins.push(admin) //administrations that are combined in the group
        
        pcoa_orderInfo[groupTime] = pcoa_orderInfo[groupTime] || []
        pcoa_orderInfo[groupTime].push(order)
      }
    }

    this.patientConsolidatedOrderAdministrations[patientId] = pcoa
    this.patientConsolidatedOrderAdministrationOrderInfo[patientId] = pcoa_orderInfo

    // this.groupPatientConsolidatedOrderAdministrations(this.minutesPerGroup)
  }

  compareMostSignificantAdminStatus(admin1, admin2) {
    if (!admin1) return admin2
    // console.log({admin1, admin2})

    const admin1Status = admin1.administrationStatus.toLowerCase()
    const admin2Status = admin2.administrationStatus.toLowerCase()

    if (admin1Status !== 'late' && admin2Status === 'late') {
      return admin2

    } else if (admin2Status === 'pending') {
      if (admin1Status !== 'late') {
        return admin2
      }

    } else if (admin2Status === 'onhold') {
      if (admin1Status !== 'late' 
           && admin1Status !== 'pending') {
        return admin2
      }

    } else if (admin2Status === 'ongoing') {
      if (admin1Status !== 'late' 
          && admin1Status !== 'pending' 
          && admin1Status !== 'onhold') {
        return admin2
      }

    } else if (admin2Status === 'missed' || admin2Status === 'given') {
      if (admin1Status !== 'late' 
          && admin1Status !== 'pending' 
          && admin1Status !== 'onhold'
          && admin1Status !== 'ongoing') {
        return admin2
      }
    }

    return admin1
  }

  groupPatientConsolidatedOrderAdministrations(minPerGroup: number) {
    let newAdminGroup = {}

    for (const patientId in this.patientConsolidatedOrderAdministrations) {
      const pcoa_orderInfo = this.patientConsolidatedOrderAdministrationOrderInfo[patientId]

      for (const adminTime in this.patientConsolidatedOrderAdministrations[patientId]) {
        const admin = this.patientConsolidatedOrderAdministrations[patientId][adminTime]
        const adminTimeHour = moment(adminTime).hour()
        // const hourGroup = moment(adminTime).minutes() / minPerGroup
        const minuteGroup = Math.floor(moment(adminTime).minutes() / minPerGroup) * minPerGroup
        const groupTime = moment(adminTime).set('hour', adminTimeHour).set('minute', minuteGroup).set('second', 0).format()

        newAdminGroup[patientId] = newAdminGroup[patientId] || {}
        newAdminGroup[patientId][groupTime] = newAdminGroup[patientId][groupTime] || {
          adminCount: 0, 
          mostSignificantStatusAdmin: {},
          orderInfos: [], 
          admins: [],
        }

        if (newAdminGroup[patientId][groupTime].adminCount === 0) {
          newAdminGroup[patientId][groupTime].mostSignificantStatusAdmin = {}
        }

        const orderInfos = pcoa_orderInfo[adminTime]
        newAdminGroup[patientId][groupTime].adminCount = newAdminGroup[patientId][groupTime].adminCount + orderInfos.length
        newAdminGroup[patientId][groupTime].orderInfos = [...newAdminGroup[patientId][groupTime].orderInfos, ...orderInfos]
        newAdminGroup[patientId][groupTime].admins = [...newAdminGroup[patientId][groupTime].admins, admin]
        
        // newAdminGroup[patientId][groupTime] = [...newAdminGroup[patientId][groupTime], ...pcoa_orderInfo[adminTime]]
        
      }
    }
    
    // get most significant admin status from each group
    for (const patientId in newAdminGroup) {
      const groupTimes = newAdminGroup[patientId];

      for (const groupTime in groupTimes) {
        const bucket = groupTimes[groupTime];

        bucket.mostSignificantStatusAdmin = bucket.orderInfos.reduce((prevOuter, orderInfo) => {
          return orderInfo.orderAdministrations.reduce((prevInner, admin) => this.compareMostSignificantAdminStatus(prevInner, admin), prevOuter)
        }, null)
      }
    }


    console.log('---------------', minPerGroup)
    console.log('---------------', {newAdminGroup})
    this.consolidatedOrderAdministrationGroup = newAdminGroup
  }


  search = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      // tap(() => this.searching = true),
      // tap(() => console.log('SEARCH: this.filter: ', this.filter, this[this.filter].patients)),
      map(term => term === '' ? []
        : this[this.filter].patients.filter(p => p.fullName.toLowerCase().indexOf(term.toLowerCase()) > -1)
        

        // // this.medOrderService.searchHttp(term, this.selectedSource).pipe(
        // this.medOrderService.brandNameSearch(term, 'this.selectedSource').pipe(
        //   tap(() => this.searchFailed = false),
        //   catchError(() => {
        //     console.log('---------')
        //     this.searchFailed = true;
        //     return of([]);
        //   }))
      ),
      // tap(() => this.searching = false),
      // tap(() => console.log('this.patientsList.patients: ', this.patientsList.patients))
    )

    // formatter = (x: {name: string}) => x.name;
    // inputFormatter = (x: {name: string}) => x.name;

  inputFormat(value: any) {
    // return value.brandName ? value.brandName : value;
    console.log('inputFormat value: ', value)
    return value;
  }

  // resultFormat(value: any) {
  //   return value.fullName;
  //   // return value;
  // }

  onSelect($event, input) {
    $event.preventDefault();
    // console.log('onSelect: ', $event.item);
    console.log('next from NEW: ', $event.item);

    input.value =  $event.item.fullName;
    this.patientsList.patients = [$event.item]
    // console.log('patientsList: ', this.patientsList)
    input.blur();
  }

  onSubmit(model) {
    console.log('onSubmit: model: ', model)

    if (model === '') {
      console.log('onSubmit: list: ', this[this.filter])
      // console.log('onSubmit: this.upcomingOrders: ', this.upcomingOrders)
      this.patientsList = {...this[this.filter]}
    } else {
      this.patientsList.patients = this[this.filter].patients.filter(p => p.fullName.toLowerCase().indexOf(model.toLowerCase()) > -1)
      // this.patientsList.patients = this[this.filter].patients.filter(p => p.fullName.indexOf(model) > -1)
    }
    // console.log('onSubmit: this.patientsList: ', this.patientsList)
  }

  rt(value: any) {
    return value.fullName;
    // return value;
  }


  compare(a, b) {
    // console.log(`compare: ${b.wardCode} ${b.roomBedCode}`)
    if (`${a.wardCode} ${a.roomBedCode}` < `${b.wardCode} ${b.roomBedCode}`) {
      return -1;
    }
    if (`${a.wardCode} ${a.roomBedCode}` > `${b.wardCode} ${b.roomBedCode}`) {
      return 1;
    }
    return 0;
  }

  refreshOrder(order) {
    if (order !== null) {
      this.orders.map( (ord, i) => {
        if (ord.id == order.id) {
          this.orders[i] = order;
        }
      });
    }
  }

  /* catch now set of orders and refresh screen */
  refresh() {
    console.log('REFRESH');
    this.currentTime = moment().format();
    if (MOCKUP) {
      this.orders = ORDERS.slice(0);
      // console.log('ORDERS FROM MOCKUP');
      this.moveOrdersToday();
      this.moveTimes(); // compute time header
      this.refreshScreen();
    } else {
      // this.setIntervals();


      // const patientId: number = this.patientStoreService.patientId;
      if (this.reload !== null) {
        clearTimeout(this.reload);
      }
      // this.patientMedOrderService.getPatientCurrentOrders(patientId).subscribe( orders => {
        // console.log('ORDERS FROM API', orders);
        // this.orders = [];
        // if (typeof orders !== 'undefined' ) this.orders = orders.slice(0);
        // if (MOCKUP) this.moveOrdersToday();
        this.moveTimes(); // compute time header
        this.refreshScreen();
        // this.fetching = false;
        this.updating = false;
        /* very dirty - anti angular: it needs to be reviewed when no more support of IE11.
        IE11 does not support position: sticky for a table header, and can not change scrollbar width
        So have to change the table with sticky header to 2 tables and sync the non fixed column width together
        */
       let ready = setInterval( () => {
        const id1 = document.getElementById('orderNameCol0');
        const id2 = document.getElementById('administrationCol0');
        // should have a component to be able to catch when the component is displayed
        // no prove that when the id exists the width is computed
        if (id1 && id2) {
          this.orderNameColumnWidth = id1.offsetWidth.toString() + 'px';
          this.administrationsColumnWidth = id2.offsetWidth.toString() + 'px';
          clearInterval(ready);
        }
       }, 500)
      // });
    }
  }

  /* executed after each new set of Orders */
  refreshScreen() {
    console.log('REFRESHScreen')

    if (this.reload !== null) {
      clearTimeout(this.reload);
    }
    this.setIntervals();
    this.reload = setTimeout(() => {
      this.refresh();
      this.setIntervals();
    }, 1000 * RELOAD_SECONDS);
  }

  // return if order is stat or prn or scheduled or timed or continuous
  isOrder(order: Order, type: string): boolean {
    switch (type) {
      case 'prn':
        return typeof order.prn !== 'undefined' && order.prn;
      case 'stat':
          return typeof order.priority !== 'undefined' && (
            (typeof order.priority === 'string' && order.priority.toUpperCase() === 'STAT') || 
            (typeof order.priority === 'number' && order.priority === STAT_ID));
      // Start time is in the future. Ex: 1 times a day, 3 times a day
      case 'scheduled':
        return moment(order.beginDatetime).isAfter(moment());
      // Timed is a very specific time. Ex: 6:04pm
      case 'timed':
        return typeof order.pointInTime !== 'undefined' && order.pointInTime;
      // Anything that is running. NON point in time. Could include breathing treatments
      case 'continuous':
        let now = moment();
        return typeof order.pointInTime !== 'undefined' && !order.pointInTime &&
          (moment(order.beginDatetime).isBefore(now) || moment(order.beginDatetime).isSame(now)) &&
          (moment(order.endDatetime).isAfter(now) || moment(order.endDatetime).isSame(now));
      case 'iv':
      case 'ancilliary':
        return false; //TODO
      }
    return false;
  }

  // filter the orders
  filterOrders(): void {
    if (this.filter != 'all') {
      this.orders = this.orders.filter( order => this.isOrder(order, this.filter) );
    }
    // console.log('ORDERS BEFORE SORT', this.orders);
    this.orders.sort(this.compareOrders.bind(this));
    console.log('ORDERS AFTER SORT', this.orders);
  }

  // Compare 2 orders
  compareOrders(o1: Order, o2:Order): number {
    const m1: string = (o1.beginDatetime !== null)? o1.beginDatetime: o1.addDatetime;
    const m2: string = (o2.beginDatetime !== null)? o2.beginDatetime: o2.addDatetime;
    var ret;
    // completed at the bottom
    if (o1.orderStatus.toLowerCase() == 'completed') {
      if (o2.orderStatus.toLowerCase() == 'completed') {
        // Completed are sort together on date or if same date on name
        if (m1 == m2) return o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
        return moment(m2).isBefore(m1)? 1: -1;
      }
      return 1; // o2<o1 - Completed are below any other status
    } else if (o2.orderStatus.toLowerCase() == 'completed') {
      return -1; // o1<o2 - Completed are below any other status
    }

    if (this.sort.toLowerCase() == 'administration time') {
      let due1 = null;
      let due2 = null;
      if (typeof o1.nextActionTime === 'undefined' || o1.nextActionTime == null) {
        due1 = this.getOrderOldestOverdue(o1, null);
      } else {
        if (!moment().isBefore(o1.nextActionTime)) {
          due1 = moment(o1.nextActionTime);
        }
      }
      if (typeof o2.nextActionTime === 'undefined' || o2.nextActionTime == null) {
        due2 = this.getOrderOldestOverdue(o2, null);
      } else {
        if (!moment().isBefore(o2.nextActionTime)) {
          due2 = moment(o2.nextActionTime);
        }
      }
      // due at the top
      if (due1 !== null) {
        // console.log('COMPARE o1',o1.id,'-',due1.format(), o1.nextActionTime);
        if (due2 !== null) {
          // console.log('COMPARE o2',o2.id,'-',due2.format(), o2.nextActionTime);
          if (due1.isSame(due2)) {
            ret = o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
            // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' due name');
            return ret;
          }
          ret = moment(due2).isBefore(due1)? 1: -1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' due','/',o1.medication.displayName,'/',o2.medication.displayName);
          return ret;
        }
        ret = -1;
        // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first due','/',o1.medication.displayName,'/',o2.medication.displayName);
        return ret; // o1<o2 o1 is due not o2
      } else if (due2 !== null) {
        // console.log('COMPARE o2',o2.id,'-',due2.format(), o2.nextActionTime);
        ret = 1;
        // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second due','/',o1.medication.displayName,'/',o2.medication.displayName);
        return 1; // o2<o1 o2 is due not o1
      }
      // then the stat
      const stat1 = this.isOrder(o1, 'stat');
      const stat2 = this.isOrder(o2, 'stat');
      if (stat1) {
        if (stat2) {
          ret = o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' name stat');
          return ret;
        } else {
          ret = -1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first stat');
          return -1; // o1<o2 o1 is stat not o2
        }
      } else if (stat2) {
        ret = 1;
        // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second stat');
        return ret; // o2<o1 o2 stat , not o1
      }
      // then the non point in time(IV) running
      const pti1 = o1.pointInTime;
      const pti2 = o2.pointInTime;
      if (!pti1) {
        if (!pti2) {
          ret = o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' name pit');
          return ret;
        } else {
          ret = -1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first pit');
          return ret; // o1<o2 o1 is IV not o2
        }
      } else if (!pti2) {
        ret = 1;
        // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second pit');
        return ret; // o2<o1 o2 is IV , not o1
      }

      // then prn
      const prn1 = this.isOrder(o1, 'prn');
      const prn2 = this.isOrder(o2, 'prn');
      if (prn1) {
        if (prn2) {
          ret = o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' name prn');
          return ret;
        } else {
          ret = -1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first prn');
          return ret; // o1<o2 o1 is prn not o2
        }
      } else if (prn2) {
        ret = 1;
        // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second prn');
        return ret; // o2<o1 o2 is prn , not o1
      }

      // then the rest .... should have been in the previous group
      // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' DEFAUKT');
      return o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;

    } else { // sort on enter time, then name, then dose
      if (m1 == m2) {
        if (o1.medication.displayName == o2.medication.displayName) {
          ret = o1.dose < o2.dose? -1: 1;
          // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' dose');
          return ret;
        }
        // localeCompare does not give same result IE11 and Chrome
        ret = o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
        // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' name.');
        return ret;
      }
      ret = moment(m1).isBefore(m2)? -1: 1
      // console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' date');
      return ret;
    }
  }

  // count the orders for each type
  // countOrders(): void {
  //   this.nbOrders.all = this.orders.length;
  //   this.nbOrders.stat = this.orders.reduce( (total, order) => {
  //     return this.isOrder(order, 'stat')? total + 1: total;
  //   }, 0);
  //   this.nbOrders.prn = this.orders.reduce( (total, order) => {
  //     return this.isOrder(order, 'prn')? total + 1: total;
  //   }, 0);
  //   this.nbOrders.scheduled = this.orders.reduce( (total, order) => {
  //     return this.isOrder(order, 'scheduled')? total + 1: total;
  //   }, 0);
  //   this.nbOrders.timed = this.orders.reduce( (total, order) => {
  //     return this.isOrder(order, 'timed')? total + 1: total;
  //   }, 0);
  //   this.nbOrders.continuous = this.orders.reduce( (total, order) => {
  //     return this.isOrder(order, 'continuous')? total + 1: total;
  //   }, 0);
  //   this.nbOrders.iv = this.orders.reduce( (total, order) => {
  //     return this.isOrder(order, 'iv')? total + 1: total;
  //   }, 0);
  //   this.nbOrders.ancilliary = this.orders.reduce( (total, order) => {
  //     return this.isOrder(order, 'ancilliary')? total + 1: total;
  //   }, 0);
  // }
  
  countPatientOrders(patientId: number): void {
    // console.log('MAR DEPT: countPatientOrders: patientId: ', patientId)

    let nbOrders: TypeCount = {
      all:0, 
      prn:0, 
      stat:0, 
      scheduled:0, 
      continuous:0, 
      timed:0, 
      iv:0, 
      ancilliary:0,
      rxVerificationComplete:0,
      rxVerificationNeeded:0,
    };
    // const patientOrders = `this.${this.filter}PatientOrders`
    // console.log('MAR DEPT: countPatientOrders: filter: ', this.filter)
    // console.log('MAR DEPT: countPatientOrders: patientOrders: ', this.patientOrders)

    if (this.patientOrders[patientId]) {
      nbOrders.all = this.patientOrders[patientId].length
      nbOrders.stat = this.patientOrders[patientId].reduce( (total, order) => {
        return this.isOrder(order, 'stat')? total + 1: total;
      }, 0);
      nbOrders.prn = this.patientOrders[patientId].reduce( (total, order) => {
        return this.isOrder(order, 'prn')? total + 1: total;
      }, 0);
      nbOrders.scheduled = this.patientOrders[patientId].reduce( (total, order) => {
        return this.isOrder(order, 'scheduled')? total + 1: total;
      }, 0);
      nbOrders.timed = this.patientOrders[patientId].reduce( (total, order) => {
        return this.isOrder(order, 'timed')? total + 1: total;
      }, 0);
      nbOrders.continuous = this.patientOrders[patientId].reduce( (total, order) => {
        return this.isOrder(order, 'continuous')? total + 1: total;
      }, 0);
      nbOrders.iv = this.patientOrders[patientId].reduce( (total, order) => {
        return this.isOrder(order, 'iv')? total + 1: total;
      }, 0);
      nbOrders.ancilliary = this.patientOrders[patientId].reduce( (total, order) => {
        return this.isOrder(order, 'ancilliary')? total + 1: total;
      }, 0);
      nbOrders.rxVerificationNeeded = this.patientOrders[patientId].reduce( (total, order) => {
        return order.pharmacyVerificationStatus === 1 ? total + 1: total;
      }, 0);
      nbOrders.rxVerificationComplete = this.patientOrders[patientId].reduce( (total, order) => {
        return order.pharmacyVerificationStatus === 2 ? total + 1: total;
      }, 0);
    }

    this.patientOrderCounts[patientId] = nbOrders
  }

  // count the patient for each tab
  countPatients(): void {
    // console.log('countPatient: myPatients: ', this.myPatients)
    this.nbPatients.all = this.all?.patients?.length || 0;
    this.nbPatients.upcomingOrders = this.upcomingOrders?.patients?.length || 0;
    this.nbPatients.myPatients = this.myPatients?.patients?.length || 0;
    this.nbPatients.rxVerificationNeeded = this.rxVerificationNeeded?.patients?.length || 0;

    // console.log('countPatient: nbPatients: ', this.nbPatients)

  }

  // change the initial hour
  changeNbHours(delta: number): void {
    // console.log('CHANGEHOURS');
    if (this.nbHours + delta < 1 || this.nbHours + delta > 24)
      return;
    this.nbHours = this.nbHours + delta;
    this.moveTimes(0);
    this.setIntervals();
    this.refreshScreen();
  }

  // default list of times: the hour of now + Nb_HOURS hours
  // Call each time move time period on nb of hours
  moveTimes(delta?: number, problem?: boolean): void {
    if (window.innerWidth < 1000) {
      this.nbHours = 5;
    }
    this.oldestDue = this.getOldestOverdue();
    let startMoment: moment.Moment;
    if (typeof delta === 'undefined' || problem === true) {
      if (this.timeHasBeenMoved) {
        startMoment = moment(this.times[0]);
      } else {
        // TODO perhaps it is nextActionTime
        const time = problem? this.oldestDue: this.currentTime;
        if (problem) this.timeHasBeenMoved = true;
        const start = moment().subtract(PRE_HOURS_DISPLAY, 'hour');
        startMoment = (time === '')? start: moment(time).diff(start, 'hours') < 0 ? moment(time): start;
      }
    } else if (delta == 0) {
      startMoment = moment();
      this.timeHasBeenMoved = false; // start time can moved again
    } else {
      if (delta < 0) {
        startMoment = moment(this.times[0]).clone().subtract(-delta, 'hour');
      } else {
        startMoment = moment(this.times[0]).clone().add(delta, 'hour');
      }
      this.timeHasBeenMoved = true; // keep start time at refresh
    }
    this.times = [];
    this.times[0] = startMoment.startOf('hour').format();
    for (let ii = 1; ii < this.nbHours; ++ii) {
      this.times[ii] = startMoment.add(1, 'hour').format(); // startMoment is mutable
    }
    if (typeof delta !== 'undefined') this.setIntervals();

    if (this.widthFromStart(this.currentTime) === '0') {
      this.showCurrentTimeLine = false
    } else {
      this.showCurrentTimeLine = true
    }
  }

  // Return the oldest overdue date of the orders
  getOldestOverdue(): string {
    let time: moment.Moment = null;
    for (const order of this.orders) {
      time = this.getOrderOldestOverdue(order, time);
    }
    return (time == null)? "": time.format();
  }
  getOrderOldestOverdue(order: Order, time: moment.Moment) {
    if (Array.isArray(order.orderAdministrations)) {
      for (const admin of order.orderAdministrations) {
        if (admin.administrationStatus == 'Late') {
          let missedTime: moment.Moment = moment(admin.administrationScheduledDatetime);
          if (time == null || missedTime.isBefore(time)) {
            time = missedTime;
          }
        }
      }
    }
    return time;
  }
  hasOverDue() {
    return moment(this.oldestDue).isBefore(this.currentTime);
  }

  // Mockup: move orders to today, compute Late, move visitstart
  moveOrdersToday() {
    console.log('DEBUG MOVE AROUND');
    const currentMoment = moment();
    for (let ii = 0; ii < this.orders.length; ++ii) {
      const delta = moment(moment().format('YYYY-MM-DD')).diff(moment(this.orders[ii].beginDatetime).format('YYYY-MM-DD'), 'days');
      if (delta != 0) {
        if (this.patientMedOrderStoreService.validDateTime(this.orders[ii].signedOn)) {
          this.orders[ii].signedOn = moment(this.orders[ii].signedOn).add(delta, 'days').format(); // mutable
        }
        if (this.patientMedOrderStoreService.validDateTime(this.orders[ii].beginDatetime)) {
          this.orders[ii].beginDatetime = moment(this.orders[ii].beginDatetime).add(delta, 'days').format();
        }
        if (this.patientMedOrderStoreService.validDateTime(this.orders[ii].endDatetime)) {
          this.orders[ii].endDatetime = moment(this.orders[ii].endDatetime).add(delta, 'days').format();
        }
        if (this.patientMedOrderStoreService.validDateTime(this.orders[ii].addDatetime)) {
          this.orders[ii].addDatetime = moment(this.orders[ii].addDatetime).add(delta, 'days').format();
        }
        if (this.patientMedOrderStoreService.validDateTime(this.orders[ii].nextActionTime)) {
          this.orders[ii].nextActionTime = moment(this.orders[ii].nextActionTime).add(delta, 'days').format();
        }
        if (typeof this.orders[ii].orderEvents !== 'undefined' && this.orders[ii].orderEvents !== null) {
          for (let jj = 0; jj < this.orders[ii].orderEvents.length; ++jj) {
            if (this.patientMedOrderStoreService.validDateTime(this.orders[ii].orderEvents[jj].eventDatetime)) {
              this.orders[ii].orderEvents[jj].eventDatetime = moment(this.orders[ii].orderEvents[jj].eventDatetime).add(delta, 'days').format();
            }
          }
        }
        if (typeof this.orders[ii].orderAdministrations !== 'undefined' && this.orders[ii].orderAdministrations !== null) {
          for (let jj = 0; jj < this.orders[ii].orderAdministrations.length; ++jj) {
            let admin = this.orders[ii].orderAdministrations[jj];
            if (this.patientMedOrderStoreService.validDateTime(admin.administrationDatetime)) {
              this.orders[ii].orderAdministrations[jj].administrationDatetime = moment(admin.administrationDatetime).add(delta, 'days').format();
            }
            if (this.patientMedOrderStoreService.validDateTime(admin.administrationScheduledDatetime)) {
              this.orders[ii].orderAdministrations[jj].administrationScheduledDatetime = moment(admin.administrationScheduledDatetime).add(delta, 'days').format();
            }
            if (this.patientMedOrderStoreService.validDateTime(admin.administrationInputDatetime)) {
              this.orders[ii].orderAdministrations[jj].administrationInputDatetime = moment(admin.administrationInputDatetime).add(delta, 'days').format();
            }
            if (this.patientMedOrderStoreService.validDateTime(admin.stopScheduledDatetime)) {
              this.orders[ii].orderAdministrations[jj].stopScheduledDatetime = moment(admin.stopScheduledDatetime).add(delta, 'days').format();
            }
            if (this.patientMedOrderStoreService.validDateTime(admin.acknowledgeDatetime)) {
              this.orders[ii].orderAdministrations[jj].acknowledgeDatetime = moment(admin.acknowledgeDatetime).add(delta, 'days').format();
            }
            if (typeof admin.administrationEvents !== 'undefined' && admin.administrationEvents !== null) {
              for (let kk = 0; kk < admin.administrationEvents.length; ++kk) {
                if (this.patientMedOrderStoreService.validDateTime(this.orders[ii].orderAdministrations[jj].administrationEvents[kk].eventDatetime)) {
                  this.orders[ii].orderAdministrations[jj].administrationEvents[kk].eventDatetime = moment(admin.administrationEvents[kk].eventDatetime).add(delta, 'days').format();
                }
              }
            }
          }
      }
      if (typeof this.orders[ii].orderAdministrations !== 'undefined' && this.orders[ii].orderAdministrations !== null) {
        for (let jj = 0; jj < this.orders[ii].orderAdministrations.length; ++jj) {
          let admin = this.orders[ii].orderAdministrations[jj];
          if (admin.administrationStatus == 'Late') {
            this.orders[ii].orderAdministrations[jj].administrationStatus = 'Pending';
          }
          if (admin.administrationStatus == "Pending" && moment(admin.administrationScheduledDatetime).isBefore(currentMoment)) {
              this.orders[ii].orderAdministrations[jj].administrationStatus = 'Late';
            }
          }
        }
      }
    }
    // console.log('ORDERS AFTER TRICK', this.orders)
  }

  moveToday(dateTime: string) {
    const today = moment();
    let mo = moment(dateTime);
    return mo.set({year: today.year(), month: today.month(), date: today.date(), hour: mo.hour(), minute: mo.minute()}).format();
  }

  isToday(dateTime: string) {
    return moment().format('YYY-MM-DD') == moment(dateTime).format('YYY-MM-DD');
  }

  setIntervals() {
    let mo;
    this.intervals = [];

    for (let zz = 0; zz < this.patientsList.patients?.length; ++zz) {
      // console.log('--zz: ', zz, '  patientId: ', this.patientsList.patients[zz].id)
      // for (let ii = 0; ii < this.patientOrders[this.patientsList.patients[zz].id]?.length; ++ii) {
      if (this.patientOrders[this.patientsList.patients[zz].id]?.length) {
        // console.log('--ii: ', ii, ' orderId: ', this.patientOrders[this.patientsList.patients[zz].id][ii].id)

        // use the first order of each patient to setup the intervals for the performance purpose
        // let order = this.patientOrders[this.patientsList.patients[zz].id][ii];
        let order = this.patientOrders[this.patientsList.patients[zz].id][0];
        let start = moment(order.beginDatetime);
        let end = null;
        if (order.endDatetime != null) {
          end = moment(order.endDatetime);
        }
        mo = moment(this.times[0]);
        let values: Interval[] = [];
        for (let jj = 0; jj < this.nbHours * 60 / INTERVAL_MINUTES; ++jj) {
          let val: Interval = {event: '', isHour: false, isNow: false, time: ''};
          if (mo.isSame(start) || (end != null && mo.isSame(end))) {
            val.event = '-';
          } else if (mo.isBefore(start)) {
            val.event = ' ';
          } else if (end == null) {
            val.event = '-';
          } else if (mo.isBefore(end)) {
            val.event = '-';
          } else {
            val.event = ' ';
          }
          val.time = mo.format();
          val.isHour = (mo.format("mm") === "00");
          let copy = mo.clone().subtract(INTERVAL_MINUTES*30, 's');
          let copy2 = mo.clone().add(INTERVAL_MINUTES*30, 's');
          val.isNow = moment(this.currentTime).isBetween(copy, copy2);
          mo.add(INTERVAL_MINUTES,'m'); // mo is mutable
          values.push(val);
        }
        this.intervals.push({orderId: order.id, intervals: values});
      // } // for ii

      // console.log('INTERVALS', this.nbHours, this.intervals);
      }

      // setup a filler intervals for patients who has no orders (filter on 'All')
      this.fillerIntervals = []
      // console.log('MAR DEPR: INTERVALS: no orders')
      let start = null;
      let end = null;
      
      mo = moment(this.times[0]);

      for (let jj = 0; jj < this.nbHours * 60 / INTERVAL_MINUTES; ++jj) {
        let val: Interval = {event: '', isHour: false, isNow: false, time: ''};
        if (mo.isSame(start) || (end != null && mo.isSame(end))) {
          val.event = '-';
        } else if (mo.isBefore(start)) {
          val.event = ' ';
        } else if (end == null) {
          val.event = '-';
        } else if (mo.isBefore(end)) {
          val.event = '-';
        } else {
          val.event = ' ';
        }
        val.time = mo.format();
        val.isHour = (mo.format("mm") === "00");
        let copy = mo.clone().subtract(INTERVAL_MINUTES*30, 's');
        let copy2 = mo.clone().add(INTERVAL_MINUTES*30, 's');
        val.isNow = moment(this.currentTime).isBetween(copy, copy2);
        mo.add(INTERVAL_MINUTES,'m'); // mo is mutable
        this.fillerIntervals.push(val);
      }
    }
  }
    

  getIntervals(orderId: number) {
    let interval = this.intervals.find( (interval) => interval.orderId === orderId);

    // console.log('MAR DEPT: getIntervals: orderId: ', orderId)
    // console.log('MAR DEPT: getIntervals: interval: ', interval)
    if (typeof interval === 'undefined' || interval === null) return null;

    return interval.intervals;
  }

  // type = what is returned = icon, textClass
  getOrderStatus(order: Order, type: string): string {
    return this.patientMedOrderStoreService.getOrderStatus(order, type);
  }

  // type = what is returnend = icon, tooltipText, tooltipClass, textClass, text
  getAdminStatus(admin: OrderAdministration, type:string): string {
    return this.patientMedOrderStoreService.getOrderAdministrationStatus(admin, type);
  }

  selectedPatient() {
    return this.patient;
  }

  // size of a column representing an hour
  hourWidth() {
    return (100 / this.times.length).toString() + '%';
  }

  // size between starting time table and a time
  widthFromStart(time: string): string {
    if (typeof time === 'undefined' || time === '') return '0';
    const start: moment.Moment = moment(this.times[0]);
    const there: moment.Moment = moment(time);
    const minutes = moment.duration(there.diff(start)).as('minutes');
    if (minutes < 0 || minutes > this.nbHours * 60) return '0';
    return ((minutes * 100) / (this.times.length * 60)).toString() + '%';
  }

  widthFromEnd(time: string): string {
    if (time == null) return '0';
    const fromStart = this.widthFromStart(time).replace('%', '');
    if (fromStart == '0') return fromStart;
    const val = 100 - (+fromStart);
    return val.toString() + '%';
  }

  // size of a unit interval
  intervalWidth(): string {
    return ((INTERVAL_MINUTES * 100) / (this.times.length * 60)).toString() + '%';
  }
  intervalLeft(iInterval: number, interval: Interval): string {
    if (interval.isHour) { // on an hour
    }
    const delta = ((iInterval-0.5) * INTERVAL_MINUTES * 100) / (this.times.length * 60);
    return delta.toString() + '%';
  }
  intervalBackground(interval: Interval): string {
    let bg: string = '';
    let sep:string = '';
    let color:string = (interval.time == this.currentIntervalTime)? "#deeff5": "";

    if (interval.isHour) { // #C7C7C7
      bg = color + ' url("/assets/img/grayLine.png") 50% repeat-y';
      sep = ',';
    }
    if (interval.isNow) {
      // TODO 50% -> exact % of the now
      bg = bg + sep + color + ' url("/assets/img/blueLine.png") 50% repeat-y';
      sep = ',';
    }
    if (!interval.isHour && !interval.isNow && color !== '') {
      bg = bg + sep + color;
    }
    return bg;
  }

  // backround for the order is current (start and end time)
  currentBackground(order: Order): string {
    if (typeof order.pointInTime !== 'undefined' && order.pointInTime === false) {
      return 'url("/assets/icon/horizontal-line.svg") 50% repeat-x';
    }
    return ''; //'url("/assets/icon/three-dots.svg") 50% repeat-x';
  }

  // test order start before first hour
  startBefore(order: Order): boolean {
    return moment(order.beginDatetime).isBefore(moment(this.times[0])) && this.activeOrder(order);
  }

  // initial padding to have the hour line under the : of the hour
  initialPadding(): string {
    // let delta = +this.intervalWidth().replace('%','') / 2;
    // return 'calc(25px - '+ delta.toString() + '%)';
    // console.log('GRID',this.grid)
    // TODO
    return '23px';
  }

  // position of the line : middle for the hour, % for the now 
  percentInterval(interval:Interval): string {
    if (interval.isNow) {
      let arr = this.currentTime.split(':');
      let m = ((+arr[1] % INTERVAL_MINUTES)*100)/INTERVAL_MINUTES;
      return m.toString()+'%';
    }
    return '50%'; // middle
  }

  // change the filter
  onFilter(type: string): void {
    this.filter = type;
    this.updating = true;
    console.log('onFilter: this.filter:', this.filter)
    // console.log('this.myPatients:', this.myPatients)
    // console.log('this.upcomingOrders:', this.upcomingOrders)

    this.prepare()  // does re-count/re-consolidate the administrations/orders 

    // this.patientsList = {...this[this.filter]}
    // console.log('----------onFilter: patietsList: ', {...this[this.filter]})
    // this.patientOrders = { ...this[`${this.filter + 'PatientOrders'}`]}
    // console.log('----------onFilter: patietsOrders: ', { ...this[`${this.filter + 'PatientOrders'}`]})

    
    this.refresh(); // TODO not overwrite orders - ust set inactive
  }

  // current interval on mouse change
  onMouseCell(over: boolean, interval: Interval): void {
    if (over)
      this.currentIntervalTime = interval.time;
    else
      this.currentIntervalTime = null;
  }

  // does the order exist (start and end time) in the displayed hours
  activeOrder(order: Order): boolean {
    const mStart = moment(this.times[0]);
    const mEnd = moment(this.times[this.times.length - 1]).add(1, 'hour');
    if (moment(order.beginDatetime).isBetween(mStart, mEnd)
      || (order.endDatetime !== null && moment(order.endDatetime).isBetween(mStart, mEnd))
      || (moment(order.beginDatetime).isBefore(mStart) && (order.endDatetime === null || moment(order.endDatetime).isAfter(mEnd)))) {
      // console.log("ACTIVEORDER", order.medication.displayName,order.beginDatetime, order.endDatetime);
      return true;
    }
    return false;
  }

  activeAdmin(admin: OrderAdministration, order?: Order): boolean {
    const time = this.adminTime(admin);
    return this.activeTime(time, order);
  }

  // test if time is in the displayed hours
  activeTime(time: string, order?: Order) : boolean {
    const mStart = moment(this.times[0]);
    let mEnd = mStart.clone();
    mEnd.add(this.nbHours, 'hours');
    // console.log("ACTIVETIME", (order != null)? order.id: '', time, this.times[0], mStart.format(), mEnd.format(), moment(time).isBetween(mStart, mEnd, 'minute', '[]'));
    return moment(time).isBetween(mStart, mEnd, 'minute', '[]');
  }

  // return time of the administration
  adminTime(admin: OrderAdministration): string {
    // console.log("ADMINTIME",admin);
    // if (this.patientMedOrderStoreService.validDateTime(admin.administrationDatetime)) {
    //  return admin.administrationDatetime;
    // }

    // well, for the MAR DEPT, only use administration Scheduled datetime

    if (this.patientMedOrderStoreService.validDateTime(admin.administrationScheduledDatetime)) {
      return admin.administrationScheduledDatetime;
    }
    return '';
  }

  rxVerificationStatusSummary(orderCount: TypeCount) {
    if (Object.keys(orderCount).length === 0) return

    if (orderCount.rxVerificationNeeded > 0) {
    
      return {color: 'red', tooltip: `Verification needed (${orderCount.rxVerificationNeeded})`}

    } else if (orderCount.rxVerificationNeeded === 0 && orderCount.rxVerificationComplete > 0) {
    
      return {color: 'green', tooltip: `Verification completed`}
      // return {color: 'green', tooltip: `Verification completed (${orderCount.rxVerificationComplete})`}
    } else {

      return {color: 'pink', tooltip: 'oh,no!'}
    }
  }

  launchPatientAdministrations(patientId: number): void {
    console.log('~~~~~~~~~~MAR DEPT launchPatientAdministrations FROM RX ICON: patientId: ', patientId, ' at ', new Date().toUTCString())

    this.router.navigate([`patients`, patientId])
  }
}
