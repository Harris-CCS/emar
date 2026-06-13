import { Component, OnInit } from '@angular/core';

import { Patient } from '../../app/interfaces/patient';
import { PatientService } from '../../services/patient.service';
import { MedOrderService } from '../../services/med-order.service';

@Component({
  selector: 'patients-dashboard',
  templateUrl: './patients-dashboard.component.html',
  styleUrls: ['./patients-dashboard.component.scss']
})
export class PatientsDashboardComponent implements OnInit {

  patients: Patient[];

  constructor(
    private patientService: PatientService,
    private medOrderService: MedOrderService,
  ) { }

  ngOnInit(): void {
    this.getPatients();
  }

  getPatients(): void {
    // this.patientService.getPatients()
    //   .subscribe(patientsRes => this.patients = patientsRes.patients);
  }
}
