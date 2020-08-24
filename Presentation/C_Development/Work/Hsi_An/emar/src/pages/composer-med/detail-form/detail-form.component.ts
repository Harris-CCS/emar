import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';

@Component({
  selector: 'detail-form',
  templateUrl: './detail-form.component.html',
  styleUrls: ['../composer-med.component.scss'],
})
export class DetailFormComponent implements OnInit {
  @Output() formReady = new EventEmitter<FormGroup>();
  detailForm: FormGroup;
  diagnoses: string[] = ['Hypertension', 'Diabetes', 'Back pain']; //TODO get from service
  indications: string[] = ['Sepsis', 'Pneumonia']; //TODO get from service
  mandatoryIndication: boolean = true; //TODO get from service
  selectedDiagnosis: string = ''; //TODO from service
  selectedIndication: string = ''; //TODO from service
  otherIndication: string = ''; //TODO get from service

  constructor(
    private fb: FormBuilder,
    private composerSchedulerService: ComposerSchedulerService
  ) {}

  ngOnInit() {
    if (this.mandatoryIndication) {
      this.detailForm = this.fb.group(
        {
          // diagnosis: null,
          // antimicrobialIndication: null,
          // otherAntimicrobialIndication: null,
          diagnosis: new FormControl(null),
          antimicrobialIndication: new FormControl(null, [
            Validators.required,
            this.indicationValidator,
          ]),
          otherAntimicrobialIndication: new FormControl(null),
        }
        // { validators: this.validator.bind(this) }
      );
    } else {
      this.detailForm = this.fb.group({
        diagnosis: new FormControl(null),
      });
    }
    // this.formReady.emit(this.detailForm);
    this.composerSchedulerService.addFormGroup('detail', this.detailForm);
    this.composerSchedulerService.performFormReset.subscribe(() => {
      if (this.composerSchedulerService.performFormReset.value) {
        this.resetDetailForm();
      }
    });
  }

  resetDetailForm(): void {
    this.selectedDiagnosis = '';
    this.selectedIndication = '';
  }

  changeSelectedDiagnosis(diagnosis: string) {
    this.selectedDiagnosis = diagnosis;
    this.detailForm.controls['diagnosis'].setValue(diagnosis);

    // console.log('changeSelectedDiagnosisThis', this);
  }

  changeSelectedIndication(indication: string) {
    this.selectedIndication = indication;
    // if (indication === '-- indication --') {
    //   this.detailForm.controls['antimicrobialIndication'].setValue('');
    //   this.detailForm.controls['otherAntimicrobialIndication'].setValue('');
    // } else {
    this.detailForm.controls['antimicrobialIndication'].setValue(indication);
    this.detailForm.controls['otherAntimicrobialIndication'].setValue('');

    // console.log('changeSelectedIndicationThis', this);
  }

  changeOtherIndication() {
    // this.selectedIndication = this.detailForm.controls[
    //   'otherAntimicrobialIndication'
    // ].value;
    // this.detailForm.controls['otherAntimicrobialIndication'].setValue('');
    this.changeSelectedIndication(
      this.detailForm.controls['otherAntimicrobialIndication'].value
    );
  }

  validator() {
    if (typeof this.detailForm === 'undefined') {
      return null; //TODO why do I have to add this test
    }
    if (this.selectedIndication != '') {
      return null;
    }
    return { atLeastOne: true };
  }

  indicationValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return { error: '** Indication is required' };
    }
    return null;
  }
}
