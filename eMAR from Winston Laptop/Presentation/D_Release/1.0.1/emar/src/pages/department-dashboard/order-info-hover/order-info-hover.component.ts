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
  @Input() orders: Order;
  @ViewChild('popOver', {static: true}) popover: NgbPopover;
  
  siteUTCOffset: string;

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
    // console.log('orderInfo: orders: ', this.orders)
  }

  getOrderStatus(order: Order, type: string): string {
    return this.patientMedOrderStoreService.getOrderStatus(order, type);
  }

  close() {
    // this.popover.close();
  }
  
  launchPatientAdministrations(patientId: number): void {
    console.log('launchPatientAdministrations: patientId: ', patientId)

    this.router.navigate([`patients`, patientId])
  }

  ngOnDestroy() {
    console.log('NGONDESTROY ORDER INFO HOVER');
    // if (this.actionSubscribe !== null) this.actionSubscribe.unsubscribe();
  }

}
