import { TestBed } from '@angular/core/testing';

import { PatientStoreService } from './patient-store.service';

describe('PatientStoreService', () => {
  let service: PatientStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PatientStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
