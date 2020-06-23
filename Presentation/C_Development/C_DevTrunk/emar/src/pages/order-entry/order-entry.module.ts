import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { OrderEntryComponent } from './order-entry.component';
import { QuickListComponent } from './tabs/quick-list/quick-list.component';
import { GroupsComponent } from './tabs/groups/groups.component';
import { DeptPreferredComponent } from './tabs/dept-preferred/dept-preferred.component';

@NgModule({
  declarations: [
    OrderEntryComponent,
    QuickListComponent,
    GroupsComponent,
    DeptPreferredComponent,
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
  ],
  exports: [
    OrderEntryComponent,
  ]
})
export class OrderEntryModule { }
