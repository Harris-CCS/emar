import { Component, OnInit } from '@angular/core';

import { MedOrderService } from '../../../../services/med-order.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { UserStoreService } from '../../../../services/user-store.service';
import { PatientStoreService } from '../../../../services/patient-store.service';

import { ModalService } from '../../../../services/modal.service';

@Component({
  selector: 'groups',
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.scss']
})
export class GroupsComponent implements OnInit {

  private groupPanels = []
  private groupContent= []
  private userId = this.userStoreService.userId
  private patientId = this.patientStoreService.patientId

  constructor(
    private medOrderService: MedOrderService,
    private modalService: ModalService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
  ) { }

  ngOnInit(): void {
    this.getGroupOrdersList()
  }

  groups() {
    return 'groups';
  }

  groupsOrders() {
    // return this.medOrderService.getGroupsOrders();
    // console.log('groupOrders: ', this.groupPanels)
    return this.groupPanels
  }

  getGroupOrdersList() {
    this.medOrderService.getGroupsOrdersList().subscribe((g) => {
      this.groupPanels = g.map((o) => ({
        displayGroupName: o.groupName,
        orders: o.orders.map((i) => ({
          ...i,
          displayName: i.brandName,
          displayRoute: i.medicationRoute ? i.medicationRoute.routeName : '',
          displayFrequency: i.frequencyId,
          displayDose: i.dose,
          displayDoseUnit: i.doseUnit ? i.doseUnit.printName : ''
        })),
      }))
    });
  }

  addToCart = (med) => {
    this.cartStoreService.postCartOrder(med, this.patientId, this.userId, this.groups());
    med.hasBeenAdded = true
    console.log(`addToCart from Group list: ${med.id}  name: ${med.brandName}`);
  }

  editOrder = (med) => {
    this.modalService.open('medComposer', {action: 'add', med});
    console.log(`editOrder from Group list: ${med.brandName}`);
  }
}
