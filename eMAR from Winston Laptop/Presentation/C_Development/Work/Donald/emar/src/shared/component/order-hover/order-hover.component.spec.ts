import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderHoverComponent } from './order-hover.component';

describe('OrderHoverComponent', () => {
  let component: OrderHoverComponent;
  let fixture: ComponentFixture<OrderHoverComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OrderHoverComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderHoverComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
