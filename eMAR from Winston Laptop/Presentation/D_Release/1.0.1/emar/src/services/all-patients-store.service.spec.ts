import { TestBed } from '@angular/core/testing';

import { AllPatientsStoreService } from './all-patients-store.service';

describe('AllPatientsStoreService', () => {
  let service: AllPatientsStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AllPatientsStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
