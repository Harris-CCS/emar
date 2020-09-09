import { TestBed } from '@angular/core/testing';

import { PatientMedOrderStoreService } from './patient-med-order-store.service';

describe('PatientMedOrderStoreService', () => {
  let service: PatientMedOrderStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PatientMedOrderStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
