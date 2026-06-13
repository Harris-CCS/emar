import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserPrinterInfoModalComponent } from './user-printer-info-modal.component';

describe('UserPrinterInfoModalComponent', () => {
  let component: UserPrinterInfoModalComponent;
  let fixture: ComponentFixture<UserPrinterInfoModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UserPrinterInfoModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UserPrinterInfoModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
