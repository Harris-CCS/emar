import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SimpleOrderListComponent } from './simple-order-list.component';

describe('SimpleOrderListComponent', () => {
  let component: SimpleOrderListComponent;
  let fixture: ComponentFixture<SimpleOrderListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SimpleOrderListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SimpleOrderListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
