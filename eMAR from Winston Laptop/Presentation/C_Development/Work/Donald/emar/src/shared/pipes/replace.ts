import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'replace',
  })
  export class replace implements PipeTransform {
    transform(value: string, from: string, to: string) {
      if (value === null) {
        return '';
      }
      return value.replace(from, to);
    }
}