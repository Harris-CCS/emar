import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { forkJoin, Subject } from 'rxjs'
import { takeUntil } from 'rxjs/operators'

import { MedOrderService } from '../../../../services/med-order.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { UserStoreService } from '../../../../services/user-store.service';
import { PatientStoreService } from '../../../../services/patient-store.service';
import { ModalService } from '../../../../services/modal.service';
import { ComposerSchedulerService } from '../../../../services/composer-scheduler.service';


@Component({
  selector: 'dept-preferred-with-tab',
  templateUrl: './dept-preferred-with-tab.component.html',
  styleUrls: ['./dept-preferred-with-tab.component.scss']
})
export class DeptPreferredWithTabComponent implements OnInit, OnDestroy {
  @Output() isTabValid: EventEmitter<boolean> = new EventEmitter();
  private currentTab: string = 'A';
  private tabListTabs = []
  private currentTabContents = []
  private userId = this.userStoreService.userId
  private patientId = this.patientStoreService.patientId

  @Input() auth: boolean;
  ngUnsubscribe = new Subject<void>();
  isLoading: boolean = true

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private medOrderService: MedOrderService,
    private modalService: ModalService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private composerSchedulerService: ComposerSchedulerService,
  ) { 
    console.log('dept-preferred-with-tab component constructor')
  }

  ngOnInit(): void {
    this.getDeptPreferredWithTabList()

    this.medOrderService.refreshRequest_listOrders
      .pipe(takeUntil(this.ngUnsubscribe)) 
      .subscribe( e => {
        this.getDeptPreferredWithTabList();
      });
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  deptPreferredWithTab() {
    return 'deptPreferredWithTab';
  }

  deptPreferredSelectedTab() {
    return this.currentTab;
  }

  deptPreferredTabList() {
    return this.tabListTabs;
  }

  deptPreferredOrders() {
    return this.currentTabContents;
  }

  changeTab = (tab, listType) => {
    console.log('dept-preferred-with-tab: changeDeptPreferredTab: change from: ', this.currentTab);
    this.currentTab = tab;
    console.log('dept-preferred-with-tab: changeDeptPreferredTab: change to: ', this.currentTab);

    const t = this.currentTab === '#' ? '%23' : this.currentTab;
    this.medOrderService.getMedListBySelectedTab(t, listType)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((o) => {
        console.log('dept-preferred-with-tab CHNAGETAB.....', o.length);
        this.currentTabContents = o.map((x) => ({
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
            doseUnit: m.medicationUnit ? m.doseUnit.printName : ''
          })) : [],
        }));
      });
  };

  addToCart = (med) => {
    console.log('addToCart from dept-preferred-with-tab: med: ', med);

    this.cartStoreService.postCartOrderByListOrderId(med, med.id, this.patientId, this.userId, this.deptPreferredWithTab());
    console.log(`addToCart from dept-preferred-with-tab: ${med.id}  name: ${med.medication.displayName} by userId: ${this.userId}`);
    med.hasBeenAdded = true;
  };

  editOrder = (med) => {
    this.launchMedComposer(med.id, med);
    console.log(`editOrder from dept-preferred-with-tab: med id: ${med.id}`);
  };

  launchMedComposer(medId: number, medData: object): void {
    this.composerSchedulerService.setInitialComposerData({ action: 'add', source: 'dept-list', med: medData });
    this.router.navigate(['new-order', medId],
      {
        queryParams: {},
        relativeTo: this.route
      });
  }

  async getDeptPreferredWithTabList() {

    this.isLoading = true
    this.medOrderService.getDeptPreferredOrdersWithTabList()
    .pipe(takeUntil(this.ngUnsubscribe))
    .subscribe((o) => {
      console.log('dept-preferred-with-tab DeptPreferredINITIALTABS....', o);
      this.isLoading = false
      if (!o) {
        this.isTabValid.emit(false);
        return
      }
      this.isTabValid.emit(true);
      this.tabListTabs = o.tabListing.map((x) => x.tabName);
      this.currentTab = o.currentTab.tabName + this.deptPreferredWithTab();
      console.log('dept-preferred-with-tab currentTab....', this.currentTab);
      this.currentTabContents = o.currentTabContents.map((x) => ({
        ...x,
        displayName: x.medication?.displayName,
        displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
        // displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName : '',
        displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName + ( x.frequencySchedule.prn ? ' - ' + x.prnIndication : '' ) : '',
        displayDose: x.dose,
        displayDoseUnit: x.doseUnit ? x.doseUnit.printName : '',
        isComboMed: x.medication?.medicationDetails.length > 1 ? true : false,  // TODO: check the drugId &&
        comboMedDetails: x.medication?.medicationDetails.length > 1 ? x.medication.medicationDetails.map((m) => ({
          brandName: m.brandName,
          dose: m.dose,
          doseUnit: m.doseUnit ? m.doseUnit.printName : ''
        })) : [],
      }));
    });
  }
}
