import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe, formatDate } from '@angular/common';
import * as moment from 'moment';

// TODO API
const TIME_FORMAT = 'HH:mm';
const DATE_FORMAT = 'MM/dd/yyyy';

@Pipe({
  name: 'dateTimePipe',
})
export class DateTimePipe implements PipeTransform {
  transform(value: string, format: string, timeZoneOffset?: string) {
    const datePipe = new DatePipe("en-US");
    if (format === 'date') {
      value = datePipe.transform(value, 'MM-dd-yyyy', timeZoneOffset);
    }
    else if (format === 'dateDisplay') {
      value = datePipe.transform(value, 'MM/dd/yyyy', timeZoneOffset);
    }
    else if (format === 'time') {
      value = datePipe.transform(value, 'HH:mm', timeZoneOffset);
    }
    else if (format === 'UTC') {
      value = datePipe.transform(value, 'yyyy-MM-ddTHH:mm:ssZZZZZ', timeZoneOffset);
    }
    else if (format === 'dateTime') {
      value = datePipe.transform(value, 'yyyy-MM-dd HH:mm', timeZoneOffset);
    }
    else if (format === 'dateTimeSeconds') {
      value = datePipe.transform(value, 'yyyy-MM-ddTHH:mm:ss', timeZoneOffset);
    }
    else if (format === 'dateTimeDisplay') {
      value = datePipe.transform(value, 'MM/dd/yyyy HH:mm', timeZoneOffset);
    }
    else if (format === 'friendly') {
      if (value == null) return '';
      const mo = (value === 'now') ? moment() : moment(value);
      if (!mo.isValid()) return '';
      value = datePipe.transform(mo.format(), TIME_FORMAT, timeZoneOffset);
      /* datepipe uses MM/dd/yyyy, moment uses MM/DD/yyyy */
      if (mo.format('MM/DD/yyyy') != moment().format('MM/DD/yyyy')) {
        value = value + ' ' + datePipe.transform(mo.format(), DATE_FORMAT, timeZoneOffset);
      }
    }
    else if (format === 'friendlyDate') {
      if (value == null) return '';
      const mo = (value === 'now') ? moment() : moment(value);
      return datePipe.transform(mo.format(), DATE_FORMAT, timeZoneOffset);
    }
    return value;
  }
}
