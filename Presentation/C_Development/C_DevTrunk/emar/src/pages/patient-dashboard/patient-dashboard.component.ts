import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as moment from 'moment';
import { FormBuilder, FormGroup } from '@angular/forms';

import { Patient } from '../../app/interfaces/patient';
import { PatientService } from '../../services/patient.service';
import { MedOrderService } from '../../services/med-order.service';
import { PatientMedOrderStoreService } from '../../services/patient-med-order-store.service';
import { Order, OrderAdministration } from 'src/app/interfaces/order';
import { GivenTemplateModalComponent } from './given-template-modal/given-template-modal.component';
import { ModalService } from 'src/services/modal.service';
import { PatientStoreService } from '../../services/patient-store.service';

const INTERVAL_MINUTES = 5; // length in minutes of an interval
const NB_HOURS = 8; // default number of hours displayed (will be reduce on  smaller screen)
const RELOAD_SECONDS = 60; // reload time
const STAT_ID = 2;

import { ORDERS } from '../../app/mockup/orders';
import { timeInterval } from 'rxjs/operators';

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
export class PatientDashboardComponent implements OnInit {
    @ViewChild('grid', {static: true}) grid: ElementRef;
    patient: Patient;
    filter: string = 'all';
    orders: Order[];
    nbOrders: TypeCount = {all:0, prn:0, stat:0, scheduled:0, continuous:0, timed:0, iv:0, ancilliary:0};
    times: string[] = [];
    currentTime: string; // 2020-09-04T13:46:59-04:00
    intervals: OrderInterval[] = [];
    nbHours: number = NB_HOURS;
    displayDose: boolean = true;
    displayStrength: boolean = true;
    reload: number = null; // reload  var
    currentIntervalTime: string = null;

  constructor(
    private route: ActivatedRoute,
    private patientService: PatientService,
    private medOrderService: MedOrderService,
    private formBuilder: FormBuilder,
    private modalService: ModalService,
    private patientMedOrderStoreService: PatientMedOrderStoreService,
    public patientStoreService: PatientStoreService
  ) {}

  ngOnInit(): void {
    // const patientId: number = +this.route.snapshot.params['id'];
    // this.patient = this.patientService.getPatient(patientId);
    const patientId = this.patientStoreService.patientId;
    this.patient = this.patientStoreService.patient;
    this.refresh();
    this.moveTimes();
  }

  refresh(): void {
    // this.orders = this.medOrderService.getCurrentOrders(this.patient.id);
    const patientId: number = this.patientStoreService.patientId;
    this.patientMedOrderStoreService.fetchPatientMedOrder(patientId);
    this.orders = ORDERS; // this.patientMedOrderStoreService.patientMedOrder();
    this.countOrders();
    this.filterOrders();
    this.moveOrdersToday(); // TODO only here for demo purpose - take away when api
    this.currentTime = moment().format();
    if (this.reload !== null) {
      clearTimeout(this.reload);
    }
    this.reload = setTimeout(() => {
      this.refresh();
      this.setIntervals();
    }, 1000 * RELOAD_SECONDS);
  }

  // filter the orders
  filterOrders(): void {
    switch (this.filter) {
      case 'prn':
        this.orders = this.orders.filter( order => typeof order.prn !== 'undefined' && order.prn );
        break;
      case 'stat':
        this.orders = this.orders.filter( order => {
          return typeof order.priority !== 'undefined' && (order.priority.toUpperCase() === 'STAT' || +order.priority === STAT_ID)
        });
        break;
      // Start time is in the future. Ex: 1 times a day, 3 times a day
      case 'scheduled':
        this.orders = this.orders.filter( order => {
          return typeof order.orderType !== 'undefined' && order.orderType.toUpperCase() === 'SCHEDULED';
        });
        break;
      // Timed is a very specific time. Ex: 6:04pm
      case 'timed':
        this.orders = this.orders.filter( order => {
          return typeof order.endTime === 'undefined' || order.endTime === '' || order.endTime === order.startTime
        });
        break;
      // Anything that is running. NON point in time. Could include breathing treatments
      case 'continuous':
        this.orders = this.orders.filter( order => {
          return typeof order.orderType !== 'undefined' && order.orderType == 'Continuous'
        });
        break;
      case 'iv':
        break;
      case 'ancilliary':
        break;
    }
    // sort: entry time, administration time - (alphabetic med name if same time) - complete at the bottom -
  }

  // count the orders for each type
  countOrders(): void {
    this.nbOrders.all = this.orders.length;
    this.nbOrders.stat = this.orders.reduce( (total, order) => {
      return (typeof order.priority !== 'undefined' && (order.priority.toUpperCase() === 'STAT' || +order.priority === STAT_ID))? total + 1: total;
    }, 0);
    this.nbOrders.prn = this.orders.reduce( (total, order) => {
      return (typeof order.priority !== 'undefined' && order.prn)? total + 1: total;
    }, 0);
    this.nbOrders.scheduled = 0; // TODO
    this.nbOrders.timed = 0; // TODO
    this.nbOrders.continuous = 0; // TODO
    this.nbOrders.iv = 0; //TODO
    this.nbOrders.ancilliary = 0; // TODO
  }

