import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { OrderEntryComponent } from './order-entry.component';
import { QuickListComponent } from './tabs/quick-list/quick-list.component';
import { GroupsComponent } from './tabs/groups/groups.component';
import { DeptPreferredComponent } from './tabs/dept-preferred/dept-preferred.component';
import { OrderCartListComponent } from './order-cart/order-cart-list/order-cart-list.component';
import { ComposerMedModalComponent } from './composer-med-modal/composer-med-modal.component';
import { ComposerMedModule } from '../composer-med/composer-med.module';
import { RouterModule } from '@angular/router';
import { InteractionModalComponent } from './interaction-modal/interaction-modal.component';
import { ReactiveFormsModule } from '@angular/forms';
import { DeleteAllCartOrdersModalComponent } from './delete-all-cart-orders-modal/delete-all-cart-orders-modal.component';
import { SignCartOrderModalComponent } from './sign-cart-order-modal/sign-cart-order-modal.component';
import { PatientDashboardModule } from 'src/pages/patient-dashboard/patient-dashboard.module'
import { GivenTemplateModalComponent } from '../patient-dashboard/given-template-modal/given-template-modal.component';

@NgModule({
  declarations: [
    OrderEntryComponent,
    QuickListComponent,
    GroupsComponent,
    DeptPreferredComponent,
    OrderCartListComponent,
    ComposerMedModalComponent,
    InteractionModalComponent,
    DeleteAllCartOrdersModalComponent,
    SignCartOrderModalComponent,
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ComposerMedModule,
    RouterModule,
    ReactiveFormsModule,
    NgbModule,
    PatientDashboardModule,
  ],
  exports: [
    OrderEntryComponent,
  ]
})
export class OrderEntryModule { }
