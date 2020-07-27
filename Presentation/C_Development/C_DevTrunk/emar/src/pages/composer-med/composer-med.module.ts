import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { ComposerMedComponent } from './composer-med.component';
import { DetailFormComponent } from './detail-form/detail-form.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [
    ComposerMedComponent,
    DetailFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgbModule,
    FormsModule
  ],
  exports: [
    ComposerMedComponent,
  ]
})
export class ComposerMedModule { }
