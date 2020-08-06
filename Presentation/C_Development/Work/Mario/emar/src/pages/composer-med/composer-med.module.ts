import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { ComposerMedComponent } from './composer-med.component';
import { DetailFormComponent } from './detail-form/detail-form.component';
import { FrequencyFormComponent } from './frequency-form/frequency-form.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { CalendarFormComponent } from './calendar-form/calendar-form.component';
import { SharedComponentsModule } from 'src/shared/shared.components.module';

@NgModule({
  declarations: [
    ComposerMedComponent,
    DetailFormComponent,
    FrequencyFormComponent,
    CalendarFormComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgbModule,
    FormsModule,
    SharedComponentsModule
  ],
  exports: [
    ComposerMedComponent,
  ]
})
export class ComposerMedModule { }
