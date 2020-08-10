import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, tap, switchMap } from 'rxjs/operators';

import { Medication } from '../app/interfaces/medication';
import { Order } from '../app/interfaces/order';

import { ORDERS } from '../app/mockup/orders';
import { MEDICATIONS } from '../app/mockup/medications';
import { SelectorMatcher } from '@angular/compiler';

@Injectable({
  providedIn: 'root'
})

export class MedOrderService {

  private currentOrders: Order[];
  private cartOrders: Order[];
  private quickListOrders: Medication[];
  private dptPreferredOrders: Medication[];
  private groupsOrders: Medication[];
  allergiesInteractionChanged : Subject<any> = new Subject();
  drugsInteractionChanged : Subject<any> = new Subject();

  private searchResults: Observable<Medication[]>;

  //constructor( private http: HttpClient ) { 
  constructor() { 
    
    this.currentOrders = ORDERS.slice(0, 6);
    this.cartOrders = ORDERS.slice(5, 9);

    this.quickListOrders = MEDICATIONS.slice(100, 108);
    this.dptPreferredOrders = MEDICATIONS.slice(10, 12);
    this.groupsOrders = MEDICATIONS.slice(30, 40);
  }

  /* Current Orders */
  getCurrentOrders(): Order[] {
    console.log('MedOrderService: getCurrentOrders: ', this.currentOrders)
    return this.currentOrders;
    //return [];
  }

  /* Cart Orders */
  getCartOrders(): Order[] {
    console.log('MedOrderService: getCartOrders: ', this.cartOrders)
    return this.cartOrders;
    //return [];
  }

  postCartOrder(med: Medication, listType?: string) {
    console.log('postCartOrder: selected med:', med)
    let ord: Order = {
      id: 99,
      patientId: 2,
      name: med.name,
      startTime: '2019-06-28T14:00:00',
      endTime: '2019-06-30T14:00:00',
      dose: med.dose,
      route: med.route,
      frequency: {id: 1 , frequencyName: "ONCE"},
      signedOn: '2019-06-28T14:11:00',
      signedBy: 'mePost',
    };

    console.log('postCartOrder: new added ord:', ord)
    this.cartOrders.unshift(ord);
  }

  updateCartOrder(med: Medication, listType?: string) {
    console.log('updateCartOrder: selected med:', med)
    let ord: Order = {
      id: 555,
      patientId: 2,
      name: med.name,
      startTime: '2019-06-28T14:00:00',
      endTime: '2019-06-30T14:00:00',
      dose: med.dose,
      route: med.route,
      frequency: {id: 2 , frequencyName: "2TIMESDAILY"},
      signedOn: '2019-06-28T14:11:00',
      signedBy: 'meUpdate'
    };

    console.log('updateCartOrder: updated ord:', ord)
  }

  removeCartOrder(ord: Order) {
    console.log('MedOrderService: removeCartOrder: ord:', ord);
    this.cartOrders = this.cartOrders.filter(cartord => cartord.name !== ord.name);
  }

  removeAllCartOrder(patientId: number) {
    console.log('MedOrderService: removeAllCartOrder: patientId: ', patientId);
    this.cartOrders = [];
  }

  /* QuickList Orders */
  getQuickListOrders(): Medication[] {
    console.log('MedOrderService: getQuickListOrders: ', this.quickListOrders);
    return this.quickListOrders;
  }
  
  /* Department Orders */
  getDptPreferredOrders(): Medication[] {
    return this.dptPreferredOrders;
  }
  
  /* Groups Orders */
  getGroupsOrders(): Medication[] {
    return this.groupsOrders;
  }

  /* Typeahead Search */
  searchHttp(term: string) {
    if (term === '') {
      return of([]);
    }

    /*
    return this.http
    .get(WIKI_URL, {params: PARAMS.set('search', term)}).pipe(
      map(response => response[1])
    );
    */
  }

  /*
  search = (text$: Observable<string>) => {
    text$.pipe(
    map(term => term.length < 2 ? []
      : MEDICATIONS.filter(med => med.name.toLowerCase().indexOf(term.toLowerCase()) > -1).slice(0, 10))
    )
  }*/

  search(term: string): Medication[] {
    return MEDICATIONS.filter(med => med.name.toLowerCase().indexOf(term.toLowerCase()) > -1).slice(0, 10);
  }
}
