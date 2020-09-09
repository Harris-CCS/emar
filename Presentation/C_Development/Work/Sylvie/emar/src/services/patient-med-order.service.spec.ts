import { TestBed } from '@angular/core/testing';

import { PatientMedOrderService } from './patient-med-order.service';

describe('PatientMedOrderService', () => {
  let service: PatientMedOrderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PatientMedOrderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
