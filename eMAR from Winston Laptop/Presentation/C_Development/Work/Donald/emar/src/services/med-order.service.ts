import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, tap, switchMap } from 'rxjs/operators';

import { Medication } from '../app/interfaces/medication';
import { Order } from '../app/interfaces/order';
import { CartOrder } from '../app/interfaces/cart-order';

import { ORDERS } from '../app/mockup/orders';
import { MEDICATIONS } from '../app/mockup/medications';
import { SelectorMatcher } from '@angular/compiler';

import { UserStoreService } from '../services/user-store.service';
import { PatientStoreService } from '../services/patient-store.service';

@Injectable({
  providedIn: 'root'
})

export class MedOrderService {

  refreshRequest_listOrders = new EventEmitter<any>();
  // private siteId = this.userStoreService.userSiteId
  // private patientId = this.patientStoreService.patientId
  private patDept = this.patientStoreService.patientDeptCode

  /* URL to WebAPI */
  private userQuickListsUrl = '/api/userquicklists'
  private deptPreferredListUrl = `/api/sites/${this.userStoreService.userSiteId}/departmentPreferredLists`
  private groupListUrl = `/api/sites/${this.userStoreService.userSiteId}/groupsrememberedorderslists`
  private orderUrl = '/api/orders'
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
    private patientStoreService: PatientStoreService,
  ) {
    // this.currentOrders = ORDERS.slice(0, 6);
    // this.cartOrders = ORDERS.slice(5, 9);

    // //this.quickListOrders = MEDICATIONS.slice(100, 108);
    // this.quickListOrders = MEDICATIONS;
    // this.dptPreferredOrders = MEDICATIONS.slice(10, 12);
    // this.groupsOrders = MEDICATIONS.slice(30, 40);
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
  // getCurrentOrders(patientId: number): Observable<any> {
  //   const headers = new HttpHeaders({ Accept: 'application/json'})
  //   const patientCurOrderUrl = `${this.orderUrl}?patientId=${patientId}`
  //   console.log('MedOrderService: getCurrentOrdersAPI: patientCurOrderUrl: ', patientCurOrderUrl)

  //   return this.http
  //     .get<any>(patientCurOrderUrl, { headers })
  //     .pipe(catchError(this.handleError<any>('getCurrentOrdersAPI')))
  // }

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
    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-User': xuser, 
      'EMAR-Site': `${this.userStoreService.userSiteId}`, 
      'EMAR-Patient': `${this.patientStoreService.patientId}` 
    })
    // const url = `${this.userQuickListsUrl}?siteId=${this.siteId}&patientId=${this.patientId}`

    return this.http
      .get<any>(`${this.userQuickListsUrl}?r=${Math.random()}`, { headers })
      .pipe(
        tap(_ => console.log('med-order.service: getUserQuickLists HTTP Client - GET')),
        catchError(this.handleError<any>('getUserQuickLists')))
  }

  getMedListBySelectedTab(tab: string, type?: string) {
    console.log('MedOrderService: getMedListBySelectedTab: ', tab)
    //console.log('MedOrderService: getMedListBySelectedTab: return ', this.quickListOrders.filter((m) => m.name.startsWith(tab)).length, ' meds');
    //this.selectedTab = tab
    console.log('MedOrderService: getMedListBySelectedTab: selectedTab', tab)

    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-User': this.userStoreService.userId?.toString(), 
      'EMAR-Site': `${this.userStoreService.userSiteId}`, 
      'EMAR-Patient': `${this.patientStoreService.patientId}` 
    })
    // const userQuickListsByTabUrl = `${this.userQuickListsUrl}/tabs/${tab}?siteId=${this.siteId}&patientId=${this.patientId}`

    const tabUrl = (type === 'deptPreferredWithTab') ? this.deptPreferredListUrl : this.userQuickListsUrl

    return this.http
      .get<any>(`${tabUrl}/tabs/${tab}?r=${Math.random()}`, { headers })
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

  /* POST - post a cart order by UserQuickList item id*/
  postCartOrderByListOrderId(listOrderId: number, patientId: number, userId: number, order: CartOrder): Observable<any> {

    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}`, 'EMAR-Site': `${this.userStoreService.userSiteId}`, 'EMAR-Patient': `${this.patientStoreService.patientId}`})
    let url = `${this.userQuickListsUrl}/${listOrderId}/cartOrders/${patientId}`
    if (order.durationUnitId !== null) {
      url = url + `?duration=${order.duration}&durationUnitId=${order.durationUnitId}`;
    }
    console.log('MedOrderService: postCartOrderByListOrderId: url: ', url)

    return this.http
      .post<any>(url, null, { headers })
      .pipe(
        tap(_ => console.log(`POST CART ORDER (UserQuick) List Order ID=${listOrderId} by userID=${userId} for paitnetID=${patientId}`)),
        catchError(this.handleError<any>('postCartOrderByListOrderId'))
      )
  }

  /* QuickList Orders */
  // getQuickListOrders(): Medication[] {
  //   //console.log('MedOrderService: getQuickListOrders: selectedTab? ', selectedTab);

  //   return this.getMedListBySelectedTab();
  // }
  
  /* Department Preferred List Orders */
  getDeptPreferredOrdersList(): Observable<any> {
    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-Site': `${this.userStoreService.userSiteId}`,
      'EMAR-User': this.userStoreService.userId?.toString(),
      'EMAR-Patient': this.patientStoreService.patientId?.toString()
    })
    
    // TODO: do we need to filter by patient department code?
    return this.http
      .get<any>(`${this.deptPreferredListUrl}?r=${Math.random()}`, { headers })
      .pipe(
        tap(_ => console.log('med-order.service: getDeptPreferredOrdersList HTTP Client - GET')),
        catchError(this.handleError<any>('getDeptPreferredOrdersList', []))
      )
  }

  /* Department Preferred List Orders with Tabs Display */
  getDeptPreferredOrdersWithTabList(): Observable<any> {
    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-Site': `${this.userStoreService.userSiteId}`,
      'EMAR-User': this.userStoreService.userId?.toString(),
      'EMAR-Patient': this.patientStoreService.patientId?.toString()
    })
    
    // TODO: do we need to filter by patient department code?
    return this.http
      .get<any>(`${this.deptPreferredListUrl}/tabs/initial?r=${Math.random()}`, { headers })
      .pipe(
        tap(_ => console.log('med-order.service: getDeptPreferredOrdersList HTTP Client - GET')),
        catchError(this.handleError<any>('getDeptPreferredOrdersList', []))
      )
  }

  /* POST - post a cart order by DeptPreferredList item id*/
  postCartOrderByDeptPreferredListOrderId(listOrderId: number, patientId: number, userId: number, order: CartOrder): Observable<any> {

    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}`, 'EMAR-Site': `${this.userStoreService.userSiteId}`, 'EMAR-Patient': `${this.patientStoreService.patientId}`})
    let url = `/api/patients/${patientId}/departmentPreferredLists/${listOrderId}/cartOrders`
    if (order.durationUnitId !== null) {
      url = url + `?duration=${order.duration}&durationUnitId=${order.durationUnitId}`;
    }
    console.log('MedOrderService: postCartOrderByDeptPreferredListOrderId: url: ', url)

    return this.http
      .post<any>(url, null, { headers })
      .pipe(
        tap(_ => console.log(`POST CART ORDER (DeptPreferred) List Order ID=${listOrderId} by userID=${userId} for paitnetID=${patientId}`)),
        catchError(this.handleError<any>('postCartOrderByDeptPreferredListOrderId'))
      )
  }
  
  /* Groups Orders */
  getGroupsOrdersList(): Observable<any> {
    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-Site': `${this.userStoreService.userSiteId}`,
      'EMAR-User': this.userStoreService.userId?.toString(),
      'EMAR-Patient': this.patientStoreService.patientId?.toString()
    })

    return this.http
      .get<any>(`${this.groupListUrl}?r=${Math.random()}`, { headers })
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

  /* POST - post a cart order by GroupsList item id*/
  postCartOrderByGroupsListOrderId(listOrderId: number, patientId: number, userId: number, order: CartOrder): Observable<any> {

    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-User': `${userId}`, 'EMAR-Site': `${this.userStoreService.userSiteId}`, 'EMAR-Patient': `${patientId}`})
    let url = `/api/patients/${patientId}/groupsrememberedorderslists/${listOrderId}/cartOrders`
    if (order.durationUnitId !== null) {
      url = url + `?duration=${order.duration}&durationUnitId=${order.durationUnitId}`;
    }
    console.log('MedOrderService: postCartOrderByGroupsListOrderId: url: ', url)

    return this.http
      .post<any>(url, null, { headers })
      .pipe(
        tap(_ => console.log(`POST CART ORDER (Groups) List Order ID=${listOrderId} by userID=${userId} for paitnetID=${patientId}`)),
        catchError(this.handleError<any>('postCartOrderByGroupsListOrderId'))
      )
  }

  /* Typeahead Search */
  brandNameSearch(term: string, source: string) {
    if (term === '') {
      return of([]);
    }

    const headers = new HttpHeaders({ 
      Accept: 'application/json', 
      'EMAR-Site': `${this.userStoreService.userSiteId}`,
      'EMAR-User': `${this.userStoreService.userId}`,
      'EMAR-PatientDepartment': `${this.patientStoreService.patientDeptCode}`
    })
    const url = `/api/BrandNameList/${term}/${source}/site/${this.userStoreService.userSiteId}`
    console.log('MedOrderService: getMedicationSearchOptions: url: ', url)

    return this.http
      .get<any>(url, { headers })
      // .pipe(
      //   catchError(this.handleError<any>('brandNameSearch'))
      // )
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

  /* Medication Search - dropdown options */
  getMedicationSearchOptions(): Observable<any> {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': `${this.userStoreService.userSiteId}` })
    const url = '/api/SearchDropdownList'
    console.log('MedOrderService: getMedicationSearchOptions: url: ', url)

    return this.http
      .get<any>(url, { headers })
      .pipe(
        catchError(this.handleError<any>('getMedicationSearchOptions'))
      )
  }

  /* Handle Http failed */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error('MedOrderService-handleError: ERROR: ', error);
      console.error('MedOrderService-handleError: STATUS: ', error.status);
      return of(result as T);
    };
  }

  deleteQuickListItem(itemId: number) {
    const headers = new HttpHeaders({ Accept: 'application/json', 'EMAR-Site': `${this.userStoreService.userSiteId}`, 'EMAR-User': `${this.userStoreService.userId}` });
    const url = '/api/userquicklists/delete/'+ itemId.toString();
    return this.http
      .delete(url, { headers })
      .subscribe({
        next: data => {
        },
        error: error => {
          console.log('Error in deleteQuickListItem')
        }
      });
  }
}
