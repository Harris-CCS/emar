import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeptPreferredWithTabComponent } from './dept-preferred-with-tab.component';

describe('DeptPreferredWithTabComponent', () => {
  let component: DeptPreferredWithTabComponent;
  let fixture: ComponentFixture<DeptPreferredWithTabComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DeptPreferredWithTabComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DeptPreferredWithTabComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
