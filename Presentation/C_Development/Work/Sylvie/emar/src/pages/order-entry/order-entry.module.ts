import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { OrderEntryComponent } from './order-entry.component';
import { QuickListComponent } from './tabs/quick-list/quick-list.component';
import { GroupsComponent } from './tabs/groups/groups.component';
import { DeptPreferredComponent } from './tabs/dept-preferred/dept-preferred.component';
import { OrderCartListComponent } from './order-cart/order-cart-list/order-cart-list.component';
import { ComposerMedModalComponent } from './composer-med-modal/composer-med-modal.component';
import { ComposerMedModule } from '../composer-med/composer-med.module';
import { InteractionModalComponent } from './interaction-modal/interaction-modal.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    OrderEntryComponent,
    QuickListComponent,
    GroupsComponent,
    DeptPreferredComponent,
    OrderCartListComponent,
    ComposerMedModalComponent,
    InteractionModalComponent
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ComposerMedModule,
    ReactiveFormsModule
  ],
  exports: [
    OrderEntryComponent,
  ]
})
export class OrderEntryModule { }
