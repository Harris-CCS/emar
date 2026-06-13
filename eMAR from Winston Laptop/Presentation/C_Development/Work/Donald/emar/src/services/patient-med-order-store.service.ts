import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as moment from 'moment';

import { PatientStoreService } from '../services/patient-store.service';
import { PatientMedOrderService } from '../services/patient-med-order.service';
import { Order, OrderAdministration } from '../app/interfaces/order';
import { User } from '../app/interfaces/user';

const TIME_FORMAT = 'HH:mm'; // TODO Api
const DATE_FORMAT = 'MM/DD/yyyy'; // TODO Api

@Injectable({
  providedIn: 'root'
})
export class PatientMedOrderStoreService {

  constructor(
    private patientStoreService: PatientStoreService,
    private patientMedOrderService: PatientMedOrderService,
  ) {
      this.patientStoreService.patient$.subscribe(async () => {

        if (this.patientStoreService.patientId) {
          await this.fetchPatientMedOrder(this.patientStoreService.patientId)
        }
      })

    // setTimeout(() => this.fetchPatientMedOrder(this.patientId), 10000)
  }

  // private patientId = this.patientStoreService.patientId
  private readonly _patientMedOrder = new BehaviorSubject<any>([])
  readonly patientMedOrder$ = this._patientMedOrder.asObservable().pipe(
    map(orders => {
      orders = orders ? orders.map((ord) => {
        if ('pharmacyVerificationStatus' in ord && ord.pharmacyVerificationStatus !== 1) {
          const rxvIndex = ord.availableActions.findIndex((action) => action.availableAction === 'PharmVerification')

          if (rxvIndex > 0) {
            // remove action if pharmacyVerificationStatus is completed (2) or not needed (0)
            ord.availableActions.splice(rxvIndex, 1)
          }
        }

        return {
          ...ord,
          displayName: ord.medication?.displayName,
          displayRoute: ord.medicationRoute ? ord.medicationRoute.routeName : '',
          displayFrequency: ord.frequencySchedule ? ord.frequencySchedule.scheduleName : '',
          displayDose: ord.dose,
          displayDoseUnit: ord.doseUnit ? ord.doseUnit.printName : '',
          // allergyReactionsText: ord.allergyReactions?.map((alg) => alg.orderBrandName).join(', '),
          // allergyReactionsText: ord.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
          // drugInteractionsText: ord.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
        }
      }) : []

      const parts = orders.reduce((prev, order) => {
        switch (order.orderStatus) {
          case 'Deleted':
            break;
          case 'Cancelled':
            prev[1].push(order)
            break;
          default:
            prev[0].push(order)
        }

        return prev
      }, [[], []])

      return [...parts[0], ...parts[1]]
    })
  )

  get patientMedOrder(): [] {
    return this._patientMedOrder.getValue() || []
  }

  set patientMedOrder(val: []) {
    this._patientMedOrder.next(val)
  }

  async fetchPatientMedOrder(patientId) {
    this.patientMedOrder = await this.patientMedOrderService.getPatientCurrentOrders(patientId).toPromise();
    // console.log('PatientMedOrderStore - fetchPatientMedOrder: patientMedOrder: ', this.patientMedOrder, this._patientMedOrder.getValue()[7])
  }

  // type = what is returned = icon, textClass, text
  getOrderStatus(order: Order, type: string): string {
    let icon: string = '';
    let textClass: string = '';
    let text: string = '';
    if (order.orderStatus !== null) {
      let status = order.orderStatus;
      if (status == "OnHold") status = "Held";
      text = order.orderStatus;
      if (text == 'PendingDiscontinue') text = 'Pending Discontinue';
      if (text == 'OnHold') text = 'On Hold';
      if (text == 'Pending') text = 'Scheduled';
      icon = status.toLowerCase().replace(' ','');
      textClass = 'order-status-' + icon;
      icon = '../../assets/icon/order-' + icon + '.svg'; // TODO more generic
    }
    return (type == 'textClass')? textClass: ( 
            ( type == 'text')? text : icon);
  }

