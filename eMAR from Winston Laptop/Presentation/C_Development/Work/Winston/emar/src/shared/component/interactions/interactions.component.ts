import { Component, OnInit, Input } from '@angular/core';
import { Order } from '../../../app/interfaces/order';

@Component({
  selector: 'interactions',
  templateUrl: './interactions.component.html',
  styleUrls: ['./interactions.component.scss'],
})
export class InteractionsComponent implements OnInit {
    @Input() order: Order;
    @Input() placement: string = "";
    @Input() resolution: string = ""; //"unresolved"

  constructor() {}

  ngOnInit(): void {
  }

}