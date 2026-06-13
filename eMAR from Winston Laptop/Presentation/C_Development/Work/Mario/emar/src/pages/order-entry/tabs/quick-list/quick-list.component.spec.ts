import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickListComponent } from './quick-list.component';

describe('QuickListComponent', () => {
  let component: QuickListComponent;
  let fixture: ComponentFixture<QuickListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ QuickListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(QuickListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
