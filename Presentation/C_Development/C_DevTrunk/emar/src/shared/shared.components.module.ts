import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule } from '@angular/forms';

import { HeaderUserComponent } from './component/header-user/header-user.component';
import { HeaderPatientComponent } from './component/header-patient/header-patient.component';
import { SimpleOrderListComponent } from './component/simple-order-list/simple-order-list.component';
import { ModalComponent } from './component/modal/modal.component';
import { MedSearchComponent } from './component/med-search/med-search.component';


@NgModule({
  declarations: [
    HeaderUserComponent, 
    HeaderPatientComponent, 
    SimpleOrderListComponent, 
    ModalComponent, 
    MedSearchComponent,
  ],
  imports: [
    CommonModule,
    NgbModule,
    FormsModule,
  ],
  exports: [
    HeaderUserComponent,
    HeaderPatientComponent,
    SimpleOrderListComponent,
    ModalComponent,
    MedSearchComponent,
  ],
  bootstrap: [
    MedSearchComponent,
  ]
})
export class SharedComponentsModule { }
