import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { PatientDashboardComponent } from '../patient-dashboard/patient-dashboard.component';
import { GivenTemplateModalComponent } from './given-template-modal/given-template-modal.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [
    PatientDashboardComponent,
    GivenTemplateModalComponent
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ReactiveFormsModule,
    NgbModule
  ],
  exports: [
    PatientDashboardComponent
  ]
})
export class PatientDashboardModule {

}