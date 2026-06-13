import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs'
import { map } from 'rxjs/operators'

import { UserStoreService } from './user-store.service'
import { PatientService } from './patient.service'
import { PatientMedOrderService } from './patient-med-order.service';

import { PatientResponse } from 'src/app/interfaces/patient-response';
import { Order, OrderAdministration } from 'src/app/interfaces/order';


@Injectable({
  providedIn: 'root'
})
export class AllPatientsStoreService {

  private readonly _allPatientsResp = new BehaviorSubject<PatientResponse>(<PatientResponse>{})
  readonly allPatientsResp$ = this._allPatientsResp.asObservable()
  readonly allPatients$ = this.allPatientsResp$.pipe(
    map( (resp) => resp?.patients ? resp.patients : [])
  )
  private readonly _allPatientsOrders = new BehaviorSubject<Object>({})
  readonly allPatientsOrders$ = this._allPatientsOrders.asObservable()

  private readonly _upcomingOrdersPatientsOrders = new BehaviorSubject<Object>({})
  readonly upcomingOrdersPatientsOrders$ = this._upcomingOrdersPatientsOrders.asObservable()


  constructor(
    private userStoreService: UserStoreService,
    private patientService: PatientService,
    private patientMedOrderService: PatientMedOrderService,
  ) { 

    console.log('AllPatientsStoreService')
    this.userStoreService.user$.subscribe( async() => {
    
      const userId = this.userStoreService.userId
      const userSiteId = this.userStoreService.userSite.id
      const departmentCode = this.userStoreService.departmentCode
      const wardCode = this.userStoreService.wardCode

      console.log('AllPatientsStoreService (constructor)(subscribed): subscribed UserStoreService: userId: ', userId, '  userSiteId:', userSiteId)
      
      if (userId && userSiteId) {
        this.fetchPatients(userId, userSiteId, departmentCode, wardCode)
      }
    })

    this.allPatients$.subscribe( async(patients) => {
      console.log('AllPatientsStoreService (constructor)(subscribed): subscribed allPaitents$')
      console.log('AllPatientsStoreService (constructor)(subscribed): patients: ', patients)

      const patientsOrders = await Promise.all(patients.map((patient) => this.getPatientOrders(patient.id)))

      this.allPatientsOrders = patients.reduce((prev, patient, index) => {
        prev[patient.id] = patientsOrders[index]

        return prev
      }, {})
    })
  }

  get allPatientsResp(): PatientResponse {
    return this._allPatientsResp.getValue() || <PatientResponse>{}
  }

  set allPatientsResp(val: PatientResponse) {
    this._allPatientsResp.next(val)
  }

  get allPatientsOrders() {
    return this._allPatientsOrders.getValue() || {}
  }

  set allPatientsOrders(val: Object) {
    this._allPatientsOrders.next(val)
  }
  
  get upcomingOrdersPatientsOrders() {
    return this._upcomingOrdersPatientsOrders.getValue() || {}
  }

  set upcomingOrdersPatientsOrders(val: Object) {
    this._upcomingOrdersPatientsOrders.next(val)
  }

  async fetchPatients(userId: number, userSiteId: number, departmentCode: string, wardCode: string) {

    console.log('AllPatientsStoreService: fetchPatients')
    this.allPatientsResp = await this.patientService.getPatients(userSiteId, userId, departmentCode, wardCode).toPromise()
    console.log('AllPatientsStoreService: fetchPatients: ', this.allPatientsResp)
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
