import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { shareReplay, map } from 'rxjs/operators';

import { CartService } from './cart.service';
import { OrderCartListComponent } from 'src/pages/order-entry/order-cart/order-cart-list/order-cart-list.component';

import { CartOrder } from '../app/interfaces/cart-order';
import { uuid } from '../shared/functions/uuid';

interface Order {
  id: number
}

@Injectable({
  providedIn: 'root'
})
export class CartStoreService {

  constructor( private cartService: CartService ) {
    this.fetchAll()
  }

  private readonly _cart = new BehaviorSubject<any>({});
  readonly cart$ = this._cart.asObservable();
  readonly cartLinks$ = this.cart$.pipe(
    map(carts => carts.links ? carts.links : [])
  )
  readonly cartOrders$ = this.cart$.pipe(
    map(carts => (carts && carts.orders) ? carts.orders.map((ord) => ({
      ...ord,
      displayName: ord.brandName,
      displayRoute: ord.medicationRoute ? ord.medicationRoute.routeName : '',
      displayFrequency: ord.frequencyId,
      displayDose: ord.dose,
      displayDoseUnit: ord.doseUnit ? ord.doseUnit.printName : '',
      allergies: [],
      drugs: []
    })) : [])
  )

  readonly totalCount$ = this.cart$.pipe(
    map(carts => carts && carts.xPagination ? carts.xPagination.totalCount : 0)
  )

  /* GETTER - returns the last value emitted in _cart subject */
  get cart(): {} {
    console.log('CARTSTORE GETTER: ', this._cart.getValue())
    return this._cart.getValue() || {};
  }

  get cartOrders(): Order[] {
    const value = this._cart.getValue() || {orders: []}
    console.log('CARTSTORE GETTER: cartOrders', value.orders)
    return value.orders
  }

  get totalCount(): number {
    const value = this._cart.getValue()
    // || {xPagination: {totalCount: 0}}
    console.log('CARTSTORE GETTER: totalCount: ', value.xPagination?.totalCount)
    return value.xPagination?.totalCount || 0
  }

  /* SETTER - set value to this.cart= and push it onto the observable and down to all of its subscribers */
  set cart(val: {}) {
    this._cart.next(val)
  }

  set cartOrders(val: Order[]) {
    const cart = this._cart.getValue() || {}
    cart.orders = val

    this._cart.next(cart)
  }

  set totalCount(val: number) {
    const cart = this._cart.getValue()

    cart.xPagination = {
      ...cart.xPagination,
      totalCount: val,
    }

    this._cart.next(cart)
  }

  /* POST */
  async postCartOrder(order: CartOrder, patientId: number, userId: number, listType?: string) {
  // async postCartOrder(order: {}, patientId: number, userId: number, listType?: string) {
    console.log('CartStore: POST')

    let ord: CartOrder = {
      patientId: patientId,
      userId: userId,
      addDatetime: "2020-08-14T22:01:53.589Z",
      //addDate: "2020-08-14",
      //addTime: "22:01:53",
      priority: 2,
      prn: true,
      beginDatetime: "2020-08-14T22:01:53.589Z",
      //beginDate: "2020-08-14",
      //beginTime: "22:01:53",
      endDatetime: "2020-08-14T22:01:53.589Z",
      userQuickListItemId: 0,
      cartOrderAdministrations: [
        {
          id: 0,
          patientCartOrderId: 0,
          administrationScheduledDatetime: "2020-08-14T22:01:53.589Z",
          administrationScheduledDate: "2020-08-14",
          administrationScheduledTime: "22:01:53",
          stopScheduledDatetime: "2020-08-14T22:01:53.589Z",
          stopScheduledDate: "2020-08-14",
          stopScheduledTime: "22:01:53",
          pointInTime: true
        }
      ],
      id: 0,
      ndc: "string",
      drugId: "string",
      brandName: order.brandName,
      dose: 2,
      //doseUnit: "ea",
      medicationUnitId: 0,
      frequencyId: 0,
      pointInTime: true,
      orderNotes: "string",
      medicationRouteId: 0
    };
    
    //optimistic update
    const tempId = uuid()
    
    const tempCartOrder = {
      ...ord
    }
    
    tempCartOrder.id = tempId
    
    const orders = [
      ...this.cartOrders
    ]

    this.cartOrders = [
      tempCartOrder,
      ...this.cartOrders
    ]

    this.totalCount = this.totalCount + 1

    try {
      console.log('CartStore: POSTED ALL 1: patientId: ', patientId, ' userId: ', userId)

      const cartOrder = await this.cartService.postCartOrder(ord, patientId, userId).toPromise()

      //reload the cart order from database
      const idx = this.cartOrders.indexOf(this.cartOrders.find( o => 
        typeof o.id ==='string' &&  o.id === tempId
      ))

      this.cartOrders[idx] = {
        ...cartOrder
      }
      this.cartOrders = [...this.cartOrders]
      console.log('CartStore: POSTED & UPDATED')
    } catch (e) {
      console.log('CartStore: POST ERROR: ', e)
      this.cartOrders = orders
      this.totalCount = this.totalCount - 1
    }
  }

