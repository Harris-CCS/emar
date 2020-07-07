import { Component, OnInit } from '@angular/core';

import { MedOrderService } from '../../../../services/med-order.service';
import { ModalService } from '../../../../services/modal.service';

@Component({
  selector: 'groups',
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.scss']
})
export class GroupsComponent implements OnInit {

  constructor(
    private medOrderService: MedOrderService,
    private modalService: ModalService,
  ) { }

  ngOnInit(): void {
  }

  groups() {
    return 'groups';
  }

  groupsOrders() {
    return this.medOrderService.getGroupsOrders();
  }

  addToCart = (med) => {
    this.medOrderService.postCartOrder(med, this.groups());
    console.log(`addToCart from Group list: ${med.id}  name: ${med.name}`);
  }

  editOrder = (med) => {
    this.modalService.open('medComposer', {action: 'add', med});
    console.log(`editOrder from Group list: ${med.name}`);
  }
}
