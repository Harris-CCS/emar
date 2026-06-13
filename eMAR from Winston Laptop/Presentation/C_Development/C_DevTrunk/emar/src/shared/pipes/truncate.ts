import { Pipe, PipeTransform } from '@angular/core';
const DEFAULT = 10;

@Pipe({
    name: 'truncate',
  })
  export class truncate implements PipeTransform {
    transform(value: string, limit?: number, trail?: string): string {
    if (limit == null) limit = DEFAULT;
    if (trail == null) trail = '…';

    return value.length > limit ? value.substring(0, limit) + trail : value;
    }
}