  changeNbHours(delta: number): void {
    if (this.nbHours + delta < 1 || this.nbHours + delta > 24)
      return;
    this.nbHours = this.nbHours + delta;
    this.moveTimes(0);
  }

  // default list of times: the hour of now + Nb_HOURS hours
  moveTimes(delta?: number): void {
    if (window.innerWidth < 1000) {
      this.nbHours = 5;
    }
    let startMoment: moment.Moment;
    if (typeof delta === 'undefined') {
      let time = this.getOldestOverdue();
      startMoment = (time === '')? moment().subtract(2, 'hour'): moment(time);
    } else if (delta == 0) {
      startMoment = moment();
    } else {
      if (delta < 0) {
        startMoment = moment(this.times[0]).subtract(-delta, 'hour');
      } else {
        startMoment = moment(this.times[0]).add(delta, 'hour');
      }
    }
    this.times = [];
    this.times[0] = startMoment.startOf('hour').format();
    for (let ii = 1; ii < this.nbHours; ++ii) {
      this.times[ii] = startMoment.add(1, 'hour').format(); // startMoment is mutable
    }
    this.setIntervals();
  }

  // Return the oldest overdue date of the orders
  getOldestOverdue(): string {
    let time: moment.Moment = null;
    for (const order of this.orders) {
      if (Array.isArray(order.orderAdministrations)) {
        for (const admin of order.orderAdministrations) {
          if (admin.missedDose) {
            let missedTime: moment.Moment = moment(admin.administrationScheduledDatetime);
            if (time == null || missedTime.isBefore(time)) {
              time = missedTime;
            }
          }
        }
      }
    }
    return (time == null)? "": time.format();
  }

  // Mockup: move orders to today, compute misseddose
  moveOrdersToday() {
    const currentMoment = moment();
    for (let ii = 0; ii < this.orders.length; ++ii) {
      this.orders[ii].signedOn = this.moveToday(this.orders[ii].signedOn);
      this.orders[ii].startTime = this.moveToday(this.orders[ii].startTime);
      this.orders[ii].endTime = this.moveToday(this.orders[ii].endTime);
      if (typeof this.orders[ii].orderAdministrations !== 'undefined') {
        for (let jj = 0; jj < this.orders[ii].orderAdministrations.length; ++jj) {
          let admin = this.orders[ii].orderAdministrations[jj];
          if (typeof admin.administrationDatetime !== 'undefined' && admin.administrationDatetime != '') {
            this.orders[ii].orderAdministrations[jj].administrationDatetime = this.moveToday(admin.administrationDatetime);
          }
          if (typeof admin.administrationScheduledDatetime !== 'undefined' && admin.administrationScheduledDatetime != '') {
            this.orders[ii].orderAdministrations[jj].administrationScheduledDatetime = this.moveToday(admin.administrationScheduledDatetime);
          }
          if (typeof admin.administrationInputDatetime !== 'undefined' && admin.administrationInputDatetime != '') {
            this.orders[ii].orderAdministrations[jj].administrationInputDatetime = this.moveToday(admin.administrationInputDatetime);
          }
          if (typeof admin.stopScheduledDatetime !== 'undefined' && admin.stopScheduledDatetime != '') {
            this.orders[ii].orderAdministrations[jj].stopScheduledDatetime = this.moveToday(admin.stopScheduledDatetime);
          }
          if (typeof admin.acknowledgeDatetime !== 'undefined' && admin.acknowledgeDatetime != '') {
            this.orders[ii].orderAdministrations[jj].acknowledgeDatetime = this.moveToday(admin.acknowledgeDatetime);
          }
          if (admin.administrationStatus == "Pending") {
            this.orders[ii].orderAdministrations[jj].missedDose = moment(admin.administrationScheduledDatetime).isBefore(currentMoment);
          }
        }
      }
    }
  }

  moveToday(dateTime: string) {
    const today = moment();
    let mo = moment(dateTime);
    return mo.set({year: today.year(), month: today.month(), date: today.date(), hour: mo.hour(), minute: mo.minute()}).format();
  }

