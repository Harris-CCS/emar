import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'trimAfter',
  })
  export class trimAfter implements PipeTransform {
    transform(value: string, sep: string) {
        const n = value.indexOf(sep);
        if (n > 0) return value.substring(0,n);
        return value;
    }
}