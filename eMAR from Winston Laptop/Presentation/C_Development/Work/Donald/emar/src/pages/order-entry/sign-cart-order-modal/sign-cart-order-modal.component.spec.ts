import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SignCartOrderModalComponent } from './sign-cart-order-modal.component';

describe('SignCartOrderModalComponent', () => {
  let component: SignCartOrderModalComponent;
  let fixture: ComponentFixture<SignCartOrderModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SignCartOrderModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SignCartOrderModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
