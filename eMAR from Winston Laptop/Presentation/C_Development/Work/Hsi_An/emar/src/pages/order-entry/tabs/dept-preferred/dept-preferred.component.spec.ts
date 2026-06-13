import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DeptPreferredComponent } from './dept-preferred.component';

describe('DeptPreferredComponent', () => {
  let component: DeptPreferredComponent;
  let fixture: ComponentFixture<DeptPreferredComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DeptPreferredComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeptPreferredComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
