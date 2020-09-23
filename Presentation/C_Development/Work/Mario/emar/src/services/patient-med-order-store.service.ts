import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { PatientStoreService } from '../services/patient-store.service';
import { PatientMedOrderService } from '../services/patient-med-order.service';

@Injectable({
  providedIn: 'root'
})
export class PatientMedOrderStoreService {

  constructor(
    private patientStoreService: PatientStoreService,
    private patientMedOrderService: PatientMedOrderService,
  ) { 
    this.fetchPatientMedOrder(this.patientId)

    // setTimeout(() => this.fetchPatientMedOrder(this.patientId), 10000)
  }

  private patientId = this.patientStoreService.patientId
  private readonly _patientMedOrder = new BehaviorSubject<any>([])
  readonly patientMedOrder$ = this._patientMedOrder.asObservable()

  get patientMedOrder(): [] {
    return this._patientMedOrder.getValue() || []
  }

  set patientMedOrder(val: []) {
    this._patientMedOrder.next(val)
  }

  async fetchPatientMedOrder(patientId) {
    this.patientMedOrder = await this.patientMedOrderService.getPatientCurrentOrders(patientId).toPromise();
    // console.log('PatientMedOrderStore - fetchPatientMedOrder: patientMedOrder: ', this.patientMedOrder, this._patientMedOrder.getValue()[7])
  }
}
