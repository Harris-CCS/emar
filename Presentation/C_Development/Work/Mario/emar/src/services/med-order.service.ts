import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, tap, switchMap } from 'rxjs/operators';

import { Medication } from '../app/interfaces/medication';
import { Order } from '../app/interfaces/order';

import { ORDERS } from '../app/mockup/orders';
import { MEDICATIONS } from '../app/mockup/medications';
import { SelectorMatcher } from '@angular/compiler';

import { UserStoreService } from '../services/user-store.service';

@Injectable({
  providedIn: 'root'
})

export class MedOrderService {

  private siteId = this.userStoreService.userSiteId

  /* URL to WebAPI */
  private userQuickListsUrl = 'api/userquicklists'
  private deptPreferredListUrl = `/api/sites/${this.siteId}/departmentPreferredLists`
  private groupListUrl = `/api/sites/${this.siteId}/groupsrememberedorderslists`
  private orderUrl = 'api/orders'
  //private cartUrl = 'api/carts'

  private currentOrders: Order[];
  private cartOrders: Order[];
  private quickListOrders: Medication[];
  private dptPreferredOrders: Medication[];
  private groupsOrders: Medication[];
  
  //private selectedTab: string = 'B';  //default tab
  //private tabListTabs = ['Most Used', '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];


  allergiesInteractionChanged : Subject<any> = new Subject();
  drugsInteractionChanged : Subject<any> = new Subject();

  private searchResults: Observable<Medication[]>;

  constructor( 
    private http: HttpClient,
    private userStoreService: UserStoreService,
  ) { 
  
    this.currentOrders = ORDERS.slice(0, 6);
    this.cartOrders = ORDERS.slice(5, 9);

    //this.quickListOrders = MEDICATIONS.slice(100, 108);
    this.quickListOrders = MEDICATIONS;
    this.dptPreferredOrders = MEDICATIONS.slice(10, 12);
    this.groupsOrders = MEDICATIONS.slice(30, 40);
    // console.log('MedOrderService: OOOOOOOOOOOO siteId: ', this.userStoreService.userSiteId)
  }
  
  ngOnInit(): void {
    // this.currentOrders = ORDERS.slice(0, 6);
    // this.cartOrders = ORDERS.slice(5, 9);

    // //this.quickListOrders = MEDICATIONS.slice(100, 108);
    // this.quickListOrders = MEDICATIONS;
    // this.dptPreferredOrders = MEDICATIONS.slice(10, 12);
    // this.groupsOrders = MEDICATIONS.slice(30, 40);
    // console.log('MedOrderService: XXXXXXXXXXXX siteId: ', this.userStoreService.userSiteId)
  }

  /* Current Orders */
  //mock data
  // getCurrentOrders(): Order[] {
  //   console.log('MedOrderService: getCurrentOrders: ', this.currentOrders)
  //   return this.currentOrders;
  //   //return [];
  // }

  //API data  
  getCurrentOrders(patientId: number): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json'})
    const patientCurOrderUrl = `${this.orderUrl}?patientId=${patientId}`
    console.log('MedOrderService: getCurrentOrdersAPI: patientCurOrderUrl: ', patientCurOrderUrl)

