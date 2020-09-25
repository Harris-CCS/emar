import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';

import { UserStoreService } from '../../../services/user-store.service';
import { PatientStoreService } from '../../../services/patient-store.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-bootstrap',
  templateUrl: './bootstrap.component.html',
  styleUrls: ['./bootstrap.component.scss']
})
export class BootstrapComponent implements OnInit {

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private authService: AuthService,
  ) { }

  async ngOnInit(): Promise<void> {
    console.log('BootstrapComponent: I am here')

    const patientId: number = +this.route.snapshot.queryParams['patientId']
    const userId: number = +this.route.snapshot.queryParams['userId']
    // const groupIds: number = +this.route.snapshot.queryParams['group']
    const ibex: number = +this.route.snapshot.queryParams['ibex']
    const drsNum: number = +this.route.snapshot.queryParams['drs']
    const dest: string = this.route.snapshot.queryParams['dest']

    if (this.route.snapshot.queryParams['userId']) {
      await this.userStoreService.fetchUser(+this.route.snapshot.queryParams['userId'])
      
      this.authService.login()
      console.log('BootstrapComponent: confirmed emarUserId: ', this.route.snapshot.queryParams['userId'], ' logged IN.  SITE: ', this.userStoreService.userSiteId)

    } else if (this.route.snapshot.queryParams['drs']) {
      await this.userStoreService.fetchUserByExtId(this.route.snapshot.queryParams['drs'])
      
      this.authService.login()
      console.log('BootstrapComponent: confirmed DRS: ', this.route.snapshot.queryParams['drs'], ' logged IN.  SITE: dunno. Do NOT have this INFO')
    }

    if (this.route.snapshot.queryParams['patientId']) {
      await this.patientStoreService.fetchPatient(this.route.snapshot.queryParams['patientId'])

    } else if (this.route.snapshot.queryParams['ibex']) {
      // await this.patientStoreService.fetchPatient(this.userStoreService.userSiteId, this.route.snapshot.queryParams['ibex'])
      try {
        //TODO: need PCED ibex site id NOT emar patient/user site id
        await this.patientStoreService.fetchPatientByExtIds(this.userStoreService.userSiteId, this.route.snapshot.queryParams['ibex'])
      } catch (error) {
        console.log('BootstrapComponent: STORE FETCHPATIENT by extID ERROR: ', error)
      }
      console.log('BootstrapComponent: fetch patient by IBEX: ', this.route.snapshot.queryParams['ibex'], '  SITE: ', this.userStoreService.userSiteId)
    }

    console.log('BootstrapComponent: route snapshot: ', this.route.snapshot)
    // console.log('BootstrapComponent: route snapshot groupIds: ', groupIds)

    //this.router.navigate(['/patients'])
    if (this.route.snapshot.queryParams['dest'] === 'medservice') {
      console.log(`BootstrapComponent: NAVIGATE to patients/${this.patientStoreService.patientId}/orders`)
      this.router.navigate([`patients/${this.patientStoreService.patientId}/orders`])
    } else if (this.route.snapshot.queryParams['dest'] === 'marpatient') {
      this.router.navigate([`patients/${this.patientStoreService.patientId}`])
    } else {
      this.router.navigate(['patients'])
    }
  }

}
