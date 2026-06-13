import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'Age',
})
export class Age implements PipeTransform {
  transform(value: any, age: any, ageUnit: any): string {
    let unit = '';

    if (ageUnit.indexOf('year') === 0) {
      unit = ' y/o';
    } else if (ageUnit.indexOf('day') === 0) {
      unit = ' d/o';
    } else if (ageUnit.indexOf('month') === 0) {
      unit = ' m/o';
    } else if (ageUnit.indexOf('hour') === 0) {
      unit = ' h/o';
    }
    return `${age}${unit}`;
  }
}
