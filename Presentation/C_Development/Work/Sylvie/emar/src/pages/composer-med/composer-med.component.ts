import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Patient } from 'src/app/interfaces/patient';

import { PatientService } from 'src/app/services/patient.service';

@Component({
  selector: 'composer-med',
  templateUrl: './composer-med.component.html',
  styleUrls: ['./composer-med.component.scss','../../assets/css/site.css']
})
export class ComposerMedComponent implements OnInit {
  patient: Patient;

  constructor(private route: ActivatedRoute,
    private patientService: PatientService) { }

  ngOnInit(): void {
    const patientId:number = +this.route.snapshot.params['id'];
    this.patient = this.patientService.getPatient(patientId);
  }
  selectedPatient() {
    return this.patient;
  }
}
