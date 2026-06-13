import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrinterAdministrationModalComponent } from './printer-administration-modal.component';

describe('PrinterAdministrationModalComponent', () => {
  let component: PrinterAdministrationModalComponent;
  let fixture: ComponentFixture<PrinterAdministrationModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PrinterAdministrationModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PrinterAdministrationModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
