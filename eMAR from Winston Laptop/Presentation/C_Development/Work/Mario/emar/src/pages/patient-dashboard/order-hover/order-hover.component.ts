import { Component, OnInit, Input, ViewChild, OnDestroy } from '@angular/core';
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { Subject, Subscription } from 'rxjs';
import * as moment from 'moment';

import { ModalService } from '../../../services/modal.service';
import { PatientMedOrderStoreService } from '../../../services/patient-med-order-store.service';
import { PatientMedOrderService } from '../../../services/patient-med-order.service';
import { Order, AdministrationAction, OrderAdministration, Event, Action } from '../../../app/interfaces/order';
import { User } from '../../../app/interfaces/user';
import { GivenTemplate } from '../../../app/interfaces/given-template';
import { Router, ActivatedRoute } from '@angular/router';
import { ComposerSchedulerService } from '../../../services/composer-scheduler.service';
import { PatientStoreService } from '../../../services/patient-store.service';
import { SiteStoreService } from '../../../services/site-store.service';

const ACTION_COSIGN = 5;
const ACTION_ACKNOWLEDGE: Action = {actionId:1, actionCode:'Acknowledge'}

// TODO take away when API done
const MOCKUP: boolean = false;
import { GIVEN_TEMPLATE_EAR } from '../../../app/mockup/given-template-ear';
import { GIVEN_TEMPLATE_ORAL } from '../../../app/mockup/given-template-oral';
import { GIVEN_TEMPLATE_INTRAMUSCULAIRE } from '../../../app/mockup/given-template-intramusculaire';
import { ACTION_TEMPLATE_HOLD } from '../../../app/mockup/action-template-hold';
import { ACTION_TEMPLATE_UNHOLD } from '../../../app/mockup/action-template-unhold';
import { ACTION_TEMPLATE_CANCEL } from '../../../app/mockup/action-template-cancel';
import { ACTION_TEMPLATE_DELETE } from '../../../app/mockup/action-template-delete';
import { ACTION_TEMPLATE_DISCONTINUE } from '../../../app/mockup/action-template-discontinue';
import { ACTION_TEMPLATE_DISCONTINUED } from '../../../app/mockup/action-template-discontinued';
import { ACTION_TEMPLATE_MISSED_DOSE } from '../../../app/mockup/action-template-missed-dose';
import { ACTION_TEMPLATE_RESCHEDULE } from '../../../app/mockup/action-template-reschedule';
import { ACTION_TEMPLATE_FOLLOWUP } from '../../../app/mockup/action-template-follow-up';
import { UserStoreService } from '../../../services/user-store.service';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'order-hover',
    templateUrl: './order-hover.component.html',
    styleUrls: ['./order-hover.component.scss']
})

export class OrderHoverComponent implements OnInit, OnDestroy {
    @Input() actions: AdministrationAction[];
    @Input() order: Order;
    @Input() admin: OrderAdministration;
    @ViewChild('popOver', {static: true}) popover: NgbPopover;
    events: Event[];
    actionSubscribe: Subscription = null;
    siteUTCOffset: string; // -06:00
    hasMedicationServicePermissionReadOnly: boolean = false
    notifierActionSubscribe = new Subject();
    @Input() adminActionOnFire: (adminId: number) => void
    @Input() adminActionFireExtinguished: (adminId: number) => void

    constructor(private modalService: ModalService,
      private patientMedOrderStoreService: PatientMedOrderStoreService,
      private patientMedOrderService: PatientMedOrderService,
      private patientStoreService: PatientStoreService,
      private siteStoreService: SiteStoreService,
      private userStoreService: UserStoreService,
      private router: Router,
      private route: ActivatedRoute,
      private composerSchedulerService: ComposerSchedulerService) {
        this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
    }

