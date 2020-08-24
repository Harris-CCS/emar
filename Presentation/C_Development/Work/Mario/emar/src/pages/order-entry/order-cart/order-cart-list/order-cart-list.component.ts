import { Component, OnInit, Input, ComponentFactoryResolver } from '@angular/core';

import { Order } from 'src/app/interfaces/order';
// import { MedOrderService } from '../../../../services/med-order.service';

import { ModalService } from 'src/services/modal.service';

import { CartStoreService } from '../../../../services/cart-store.service';
import { CartService } from '../../../../services/cart.service';

@Component({
  selector: 'order-cart-list',
  templateUrl: './order-cart-list.component.html',
  styleUrls: ['./order-cart-list.component.scss']
})
export class OrderCartListComponent implements OnInit {

  // displayCartItems: Order[];

  // optimization - rerenders only cartOrders that change instead of the entire list of cartOrders
  cartOrdersTrackFn = (i, cartOrder) => cartOrder.id

  @Input() patientId
  // @Input() set items(data) {
  //   this.displayCartItems = data || [];
  // }

  delete(cartOrderId: number): void {
    // this.cartService.deleteCartOrder(cartOrderId, 6473).subscribe()
    // this.displayCartItems = this.displayCartItems.filter(item => item.id !== cartOrderId)
    this.cartStoreService.deleteCartOrder(cartOrderId, 5555)

    console.log('deleting...cartOrderId=', cartOrderId, ' userId=5555')
  }

  deleteAll(patientId: number): void {
    //this.displayCartItems = []
    this.showDeleteAllCartOrderModal(patientId)
    //this.cartService.deleteCartOrder(cartOrderId, 6473).subscribe()
    console.log('deleting ALL...pateintId=', patientId)
  }

  postAll(patientId: number): void {
    // this.cartService.postAllCartOrders(patientId, 6473).subscribe()
    this.cartStoreService.postAllCartOrders(patientId, 5555)
    console.log('posting... patientId=', patientId, ' userId=5555')
  }

  showDeleteAllCartOrderModal = (patientId: number) => {
    console.log(`showDeleteAllCartOrderModal - Delete All cart orders? for patientId: ${patientId}`);
    this.modalService.open('deleteAllCartOrder', {patientId});
  }


  

  // removeCartItem = (item: Order) => {
  //   this.medOrdService.removeCartOrder(item)
  //   console.log('removeCartItem')
  // }

  editCartItem = (item: Order) => {
    this.modalService.open('medComposer', {action: 'update', med: item});
  }

  constructor(
    // private medOrdService: MedOrderService,
    private modalService: ModalService,
    public cartStoreService: CartStoreService,
    private cartService: CartService,
  ) { }

  ngOnInit(): void {
  }

}
