import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'medBold',
  })
  // put in bold whatever is before a nummeric (the dose)
  export class medBold implements PipeTransform {
    transform(value: string): string {
        let regexp = /([^0-9]*)(.*)/
        let match = regexp.exec(value);
        return '<b>' + `${match[1]}` + '</b>' + `${match[2]}`;
    }
}