    return this.http
      .get<any>(patientCurOrderUrl, { headers })
      .pipe(catchError(this.handleError<any>('getCurrentOrddrAPI')))
  }

  /* Cart Orders */
  // getCartOrders(): Order[] {
  //   console.log('MedOrderService: getCartOrders: ', this.cartOrders)
  //   return this.cartOrders;
  //   //return [];
  // }

  // getCartOrders(patientId: number, userId: number): Observable<any> {
  //   const headers = new HttpHeaders({ Accept: 'application/json', 'X-User': `${userId}`})
  //   const cartOrderUrl = `${this.cartUrl}/${patientId}`
  //   console.log('MedOrderService: getCartOrders: cartOrderUrl: ', cartOrderUrl)
    
  //   return this.http
  //     .get<any>(cartOrderUrl, { headers })
  //     .pipe(catchError(this.handleError<any>('getCartOrders')))
  // }

  // postCartOrder(med: Medication, listType?: string) {
  //   console.log('postCartOrder: selected med:', med)
  //   let ord: Order = {
  //     id: med.id,
  //     patientId: 2,
  //     name: med.brandName,
  //     startTime: '2019-06-28T14:00:00',
  //     endTime: '2019-06-30T14:00:00',
  //     dose: med.dose,
  //     route: med.route,
  //     frequency: {id: 1 , frequencyName: "ONCE"},
  //     signedOn: '2019-06-28T14:11:00',
  //     signedBy: 'mePost',
  //   };

  //   console.log('postCartOrder: new added ord:', ord)
  //   this.cartOrders.unshift(ord);
  // }

  // updateCartOrder(med: Medication, listType?: string) {
  //   console.log('updateCartOrder: selected med:', med)
  //   let ord: Order = {
  //     id: 555,
  //     patientId: 2,
  //     name: med.brandName,
  //     startTime: '2019-06-28T14:00:00',
  //     endTime: '2019-06-30T14:00:00',
  //     dose: med.dose,
  //     route: med.route,
  //     frequency: {id: 2 , frequencyName: "2TIMESDAILY"},
  //     signedOn: '2019-06-28T14:11:00',
  //     signedBy: 'meUpdate'
  //   };

  //   console.log('updateCartOrder: updated ord:', ord)
  // }

  // removeCartOrder(ord: Order) {
  //   console.log('MedOrderService: removeCartOrder: ord:', ord);
  //   this.cartOrders = this.cartOrders.filter(cartord => cartord.name !== ord.name);
  // }

  // removeAllCartOrder(patientId: number) {
  //   console.log('MedOrderService: removeAllCartOrder: patientId: ', patientId);
  //   this.cartOrders = [];
  // }

  /* QuickList Tabs */
  // getTabListTabs() {
  //   //console.log('MedOrderService: getTabListTabs: ', this.tabListTabs);
  //   return this.tabListTabs;
  // }

  getUserQuickLists(): Observable<any> {
    const xuser = this.userStoreService.userId?.toString()
    console.log('XXXXXXXXXXXX xuser:', xuser)
    const headers = new HttpHeaders({ Accept: 'application/json', 'X-User': xuser })

    return this.http
      .get<any>(this.userQuickListsUrl, { headers })
      .pipe(
        tap(_ => console.log('med-order.service: getUserQuickLists HTTP Client - GET')),
        catchError(this.handleError<any>('getUserQuickLists')))
  }

  getMedListBySelectedTab(tab: string) {
    console.log('MedOrderService: getMedListBySelectedTab: ', tab)
    //console.log('MedOrderService: getMedListBySelectedTab: return ', this.quickListOrders.filter((m) => m.name.startsWith(tab)).length, ' meds');
    //this.selectedTab = tab
    console.log('MedOrderService: getMedListBySelectedTab: selectedTab', tab)

    const headers = new HttpHeaders({ Accept: 'application/json', 'X-User': this.userStoreService.userId?.toString() })
    const userQuickListsByTabUrl = `${this.userQuickListsUrl}/tabs/${tab}`
    console.log('MedOrderService: getMedListBySelectedTab: userQuickListsByTabUrl: ', userQuickListsByTabUrl)

    return this.http
      .get<any>(userQuickListsByTabUrl, { headers })
      .pipe(
        tap(_ => console.log(`med-order.service: getMedListBySelectedTab: ${tab}`)),
        catchError(this.handleError<any>('getMedListBySelectedTab'))
      )

    // if (tab === 'Most Used') {
    //   return this.quickListOrders
    // } else if (tab === '#') {
    //   return this.quickListOrders.filter((m) => /^[^A-Za-z]/.test(m.name))
    // } else {
    //   return this.quickListOrders.filter((m) => m.name.startsWith(tab))
    // }
  }

  /* QuickList Orders */
  // getQuickListOrders(): Medication[] {
  //   //console.log('MedOrderService: getQuickListOrders: selectedTab? ', selectedTab);

  //   return this.getMedListBySelectedTab();
  // }
  
  /* Department Orders */
  getDeptPreferredOrdersList(): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json' })
    
    return this.http
      .get<any>(this.deptPreferredListUrl, { headers })
      .pipe(
        tap(_ => console.log('med-order.service: getDeptPreferredOrdersList HTTP Client - GET')),
        catchError(this.handleError<any>('getDeptPreferredOrdersList', []))
      )
  }
  
  /* Groups Orders */
  getGroupsOrdersList(): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json' })

    return this.http
      .get<any>(this.groupListUrl, { headers })
      .pipe(
        tap(_ => console.log('med-order.service: getGroupOrdersList HTTP Client - GET')),
        map(resp => (resp && resp.groups) ? resp.groups.map((group) => ({
          ...group
          // displayGroupName: group.groupName,
          // ...group.orders,
        })) : []),
        catchError(this.handleError<any>('getGroupOrdersList', []))
      )
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
    return MEDICATIONS.filter(med => med.brandName.toLowerCase().indexOf(term.toLowerCase()) > -1).slice(0, 10);
  }


  /* Handle Http failed */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('MedOrderService-handleError: ERROR: ', error);
      console.error('MedOrderService-handleError: STATUS: ', error.status);
      return of(result as T);
    };
  }
}
