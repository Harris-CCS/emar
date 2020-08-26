import { Component, OnInit, Output, EventEmitter } from '@angular/core';

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

  @Output() isTabValid: EventEmitter<boolean> = new EventEmitter()
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
    this.medOrderService.getDeptPreferredOrdersList().subscribe((resp) => {
      if (!resp || resp.length === 0) {
        this.isTabValid.emit(false)
      } else {
        console.log('dept pref has data, resp:', resp)
        this.isTabValid.emit(true)
        this.listContents = resp.map((o) => ({
          ...o,
          displayName: o.brandName,
          displayRoute: o.medicationRoute ? o.medicationRoute.routeName : '',
          displayFrequency: o.frequencyId,
          displayDose: o.dose,
          displayDoseUnit: o.doseUnit ? o.doseUnit.printName : ''
        }))
      }
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
