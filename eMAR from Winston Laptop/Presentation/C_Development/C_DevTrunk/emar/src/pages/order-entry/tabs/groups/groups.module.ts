import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { GroupsComponent } from './groups.component';
import { SharedComponentsModule } from '../../../../shared/shared.components.module';

@NgModule({
  declarations: [
    GroupsComponent,
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
  ]
})
export class GroupsModule { }
