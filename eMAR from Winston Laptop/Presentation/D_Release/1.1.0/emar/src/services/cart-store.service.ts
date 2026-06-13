import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { shareReplay, map } from 'rxjs/operators';

import { UserStoreService } from '../services/user-store.service';
import { PatientStoreService } from '../services/patient-store.service';
import { PatientMedOrderStoreService } from '../services/patient-med-order-store.service';
import { CartService } from './cart.service';
import { MedOrderService } from './med-order.service';
// import { OrderCartListComponent } from 'src/pages/order-entry/order-cart/order-cart-list/order-cart-list.component';

import { CartOrder } from '../app/interfaces/cart-order';
import { uuid } from '../shared/functions/uuid';
import { async } from 'rxjs/internal/scheduler/async';

interface Order {
  id: number,
  isDisabled?: boolean,
}

@Injectable({
  providedIn: 'root'
})
export class CartStoreService {

  constructor(
    private cartService: CartService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private patientMedOrderStoreService: PatientMedOrderStoreService,
    private medOrderService: MedOrderService,
  ) {
    console.log('CartStoreService constructor')
    // this.fetchAll()
    // this.fetchPatientCartOrders(this.patientId, this.userId)

    this.patientStoreService.patient$.subscribe(async () => {
      console.log('after subscribe for patient store service', this.patientStoreService.patientId)

      if (this.patientStoreService.patientId) {
        await this.fetchPatientCartOrders(this.patientStoreService.patientId, this.userId)
      }
      console.log('after after subscribe for patient store service')
    })
  }

