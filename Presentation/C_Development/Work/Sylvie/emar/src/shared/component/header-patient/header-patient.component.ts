import { Component, OnInit, Input } from '@angular/core';

import { Patient } from '../../../app/interfaces/patient';

@Component({
  selector: 'header-patient',
  templateUrl: './header-patient.component.html',
  styleUrls: ['./header-patient.component.scss', '../../../assets/css/site.css']
})
export class HeaderPatientComponent implements OnInit {

  @Input() patient: Patient;

  constructor() { }

  ngOnInit(): void {
  }

}
