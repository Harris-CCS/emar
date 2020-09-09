import { Component, OnInit } from '@angular/core';

import { MedOrderService } from '../../../../services/med-order.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { UserStoreService } from '../../../../services/user-store.service';
import { PatientStoreService } from '../../../../services/patient-store.service';
import { ModalService } from '../../../../services/modal.service';


import { PatientService } from '../../../../services/patient.service';

@Component({
  selector: 'quick-list',
  templateUrl: './quick-list.component.html',
  styleUrls: ['./quick-list.component.scss'],
})
export class QuickListComponent implements OnInit {

  private currentTab: string = 'A'
  // private tabListTabs = ['Most Used', '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];
  private tabListTabs = []
  private currentTabContents = []
  private userId = this.userStoreService.userId
  private patientId = this.patientStoreService.patientId
  
  constructor(
    private medOrderService: MedOrderService,
    private modalService: ModalService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    //private patientService: PatientService,
  ) {}
  
  ngOnInit(): void {
    this.getQuickListTabList()
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
    return this.currentTabContents
  }

  changeTab = (tab) => {
    console.log('quick-list: changeTab: change from: ', this.currentTab)
    this.currentTab = tab;
    console.log('quick-list: changeTab: change to: ', this.currentTab)

    // this.medOrderService.getMedListBySelectedTab(this.currentTab)
    const t = this.currentTab === '#' ? '%23' : this.currentTab
    this.medOrderService.getMedListBySelectedTab(t).subscribe((o) => {
      console.log('CHNAGETAB.....', o.length)
      this.currentTabContents = o.map((x) => ({
        ...x,
        displayName: x.brandName,
        displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
        displayFrequency: x.frequencyId,
        displayDose: x.dose,
        displayDoseUnit: x.doseUnit ? x.doseUnit.printName : ''
      }))
    })
    //this.quickListOrders()
  }

  //addToCart = (...args) => console.log(`addToCart from quick list:`, ...args);
  addToCart = (med) => {
    // this.medOrderService.postCartOrder(med, this.quickList());
    console.log('addToCart from quick list: med: ', med);

    this.cartStoreService.postCartOrder(med, this.patientId, this.userId, this.quickList())
    console.log(`addToCart from quick list: ${med.id}  name: ${med.brandName}`);
    med.hasBeenAdded = true
  }

  editOrder = (med) => {
    this.modalService.open('medComposer', {action: 'add', med});
    console.log(`editOrder from quick list: ${med.brandName}`);
  }








  getQuickListTabList() {
    //console.log('quickListTabList: ', this.medOrderService.getTabListTabs());
    // let patOne = this.patientService.getPatient(1);
    // console.log('patONE: ', patOne)

    this.medOrderService.getUserQuickLists().subscribe((o) => {
      console.log('QUICKLISTTABS....', o)
      this.tabListTabs = o.tabListing.map( x => x.tabName)
      this.currentTab = o.currentTab.tabName
      this.currentTabContents = o.currentTabContents.map((x) => ({
        ...x,
        displayName: x.brandName,
        displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
        displayFrequency: x.frequencyId,
        displayDose: x.dose,
        displayDoseUnit: x.doseUnit ? x.doseUnit.printName : ''
      }))
    });

    //return this.medOrderService.getTabListTabs();
  }

}
