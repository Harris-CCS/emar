import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { User } from 'src/app/interfaces/user';
import { Patient } from 'src/app/interfaces/patient';
import { Order } from 'src/app/interfaces/order';
import { Medication } from 'src/app/interfaces/medication';

import { ORDERS } from '../../app/mockup/orders';
import { USER } from '../../app/mockup/user';
import { PATIENT } from '../../app/mockup/patient';
import { MEDICATIONS } from '../../app/mockup/medications';

//import { ORDERS } from '../../app/mockup/orders';
import { MedOrderService } from '../../services/med-order.service';
import { PatientService } from 'src/services/patient.service';
import { ModalService } from '../../services/modal.service';


import { CartService } from '../../services/cart.service';
import { CartStoreService } from '../../services/cart-store.service';

@Component({
  selector: 'order-entry',
  templateUrl: './order-entry.component.html',
  styleUrls: ['./order-entry.component.scss', '../../assets/css/site.css'],
})
export class OrderEntryComponent implements OnInit {
  patientId: number;
  patient: Patient;
  orders: Order[];
  //currentOrders = ORDERS;
  currentOrders: Order[];
  cartOrders: Order[];
  //cartOrders: Array<Object>;

  qlSelected: boolean = true;
  dpSelected: boolean = false;
  gSelected: boolean = false;

  hasDeptPreferredDefined: boolean = false;
  isTabValidHandler(event) {
    this.hasDeptPreferredDefined = event
    console.log('isTabValidHandler: hasDeptPreferredDefined? ', this.hasDeptPreferredDefined)
  }

  constructor(
    private route: ActivatedRoute,
    private patientService: PatientService,
    private medOrderService: MedOrderService,

    public cartStoreService: CartStoreService,
    private cartService: CartService,
    private modalService: ModalService
  ) {}

  ngOnInit(): void {
    //const patientId:number = +this.route.snapshot.params['id'];
    this.patientId = +this.route.snapshot.params['id'];
    console.log('OrderEntry: patientId: ', this.patientId);
    this.patient = this.patientService.getPatient(this.patientId);
    //this.orders = this.patientService.getPatientOrders(this.patientId);
    this.getCurrentListOrders()
    this.getCartListOrders()

    // this.patientService
    //   .getPatient(this.patientId)
    //   .subscribe((patient) => (this.patient = patient));
  }

  //currentUser() {
  //  this.user = USER
  //
  //  return this.user;
  //}

  performModalActions(modalId: string, modalContainerId: string): boolean {
    if (this.isModalVisible(modalId)) {
      const container = document.getElementById(modalContainerId);
      container.scrollTop = 0;
    }
    return true;
  }

  isModalVisible(modalId: string): boolean {
    const foundModalId = this.modalService.findModal(modalId);
    return foundModalId && foundModalId.isOpen ? true : false;
  }

  selectedPatient() {
    return this.patient;
  }

  onSelectTab(tab: string) {
    if (tab === 'list-dept-preferred-orders') {
      this.qlSelected = false;
      this.dpSelected = true;
      this.gSelected = false;
    } else if (tab === 'list-groups-orders') {
      this.qlSelected = false;
      this.dpSelected = false;
      this.gSelected = true;
    } else {
      //default list-quick-orders

      this.qlSelected = true;
      this.dpSelected = false;
      this.gSelected = false;
    }
  }

  getCurrentListOrders() {
    //return (this.currentOrders = this.medOrderService.getCurrentOrders());
    this.medOrderService.getCurrentOrdersAPI(this.patientId).subscribe((o) => {
      console.log('getCurrentListOrders: ', o.orders)
      this.currentOrders = o.orders.map( x => ({
        ...x,
        displayName: x.brandName,
        displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
        displayDose: x.dose ? x.dose : '',
        displayDoseUnit: x.doseUnit ? x.doseUnit.printName : '',
        displayFrequency: x.frequencyId,
        displaySignedOn: x.addDatetime,
        displaySignedBy: x.orderingPhysicianUser.displayName || '',
        allergies: [],
        drugs: []
      }))
    })
  }

  cartListOrders() {
    return this.cartOrders
  }

  getCartListOrders() {
    //return ORDERS.slice(2, 5);
    //return (this.cartOrders = this.medOrderService.getCartOrders());
    this.cartService.getCartOrders(this.patientId, 5555).subscribe((o) => {
      if (o) {
        console.log('OrderEntry: getCartListOrders: ', o.orders)
        this.cartOrders = o.orders.map((x) => ({
          ...x,
          displayName: x.brandName,
          displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
          displayFrequency: x.frequencyId,
          displayDose: x.dose,
          displayDoseUnit: x.doseUnit ? x.doseUnit.printName : '',
          allergies: [],
          drugs: []
        }))
      }
    }) 
  }



  // getCartListOrders() {
  //   //return ORDERS.slice(2, 5);
  //   //return (this.cartOrders = this.medOrderService.getCartOrders());
  //   this.cartService.getCartOrders(this.patientId, 6473).subscribe((resp) => {
  //     const keys = resp.headers.keys()
  //     const headers = keys.map(key =>
  //        `${key}: ${resp.headers.get(key)}`);

  //     // // access the body directly, which is typed as `Config`.
  //     // const config = { ... o.body };
  //     console.log('RESP O: ', resp)
  //     //console.log('RESP headers: ', headers)

  //     if (resp) {
  //       console.log('OrderEntry: getCartListOrders: ', resp.body.orders)
  //       this.cartOrders = resp.body.orders.map((x) => ({
  //         ...x,
  //         displayName: x.brandName,
  //         displayRoute: x.medicationRoute ? x.medicationRoute.routeName : '',
  //         displayFrequency: x.frequencyId,
  //         displayDose: x.dose,
  //         displayDoseUnit: x.doseUnit ? x.doseUnit.printName : '',
  //         allergies: [],
  //         drugs: []
  //       }))
  //     }
  //   }) 
  // }
}
