import { Component, OnInit, Input } from '@angular/core';

import { Order } from 'src/app/interfaces/order';
import { MedOrderService } from '../../../../services/med-order.service';
import { ModalService } from 'src/services/modal.service';

@Component({
  selector: 'order-cart-list',
  templateUrl: './order-cart-list.component.html',
  styleUrls: ['./order-cart-list.component.scss']
})
export class OrderCartListComponent implements OnInit {

  displayCartItems: Order[];

  @Input() set items(data) {
    this.displayCartItems = data;
  }

  removeCartItem = (item: Order) => {
    this.medOrdService.removeCartOrder(item)
    console.log('removeCartItem')
  }

  editCartItem = (item: Order) => {
    this.modalService.open('medComposer', {action: 'update', med: item});
  }

  constructor(
    private medOrdService: MedOrderService,
    private modalService: ModalService,
  ) { }

  ngOnInit(): void {
  }

}
