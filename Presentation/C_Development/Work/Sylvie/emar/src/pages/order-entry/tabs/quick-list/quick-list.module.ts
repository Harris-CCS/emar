import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { QuickListComponent } from './quick-list.component';
import { SharedComponentsModule } from '../../../../shared/shared.components.module';


@NgModule({
  declarations: [
    QuickListComponent,
  ],
  imports: [
    CommonModule,
    SharedComponentsModule
  ]
})
export class QuickListModule { }
