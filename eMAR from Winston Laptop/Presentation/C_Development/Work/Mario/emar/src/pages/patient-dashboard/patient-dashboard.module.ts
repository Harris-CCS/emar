import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { PatientDashboardComponent } from '../patient-dashboard/patient-dashboard.component';
import { GivenTemplateModalComponent } from './given-template-modal/given-template-modal.component';
import { OrderHoverComponent } from './order-hover/order-hover.component';
import { FiveRightsComponent } from './five-rights/five-rights-modal.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { PipesModule } from 'src/shared/pipes/pipes.module';
import { UserPrinterInfoModalComponent } from '../printer/printer-user-info/user-printer-info-modal.component';

@NgModule({
  declarations: [
    PatientDashboardComponent,
    GivenTemplateModalComponent,
    OrderHoverComponent,
    FiveRightsComponent,
    UserPrinterInfoModalComponent
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ReactiveFormsModule,
    NgbModule,
    PipesModule
  ],
  exports: [
    PatientDashboardComponent,
    GivenTemplateModalComponent,
  ]
})
export class PatientDashboardModule {

}