    ngOnInit(): void {
      this.events = [];
      if (this.admin == null) {
        if (typeof this.order.orderEvents !== 'undefined' && this.order.orderEvents !== null) {
          this.events = this.order.orderEvents;
          /* this.order.orderEvents.map( event => {
            if (event.actionId == ACTION_COSIGN) this.coSignUsers.push(this.patientMedOrderStoreService.formatDetail(event.user, event.eventDatetime));
          }); */
        }
      } else {
        if (typeof this.admin.administrationEvents !== 'undefined') {
          this.events = this.admin.administrationEvents;
          /* this.admin.administrationEvents.map( event => {
            if (event.actionId == ACTION_COSIGN) this.coSignUsers.push(this.patientMedOrderStoreService.formatDetail(event.user, event.eventDatetime));
          }); */
        }
        if (this.admin.acknowledgeDatetime !== null && this.admin.acknowledgeDatetime !== ''
          && this.getAdminStatus(this.admin, 'text') !== 'Acknowledged' ) {
          let event: Event =  {
            'eventDatetime': this.admin.acknowledgeDatetime,
            'action': ACTION_ACKNOWLEDGE,
            'user': this.admin.acknowledgeUser
          };
          this.events.push(event);
          this.events.sort(this.compareEvent);
        }
        console.log('EVENTS', this.events)
      }

      const MEDICATION_SERVICES = this.userStoreService.MEDICATION_SERVICES
      // We should not seen Exclude (E) here since PCED should not have eMAR access for the permission 
      this.hasMedicationServicePermissionReadOnly = MEDICATION_SERVICES === 'R' || MEDICATION_SERVICES === 'E'
    }

    compareEvent(a: Event, b: Event): number {
      const mo: moment.Moment = moment(a.eventDatetime);
      if (mo.isSame(b.eventDatetime)) return 0;
      return mo.isBefore(b.eventDatetime)? -1: 1;
    }

  // type = what is returned = icon, textClass....
  getOrderStatus(order: Order, type: string): string {
    return this.patientMedOrderStoreService.getOrderStatus(order, type);
  }

  // type = what is returnend = icon, tooltipText, tooltipClass, textClass, text
  // TODO complete
  getAdminStatus(admin: OrderAdministration, type:string): string {
    return this.patientMedOrderStoreService.getOrderAdministrationStatus(admin, type);
  }

