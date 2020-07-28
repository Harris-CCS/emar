import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedComponentsModule } from '../../shared/shared.components.module';
import { ComposerMedModule } from '../composer-med/composer-med.module';
import { ComposerMedSelfComponent } from './composer-med-self.component';

@NgModule({
  declarations: [
    ComposerMedSelfComponent
  ],
  imports: [
    CommonModule,
    SharedComponentsModule,
    ComposerMedModule
  ],
  exports: [
  ]
})
export class ComposerMedSelfModule { }