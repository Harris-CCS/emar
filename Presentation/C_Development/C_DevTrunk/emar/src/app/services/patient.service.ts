import { Injectable } from '@angular/core';

import { Patient } from 'src/app/interfaces/patient';
import { Order } from 'src/app/interfaces/order';

import { PATIENTS } from '../mockup/patients';
import { ORDERS } from '../mockup/orders';

@Injectable({
  providedIn: 'root'
})
export class PatientService {

  constructor() { }

  getPatients(): Patient[] {
    return PATIENTS;
  }

  getPatient(patientId: number): Patient {
    const patient = PATIENTS.find( (p) => {
        return p.id === patientId;
      }
    );
    return patient;
  }
  getPatientOrders(patientId: number): Order[] {
    const orders = ORDERS.filter( (o) => {
        return o.patientId === patientId;
      }
    );
    return orders;
  }
}