  // type = what is returned = icon, tooltipText, tooltipClass, textClass, text, detail, time
  getOrderAdministrationStatus(admin: OrderAdministration, type:string): string {
    let icon: string = '';
    let ttText: string = '';
    let ttClass: string = '';
    let textClass: string = '';
    let text: string = '';
    let detail: string = '';
    let time: string = '';
    switch(admin.administrationStatus) {
      case "Late":
        if (this.validDateTime(admin.acknowledgeDatetime )) {
          icon = 'acknowledged-due-event';
          ttText = 'Due';
          ttClass = 'pd-due';
          textClass = 'order-status-acknowledged';
          text = "Acknowledged";
          time = admin.acknowledgeDatetime;
          detail = admin.acknowledgeUser.lastName + ', ' + admin.acknowledgeUser.firstName;
        } else {
          icon = 'scheduled-due';
          ttText = 'Due';
          ttClass = 'pd-due';
          textClass = 'order-status-ongoing';
          text = "Scheduled";
          time = admin.administrationScheduledDatetime;
        }
        break;
      case "Pending":
        if (this.validDateTime(admin.acknowledgeDatetime)) {
          if (admin.missedDose) {
            icon = 'acknowledged-due-event';
            ttText = 'Due';
            ttClass = 'pd-due';
            textClass = 'order-status-acknowledged';
          } else {
            icon = 'acknowledged-event';
            ttText = 'Acknowledged';
            ttClass = 'pd-acknowledged';
            textClass = 'order-status-acknowledged';
            time = admin.acknowledgeDatetime;
          }
          text = "Acknowledged";
          time = admin.acknowledgeDatetime;
          detail = admin.acknowledgeUser.lastName + ', ' + admin.acknowledgeUser.firstName;
        } else {
          if (admin.missedDose) {
            icon = 'scheduled-due';
            ttText = 'Due';
            ttClass = 'pd-due';
            textClass = 'order-status-ongoing';
            text = 'Scheduled';
          } else {
            icon = 'scheduled';
            ttText = 'Scheduled';
            ttClass = 'pd-pending';
            textClass = 'order-status-ongoing';
            text = "Scheduled";
            time = admin.administrationScheduledDatetime;
          }
        }
        break;
      case 'OnGoing':
        icon = admin.administrationStatus.toLowerCase();
        ttText = admin.administrationStatus;
        text = admin.administrationStatus;
        ttClass = 'pd-' + icon;
        textClass = 'order-status-' + icon;
        icon = icon + '-event';
        break;
      default:
        icon = admin.administrationStatus.toLowerCase();
        ttText = admin.administrationStatus;
        text = admin.administrationStatus;
        ttClass = 'pd-' + icon;
        textClass = 'order-status-' + icon;
    }
    if ((type == 'detail' || type == 'time') && admin.administrationStatus == 'Given') {
      detail = admin.administeringUser.lastName + ', ' + admin.administeringUser.firstName;
      time = admin.administrationDatetime;
    }
    if ((type == 'detail' || type == 'time') && admin.administrationStatus == 'OnHold') {
      const ii = admin.administrationEvents.length - 1;
      if (ii >= 0 && admin.administrationEvents[ii].action.actionCode == 'Hold') {
        detail = admin.administrationEvents[ii].user.lastName + ', ' + admin.administrationEvents[ii].user.firstName;
        time = admin.administrationEvents[ii].eventDatetime;
      }
    }
    if (admin.administrationStatus)
    icon = '../../assets/icon/order-' + icon + '.svg';
    return (type == 'tooltipText') ? ttText: (
              (type == 'tooltipClass') ? ttClass: (
                (type == 'textClass') ? textClass: (
                  (type == 'text') ? text : (
                    (type == 'time') ? time: (
                      (type == 'detail') ? detail: icon
                    )
                  )
                )
              )
            )
  }

  // format detail user/date for hover
  formatDetail(user: User, dateTime: string): string {
    const mo = moment(dateTime);
    let detail = mo.format(TIME_FORMAT);
    if (!moment().isSame(mo, 'day')) {
      detail = detail + ' on ' + mo.format(DATE_FORMAT);
    }
    if (user != null) {
      detail = detail + ' - ' + user.lastName + ', ' + user.firstName ;
    }
    return detail;
  }

  validDateTime(dateTime: string): boolean {
    if (typeof dateTime === 'undefined') return false;
    if (dateTime === null) return false;
    if (dateTime === '') return false;
    if (dateTime.indexOf('Invalid') !== -1) return false;
    return true;
  }

}
