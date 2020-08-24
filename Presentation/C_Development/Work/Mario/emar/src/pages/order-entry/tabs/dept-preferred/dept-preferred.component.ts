import { Component, OnInit } from '@angular/core';

//import { MEDICATIONS } from '../../../../app/mockup/medications';
import { MedOrderService } from '../../../../services/med-order.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { ModalService } from '../../../../services/modal.service';

@Component({
  selector: 'dept-preferred',
  templateUrl: './dept-preferred.component.html',
  styleUrls: ['./dept-preferred.component.scss']
})
export class DeptPreferredComponent implements OnInit {

  private listContents = []

  constructor(
    private medOrderService: MedOrderService,
    private modalService: ModalService,
    private cartStoreService: CartStoreService,
  ) { }

  ngOnInit(): void {
    this.getDeptPreferredOrdersList()
  }

  deptPreferred() {
    return 'dp';
  }

  deptPreferredOrders() {
    // return this.medOrderService.getDeptPreferredOrdersList();
    return this.listContents
  }

  getDeptPreferredOrdersList() {
    this.medOrderService.getDeptPreferredOrdersList().subscribe((o) => {
      this.listContents = o.map((x) => ({
        ...x,
        displayName: x.brandName,
        displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
        displayFrequency: x.frequencyId,
        displayDose: x.dose,
        displayDoseUnit: x.doseUnit ? x.doseUnit.printName : ''
      }))
    });
  }

  addToCart = (med) => {
    console.log('addToCart from dept preferred list: med: ', med);

    // this.medOrderService.postCartOrder(med, this.deptPreferred());
    this.cartStoreService.postCartOrder(med, 1, 5555, this.deptPreferred())
    console.log(`addToCart from dept preferred list: ${med.id}  name: ${med.brandName}`);
    med.hasBeenAdded = true
  }

  editOrder = (med) => {
    this.modalService.open('medComposer', {action: 'add', med});
    console.log(`editOrder from Dept Preferred list: ${med.brandName}`);
  }
}
