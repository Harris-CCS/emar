import { Component, OnInit } from '@angular/core';

import { MEDICATIONS } from '../../../../app/mockup/medications';

@Component({
  selector: 'dept-preferred',
  templateUrl: './dept-preferred.component.html',
  styleUrls: ['./dept-preferred.component.scss']
})
export class DeptPreferredComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  deptPreferred() {
    return 'dp';
  }

  deptPreferredOrders() {
    return MEDICATIONS.slice(30, 40);
  }
}
