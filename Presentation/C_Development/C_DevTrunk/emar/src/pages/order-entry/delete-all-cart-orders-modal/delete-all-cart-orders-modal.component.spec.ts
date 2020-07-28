import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteAllCartOrdersModalComponent } from './delete-all-cart-orders-modal.component';

describe('DeleteAllCartOrdersModalComponent', () => {
  let component: DeleteAllCartOrdersModalComponent;
  let fixture: ComponentFixture<DeleteAllCartOrdersModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DeleteAllCartOrdersModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeleteAllCartOrdersModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