  private userId = this.userStoreService.userId
  // private patientId = this.patientStoreService.patientId
  private readonly _cart = new BehaviorSubject<any>({});
  readonly cart$ = this._cart.asObservable();
  readonly cartLinks$ = this.cart$.pipe(
    map(carts => carts.links ? carts.links : [])
  )
  readonly cartOrders$ = this.cart$.pipe(
    map(carts => (carts && carts.orders) ? carts.orders.map((ord) => ({
      ...ord,
      // displayName: ord.brandName,
      displayName: ord.medication?.displayName,
      displayRoute: ord.medicationRoute ? ord.medicationRoute.routeName : '',
      displayFrequency: ord.frequencySchedule ? ord.frequencySchedule.scheduleName : '',
      displayDose: ord.dose,
      displayDoseUnit: ord.doseUnit ? ord.doseUnit.printName : '',
      // allergyReactionsText: ord.allergyReactions?.map((alg) => alg.orderBrandName).join(', '),

      // allergyReactionsText: ord.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
      // drugInteractionsText: ord.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
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
    const value = this._cart.getValue() || { orders: [] }
    console.log('CARTSTORE GETTER: cartOrders', value.orders)
    return value.orders
  }

  get totalCount(): number {
    const value = this._cart.getValue()
    // || {xPagination: {totalCount: 0}}
    console.log('CARTSTORE GETTER: totalCount: ', value.xPagination?.totalCount)
    return value.xPagination?.totalCount || 0
  }

  // get patientId(): number {
  //   return this.patientStoreService.patientId
  // }

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

    // let ord: CartOrder = {
    //   patientId: patientId,
    //   userId: userId,
    //   addDatetime: "2020-09-23T00:30:00-04:00",
    // addDate: "2020-08-14",
    // addTime: "22:01:53",
    // priority: 1,
    // prn: false,
    // beginDatetime: "2020-09-23T01:00:00+00:00",
    // beginDate: "2020-08-14",
    // beginTime: "22:01:53",
    // endDatetime: null,
    // userQuickListItemId: null,
    // medicationId: order.medicationId,
    // cartOrderAdministrations: [
    //   {
    //     administrationScheduledDatetime: "2020-09-23T01:00:00+00:00",
    //     stopScheduledDatetime: null,
    //     pointInTime: true
    //   }
    // ],
    // id: 0,
    // ndc: null,
    // drugId: "drug888",
    // brandName: order.brandName,
    // medication: {
    //   id: order.medication.id,
    //   site: order.medication.site,
    //   drugId: order.medication.drugId,
    //   displayName: order.medication.displayName,
    //   drugVendor: order.medication.drugVendor,
    //   medicationDetails: [
    //     {
    //       id: order.medication.medicationDetails[0].id,
    //       medicationId: order.medication.medicationDetails[0].medicationId,
    //       drugId: order.medication.medicationDetails[0].drugId,
    //       brandName: order.medication.medicationDetails[0].brandName,
    //       activeList: order.medication.medicationDetails[0].activeList,
    //       dose: order.medication.medicationDetails[0].dose,
    //       medicationUnitId: order.medication.medicationDetails[0].medicationUnitId,
    //       medicationRouteId: order.medication.medicationDetails[0].medicationRouteId,
    //       isActive: order.medication.medicationDetails[0].isActive
    //     }
    //   ]
    // },
    // dose: 2,
    // doseUnit: "ea",
    // medicationUnitId: null,
    //   frequencyId: null,
    //   pointInTime: false,
    //   orderNotes: "Hello, I am a test",
    //   medicationRouteId: null
    // };
    // order.addDatetime = "2020-09-23T00:30:00+00:00";

    // optimistic update
    const tempId = uuid()

    console.log('CartStore: POSTED ALL order: ', {...order})
    const tempCartOrder = {
      // ...ord
      ...order,
      isDisabled: true,
    }

    tempCartOrder.id = tempId;

    const orders = [
      ...this.cartOrders || []
    ]

    this.cartOrders = [
      tempCartOrder,
      ...this.cartOrders || []
    ]

    this.totalCount = this.totalCount + 1;

    try {
      console.log('CartStore: POSTED ALL 1: patientId: ', patientId, ' userId: ', userId);

      // const cartOrder = await this.cartService.postCartOrder(ord, patientId, userId).toPromise()
      const cartOrder = await this.cartService.postCartOrder(order, patientId, userId).toPromise();
      console.log('CartStore: POSTED', cartOrder);

      // reload the cart order from database
      const idx = this.cartOrders.indexOf(this.cartOrders.find(o =>
        typeof o.id === 'string' && o.id === tempId
      ));

      this.cartOrders[idx] = {
        ...cartOrder,
        isDisabled: false,
      };
      this.cartOrders = [...this.cartOrders]
      console.log('CartStore: POSTED & UPDATED')

      // We need to reload the entired cart order to get latest reaction/interaction checking results
      this.fetchPatientCartOrders(this.patientStoreService.patientId, this.userId)
      console.log('CartStore: POSTED & UPDATED & RELOAD CART ORDERS completed..')

    } catch (err) {
      console.log('CartStore: POST ERROR: ', err)

      alert(`POST ${err.status} ${err.statusText}\n${err.error}`)
      this.cartOrders = orders
      this.totalCount = this.totalCount - 1;
    }
  }

  /* POST - post a cart order by UserQuickListItemId*/
  async postCartOrderByListOrderId(order: CartOrder, listOrderId: number, patientId: number, userId: number, listType?: string) {
    // async postCartOrder(order: {}, patientId: number, userId: number, listType?: string) {
    console.log('CartStore: POST - postCartOrderByListOrderId listType: ', listType, '  listOrderId:', listOrderId)

    // TODO - do not need whole ord here.  only need med name dosage etc to display in the cart
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
      // brandName: order.brandName,
      medication: {
        id: order.medication.id,
        site: order.medication.site,
        drugId: order.medication.drugId,
        displayName: order.medication.displayName,
        drugVendor: order.medication.drugVendor,
        medicationDetails: [
          {
            id: order.medication.medicationDetails[0].id,
            medicationId: order.medication.medicationDetails[0].medicationId,
            drugId: order.medication.medicationDetails[0].drugId,
            brandName: order.medication.medicationDetails[0].brandName,
            activeList: order.medication.medicationDetails[0].activeList,
            dose: order.medication.medicationDetails[0].dose,
            medicationUnitId: order.medication.medicationDetails[0].medicationUnitId,
            medicationRouteId: order.medication.medicationDetails[0].medicationRouteId,
            isActive: order.medication.medicationDetails[0].isActive
          }
        ]
      },
      dose: 2,
      //doseUnit: "ea",
      medicationUnitId: 0,
      frequencyId: 0,
      pointInTime: true,
      orderNotes: "string",
      medicationRouteId: 0,
      isDisabled: true,
    };

    //optimistic update
    const tempId = uuid()

    const tempCartOrder = {
      ...ord
    }

    tempCartOrder.id = tempId

    const orders = [
      ...this.cartOrders || []
    ]

    this.cartOrders = [
      tempCartOrder,
      ...this.cartOrders || []
    ]

    this.totalCount = this.totalCount + 1

    try {
      console.log('CartStore: POSTED 1: patientId: ', patientId, ' userId: ', userId)

      // const cartOrder = await this.cartService.postCartOrder(ord, patientId, userId).toPromise()
      // const cartOrder = await this.medOrderService.postCartOrderByListOrderId(listOrderId, patientId, userId).toPromise()

      let cartOrder: CartOrder
      if (listType === 'quicklist') {
        cartOrder = await this.medOrderService.postCartOrderByListOrderId(listOrderId, patientId, userId).toPromise()
      } else if (listType === 'deptPreferredWithTab') {
        cartOrder = await this.medOrderService.postCartOrderByDeptPreferredListOrderId(listOrderId, patientId, userId).toPromise()
      } else if (listType === 'groups') {
        cartOrder = await this.medOrderService.postCartOrderByGroupsListOrderId(listOrderId, patientId, userId).toPromise()
      }

      //reload the cart order from database
      const idx = this.cartOrders.indexOf(this.cartOrders.find(o =>
        typeof o.id === 'string' && o.id === tempId
      ))

      this.cartOrders[idx] = {
        ...cartOrder,
        isDisabled: false,
      }
      this.cartOrders = [...this.cartOrders]
      console.log('CartStore: POSTED & UPDATED')

      // We need to reload the entired cart order to get latest reaction/interaction checking results
      this.fetchPatientCartOrders(this.patientStoreService.patientId, this.userId)
      console.log('CartStore: POSTED & UPDATED & RELOAD CART ORDERS completed')

    } catch (err) {
      console.log('CartStore: POST ERROR >= 400: ', err)

      alert(`POST ${err.status} ${err.statusText}\n${err.error}`)
      this.cartOrders = orders
      this.totalCount = this.totalCount - 1
    }
  }

  /* POST ALL (CHECKOUT) */
  async postAllCartOrders(patientId: number, userId: number, data: any) {
    console.log('CartStore: POST ALL')

    //optimistic update
    const orders = [...this.cartOrders]
    const count = this.totalCount
    this.cartOrders = []
    this.totalCount = 0

    try {
      await this.cartService.postAllCartOrders(patientId, userId, data).toPromise()
      console.log('CartStore: POSTED ALL')

      //update patient current order
      await this.patientMedOrderStoreService.fetchPatientMedOrder(patientId)
      console.log('CartStore: patientMedOrderStoreService: fetchPatientMedOrder')
    } catch (err) {
      console.log('CartStore: POST ALL ERROR (CHECKOUT) >= 400: ', err)

      alert(`POST ${err.status} ${err.statusText}\n${err.error}`)

      this.cartOrders = orders
      this.totalCount = count

      throw err
    }
  }

  /* DELETE */
  async deleteCartOrder(cartOrderId: number, userId: number) {
    console.log('CartStore: DELETE start...')

    //optimistic update
    // const orders = [...this.cartOrders]
    this.cartOrders = this.cartOrders.map(o => {
      if (o.id === cartOrderId) {
        o = {
          ...o,
          isDisabled: true
        }
      }

      return o
    })
    // const count = this.totalCount
    // this.totalCount = this.totalCount - 1

    try {
      await this.cartService.deleteCartOrder(cartOrderId, userId).toPromise()

      // We need to reload the entired cart order to get latest reaction/interaction checking results
      this.fetchPatientCartOrders(this.patientStoreService.patientId, this.userId)
      this.medOrderService.refreshRequest_listOrders.emit(true);
      console.log('CartStore: DELETED & UPDATED & RELOAD CART ORDERS completed')

    } catch (err) {
      console.log('CartStore: DELETE ERROR >= 400: ', err)

      alert(`DELETE ${err.status} ${err.statusText}\n${err.error}`)

      // this.cartOrders = orders
      // this.totalCount = count
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
      // return await this.cartService.deleteAllCartOrders(patientId, userId).toPromise()
      await this.cartService.deleteAllCartOrders(patientId, userId).toPromise()
      this.medOrderService.refreshRequest_listOrders.emit(true);

    } catch (err) {
      console.log('CartStore: DELETE ALL catch ERROR >= 400: ', err)

      this.cartOrders = orders
      this.totalCount = count

      throw err
    }
  }

  /* UPDATE */
  async updateCartOrder(order: CartOrder, patientId: number, userId: number, listType?: string) {
    console.log('CartStore: UPDATE')
    console.log('CartStore: UPDATE: order: ', order)

    // let ord: CartOrder = {
    //   patientId: patientId,
    //   userId: userId,
    //   addDatetime: "2020-08-14T22:01:53.589Z",
    //   //addDate: "2020-08-14",
    //   //addTime: "22:01:53",
    //   priority: 2,
    //   prn: true,
    //   beginDatetime: "2020-08-14T22:01:53.589Z",
    //   //beginDate: "2020-08-14",
    //   //beginTime: "22:01:53",
    //   endDatetime: "2020-08-14T22:01:53.589Z",
    //   userQuickListItemId: 0,
    //   cartOrderAdministrations: [
    //     {
    //       id: 0,
    //       patientCartOrderId: 0,
    //       administrationScheduledDatetime: "2020-08-14T22:01:53.589Z",
    //       administrationScheduledDate: "2020-08-14",
    //       administrationScheduledTime: "22:01:53",
    //       stopScheduledDatetime: "2020-08-14T22:01:53.589Z",
    //       stopScheduledDate: "2020-08-14",
    //       stopScheduledTime: "22:01:53",
    //       pointInTime: true
    //     }
    //   ],
    //   id: order.id,
    //   ndc: "string",
    //   drugId: "string",
    //   // brandName: order.brandName,
    //   medication: {
    //     id: order.medication.id,
    //     site: order.medication.site,
    //     drugId: order.medication.drugId,
    //     displayName: order.medication.displayName,
    //     drugVendor: order.medication.drugVendor,
    //     medicationDetails: [
    //       {
    //         id: order.medication.medicationDetails[0].id,
    //         medicationId: order.medication.medicationDetails[0].medicationId,
    //         drugId: order.medication.medicationDetails[0].drugId,
    //         brandName: order.medication.medicationDetails[0].brandName,
    //         activeList: order.medication.medicationDetails[0].activeList,
    //         dose: order.medication.medicationDetails[0].dose,
    //         medicationUnitId: order.medication.medicationDetails[0].medicationUnitId,
    //         medicationRouteId: order.medication.medicationDetails[0].medicationRouteId,
    //         isActive: order.medication.medicationDetails[0].isActive
    //       }
    //     ]
    //   },
    //   dose: 120,
    //   //doseUnit: "ea",
    //   medicationUnitId: 0,
    //   frequencyId: 0,
    //   pointInTime: true,
    //   orderNotes: "string",
    //   medicationRouteId: 0
    // };

    console.log('CartStore: UPDATE: order: ', order)
    //optimistic update
    const orders = [...this.cartOrders]
    const idx = this.cartOrders.indexOf(this.cartOrders.find(o => o.id === order.id))
    console.log('CartStore: idx: ', idx)
    this.cartOrders[idx] = {
      ...order
    }

    this.cartOrders = [...this.cartOrders]

    try {
      await this.cartService.updateCartOrder(order, patientId, userId).toPromise();
      // We need to reload the entired cart order to get latest reaction/interaction checking results
      this.fetchPatientCartOrders(this.patientStoreService.patientId, this.userId);
      console.log('CartStore: UPDATED & RELOAD CART ORDERS completed..');

    } catch (err) {
      console.log('CartStore: UPDATE ERROR >=400: ', err);

      alert(`PUT ${err.status} ${err.statusText}\n${err.error}`);
      this.cartOrders = orders;
    }
  }

  async fetchPatientCartOrders(patientId, userId) {
    console.log('CartStore - fetchPatientCartOrder: userId: ', this.userStoreService.userId, this.patientStoreService.patientId)
    try {
      this.cart = await this.cartService.getCartOrders(patientId, userId).toPromise();
      console.log('CartStore - fetchPatientCartOrder: cart: ', this.cart)

    } catch (err) {
      console.log('CartStore: fetchPatientCartOrder ERROR >=400: ', err)

      if (err.status != 404) {  // Not found
        alert(`FETCH ${err.status} ${err.statusText}\n${err.error}`)
      } else {
        this.cart = {}
      }
    }

  }

  // async fetchAll() {
  //   console.log('CartStore - fetch all: userId: ', this.userStoreService.userId)
  //   // console.log('CartStore - fetch all: userSiteId: ', this.userStoreService.userSiteId)
  //   this.cart = await this.cartService.getCartOrders(1, this.userStoreService.userId).toPromise();
  //   // const response = await this.cartService.getCartOrders(1, 6473).toPromise();
  //   // this.cart = response

  //   // const xPagination = JSON.parse(response.headers.get('X-Pagination'))
  //   // this.totalCount = xPagination.totalCount

  //   console.log('CartStore - fetch all: cart: ', this.cart)
  //   // console.log('CartStore - fetch all: cart XXXXXX X-Pagination: ', response.headers.get('X-Pagination'))
  //   // console.log('CartStore - fetch all: cart XXXXXX X-Pagination obj: ', xPagination)

  //   // console.log('CartStore - fetch all: status: ', response.status)
  //   // console.log('CartStore - fetch all: body: ', response.body)
  // }
}
