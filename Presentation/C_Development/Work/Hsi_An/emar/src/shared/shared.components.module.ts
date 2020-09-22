import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { HeaderUserComponent } from './component/header-user/header-user.component';
import { HeaderPatientComponent } from './component/header-patient/header-patient.component';
import { SimpleOrderListComponent } from './component/simple-order-list/simple-order-list.component';
import { ModalComponent } from './component/modal/modal.component';
import { MedSearchComponent } from './component/med-search/med-search.component';
import { PipesModule } from './pipes/pipes.module';
import { SimpleTableComponent } from './component/simple-table/simple-table.component';
import { DateTimeModalComponent } from './component/date-time-modal/date-time-modal.component';
import { NotAuthComponent } from './component/not-auth/not-auth.component';
import { BootstrapComponent } from './component/bootstrap/bootstrap.component';
import { HelpIconsComponent } from './component/help-icons/help-icons.component';
import { DosingInfoComponent } from './component/dosing-info/dosing-info.component';

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
    BootstrapComponent,
    HelpIconsComponent,
    DosingInfoComponent,
  ],
  imports: [
    CommonModule,
    NgbModule,
    FormsModule,
    PipesModule,
    ReactiveFormsModule,
  ],
  exports: [
    HeaderUserComponent,
    HeaderPatientComponent,
    SimpleOrderListComponent,
    ModalComponent,
    MedSearchComponent,
    SimpleTableComponent,
    DateTimeModalComponent,
    BootstrapComponent,
    DateTimeModalComponent,
  ],
  bootstrap: [MedSearchComponent],
})
export class SharedComponentsModule {}
