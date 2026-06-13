import { TestBed } from '@angular/core/testing';

import { SiteStoreService } from './site-store.service';

describe('SiteStoreService', () => {
  let service: SiteStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SiteStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
