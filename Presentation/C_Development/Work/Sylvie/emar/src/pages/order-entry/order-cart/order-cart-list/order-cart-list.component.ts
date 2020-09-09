import { Component, OnInit, Input, ComponentFactoryResolver } from '@angular/core';

import { Order } from 'src/app/interfaces/order';
// import { MedOrderService } from '../../../../services/med-order.service';

import { ModalService } from 'src/services/modal.service';
import { CartService } from '../../../../services/cart.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { UserStoreService } from '../../../../services/user-store.service';

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

  constructor(
    // private medOrdService: MedOrderService,
    private modalService: ModalService,
    private cartService: CartService,
    public cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
  ) { }

  ngOnInit(): void {
  }

  delete(cartOrderId: number): void {
    // this.cartService.deleteCartOrder(cartOrderId, 6473).subscribe()
    // this.displayCartItems = this.displayCartItems.filter(item => item.id !== cartOrderId)
    this.cartStoreService.deleteCartOrder(cartOrderId, this.userStoreService.userId)

    console.log('deleting...cartOrderId=', cartOrderId, ' userId= ', this.userStoreService.userId)
  }

  deleteAll(patientId: number): void {
    //this.displayCartItems = []
    this.showDeleteAllCartOrderModal(patientId)
    //this.cartService.deleteCartOrder(cartOrderId, 6473).subscribe()
    console.log('deleting ALL...pateintId=', patientId)
  }

  postAll(patientId: number): void {
    // this.cartService.postAllCartOrders(patientId, 6473).subscribe()
    this.cartStoreService.postAllCartOrders(patientId, this.userStoreService.userId)
    console.log('posting... patientId=', patientId, ' userId= ', this.userStoreService.userId)
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
}
