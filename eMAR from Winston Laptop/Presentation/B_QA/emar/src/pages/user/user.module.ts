import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ReactiveFormsModule } from '@angular/forms';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { UserRoutingModule } from './user-routing.module'
import { UserComponent } from './user.component'

@NgModule({
  declarations: [
    UserComponent,
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ReactiveFormsModule,
    NgbModule,
    UserRoutingModule,
  ],
})
export class UserModule { }
