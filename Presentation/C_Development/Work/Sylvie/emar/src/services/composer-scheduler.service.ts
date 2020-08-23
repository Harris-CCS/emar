import { Injectable } from '@angular/core';
import {
  Form,
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Observable, of, Subject, BehaviorSubject } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  tap,
  switchMap,
} from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ComposerSchedulerService {
  composerMedForm: FormGroup;
  performFormReset: BehaviorSubject<boolean> = new BehaviorSubject(false);

  constructor(private fb: FormBuilder) {
    this.composerMedForm = this.fb.group({});
  }

  addFormGroup(name: string, form: FormGroup) {
    this.composerMedForm.setControl(name, form);
  }

  resetForm(): void {
    this.composerMedForm.reset();
    this.performFormReset.next(true);
  }
}
