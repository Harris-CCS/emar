import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { forkJoin, Subject } from 'rxjs'
import { takeUntil } from 'rxjs/operators'

import { MedOrderService } from '../../../../services/med-order.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { UserStoreService } from '../../../../services/user-store.service';
import { PatientStoreService } from '../../../../services/patient-store.service';
import { ModalService } from '../../../../services/modal.service';
import { ComposerSchedulerService } from '../../../../services/composer-scheduler.service';

import { PatientService } from '../../../../services/patient.service';


@Component({
  selector: 'quick-list',
  templateUrl: './quick-list.component.html',
  styleUrls: ['./quick-list.component.scss'],
})
export class QuickListComponent implements OnInit, OnDestroy {
  private currentTab: string = 'A';
  // private tabListTabs = ['Most Used', '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];
  private tabListTabs = []
  private currentTabContents = []
  private userId = this.userStoreService.userId
  private patientId = this.patientStoreService.patientId

  @Input() auth: boolean;
  @Input() select: boolean;
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
    //private patientService: PatientService,
  ) {
      console.log('quick-list component constructor')
      // forkJoin([this.userStoreService.user$.toPromise(), this.patientStoreService.patient$.toPromise()]).subscribe( async () => {

      //   if ( this.userStoreService.userId && this.patientStoreService.patientId) {
      //   }
      // })
      // this.getQuickListTabList()
  }
  
  ngOnInit(): void {
    if (this.select) { // get the content only if it is the current tab
      this.loadTab();
    }
    this.medOrderService.refreshRequest_listOrders
      .pipe(takeUntil(this.ngUnsubscribe)) 
      .subscribe( e => {
        // console.log('++++subscribe refreshRequest_listOrders - REFRESH QuickList e: ', e);
        this.getQuickListTabList();
      });
  }

  loadTab(): void {
    this.getQuickListTabList();
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  quickList() {
    return 'quicklist';
  }

  quickListSelectedTab() {
    return this.currentTab;
  }

  quickListTabList() {
    //return this.medOrderService.getTabListTabs();
    return this.tabListTabs;
  }

  quickListOrders() {
    //return this.medOrderService.getQuickListOrders();
    // return this.medOrderService.getMedListBySelectedTab(this.currentTab);
    return this.currentTabContents;
  }

  // call when clicking on a letter
  changeTab = (tab, listType) => {
    console.log('quick-list: changeTab: change from: ', this.currentTab);
    this.currentTab = tab;
    console.log('quick-list: changeTab: change to: ', this.currentTab);

    // this.medOrderService.getMedListBySelectedTab(this.currentTab)
    const t = this.currentTab === '#' ? '%23' : this.currentTab;
    this.medOrderService.getMedListBySelectedTab(t, listType)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((o) => {
        console.log('CHNAGETAB.....', o.length);
        this.currentTabContents = o.map((x) => ({
          ...x,
          displayName: x.medication?.displayName,
          displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
          // displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName : '',
          displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName + ( x.frequencySchedule.prn ? ' - ' + x.prnIndication : '' ) : '',
          displayDose: x.dose,
          displayDoseUnit: x.doseUnit ? x.doseUnit.printName : '',
					displayDuration: (x.duration && x.durationUnit && x.durationUnit.name) ? ' for ' + x.duration + ' ' + x.durationUnit.name : '',
          isComboMed: x.medication?.medicationDetails.length > 1 ? true : false,
          comboMedDetails: x.medication?.medicationDetails.length > 1 ? x.medication.medicationDetails.map((m) => ({
            brandName: m.brandName,
            dose: m.dose,
            doseUnit: m.medicationUnit ? m.doseUnit.printName : ''
          })) : [],
          // allergyReactionsText: x.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
          // drugInteractionsText: x.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
        }));
      });
    //this.quickListOrders()
  };

  //addToCart = (...args) => console.log(`addToCart from quick list:`, ...args);
  addToCart = (med) => {
    // this.medOrderService.postCartOrder(med, this.quickList());
    console.log('addToCart from quick list: med: ', med);

    // this.cartStoreService.postCartOrder(med, this.patientId, this.userId, this.quickList())
    // this.medOrderService.postCartOrderByListOrderId(med.id, this.patientId, this.userId);
    this.cartStoreService.postCartOrderByListOrderId(med, med.id, this.patientId, this.userId, this.quickList());
    console.log(`addToCart from quick list: ${med.id}  name: ${med.medication.displayName} by userId: ${this.userId}`);
    med.hasBeenAdded = true;
  };

  editOrder = (med) => {
    // this.modalService.open('medComposer', {
    //   action: 'add',
    //   source: 'quick-list',
    //   med,
    // });
    this.launchMedComposer(med.id, med);
    console.log(`editOrder from quick list: med id: ${med.id}`);
  };

  launchMedComposer(medId: number, medData: object): void {
    this.composerSchedulerService.setInitialComposerData({ action: 'add', source: 'quick-list', med: medData });
    this.router.navigate(['new-order', medId],
      {
        // state: { data: { medData } },
        queryParams: {},
        relativeTo: this.route
      });
  }

  // get the list of quicklist
  async getQuickListTabList() {
    //console.log('quickListTabList: ', this.medOrderService.getTabListTabs());
    // let patOne = this.patientService.getPatient(1);
    // console.log('patONE: ', patOne)

    // this.userStoreService.fetchUser()
    this.isLoading = true
    this.medOrderService.getUserQuickLists().subscribe((o) => {
      console.log('QUICKLISTTABS....', o);
      this.isLoading = false
      if (!o) {
        return
      }
      this.tabListTabs = o.tabListing.map((x) => x.tabName);
      this.currentTab = o.currentTab.tabName + this.quickList();
      this.currentTabContents = o.currentTabContents.map((x) => ({
        ...x,
        displayName: x.medication?.displayName,
        displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
        displayFrequency: x.frequencySchedule ? x.frequencySchedule.scheduleName + ( x.frequencySchedule.prn ? ' - ' + x.prnIndication : '' ) : '',
				displayDuration: (x.duration && x.durationUnit && x.durationUnit.name) ? ' for ' + x.duration + ' ' + x.durationUnit.name : '',				
        displayDose: x.dose,
        displayDoseUnit: x.doseUnit ? x.doseUnit.printName : '',
        isComboMed: x.medication?.medicationDetails.length > 1 ? true : false,  // TODO: check the drugId &&
        comboMedDetails: x.medication?.medicationDetails.length > 1 ? x.medication.medicationDetails.map((m) => ({
          brandName: m.brandName,
          dose: m.dose,
          doseUnit: m.doseUnit ? m.doseUnit.printName : ''
        })) : [],
        // allergyReactionsText: x.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
        // drugInteractionsText: x.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
      }));
      // console.log('CurrentTabContents: ', this.currentTabContents)
    });
    //return this.medOrderService.getTabListTabs();
  }
}
