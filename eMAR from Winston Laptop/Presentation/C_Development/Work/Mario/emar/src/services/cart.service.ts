import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders, HttpResponse, HttpParams } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';

import { CartOrder } from '../app/interfaces/cart-order';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  /* URL to WebAPI */
  private cartUrl = '/api/carts';

  // httpOptions = {
  //   headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  // };

  constructor(
    private http: HttpClient,
  ) { }

  /* GET */
  getCartOrders(patientId: number, userId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}` })

    const cartOrderUrl = `${this.cartUrl}/${patientId}?r=${Math.random()}`
    console.log('CartService: getCartOrders: cartOrderUrl: ', cartOrderUrl)

    return this.http
      .get<any>(cartOrderUrl, { headers, observe: 'response' })
      // .get<any>(cartOrderUrl, { headers })
      .pipe(
        map((res) => {
          console.log('HELLOooOOOOOOOO: ', res);
          console.log('HELLOooOOOOOOOO: ', res.status);
          console.log('HELLOooOOOOOOOO X-Pagination: ', res.headers.get('EMAR-Pagination'));
          let xPagination = { totalCount: 0 }
          try {
            xPagination = JSON.parse(res.headers.get('EMAR-Pagination'))
          } catch (e) {
            //dont care about the error
          }
          return { ...res.body, xPagination }
        }),
        // catchError(this.handleError<any>('getCartOrders'))
      )
  }

  getCartOrder(patientId: number, userId: number, siteId: number, cartOrderId: number): Observable<any> {
    const headers = new HttpHeaders({
      Accept: 'application/json',
      'EMAR-User': `${userId}`,
      'EMAR-Site': `${siteId}`,
      'EMAR-Patient': `${patientId}`
    });

    const cartOrderUrl = `${this.cartUrl}/orders/${cartOrderId}`;
    console.log('CartService: getOneCartOrder: cartOrderUrl: ', cartOrderUrl);

    return this.http
      .get<any>(cartOrderUrl, { headers })
      .pipe(catchError(this.handleError<any>('getOneCartOrderFromAPI')));
  }

  /* PUT - update the cart order */
  updateCartOrder(order: CartOrder, patientId: number, userId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}` })
    const cartOrderUrl = `${this.cartUrl}/orders/${order.id}`
    console.log('CartService: updateCartOrder: cartOrderUrl: ', cartOrderUrl)
    console.log('CartService: updateCartOrder: order: ', order)

    return this.http
      .put<any>(cartOrderUrl, order, { headers })
      .pipe(
        tap(_ => console.log(`PUT CART ORDER by userID=${userId}`)),
        // catchError(this.handleError<any>('updateCartOrder'))
      )
  }

  /* DELETE - delete a cart order */
  deleteCartOrder(cartOrderId: number, userId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}` })
    const cartOrderUrl = `${this.cartUrl}/orders/${cartOrderId}`
    console.log('CartService: deleteCartOrder: cartOrderUrl: ', cartOrderUrl)

    return this.http
      .delete<any>(cartOrderUrl, { headers, observe: 'response' })
      .pipe(
        tap(_ => console.log(`DELETE CART ORDER by ID=${cartOrderId}`)),
        // catchError(this.handleError<any>('deleteCartOrder'))
      )
  }

  /* DELETE - delete all cart orders */
  deleteAllCartOrders(patientId: number, userId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}` })
    const cartOrderUrl = `${this.cartUrl}/${patientId}`
    console.log('CartService: deleteALLCartOrders: cartOrderUrl: ', cartOrderUrl)

    return this.http
      .delete<any>(cartOrderUrl, { headers })
      .pipe(
        tap(_ => console.log(`DELETE ALL CART ORDER by PatientID=${patientId}`)),
        //   catchError(this.handleError<any>('deleteAllCartOrders'))
      )
  }

  /* POST - post a cart order */
  postCartOrder(order: CartOrder, patientId: number, userId: number): Observable<any> {
    // postCartOrder(order: {}, patientId: number, userId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}` })
    const cartOrderUrl = `${this.cartUrl}/${patientId}`
    console.log('CartService: postCartOrder: cartOrderUrl: ', cartOrderUrl)
    console.log('CartService: postCartOrder: order: ', order)
    console.log('CartService: postCartOrder: headers: ', headers);

    return this.http
      .post<any>(cartOrderUrl, order, { headers })
      .pipe(
        tap(_ => console.log(`POST CART ORDER by userID=${userId}`)),
        catchError(this.handleError<any>('postCartOrder'))
      )
  }

  /* POST - post all cart orders (checkout)*/
  postAllCartOrders(patientId: number, userId: number, data: any): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}` })
    const cartOrderUrl = `${this.cartUrl}/${patientId}/checkout`
    console.log('CartService:postAllCartOrders: cartOrderUrl: ', cartOrderUrl)

    // let body = {
    //   "orderingPhysicianUserId": data.orderingPhysicianUserId,
    //   "drugInteractionOverrideRationalia": [
    //     {
    //       "medicationInteractionId": "",
    //       "overrideReasonId": ""
    //     }
    //   ],
    //   "allergyReactionOverrideRationalia": [
    //     {
    //       "orderReactionId": "",
    //       "overrideReasonId": ""
    //     }
    //   ]
    // }

    return this.http
      .post<any>(cartOrderUrl, data, { headers })
      .pipe(
        tap(_ => console.log(`POST all cart orders for patientId=${patientId}`)),
        // catchError(this.handleError<any>('postAllCartOrders'))
      );
  }

  /* GET - get list of ordering physicians, orderride reasons for interaction/reaction, and cart orders with interaction/reactions */
  getPreCheckoutForSign(patientId: number, userId: number): Promise<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}` })

    const cartOrderUrl = `${this.cartUrl}/${patientId}/precheckout?r=${Math.random()}`
    console.log('CartService: getPreCheckoutForSign: cartOrderUrl: ', cartOrderUrl)

    return this.http
      .get<any>(cartOrderUrl, { headers })
      .pipe(
        tap(_ => console.log('cart.service: getPreCheckoutForSign HTTP Client - GET')),
        // catchError(this.handleError<any>('getPreCheckoutForSign'))
      ).toPromise()
  }

  /* Handle Http failed */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('CartService-handleError: ERROR: ', error);
      console.error('CartService-handleError: STATUS: ', error.status);
      // result = error
      // console.error('CartService-handleError: result: ', result);
      return of(result as T);
    };
  }
}
