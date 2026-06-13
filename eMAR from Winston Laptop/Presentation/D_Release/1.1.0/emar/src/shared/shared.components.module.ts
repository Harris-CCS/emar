import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { HeaderUserComponent } from './component/header-user/header-user.component';
import { HeaderPatientComponent } from './component/header-patient/header-patient.component';
import { SimpleOrderListComponent } from './component/simple-order-list/simple-order-list.component';
import { ModalComponent } from './component/modal/modal.component';
import { MedSearchComponent } from './component/med-search/med-search.component';
import { PipesModule } from './pipes/pipes.module';
import { SimpleTableComponent } from './component/simple-table/simple-table.component';
import { DateTimeModalComponent } from './component/date-time-modal/date-time-modal.component';
import { NotAuthComponent } from './component/not-auth/not-auth.component';
// import { BootstrapComponent } from './component/bootstrap/bootstrap.component';
import { HelpIconsComponent } from './component/help-icons/help-icons.component';
import { NotificationsComponent } from './component/notifications/notifications.component';
import { DosingInfoComponent } from './component/dosing-info/dosing-info.component';
import { ExternalComponent } from './component/external/external.component';
import { PatientBriefComponent } from './component/patient-brief/patient-brief.component';
import { PatientOrderInteractionsComponent } from './component/patient-order-interactions/patient-order-interactions.component';
import { ThreeStateButtonComponent } from './component/three-state-button/three-state-button.component';
import { InteractionsComponent } from './component/interactions/interactions.component';
import { OrderHoverComponent } from './component/order-hover/order-hover.component';

@NgModule({
  declarations: [
    HeaderUserComponent,
    HeaderPatientComponent,
    SimpleOrderListComponent,
    ModalComponent,
    MedSearchComponent,
    SimpleTableComponent,
    DateTimeModalComponent,
    NotAuthComponent,
    // BootstrapComponent,
    HelpIconsComponent,
    NotificationsComponent,
    DosingInfoComponent,
    ExternalComponent,
    PatientBriefComponent,
    PatientOrderInteractionsComponent,
    ThreeStateButtonComponent,
    InteractionsComponent,
    OrderHoverComponent,
  ],
  imports: [
    CommonModule,
    NgbModule,
    FormsModule,
    PipesModule,
    ReactiveFormsModule,
    RouterModule,
  ],
  exports: [
    HeaderUserComponent,
    HeaderPatientComponent,
    SimpleOrderListComponent,
    ModalComponent,
    MedSearchComponent,
    SimpleTableComponent,
    DateTimeModalComponent,
    // BootstrapComponent,
    DateTimeModalComponent,
    DosingInfoComponent,
    PatientBriefComponent,
    PatientOrderInteractionsComponent,
    ThreeStateButtonComponent,
    InteractionsComponent,
    OrderHoverComponent,
  ],
  bootstrap: [MedSearchComponent],
})
export class SharedComponentsModule { }
