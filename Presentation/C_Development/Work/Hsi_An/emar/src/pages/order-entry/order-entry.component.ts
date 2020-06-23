import { Component, OnInit } from '@angular/core';

import { User } from 'src/app/interfaces/user';
import { Patient } from 'src/app/interfaces/patient';
import { Order } from 'src/app/interfaces/order';
import { Medication } from 'src/app/interfaces/medication';

import { USER } from '../../app/mockup/user';
import { PATIENT } from '../../app/mockup/patient';
import { ORDERS } from '../../app/mockup/orders';
import { MEDICATIONS } from '../../app/mockup/medications';


@Component({
  selector: 'order-entry',
  templateUrl: './order-entry.component.html',
  styleUrls: ['./order-entry.component.scss', '../../assets/css/site.css']
})
export class OrderEntryComponent implements OnInit {

  user: User;
  patient: Patient;
  orders = ORDERS;

  qlSelected: boolean = true;
  dpSelected: boolean = false;
  gSelected: boolean = false;

  constructor() { }

  ngOnInit(): void {
  }

  loginUser() {
    this.user = USER
    
    return this.user;
  }

  selectedPatient() {
    this.patient = PATIENT

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
}
