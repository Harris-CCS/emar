import { TestBed } from '@angular/core/testing';

import { MedOrderService } from './med-order.service';

describe('MedOrderService', () => {
  let service: MedOrderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MedOrderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
