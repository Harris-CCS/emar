import { TestBed } from '@angular/core/testing';

import { MyPatientsStoreService } from './my-patients-store.service';

describe('DepartmentStoreService', () => {
  let service: MyPatientsStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MyPatientsStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
