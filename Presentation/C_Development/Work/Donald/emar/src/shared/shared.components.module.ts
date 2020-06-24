import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { HeaderUserComponent } from './component/header-user/header-user.component';
import { HeaderPatientComponent } from './component/header-patient/header-patient.component';
import { SimpleOrderListComponent } from './component/simple-order-list/simple-order-list.component';


@NgModule({
  declarations: [
    HeaderUserComponent, 
    HeaderPatientComponent, SimpleOrderListComponent,
  ],
  imports: [
    CommonModule
  ],
  exports: [
    HeaderUserComponent,
    HeaderPatientComponent,
    SimpleOrderListComponent,
  ]
})
export class SharedComponentsModule { }
