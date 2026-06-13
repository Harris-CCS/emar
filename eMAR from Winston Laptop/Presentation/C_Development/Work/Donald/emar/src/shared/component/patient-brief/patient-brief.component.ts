import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Patient } from '../../../app/interfaces/patient';
import { HeaderPatientComponent } from '../header-patient/header-patient.component';
import { PatientStoreService } from 'src/services/patient-store.service'

@Component({
  selector: 'patient-brief',
  templateUrl: './patient-brief.component.html',
  styleUrls: ['./patient-brief.component.scss']
})
export class PatientBriefComponent implements OnInit, OnDestroy {
  @Input() patient: Patient;
  patientComponent: HeaderPatientComponent;
  patientLocation: string;

  constructor(
    private patientStoreService: PatientStoreService,
    private router: Router,
  ) {

  }

  ngOnInit(): void {
    console.log('~~~~~~~~~~MAR DEPT Patient-Brief ngOnInit at ', new Date().toUTCString())
    this.patientComponent = new HeaderPatientComponent(this.patientStoreService)
    this.patientComponent.patient = this.patient;
    // console.log('PatientBriefThis', this);
  }

  getPatientLocation(): string {
    if (this.patientLocation) {
      return this.patientLocation;
    }
    else {
      // const roomBedText: string = this.patient.roomBedCode ? `\n${this.patient.roomBedCode.replace(' ', `\n`)}` : '';
      // return (this.patientLocation = `${this.patient.wardCode ? `${this.patient.wardCode} ` : ''
      //   }${roomBedText ? `${roomBedText} ` : ''}`);
      return (this.patientLocation = `${this.patient.wardCode ? `${this.patient.wardCode} ` : ''}${this.patient.wardCode && this.patient.roomBedCode ? ' ' : ''}${this.patient.roomBedCode || ''}`);
    }
  }

  launchPatientMedicationServices(patientId: number): void {
    console.log('launchPatientMedicationServices: patientId: ', patientId)
    console.log('~~~~~~~~~~MAR DEPT Patient-Brief launchPatientMedicationServices at ', new Date().toUTCString())

    this.router.navigate([`patients/${patientId}/medservice`])
  }


  ngOnDestroy(): void {
    console.log('~~~~~~~~~~MAR DEPT Patient-Brief ngOnDestroy at ', new Date().toUTCString())
  }

}
