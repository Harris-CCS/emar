import { Component, OnInit, Input, ViewChild, OnDestroy } from '@angular/core';
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';

import { ModalService } from 'src/services/modal.service';
import { PatientMedOrderStoreService } from 'src/services/patient-med-order-store.service';
import { PatientMedOrderService } from 'src/services/patient-med-order.service';
import { Order, AdministrationAction, OrderAdministration, Event, Action } from '../../../app/interfaces/order';
import { User } from '../../../app/interfaces/user';
import { GivenTemplate } from 'src/app/interfaces/given-template';
import { Router, ActivatedRoute } from '@angular/router';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
import { PatientStoreService } from 'src/services/patient-store.service';
import { SiteStoreService } from 'src/services/site-store.service';

@Component({
  selector: 'order-info-hover',
  templateUrl: './order-info-hover.component.html',
  styleUrls: ['./order-info-hover.component.scss']
})
export class OrderInfoHoverComponent implements OnInit {
  @Input() patientId: number;
  @Input() orders: Array<Order>;
  @Input() admins: Array<any>;
  @ViewChild('popOver', {static: true}) popover: NgbPopover;
  
  siteUTCOffset: string;
  ordersDisplay: Object = {}
  adminsDisplay: Object = {}

  constructor(private modalService: ModalService,
    private patientMedOrderStoreService: PatientMedOrderStoreService,
    private patientMedOrderService: PatientMedOrderService,
    private patientStoreService: PatientStoreService,
    private siteStoreService: SiteStoreService,
    private router: Router,
    private route: ActivatedRoute,
    private composerSchedulerService: ComposerSchedulerService) {

      this.siteUTCOffset = this.siteStoreService.timeZoneOffset;
  }

  ngOnInit(): void {
    console.log('~~~~~~~~~~ORDER-HOVER-INFO ngOnInit at ', new Date().toUTCString())
    // console.log('orderInfo: orders: ', this.orders)
    // console.log('orderInfo: admins: ', this.admins)

    this.ordersDisplay = this.orders.reduce( (prev, curr) => {
      
      prev[curr.id] = curr

      return prev
    }, {})

    // console.log('orderInfo: ordersDisplay: ', this.ordersDisplay)

    this.adminsDisplay = this.admins.reduce( (prev, curr) => {
        
      prev[curr.orderId] = prev[curr.orderId] || []
      prev[curr.orderId].push(curr)

      return prev
    }, {})

    // console.log('orderInfo: adminsDisplay: ', this.adminsDisplay)
  }

  getOrderStatus(order: Order, type: string): string {
    return this.patientMedOrderStoreService.getOrderStatus(order, type);
  }

  close() {
    // this.popover.close();
  }
  
  launchPatientAdministrations(patientId: number): void {
    console.log('launchPatientAdministrations: patientId: ', patientId)
    console.log('~~~~~~~~~~ORDER-HOVER-INFO launchPatientAdministrations at ', new Date().toUTCString())

    this.router.navigate([`patients`, patientId])
  }

  ngOnDestroy() {
    console.log('~~~~~~~~~~ORDER-HOVER-INFO ngOnDestroy at ', new Date().toUTCString())
    // if (this.actionSubscribe !== null) this.actionSubscribe.unsubscribe();
  }

}
