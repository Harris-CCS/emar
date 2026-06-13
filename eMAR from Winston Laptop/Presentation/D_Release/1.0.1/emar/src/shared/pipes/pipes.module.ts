import { NgModule } from '@angular/core';
import { Age } from './age';
import { DateTimePipe } from './dateTime';
import { OrderAdministrationEvent } from './orderAdministrationEvent';
import { trimAfter } from './trimAfter';
import { truncate } from './truncate';

@NgModule({
  declarations: [
    Age,
    DateTimePipe,
    OrderAdministrationEvent,
    trimAfter,
    truncate
  ],
  exports: [
    Age,
    DateTimePipe,
    OrderAdministrationEvent,
    trimAfter,
  truncate
],
})
export class PipesModule { }
