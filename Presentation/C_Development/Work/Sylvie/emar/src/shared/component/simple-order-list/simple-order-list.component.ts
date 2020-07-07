import { Component, OnInit, Input } from '@angular/core';

import { Medication } from '../../../app/interfaces/medication';

@Component({
  selector: 'simple-order-list',
  templateUrl: './simple-order-list.component.html',
  styleUrls: ['./simple-order-list.component.scss', '../../../assets/css/site.css']
})
export class SimpleOrderListComponent implements OnInit {

  displayItems: Medication;
  
  @Input() listName: string;
  @Input() orderOnClick1: any;
  @Input() orderOnClick2: any;
  @Input() set items(data) {
    this.displayItems = data;
  }

  constructor() { }

  ngOnInit(): void {
  }

}
