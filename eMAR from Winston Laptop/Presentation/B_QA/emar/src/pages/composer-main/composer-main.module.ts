import { NgModule } from '@angular/core';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedComponentsModule } from '../../shared/shared.components.module';
import { ComposerMainComponent } from './composer-main.component';
import { ComposerMedModule } from '../composer-med/composer-med.module';
import { DetailFormComponent } from '../composer-med/detail-form/detail-form.component';
import { StrengthModalComponent } from '../composer-med/strength-modal/strength-modal.component';

@NgModule({
  declarations: [
    ComposerMainComponent,
    StrengthModalComponent,
    DetailFormComponent
  ],
  imports: [
    CommonModule,
    NgbModule,
    ComposerMedModule,
    ReactiveFormsModule,
    SharedComponentsModule,
  ],
  exports: [ComposerMainComponent],
})
export class ComposerMainModule { }
