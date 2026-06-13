import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';

import { UserStoreService } from '../../../services/user-store.service';

@Component({
  selector: 'external',
  templateUrl: './external.component.html',
  styleUrls: ['./external.component.scss']
})
export class ExternalComponent implements OnInit {

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private userStoreService: UserStoreService,
  ) { }

  ngOnInit(): void {

    let userId: string
    userId = this.route.snapshot.queryParams['userId']
    // sessionStorage.setItem('userId', userId)
    this.userStoreService.userId = Number(userId)
    console.log('ExternalComponent: set sessionStorage userId: ', userId)
    
    const departmentCode = this.route.snapshot.queryParams['dept'] || ''
    // const wardCode = this.route.snapshot.queryParams['ward'] || ''
    const wardCode = this.route.snapshot.queryParams['ward']?.toLowerCase() === 'all' ? '' : (this.route.snapshot.queryParams['ward'] || '')
    const browser = this.route.snapshot.queryParams['browser'] || '';
    this.userStoreService.browser = browser;

    this.userStoreService.departmentCode = departmentCode
    this.userStoreService.wardCode = wardCode
    //this.router.navigate(['/patients'])
    if (this.route.snapshot.queryParams['dest'] === 'medservice' || this.route.snapshot.paramMap.get('dest') === 'medservice') {
      // Medication Service
      // console.log(`ExternalComponent: NAVIGATE to patients/${this.route.snapshot.queryParams['patientId']}/medservice`)
      this.router.navigate([`patients/${this.route.snapshot.queryParams['patientId']}/medservice`])
    } else if (this.route.snapshot.queryParams['dest'] === 'marpatient') {
      let patientId = this.route.snapshot.queryParams['patientId'] || ''
      
      if (patientId) {
        // MAR Patient
        this.router.navigate([`patients/${this.route.snapshot.queryParams['patientId']}`])
      } else {
        // MAR Department
        this.router.navigate([`patients`])
      }
      // this.router.navigate([`patients/${this.route.snapshot.queryParams['patientId']}`])
    // } else {
    //   this.router.navigate(['patients'])
    }

    // if (this.route.snapshot.queryParams['farkId']) {
    //   console.log(`ExternalComponent: NAVIGATE to farks/${this.route.snapshot.queryParams['farkId']}/details`)
    //   this.router.navigate([`farks/${this.route.snapshot.queryParams['farkId']}/details`])
    // }
  }

}
