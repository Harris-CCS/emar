import { Component, OnInit } from '@angular/core';

import { MedOrderService } from '../../../../services/med-order.service';
import { ModalService } from '../../../../services/modal.service';

@Component({
  selector: 'quick-list',
  templateUrl: './quick-list.component.html',
  styleUrls: ['./quick-list.component.scss'],
})
export class QuickListComponent implements OnInit {
  quickList() {
    return 'ql';
  }

  quickListOrders() {
    return this.medOrderService.getQuickListOrders();
  }

  //addToCart = (...args) => console.log(`addToCart from quick list:`, ...args);
  addToCart = (med) => {
    this.medOrderService.postCartOrder(med, this.quickList());
    console.log(`addToCart from quick list: ${med.id}  name: ${med.name}`);
  };

  editOrder = (med) => {
    this.modalService.open('medComposer', { action: 'add', med });
    console.log(`editOrder from quick list: ${med.name}`);
  };

  constructor(
    private medOrderService: MedOrderService,
    private modalService: ModalService
  ) {}

  ngOnInit(): void {}
}
