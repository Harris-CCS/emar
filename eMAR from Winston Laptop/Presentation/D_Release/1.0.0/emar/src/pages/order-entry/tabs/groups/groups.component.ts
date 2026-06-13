import { Component, OnInit, Input } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { MedOrderService } from '../../../../services/med-order.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { UserStoreService } from '../../../../services/user-store.service';
import { PatientStoreService } from '../../../../services/patient-store.service';

import { ModalService } from '../../../../services/modal.service';
import { ComposerSchedulerService } from '../../../../services/composer-scheduler.service';

@Component({
  selector: 'groups',
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.scss'],
})
export class GroupsComponent implements OnInit {
  private groupPanels = [];
  private groupContent = [];

  // private groupPanels = []
  // private groupContent= []
  private userId = this.userStoreService.userId;
  private patientId = this.patientStoreService.patientId;
  @Input() auth: boolean;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private medOrderService: MedOrderService,
    private modalService: ModalService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private composerSchedulerService: ComposerSchedulerService,

  ) { }

  ngOnInit(): void {
    this.getGroupOrdersList();
    
    this.medOrderService.refreshRequest_listOrders.subscribe( e => {
      console.log('REFRESH GroupOrdersList++++++++ e: ', e);
      this.getGroupOrdersList();
    });
  }

  groups() {
    return 'groups';
  }

  groupsOrders() {
    // return this.medOrderService.getGroupsOrders();
    // console.log('groupOrders: ', this.groupPanels)
    return this.groupPanels;
  }

  getGroupOrdersList() {
    this.medOrderService.getGroupsOrdersList().subscribe((g) => {
      this.groupPanels = g.map((o) => ({
        displayGroupName: o.groupName,
        orders: o.orders.map((x) => ({
          ...x,
          displayName: x.medication?.displayName,
          displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
          displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName : '',
          displayDose: x.dose,
          displayDoseUnit: x.doseUnit ? x.doseUnit.printName : '',
          isComboMed: x.medication?.medicationDetails.length > 1 ? true : false,
          comboMedDetails: x.medication?.medicationDetails.length > 1 ? x.medication.medicationDetails.map((m) => ({
            brandName: m.brandName,
            dose: m.dose,
            doseUnit: m.doseUnit ? m.doseUnit.printName : ''
          })) : [],
          // allergyReactionsText: x.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
          // drugInteractionsText: x.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
        })),
      }));
    });
  }

  addToCart = (med) => {
    console.log('addToCart from group list: med: ', med);

    this.cartStoreService.postCartOrderByListOrderId(med, med.id, this.patientId, this.userId, this.groups());
    console.log(`addToCart from group list: ${med.id}  name: ${med.medication.displayName} by userId: ${this.userId}`);
    med.hasBeenAdded = true;

    console.log(`addToCart from Group list: ${med.id}  name: ${med.medication.displayName}`);
  };

  editOrder = (med) => {
    // this.modalService.open('medComposer', {
    //   action: 'add',
    //   source: 'groups',
    //   med,
    // });
    this.launchMedComposer(med.id, med);
    console.log(`editOrder from Group list: ${med.medication.displayName}`);
  };

  launchMedComposer(medId: number, medData: object): void {
    this.composerSchedulerService.setInitialComposerData({ action: 'add', source: 'groups', med: medData });
    this.router.navigate(['new-order', medId],
      {
        // state: { data: { medData } },
        queryParams: {},
        relativeTo: this.route
      });
  }
}
