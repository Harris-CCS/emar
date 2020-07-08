import { Component, OnInit } from '@angular/core';
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


@Component({
  selector: 'order-entry',
  templateUrl: './order-entry.component.html',
  styleUrls: ['./order-entry.component.scss', '../../assets/css/site.css']
})
export class OrderEntryComponent implements OnInit {

  patient: Patient;
  orders: Order[];
  //currentOrders = ORDERS;
  currentOrders: Order[];
  cartOrders: Order[];

  qlSelected: boolean = true;
  dpSelected: boolean = false;
  gSelected: boolean = false;

  constructor(private route: ActivatedRoute,
      private patientService: PatientService,
      private medOrderService: MedOrderService) { }

  ngOnInit(): void {
    const patientId:number = +this.route.snapshot.params['id'];
    this.patient = this.patientService.getPatient(patientId);
    this.orders = this.patientService.getPatientOrders(patientId)
    this.currentListOrders();
  }

  //currentUser() {
  //  this.user = USER
  //  
  //  return this.user;
  //}

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
    
    } else { //default list-quick-orders
    
      this.qlSelected = true;
      this.dpSelected = false;
      this.gSelected = false;
    
    }
  }

  currentListOrders() {
    return this.currentOrders = this.medOrderService.getCurrentOrders();
  }

  cartListOrders() {
    //return ORDERS.slice(2, 5);
    return this.cartOrders = this.medOrderService.getCartOrders();
  }
}
