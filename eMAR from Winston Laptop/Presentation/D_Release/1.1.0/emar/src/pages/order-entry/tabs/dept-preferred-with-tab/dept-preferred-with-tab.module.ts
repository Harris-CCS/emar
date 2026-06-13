import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DeptPreferredWithTabComponent } from  './dept-preferred-with-tab.component';
import { SharedComponentsModule } from '../../../../shared/shared.components.module';

@NgModule({
  declarations: [
    DeptPreferredWithTabComponent
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
  ]
})
export class DeptPreferredWithTabModule { }
