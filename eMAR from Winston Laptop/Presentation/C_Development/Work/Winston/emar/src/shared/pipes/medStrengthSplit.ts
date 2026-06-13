import { Pipe, PipeTransform } from '@angular/core';
import { truncate } from './truncate';

@Pipe({
    name: 'medStrengthSplit',
})
//12 Hour Nasal 0.1 % spray, 1-Combo, 
export class medStrengthSplit implements PipeTransform {
    transform(value: string, type:string, limit: number, trail: string): string {
        let pipe = new truncate();
        const i = value.search(/[a-zA-Z]/);
        if (i >= 0) {
            const j = value.substring(i);
            const k = j.search(/\d/);
            if (type == 'm' && k >= 0) {
                value = value.substring(0, i + k);
            }
            if (type == 's' && k >= 0) {
                value = value.substring(i+k);
                limit = limit - i - k;
                if (limit < 0) return '';
            }
        }
        return pipe.transform(value, limit, trail);
    }
}