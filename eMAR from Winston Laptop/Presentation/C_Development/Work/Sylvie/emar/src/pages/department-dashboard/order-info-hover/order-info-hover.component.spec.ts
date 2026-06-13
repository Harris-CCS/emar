import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderInfoHoverComponent } from './order-info-hover.component';

describe('OrderInfoHoverComponent', () => {
  let component: OrderInfoHoverComponent;
  let fixture: ComponentFixture<OrderInfoHoverComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OrderInfoHoverComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderInfoHoverComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