  /* POST ALL (CHECKOUT) */
  async postAllCartOrders(patientId: number, userId: number) {
    console.log('CartStore: POST ALL')

    //optimistic update
    const orders = [...this.cartOrders]
    const count = this.totalCount
    this.cartOrders = []
    this.totalCount = 0

    try {
      await this.cartService.postAllCartOrders(patientId, userId).toPromise()
      console.log('CartStore: POSTED ALL')
    } catch (e) {
      console.log('CartStore: POST ALL ERROR: ', e)
      this.cartOrders = orders
      this.totalCount = count
    }
  }

  /* DELETE */
  async deleteCartOrder(cartOrderId: number, userId: number) {
    console.log('CartStore: DELETE')

    //optimistic update
    const orders = [...this.cartOrders]
    this.cartOrders = this.cartOrders.filter(o => o.id !== cartOrderId)
    const count = this.totalCount
    this.totalCount = this.totalCount - 1
    
    try {
      await this.cartService.deleteCartOrder(cartOrderId,userId).toPromise()
      console.log('CartStore: DELETED')
    } catch (e) {
      console.log('CartStore: DELETE ERROR: ', e)
      this.cartOrders = orders
      this.totalCount = count
    }
  }

  /* DELETE ALL*/
  async deleteAllCartOrders(patientId: number, userId: number) {
    console.log('CartStore: DELETE ALL')

    //optimistic update
    const orders = [...this.cartOrders]
    this.cartOrders = []
    const count = this.totalCount
    this.totalCount = 0

    try {
      await this.cartService.deleteAllCartOrders(patientId, userId).toPromise()
      console.log('CartOrder: DELETED ALL')
    } catch (e) {
      console.log('CartStore: DELETE ALL ERROR: ', e)
      this.cartOrders = orders
      this.totalCount = count
    }
  }

  /* UPDATE */
  async updateCartOrder(order: CartOrder, patientId: number, userId: number, listType?: string) {
    console.log('CartStore: UPDATE')
    console.log('CartStore: UPDATE: order: ', order)

    let ord: CartOrder = {
      patientId: patientId,
      userId: userId,
      addDatetime: "2020-08-14T22:01:53.589Z",
      //addDate: "2020-08-14",
      //addTime: "22:01:53",
      priority: 2,
      prn: true,
      beginDatetime: "2020-08-14T22:01:53.589Z",
      //beginDate: "2020-08-14",
      //beginTime: "22:01:53",
      endDatetime: "2020-08-14T22:01:53.589Z",
      userQuickListItemId: 0,
      cartOrderAdministrations: [
        {
          id: 0,
          patientCartOrderId: 0,
          administrationScheduledDatetime: "2020-08-14T22:01:53.589Z",
          administrationScheduledDate: "2020-08-14",
          administrationScheduledTime: "22:01:53",
          stopScheduledDatetime: "2020-08-14T22:01:53.589Z",
          stopScheduledDate: "2020-08-14",
          stopScheduledTime: "22:01:53",
          pointInTime: true
        }
      ],
      id: order.id,
      ndc: "string",
      drugId: "string",
      brandName: order.brandName,
      dose: 120,
      //doseUnit: "ea",
      medicationUnitId: 0,
      frequencyId: 0,
      pointInTime: true,
      orderNotes: "string",
      medicationRouteId: 0
    };

    console.log('CartStore: UPDATE: ord: ', ord)
    //optimistic update
    const orders = [...this.cartOrders]
    const idx = this.cartOrders.indexOf(this.cartOrders.find( o => o.id === order.id))
    console.log('CartStore: idx: ', idx)
    this.cartOrders[idx] = {
      ...ord
    }

    this.cartOrders = [...this.cartOrders]

    try {
      await this.cartService.updateCartOrder(ord, patientId, userId).toPromise()
      console.log('CartStore: UPDATED')
    } catch (e) {
      console.log('CartStore: UPDATE ERROR: ', e)
      this.cartOrders = orders
    }
  }

  async fetchAll() {
    this.cart = await this.cartService.getCartOrders(1, 5555).toPromise();
    // const response = await this.cartService.getCartOrders(1, 6473).toPromise();
    // this.cart = response

    // const xPagination = JSON.parse(response.headers.get('X-Pagination'))
    // this.totalCount = xPagination.totalCount

    console.log('CartStore - fetch all: cart: ', this.cart)
    // console.log('CartStore - fetch all: cart XXXXXX X-Pagination: ', response.headers.get('X-Pagination'))
    // console.log('CartStore - fetch all: cart XXXXXX X-Pagination obj: ', xPagination)

    // console.log('CartStore - fetch all: status: ', response.status)
    // console.log('CartStore - fetch all: body: ', response.body)
  }
}
