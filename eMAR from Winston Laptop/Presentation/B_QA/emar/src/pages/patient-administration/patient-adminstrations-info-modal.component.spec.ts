import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientAdminstrationsInfoModalComponent } from './patient-adminstrations-info-modal.component';

describe('PatientAdminstrationsInfoModalComponent', () => {
  let component: PatientAdminstrationsInfoModalComponent;
  let fixture: ComponentFixture<PatientAdminstrationsInfoModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PatientAdminstrationsInfoModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PatientAdminstrationsInfoModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
