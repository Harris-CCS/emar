import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'Age',
})
export class Age implements PipeTransform {
  transform(value: any, age: any, ageUnit: any): string {
    let unit = '';

    if (ageUnit === 'Y') {
      unit = ' yrs';
    } else if (ageUnit === 'D') {
      unit = ' days';
    } else if (ageUnit === 'M') {
      unit = ' months';
    } else if (ageUnit === 'H') {
      unit = ' hrs';
    }
    return `${age}${unit}`;
  }
}
