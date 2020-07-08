import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ComposerMedComponent } from './composer-med.component';

@NgModule({
  declarations: [ComposerMedComponent],
  imports: [
    CommonModule
  ],
  exports: [
    ComposerMedComponent,
  ]
})
export class ComposerMedModule { }
