import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs'
import { takeUntil } from 'rxjs/operators'

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
export class GroupsComponent implements OnInit, OnDestroy {
  private groupPanels = [];
  private groupContent = [];

  // private groupPanels = []
  // private groupContent= []
  private userId = this.userStoreService.userId;
  private patientId = this.patientStoreService.patientId;
  @Input() auth: boolean;
  @Input() select: boolean; // is the group tab selected
  @Input() selectedGroup: string; // the name of the group if come back from composer from this group
  ngUnsubscribe = new Subject<void>();
  isLoading: boolean = false

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
    if (this.select) { // load groups list only if the tab is current
      this.loadTab();
    }       
    this.medOrderService.refreshRequest_listOrders
      .pipe(takeUntil(this.ngUnsubscribe))  
      .subscribe( e => {
        // console.log('++++subscribe refreshRequest_listOrders - REFRESH GroupOrdersList e: ', e);
        this.getGroupNameOrderList();        
      });
  }

  loadTab(): void {
    this.getGroupNameOrderList();
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  groups() {
    return 'groups';
  }

  groupsOrders() {
    // return this.medOrderService.getGroupsOrders();
    // console.log('groupOrders: ', this.groupPanels)
    return this.groupPanels;
  }

  // old version to get group and detail at the same time - no more used
  getGroupOrdersList() {
    this.isLoading = true
    this.medOrderService.getGroupsOrdersList()
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((g) => {
        this.groupPanels = g.map((o) => ({
          displayGroupName: o.groupName,
          orders: o.orders.map((x) => ({
            ...x,
            displayName: x.medication?.displayName,
            displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
            // displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName : '',
            displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName + ( x.frequencySchedule.prn ? ' - ' + x.prnIndication : '' ) : '',
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
        this.isLoading = false
      });
  }

  // new version of getGroupOrdersList to get only the group names
  getGroupNameOrderList() {
    this.isLoading = true;
    this.medOrderService.getGroupNameList()
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe( (groups:string[]) => {
        this.groupPanels = groups.map( (group:string) => ({
          displayGroupName: group,
          orders: []
        }));
        if (this.selectedGroup) {
          const iGroup = this.groupPanels.findIndex( elt => elt.displayGroupName == this.selectedGroup);
          this.medOrderService.getGroupDetail(this.selectedGroup)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe ( (group) => {
              if (iGroup >= 0)
                this.groupPanels[iGroup].orders = group.orders;
          });
        }
        this.isLoading = false;
      }
    );
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
