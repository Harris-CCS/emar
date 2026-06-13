import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs'
import { ActivatedRoute, ParamMap } from '@angular/router';
import { takeUntil } from 'rxjs/operators'

import { PatientStoreService } from '../../services/patient-store.service'
import { UserStoreService } from '../../services/user-store.service';
import { Site } from 'src/app/interfaces/site';

@Component({
  selector: 'pages-patient',
  templateUrl: './patient.component.html',
  styleUrls: ['./patient.component.scss']
})
export class PatientComponent implements OnInit, OnDestroy {
  hasLoaded = new Subject<boolean>()
  ngUnsubscribe = new Subject<void>();
  correctSite: boolean = true;
  patientSite: string;

  constructor(
    private route: ActivatedRoute,
    private patientStoreService: PatientStoreService,
    private userStoreService: UserStoreService,
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

      this.userStoreService.user$.subscribe(() => {
        this.checkSite();
      });
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  async fetchPatient(patientId) {
    console.log('---- getting patient', patientId)
    
    await this.patientStoreService.fetchPatient(patientId)
    
    this.hasLoaded.next(true)

    this.checkSite();
  }

  checkSite() {
    const us: Site = this.userStoreService.userSite;
    const ps: Site = this.patientStoreService.patientSite;
    if (ps != null && us != null) {
      this.correctSite = us.id == ps.id;
      this.patientSite = ps.name;
    } else {
      this.correctSite = true;
    }
  }
}
