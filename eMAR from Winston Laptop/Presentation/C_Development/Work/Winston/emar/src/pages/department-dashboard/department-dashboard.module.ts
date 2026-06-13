import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponentsModule } from '../../shared/shared.components.module';
import { DepartmentDashboardComponent } from '../department-dashboard/department-dashboard.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { PipesModule } from 'src/shared/pipes/pipes.module';
// import { PatientDashboardModule } from '../patient-dashboard/patient-dashboard.module';
import { OrderInfoHoverComponent } from './order-info-hover/order-info-hover.component';
// May not need these for the Department MAR
// import { GivenTemplateModalComponent } from '../patient-dashboard/given-template-modal/given-template-modal.component';
// import { OrderHoverComponent } from '../patient-dashboard/order-hover/order-hover.component';
// import { FiveRightsComponent } from '../patient-dashboard/five-rights/five-rights-modal.component';


@NgModule({
  declarations: [
    DepartmentDashboardComponent,
    OrderInfoHoverComponent,
    // GivenTemplateModalComponent,
    // OrderHoverComponent,
    // FiveRightsComponent
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ReactiveFormsModule,
    NgbModule,
    FormsModule,
    // PatientDashboardModule,
    PipesModule
  ],
  exports: [
    DepartmentDashboardComponent,
  ]
})
export class DepartmentDashboardModule { }
