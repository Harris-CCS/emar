import { TestBed } from '@angular/core/testing';

import { PharmVerificationStoreService } from './pharm-verification-store.service';

describe('PharmVerificationStoreService', () => {
  let service: PharmVerificationStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PharmVerificationStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
