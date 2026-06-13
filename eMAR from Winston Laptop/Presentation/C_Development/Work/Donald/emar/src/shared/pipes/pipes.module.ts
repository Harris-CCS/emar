import { NgModule } from '@angular/core';
import { Age } from './age';
import { DateTimePipe } from './dateTime';
import { OrderAdministrationEvent } from './orderAdministrationEvent';
import { trimAfter } from './trimAfter';
import { truncate } from './truncate';
import { nl2Br } from './nl2Br';
import { medStrengthSplit } from './medStrengthSplit';
import { replace } from './replace';

@NgModule({
  declarations: [
    Age,
    DateTimePipe,
    OrderAdministrationEvent,
    trimAfter,
    nl2Br,
    medStrengthSplit,
    replace,
    truncate
  ],
  exports: [
    Age,
    DateTimePipe,
    OrderAdministrationEvent,
    trimAfter,
    nl2Br,
    medStrengthSplit,
    replace,
  truncate
],
})
export class PipesModule { }
