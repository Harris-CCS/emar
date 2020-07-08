import { Component, OnInit } from '@angular/core';

//import { MEDICATIONS } from '../../../../app/mockup/medications';
import { MedOrderService } from '../../../../services/med-order.service';
import { ModalService } from '../../../../services/modal.service';

@Component({
  selector: 'dept-preferred',
  templateUrl: './dept-preferred.component.html',
  styleUrls: ['./dept-preferred.component.scss']
})
export class DeptPreferredComponent implements OnInit {

  constructor(
    private medOrderService: MedOrderService,
    private modalService: ModalService,
  ) { }

  ngOnInit(): void {
  }

  deptPreferred() {
    return 'dp';
  }

  deptPreferredOrders() {
    return this.medOrderService.getDptPreferredOrders();
  }

  addToCart = (med) => {
    this.medOrderService.postCartOrder(med, this.deptPreferred());
    console.log(`addToCart from quick list: ${med.id}  name: ${med.name}`);
  }

  editOrder = (med) => {
    this.modalService.open('medComposer', {action: 'add', med});
    console.log(`editOrder from Dept Preferred list: ${med.name}`);
  }
}