  setIntervals() {
    let mo;
    this.intervals = [];
    for (let ii = 0; ii < this.orders.length; ++ii) {
      let start = moment(this.orders[ii].startTime);
      let end = moment(this.orders[ii].endTime);
      mo = moment(this.times[0]);
      let values: Interval[] = [];
      for (let jj = 0; jj < this.nbHours * 60 / INTERVAL_MINUTES; ++jj) {
        let val: Interval = {event: '', isHour: false, isNow: false, time: ''};
        if (mo.isBefore(start)) {
          val.event = ' ';
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
    console.log('INTERVALS', this.nbHours, this.intervals);
  }

  getIntervals(orderId: number) {
    const interval = this.intervals.find( (interval) => interval.orderId === orderId);
    if (interval === null) return null;
    return interval.intervals;
  }

  // type = wjat is returned = icon, textClass
  // TODO: complete 
  getOrderStatus(order: Order, type: string): string {
    let icon: string;
    let textClass: string;
    switch (order.orderStatus) {
      case 'Pending':
        icon = 'pending';
        textClass = 'order-status-pending';
        break;
      case 'Ongoing':
        icon = 'ongoing';
        textClass = 'order-status-ongoing';
        break;
      // TODO
      default: icon = 'error'; break;
    }
    if (type == 'textClass') {
      return textClass;
    }
    return '../../assets/icon/order-' + icon + '.svg';
  }

  // type = what is returnend = icon, tooltipText, tooltipClass, textClass,text
  // TODO complete
  getAdminStatus(admin: OrderAdministration, type:string): string {
    let icon: string;
    let ttText: string;
    let ttClass: string;
    let textClass: string;
    let text: string;
    switch(admin.administrationStatus) {
      case "Pending":
        if (typeof admin.acknowledgeDatetime !== 'undefined' && admin.acknowledgeDatetime !== '') {
          if (admin.missedDose) {
            icon = 'acknowledged-due-event';
            ttText = 'Missed Dose';
            ttClass = 'pd-due';
            textClass = 'order-status-due';
            text = "Acknowledged, Missed Dose";
          } else {
            icon = 'acknowledged-event';
            ttText = 'Acknowledged';
            ttClass = 'pd-acknowledged';
            textClass = 'order-status-acknowledged';
            text = "Acknowledged";
          }
        } else {
          if (admin.missedDose) {
            icon = 'scheduled-due';
            ttText = 'Missed Dose';
            ttClass = 'pd-due';
            textClass = 'order-status-due';
            text = 'Scheduled, Missed Dose';
          } else {
            icon = 'scheduled';
            ttText = 'Scheduled';
            ttClass = 'pd-pending';
            textClass = 'order-status-pending';
            text = "Scheduled";
          }
        }
         break;
      case "Given":
        icon = 'given';
        ttText = 'Given';
        ttClass = 'pd-given';
        textClass = 'order-status-given';
        text = "Given";
        break;
      case "Held":
      case "Onhold":
        icon = 'held';
        ttText = 'Held';
        ttClass = 'pd-held';
        textClass = 'order-status-hold';
        text = "Held";
        break;
      // TODO
      default:
        icon = 'error';
        break;
    }
    if (type == 'tooltipText') {
      return ttText;
    }
    if (type == 'tooltipClass') {
      return ttClass;
    }
    if (type == 'textClass') {
      return textClass;
    }
    if (type == "text") {
      return text;
    }
    return '../../assets/icon/order-' + icon + '.svg';
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
      return 'url("/assets/icon/dash.svg") 50% repeat-x';
    }
    return 'url("/assets/icon/three-dots.svg") 50% repeat-x';
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
    this.refresh();
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
    const mEnd = moment(this.times[this.times.length - 1]);
    if (moment(order.startTime).isBetween(mStart, mEnd) || moment(order.endTime).isBetween(mStart, mEnd)) {
      // console.log("ACTIVEORDER", order.name,order.startTime, order.endTime);
      return true;
    }
    return false;
  }

  activeAdmin(admin: OrderAdministration): boolean {
    const time = this.adminTime(admin);
    return this.activeTime(time);
  }

  // test if time is in the displayed hours
  activeTime(time: string, order?: Order) : boolean {
    const mStart = moment(this.times[0]);
    const mEnd = moment(this.times[this.times.length - 1]);
    // console.log("ACTIVETIME", order.id, order.name, time,moment(time).isBetween(mStart, mEnd, 'minute', '[]'));
    return moment(time).isBetween(mStart, mEnd, 'minute', '[]');
  }

  // return time of the administration
  adminTime(admin: OrderAdministration): string {
    // console.log("ADMINTIME",admin);
    if (typeof admin.administrationDatetime !== 'undefined' && admin.administrationDatetime !== '') {
      return admin.administrationDatetime;
    }
    // TODO Input
    if (typeof admin.administrationScheduledDatetime !== 'undefined' && admin.administrationScheduledDatetime !== '') {
      return admin.administrationScheduledDatetime;
    }
    return '';
  }
  onClickAdministration(admin: OrderAdministration) {
  
  }
  // open modal to given template
  onClickAction(admin: OrderAdministration, order: Order, action:string): void {
    if (action == 'Give') {
      const template = 'ear'; // TODO order.medicationRoute.routeName.toLowerCase()
      const title = order.name + ' ' + order.dose + ' - ' + order.medicationRoute.routeName;
      this.modalService.open(
        'given-template-modal',
        {
          template: template,
          class: ['title']
        },
        title
      );
    }
  }
}
