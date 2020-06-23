import { Component, OnInit } from '@angular/core';

import { MEDICATIONS } from 'src/app/mockup/medications';

@Component({
  selector: 'quick-list',
  templateUrl: './quick-list.component.html',
  styleUrls: ['./quick-list.component.scss']
})
export class QuickListComponent implements OnInit {

  quickList() {
    return 'ql';
  }
  
  quickListOrders() {
    return MEDICATIONS.slice(100, 117);
  }

  constructor() {}

  ngOnInit(): void {
  }

}
