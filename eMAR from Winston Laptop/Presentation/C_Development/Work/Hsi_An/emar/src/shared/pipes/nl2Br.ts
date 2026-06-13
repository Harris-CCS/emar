import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'nl2Br',
  })
  export class nl2Br implements PipeTransform {
    transform(value: string) {
      if (value === null) {
        return '';
      }
      return value.replace(/\n/g, '<br/>');
    }
}