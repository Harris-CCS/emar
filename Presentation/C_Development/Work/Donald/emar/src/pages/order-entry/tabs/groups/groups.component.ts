import { Component, OnInit } from '@angular/core';

import { MEDICATIONS } from 'src/app/mockup/medications';

@Component({
  selector: 'groups',
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.scss']
})
export class GroupsComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  groups() {
    return 'groups';
  }

  groupsOrders() {
    return MEDICATIONS.slice(10, 12);
  }
}