  // query template to api
  onClickAction(admin: OrderAdministration, order: Order, action: AdministrationAction): void {
    const unit: string = (typeof order.doseUnit == 'undefined' || order.doseUnit == null)? '': ' ' + order.doseUnit.unitName;
    let title: string = '<span class="bigger-bolder-blue">' + order.medication.displayName + ' ' + order.dose + unit + '</span>';
    const titleReschedule: string = '<span class="bigger-bolder-blue">' + order.medication.displayName + ' ' + order.dose + ' ' + order.doseUnit?.unitName + ', ' + order.frequencySchedule?.scheduleName + '</span>';

    if (MOCKUP) {
      let template: GivenTemplate = null;
      if (action.availableAction.toLowerCase() == 'give') {
        // TODO: it is not really the route ex: IV
        const route: string = order.medicationRoute.routeName.toLowerCase();
        // console.log('ROUTE', route);
        switch (route) {
          case 'oral': template = GIVEN_TEMPLATE_ORAL; break;
          case 'ear': template = GIVEN_TEMPLATE_EAR; break;
          case 'intramusculaire': template = GIVEN_TEMPLATE_INTRAMUSCULAIRE; break;
          // TODO
          default: template = GIVEN_TEMPLATE_ORAL; break;
        }
      } else {
        // console.log('GOACTION',action);
        // TODO replace with actioncode
        switch (action.availableAction.toLowerCase()) {
          case 'cosign': break;
          case 'acknowledge': break;
          case 'hold': template = ACTION_TEMPLATE_HOLD; break;
          case 'misseddose': template = ACTION_TEMPLATE_MISSED_DOSE; break;
          case 'unhold': template = ACTION_TEMPLATE_UNHOLD; break;
          case 'cancel': template = ACTION_TEMPLATE_CANCEL; break;
          case 'delete': template = ACTION_TEMPLATE_DELETE; break;
          case 'orderdiscontinue': template = ACTION_TEMPLATE_DISCONTINUE; break;
          case 'discontinued': template = ACTION_TEMPLATE_DISCONTINUED; break;
          case 'reschedule': template = ACTION_TEMPLATE_RESCHEDULE; title = titleReschedule; break;
          case 'restart': break; // TODO
          case 'followup': template = ACTION_TEMPLATE_FOLLOWUP; break;
          case 'repeat': break;
        }
      }
      if (template !== null) this.templatePopup(admin, order, action, template, title);
    } else {

      if (action.availableAction.toLowerCase() === 'repeat'
          || action.availableAction.toLowerCase() === 'modify') {
        
        this.launchMedComposer(order.id, order, action.availableAction.toLowerCase())

      } else {
        this.patientMedOrderService.updateRequest.emit(true);
        if (admin?.id) {
          this.adminActionOnFire(admin.id)
        }
        
        this.actionSubscribe = this.patientMedOrderService
          .postOrderAction(action)
          .pipe(takeUntil(this.notifierActionSubscribe))
          .subscribe(data => {
            console.log('RESULT POST ACTION', data);
            console.log('SUBSCRIBE OVER');
            if (admin?.id) {
              this.adminActionFireExtinguished(admin.id)
            }

            if (data.template == null) {  // cosign, acknowledge
              // console.log("DEBUG"); let o = order; o.orderStatus = 'Completed'; this.patientMedOrderService.refreshRequest.emit(o);
              if (typeof data.updatedOrder !== 'undefined' && data.updatedOrder !== null) {
                this.patientMedOrderService.refreshRequest.emit(data.updatedOrder);
              } else {
                this.patientMedOrderService.refreshRequest.emit(null);
              }
              return;
            }
            this.patientMedOrderService.refreshRequest.emit(null);
            this.templatePopup(admin, order, action, data.template, title);
          });
      }
    }
  }

  // display given or action template
  templatePopup(admin: OrderAdministration, order: Order, action: AdministrationAction, template: GivenTemplate, title: string) {
    if (this.siteStoreService.popup_on_give.toLowerCase() == 'y' && action.availableAction.toLowerCase() == 'give') {
      this.modalService.open(
        'five-rights',
        {
          order: order,
          patient: this.patientStoreService.patient,
          template: template
        },
        '<span class="bigger-bolder">Confirm</span>'
      );
    } else {
      this.modalService.open(
        'given-template-modal',
        {
          template: template,
          order: order,
          admin:admin,
          buttonText: action.availableAction.toLowerCase() == 'give'? 'Give': template.name
        },
        (action.availableAction.toLowerCase() == 'give') ? title + ' - <b>'+ order.medicationRoute.routeName + '</b>': title
      );
    }
  }

  close() {
    // this.popover.close();
  }

  launchMedComposer(medId: number, medData: object, action: string): void {
    console.log('launchMedComposer: medId: ', medId)
    console.log('launchMedComposer: medData: ', medData)

    this.composerSchedulerService.setInitialComposerData({ action, source: 'patient-order', med: medData });
    console.log('launchMedComposer: this.route: ', this.route)

    // this.router.navigate(['medservice/new-order', 234217],
    this.router.navigate(['medservice/new-order', medId],
      {
        // state: { data: { medData } },
        queryParams: {},
        relativeTo: this.route
      });
  }

  ngOnDestroy() {
    console.log('NGONDESTROY ORDER HOVER');
    // if (this.actionSubscribe !== null) this.actionSubscribe.unsubscribe();
    // this.notifierActionSubscribe.next();
    // this.notifierActionSubscribe.complete();
  }
}
