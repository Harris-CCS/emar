import { Component, OnInit, Input } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Medication } from '../../../app/interfaces/medication';

@Component({
  selector: 'simple-order-list',
  templateUrl: './simple-order-list.component.html',
  styleUrls: ['./simple-order-list.component.scss', '../../../assets/css/site.css']
})
export class SimpleOrderListComponent implements OnInit {

  displayItems: Medication;

  @Input() listName: string;
  @Input() set items(data) {
    this.displayItems = data;
  }

  constructor(private router: Router,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
  }

  onAddNewMed() {
    const patientId: number = this.route.snapshot.params['id'];
    this.router.navigate(['/patients',patientId,'new-order']);
  }
}
