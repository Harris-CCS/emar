import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs'
import { map } from 'rxjs/operators'

import { UserStoreService } from './user-store.service'
import { PatientService } from './patient.service'
import { PatientMedOrderService } from './patient-med-order.service';

import { PatientResponse } from 'src/app/interfaces/patient-response';
import { Order, OrderAdministration } from 'src/app/interfaces/order';
import { Patient } from 'src/app/interfaces/patient';

@Injectable({
  providedIn: 'root'
})
export class MyPatientsStoreService {

  private readonly _myPatientsResp = new BehaviorSubject<PatientResponse>(<PatientResponse>{})
  readonly myPatientsResp$ = this._myPatientsResp.asObservable()
  readonly myPatients$ = this.myPatientsResp$.pipe(
    map( (resp) => resp?.patients ? resp.patients : [])
  )

  private readonly _myPatientsOrders = new BehaviorSubject<Object>({}) 
  readonly myPatientsOrders$ = this._myPatientsOrders.asObservable()

  constructor(
    private userStoreService: UserStoreService,
    private patientService: PatientService, 
    private patientMedOrderService: PatientMedOrderService,
  ) {

    console.log('MyPatientsStoreService (constructor)')
    this.userStoreService.user$.subscribe( async() => {
    
      const userId = this.userStoreService.userId
      const userSiteId = this.userStoreService.userSite.id
      const departmentCode = this.userStoreService.departmentCode
      const wardCode = this.userStoreService.wardCode

      console.log('MyPatientsStoreService (constructor)(subscribed): subscribed UserStoreService: userId: ', userId, '  userSiteId:', userSiteId)
      
      if (userId && userSiteId) {
        this.fetchMyPatients(userId, userSiteId, departmentCode, wardCode)
      }
    })

    /* get orders from patient by includeOrders=true in API request
    this.myPatients$.subscribe( async(patients) => {
      console.log('MyPatientsStoreService (constructor)(subscribed): subscribed myPaitents$')
      console.log('MyPatientsStoreService (constructor)(subscribed): patients: ', patients)

      // for (const patient of patients) {
      //   const orders = await this.getPatientOrders(patient.id)
      // }

      const patientsOrders = await Promise.all(patients.map((patient) => this.getPatientOrders(patient.id)))

      this.myPatientsOrders = patients.reduce((prev, patient, index) => {
        prev[patient.id] = patientsOrders[index]

        return prev
      }, {})
    })
    */
  }

  get myPatientsResp(): PatientResponse {
    return this._myPatientsResp.getValue() || <PatientResponse>{}
  }

  set myPatientsResp(val: PatientResponse) {
    this._myPatientsResp.next(val)
  }

  get myPatientsOrders() {
    return this._myPatientsOrders.getValue() || {}
  }

  set myPatientsOrders(val: Object) {
    this._myPatientsOrders.next(val)
  }


  // addPatientOrders(patientId: number, orders: Order[]) {
  //   const currentMyPatientsOrders = this.myPatientsOrders
  //   const newMyPatientsOrders = { ...currentMyPatientsOrders, [patientId]: orders }

  //   this.myPatientsOrders = newMyPatientsOrders
  // }
  
  async fetchMyPatients(userId: number, userSiteId: number, departmentCode: string, wardCode: string) {

    console.log('MyPatientsStoreService: fetchMyPatients')
    this.myPatientsResp = await this.patientService.getMyPatients(userSiteId, userId, departmentCode, wardCode).toPromise()
    console.log('MyPatientsStoreService: fetchMyPatients: ', this.myPatientsResp)
  }

  async getPatientOrders(patientId: number): Promise<Order[]> {
    
    let orders = await this.patientMedOrderService.getPatientCurrentOrders(patientId).toPromise()
    
    orders = orders?.map((order) => ({
      ...order,
      displayName: order.medication?.displayName,
      displayRoute: order.medicationRoute ? order.medicationRoute.routeName : '',
      displayFrequency: order.frequencySchedule ? order.frequencySchedule.scheduleName : '',
      displayDose: order.dose,
      displayDoseUnit: order.doseUnit ? order.doseUnit.printName : '',
      isComboMed: order.medication?.medicationDetails.length > 1 ? true : false,  // TODO: check the drugId &&
      comboMedDetails: order.medication?.medicationDetails.length > 1 ? order.medication.medicationDetails.map((m) => ({
        brandName: m.brandName,
        dose: m.dose,
        doseUnit: m.doseUnit ? m.doseUnit.printName : ''
      })) : [],
      // allergyReactionsText: order.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
      // drugInteractionsText: order.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
    }));

    return orders
  }
}
