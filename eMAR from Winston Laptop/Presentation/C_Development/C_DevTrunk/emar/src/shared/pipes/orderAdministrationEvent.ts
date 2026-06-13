import { Pipe, PipeTransform } from '@angular/core';

import { Event } from '../../app/interfaces/order';

@Pipe({
  name: 'orderAdministrationEvent',
})
export class OrderAdministrationEvent implements PipeTransform {
  transform(event: Event): string {
    let description = '';
    switch (event.action.actionId) {
        case 1: description = "Acknowledge"; break;
        case 2: description = "Cancel"; break;
        case 3: description = "Complete"; break;
        case 4: description = "Complete Discontinue"; break;
        case 5: description = "Co-Sign"; break;
        case 6: description = "Delete"; break;
        case 7: description = "Follow Up"; break;
        case 8: description = "Give"; break;
        case 9: description = "Hold"; break;
        case 10: description = "Missed Dose"; break;
        case 11: description = "Order Discontinue"; break;
        case 12: description = "Repeat"; break;
        case 13: description = "Reschedule"; break;
        case 14: description = "Un-Hold"; break;
        case 16: description = "Pharmacist Verified"; break;
    }
    return description;
  }
}