import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs'
import { ActivatedRoute, ParamMap } from '@angular/router';
import { takeUntil } from 'rxjs/operators'

import { PatientStoreService } from '../../services/patient-store.service'

@Component({
  selector: 'pages-patient',
  templateUrl: './patient.component.html',
  styleUrls: ['./patient.component.scss']
})
export class PatientComponent implements OnInit, OnDestroy {
  hasLoaded = new Subject<boolean>()
  ngUnsubscribe = new Subject<void>();


  constructor(
    private route: ActivatedRoute,
    private patientStoreService: PatientStoreService,
  ) { }

  ngOnInit(): void {
    console.log('PatientComponent.ngOnInit')
    

    this.route.paramMap
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((params: ParamMap) => {
        const patientId = params.get('patientId')

        console.log('---', patientId)
        if (patientId) {
          this.fetchPatient(patientId)
        }
      })
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  async fetchPatient(patientId) {
    console.log('---- getting patient', patientId)
    
    await this.patientStoreService.fetchPatient(patientId)
    
    this.hasLoaded.next(true)
  }
}
