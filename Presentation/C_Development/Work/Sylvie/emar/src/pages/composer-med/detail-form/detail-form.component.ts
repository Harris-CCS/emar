import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import {Observable} from 'rxjs';
import {debounceTime, distinctUntilChanged, map} from 'rxjs/operators';

@Component({
    selector: 'detail-form',
    templateUrl: './detail-form.component.html',
    styleUrls: ['../composer-med.component.scss']
})
export class DetailFormComponent implements OnInit {
  @Output() formReady = new EventEmitter<FormGroup>();
  detailForm: FormGroup;
  diagnoses: string[] = ['Hypertension', 'Diabetes', 'Back pain'];  //TODO get from service
  indications: string[] = ['Sepsis','Pneumonia']; //TODO get from service
  mandatoryIndication: boolean = true; //TODO get from service
  selectedDiagnosis: string = ''; //TODO from service
  selectedIndication: string = ''; //TODO from service
  otherIndication:string = ''; //TODO get from service

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
    if (this.mandatoryIndication) {
      this.detailForm = this.fb.group({
        'diagnosis': null,
        'antimicrobialIndication': null,
        'otherAntimicrobialIndication': null
      }, { validators: this.validator.bind(this) });
    } else {
      this.detailForm = this.fb.group({
        'diagnosis': new FormControl(null)
       });
    }
    this.formReady.emit(this.detailForm);
  }

  changeSelectedDiagnosis(diagnosis: string) {
    this.selectedDiagnosis = diagnosis;
    this.detailForm.controls['diagnosis'].setValue(diagnosis);
  }

  changeSelectedIndication(indication: string) {
    this.selectedIndication = indication;
    this.detailForm.controls['antimicrobialIndication'].setValue(indication);
  }
  
 changeOtherIndication() {
  this.selectedIndication = this.detailForm.controls['otherAntimicrobialIndication'].value;
 }

 validator() {
  if (typeof this.detailForm === 'undefined') {
    return null; //TODO why do I have to add this test
  }
  if (this.selectedIndication != '') {
    return null;
  }
  return {'atLeastOne': true};
 }
}