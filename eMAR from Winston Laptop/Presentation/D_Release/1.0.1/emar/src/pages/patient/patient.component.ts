import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs'
import { ActivatedRoute, ParamMap } from '@angular/router';

import { PatientStoreService } from '../../services/patient-store.service'

@Component({
  selector: 'pages-patient',
  templateUrl: './patient.component.html',
  styleUrls: ['./patient.component.scss']
})
export class PatientComponent implements OnInit {
  hasLoaded = new Subject<boolean>()

  constructor(
    private route: ActivatedRoute,
    private patientStoreService: PatientStoreService,
  ) { }

  ngOnInit(): void {
    console.log('PatientComponent.ngOnInit')
    

    this.route.paramMap.subscribe((params: ParamMap) => {
      const patientId = params.get('patientId')

      console.log('---', patientId)
      if (patientId) {
        this.fetchPatient(patientId)
      }
    })
  }

  async fetchPatient(patientId) {
    console.log('---- getting patient', patientId)
    
    await this.patientStoreService.fetchPatient(patientId)
    
    this.hasLoaded.next(true)
  }
}
