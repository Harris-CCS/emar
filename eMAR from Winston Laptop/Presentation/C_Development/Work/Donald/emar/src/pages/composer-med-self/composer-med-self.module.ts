import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { ComposerMedModule } from '../composer-med/composer-med.module';
import { ComposerMedSelfComponent } from './composer-med-self.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    ComposerMedSelfComponent
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ComposerMedModule,
    ReactiveFormsModule
  ],
  exports: [
  ]
})
export class ComposerMedSelfModule { }