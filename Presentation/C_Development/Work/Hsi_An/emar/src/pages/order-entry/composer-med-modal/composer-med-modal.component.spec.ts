import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ComposerMedModalComponent } from './composer-med-modal.component';

describe('ComposerMedModalComponent', () => {
  let component: ComposerMedModalComponent;
  let fixture: ComponentFixture<ComposerMedModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ComposerMedModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ComposerMedModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
