import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ReactiveFormsModule } from '@angular/forms';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { PatientRoutingModule } from './patient-routing.module'
import { PatientComponent } from './patient.component'

@NgModule({
  declarations: [
    PatientComponent,
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ReactiveFormsModule,
    NgbModule,
    PatientRoutingModule,
  ],
})
export class PatientModule { }
