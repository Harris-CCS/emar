import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MedSearchComponent } from './med-search.component';

describe('MedSearchComponent', () => {
  let component: MedSearchComponent;
  let fixture: ComponentFixture<MedSearchComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MedSearchComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MedSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
