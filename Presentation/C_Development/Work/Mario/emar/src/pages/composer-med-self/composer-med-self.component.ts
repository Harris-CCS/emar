import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PatientService } from 'src/services/patient.service';
import { Patient } from 'src/app/interfaces/patient';

@Component({
  selector: 'composer-med-self',
  templateUrl: './composer-med-self.component.html',
  styleUrls: ['./composer-med-self.component.scss', '../../assets/css/site.css']
})
export class ComposerMedSelfComponent implements OnInit {
  patient: Patient;

  constructor(
    private route: ActivatedRoute,
    private patientService: PatientService) { }

  ngOnInit(): void {
    const patientId:number = +this.route.snapshot.params['id'];
    this.patientService.getPatient(patientId)
      .subscribe(patient => this.patient = patient);
  }

  selectedPatient() {
    return this.patient;
  }
}
