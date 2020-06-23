import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DeptPreferredComponent } from './dept-preferred.component';
import { SharedComponentsModule } from '../../../../shared/shared.components.module';

@NgModule({
  declarations: [
    DeptPreferredComponent,
  ],
  imports: [
    CommonModule,
    SharedComponentsModule
  ]
})
export class DeptPreferredModule { }
