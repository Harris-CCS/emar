import { Component, OnInit, ViewChild, ElementRef, ComponentFactoryResolver, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as moment from 'moment';
import { FormBuilder, FormGroup } from '@angular/forms';

import { Patient } from '../../app/interfaces/patient';
import { PatientService } from '../../services/patient.service';
import { MedOrderService } from '../../services/med-order.service';
import { PatientMedOrderStoreService } from '../../services/patient-med-order-store.service';
import { Order, OrderAdministration } from '../../app/interfaces/order';
import { ModalService } from '../../services/modal.service';
import { PatientStoreService } from '../../services/patient-store.service';
import { SiteStoreService } from '../../services/site-store.service'
 
const INTERVAL_MINUTES = 5; // length in minutes of an interval
const NB_HOURS = 8; // default number of hours displayed (will be reduce on  smaller screen)
const PRE_HOURS_DISPLAY = 2; // hours before current time display begins
const RELOAD_SECONDS = 60; // reload time
const STAT_ID = 2;
const NB_CHAR_ORDER_NAME = 60; /* order displayed name will be truncated to this size to have a fix width for sticky header for IE11 */
const MOCKUP = false; // true: mockup orders
const DEBUG_SORT: boolean = false;

import { ORDERS } from '../../app/mockup/orders';
// import { ORDERS_SORT } from '../../app/mockup/orders-sort';
import { PatientMedOrderService } from '../../services/patient-med-order.service';
import { ComposerSchedulerService } from '../../services/composer-scheduler.service';
import { UserStoreService } from '../../services/user-store.service';
import { DatePipe } from '@angular/common';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface Interval {
  id: number;
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
  all: number;
  prn: number;
  stat: number;
  scheduled: number;
  continuous: number;
  timed: number;
  iv: number;
  ancilliary: number;
}

@Component({
  selector: 'patient-dashboard',
  templateUrl: './patient-dashboard.component.html',
  styleUrls: ['./patient-dashboard.component.scss', '../../assets/css/site.css'] // TODO remove site.css
})
export class PatientDashboardComponent implements OnInit, OnDestroy {
    patient: Patient;
    filter: string = 'all';
    sort: string = 'Administration time';
    orders: Order[] = [];
    nbOrders: TypeCount = {all:0, prn:0, stat:0, scheduled:0, continuous:0, timed:0, iv:0, ancilliary:0};
    times: string[] = [];
    currentTime: string; // 2020-09-04T13:46:59-04:00
    intervals: OrderInterval[] = [];
    nbHours: number = NB_HOURS;
    displayDose: boolean = true; // TODO Api
    displayStrength: boolean = true; // TODO Api
    reload: number = null; // reload  var
    currentIntervalTime: string = null;
    timeHasBeenMoved: boolean = false;
    fetching: boolean = false; /* we have no data, we are calling the api */
    updating: boolean = false; /* the data are perhaps out of sync, we are calling the api */
    siteUTCOffset: string; // -06:00
    oldestDue: string; // 2020-09-04T13:46:59-04:00 if due adminstration or ''
    nbCharOrderName: number = NB_CHAR_ORDER_NAME;
    orderNameColumnWidth: string = (NB_CHAR_ORDER_NAME/2).toString()+'rem';
    administrationsColumnWidth: string = '100%';
    refreshSubscribe: Subscription = null;
    updateSubscribe: Subscription = null;
    orderSubscribe: Subscription = null;
    notifierOrderSubscribe = new Subject();

    rx_verification_needed: string = 'red'
    rx_verification_complete: string = 'green'

    adminActionProcessing: Object = {}
    boundAdminActionOnFireHandler: (adminId: number) => void
    boundAdminActionFireExtinguishedHandler: (adminId: number) => void

  constructor(
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
    private datePipe: DatePipe
  ) {
    this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
  }

  ngOnInit(): void {
    const patientId = this.patientStoreService.patientId;
    this.patient = this.patientStoreService.patient;
    this.fetching = true;
    this.refresh();
    this.refreshSubscribe = this.patientMedOrderService.refreshRequest.subscribe( order => {
      console.log('REFRESH ORDER', order);
      this.refreshOrder(order);
    });
    this.updateSubscribe = this.patientMedOrderService.updateRequest.subscribe( data => {
      this.updating = data;
    });

    this.adminActionProcessing = {}
    this.boundAdminActionOnFireHandler = this.adminActionOnFireHandler.bind(this)
    this.boundAdminActionFireExtinguishedHandler = this.adminActionFireExtinguishedHandler.bind(this)
  }

  adminActionOnFireHandler(adminId: number): void {
    this.adminActionProcessing[adminId] = true
  }

  adminActionFireExtinguishedHandler(adminId: number): void {
    delete this.adminActionProcessing[adminId]
  }

  refreshOrder(order: Order) {
    if (order !== null) {
      this.orders.map( (ord, i) => {
        if (ord.id == order.id) {
          this.orders[i] = order;
        }
      });
    }
    this.updating = false;
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
      const patientId: number = this.patientStoreService.patientId;
      if (this.reload !== null) {
        clearTimeout(this.reload);
      }
      this.orderSubscribe = this.patientMedOrderService.getPatientCurrentOrders(patientId)
      .pipe(takeUntil(this.notifierOrderSubscribe))
      .subscribe( orders => {
        this.orders = [];
        if (typeof orders !== 'undefined' ) {
          this.orders = orders.filter(order => order.orderStatus != 'Deleted'); // slice(0);
        }
        if (MOCKUP) this.moveOrdersToday();
        this.moveTimes(); // compute time header
        this.refreshScreen();
        this.fetching = false;
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
      });
    }
  }

  /* executed after each new set of Orders */
  refreshScreen() {
    this.countOrders();
    this.filterOrders(); // reduce the orders only to the filtered TODO set an indicator instead of rebuilding it
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
    return order.applicableFilters.includes(type);
    /*
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
    */
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
    // cancelled at the bottom
    if (o1.orderStatus.toLowerCase() == 'cancelled') {
      if (o2.orderStatus.toLowerCase() == 'cancelled') {
        // Cancelled are sort together on date or if same date on name
        if (m1 == m2) return o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
        return moment(m2).isBefore(m1)? 1: -1;
      }
      return 1; // o2<o1 - Cancelled are below any other status
    } else if (o2.orderStatus.toLowerCase() == 'cancelled') {
      return -1; // o1<o2 - Cancelled are below any other status
    }
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
      let next1 = null;
      let due2 = null;
      let next2 = null;
      if (typeof o1.nextActionTime === 'undefined' || o1.nextActionTime == null) {
        due1 = this.getOrderOldestOverdue(o1, null);
      } else {
        if (!moment().isBefore(o1.nextActionTime)) {
          due1 = moment(o1.nextActionTime);
        }
        next1 = moment(o1.nextActionTime);
      }
      if (typeof o2.nextActionTime === 'undefined' || o2.nextActionTime == null) {
        due2 = this.getOrderOldestOverdue(o2, null);
      } else {
        if (!moment().isBefore(o2.nextActionTime)) {
          due2 = moment(o2.nextActionTime);
        }
        next2 = moment(o2.nextActionTime);
      }
      // due at the top
      ret = this.sortHelper(due1, o1, due2, o2, 'due');
      if (ret != 2) return ret;
      
      // then the stat
      const stat1 = this.isOrder(o1, 'stat');
      const stat2 = this.isOrder(o2, 'stat');
      if (stat1) {
        if (stat2) {
          ret = this.sortHelper(next1, o1, next2, o2, 'stat');
          if (ret != 2) return ret;
        } else {
          ret = -1;
          if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first stat');
          return -1; // o1<o2 o1 is stat not o2
        }
      } else if (stat2) {
        ret = 1;
        if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second stat');
        return ret; // o2<o1 o2 stat , not o1
      }

      // then the non point in time(IV) running
      const pti1 = o1.pointInTime;
      const pti2 = o2.pointInTime;
      if (!pti1) {
        if (!pti2) {
          ret = this.sortHelper(next1, o1, next2, o2, 'pri');
          if (ret != 2) return ret;
        } else {
          ret = -1;
          if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first pit');
          return ret; // o1<o2 o1 is IV not o2
        }
      } else if (!pti2) {
        ret = 1;
        if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second pit');
        return ret; // o2<o1 o2 is IV , not o1
      }

      // then prn
      const prn1 = this.isOrder(o1, 'prn');
      const prn2 = this.isOrder(o2, 'prn');
      if (prn1) {
        if (prn2) {
          ret = this.sortHelper(next1, o1, next2, o2, 'prn');
          if (ret != 2) return ret;
        } else {
          ret = -1;
          if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first prn');
          return ret; // o1<o2 o1 is prn not o2
        }
      } else if (prn2) {
        ret = 1;
        if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second prn');
        return ret; // o2<o1 o2 is prn , not o1
      }

      // then the rest .... should have been in the previous group
      if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' DEFAUKT');
      return o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;

    } else { // sort on enter time, then name, then dose
      if (m1 == m2) {
        if (o1.medication.displayName == o2.medication.displayName) {
          ret = o1.dose < o2.dose? -1: 1;
          if (DEBUG_SORT) console.log('COMPARE:',o1.id+o1.medication.displayName+'('+m1+')-'+o2.id+o2.medication.displayName+'('+m2+')='+ret+' dose');
          return ret;
        }
        // localeCompare does not give same result IE11 and Chrome
        ret = o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
        if (DEBUG_SORT) console.log('COMPARE:',o1.id+o1.medication.displayName+'('+m1+')-'+o2.id+o2.medication.displayName+'('+m2+')='+ret+' name.');
        return ret;
      }
      ret = moment(m1).isBefore(m2)? -1: 1
      if (DEBUG_SORT) console.log('COMPARE:',o1.id+o1.medication.displayName+'('+m1+')-'+o2.id+o2.medication.displayName+'('+m2+')='+ret+' on date');
      return ret;
    }
  }

  sortHelper(due1: moment.Moment, o1: Order, due2: moment.Moment, o2: Order, reason: string): number {
    let ret: number;
    if (due1 !== null) {
      if (DEBUG_SORT) console.log('COMPARE o1',o1.id,'-',due1.format(), o1.nextActionTime);
      if (due2 !== null) {
        if (DEBUG_SORT) console.log('COMPARE o2',o2.id,'-',due2.format(), o2.nextActionTime);
        if (due1.isSame(due2)) {
          ret = o1.medication.displayName.toLowerCase() < o2.medication.displayName.toLowerCase()? -1: 1;
          if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,reason+' name');
          return ret;
        }
        ret = moment(due2).isBefore(due1)? 1: -1;
        if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,reason+' date','/',o1.medication.displayName,'/',o2.medication.displayName);
        return ret;
      }
      ret = -1;
      if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' first '+reason,'/',o1.medication.displayName,'/',o2.medication.displayName);
      return ret; // o1<o2 o1 is this reason not o2
    } else if (due2 !== null) {
      if (DEBUG_SORT) console.log('COMPARE o2',o2.id,'-',due2.format(), o2.nextActionTime);
      ret = 1;
      if (DEBUG_SORT) console.log('COMPARE:',o1.id,"-",o2.id,"=",ret,' second '+reason,'/',o1.medication.displayName,'/',o2.medication.displayName);
      return 1; // o2<o1 o2 is this reason not o1
    }
    return 2;
  }

  // count the orders for each type
  countOrders(): void {
    this.nbOrders.all = this.orders.length;
    this.nbOrders.stat = this.orders.reduce( (total, order) => {
      return this.isOrder(order, 'stat')? total + 1: total;
    }, 0);
    this.nbOrders.prn = this.orders.reduce( (total, order) => {
      return this.isOrder(order, 'prn')? total + 1: total;
    }, 0);
    this.nbOrders.scheduled = this.orders.reduce( (total, order) => {
      return this.isOrder(order, 'scheduled')? total + 1: total;
    }, 0);
    this.nbOrders.timed = this.orders.reduce( (total, order) => {
      return this.isOrder(order, 'timed')? total + 1: total;
    }, 0);
    this.nbOrders.continuous = this.orders.reduce( (total, order) => {
      return this.isOrder(order, 'continuous')? total + 1: total;
    }, 0);
    this.nbOrders.iv = this.orders.reduce( (total, order) => {
      return this.isOrder(order, 'iv')? total + 1: total;
    }, 0);
    this.nbOrders.ancilliary = this.orders.reduce( (total, order) => {
      return this.isOrder(order, 'ancilliary')? total + 1: total;
    }, 0);
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
  moveTimes(delta?: number, problem?: boolean, dateTime?: string): void {
    if (window.innerWidth < 1000) {
      this.nbHours = 5;
    }
    this.oldestDue = this.getOldestOverdue();
    let startMoment: moment.Moment;
    if (dateTime !== undefined) {
      startMoment = moment(dateTime);
      this.timeHasBeenMoved = true;
    } else if (typeof delta === 'undefined' || problem === true) {
      if (this.timeHasBeenMoved && problem !== true) {
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
  hasOverDueOut() {
    return moment(this.oldestDue).isBefore(this.times[0]);
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

  moveNextAdministration(order: Order) {
    this.moveTimes(0, true, order.nextActionTime);
    this.setIntervals();
    this.refreshScreen();
  }

  isOverDue(order: Order) {
    const mo: moment.Moment = moment(order.nextActionTime);
    return mo.isBefore(moment());
  }

  isToday(dateTime: string) {
    return moment().format('YYY-MM-DD') == moment(dateTime).format('YYY-MM-DD');
  }

  setIntervals() {
    let mo;
    let id: number = 0;
    this.intervals = [];
    for (let ii = 0; ii < this.orders.length; ++ii) {
      let start = moment(this.orders[ii].beginDatetime);
      let end = null;
      if (this.orders[ii].endDatetime != null) end = moment(this.orders[ii].endDatetime);
      mo = moment(this.times[0]);
      let values: Interval[] = [];
      for (let jj = 0; jj < this.nbHours * 60 / INTERVAL_MINUTES; ++jj) {
        let val: Interval = {id: id, event: '', isHour: false, isNow: false, time: ''};
        id = id + 1;
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
      this.intervals.push({orderId: this.orders[ii].id, intervals: values});
    }
    // console.log('INTERVALS', this.nbHours, this.intervals);
  }

  getIntervals(orderId: number) {
    const interval = this.intervals.find( (interval) => interval.orderId === orderId);
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
    if (this.patientMedOrderStoreService.validDateTime(admin.administrationDatetime)) {
      return admin.administrationDatetime;
    }
    if (this.patientMedOrderStoreService.validDateTime(admin.administrationScheduledDatetime)) {
      return admin.administrationScheduledDatetime;
    }
    return '';
  }

  trackByFn(index, item) {
    return item.id; // unique id corresponding to the item
  }

  ngOnDestroy() {
    console.log('NGONDESTROY DASH');
    if (this.refreshSubscribe !== null) this.refreshSubscribe.unsubscribe();
    if (this.updateSubscribe !== null) this.updateSubscribe.unsubscribe();
    //if (this.orderSubscribe !== null) this.orderSubscribe.unsubscribe();
    this.notifierOrderSubscribe.next();
    this.notifierOrderSubscribe.complete();
  }

  rxVerificationStatus(order: Order) {
    return order.inpatientMedOrder === 1 
      ? {color: this.rx_verification_needed, tooltip: 'RX Verification Needed'} 
      : {color: this.rx_verification_complete, tooltip: 'RX Verification Complete'} 
  }
}
