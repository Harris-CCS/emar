import { Component, Input, OnInit } from '@angular/core';
import { Patient } from '../../../app/interfaces/patient';
import { HeaderPatientComponent } from '../header-patient/header-patient.component';
import { PatientStoreService } from 'src/services/patient-store.service'

@Component({
  selector: 'patient-brief',
  templateUrl: './patient-brief.component.html',
  styleUrls: ['./patient-brief.component.scss']
})
export class PatientBriefComponent implements OnInit {
  @Input() patient: Patient;
  patientComponent: HeaderPatientComponent;
  patientLocation: string;

  constructor(
    private patientStoreService: PatientStoreService,
  ) {

  }

  ngOnInit(): void {
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

}
