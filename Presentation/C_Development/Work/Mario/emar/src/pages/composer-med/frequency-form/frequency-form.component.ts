import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  Input,
  ɵisDefaultChangeDetectionStrategy,
  ViewChild,
} from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormControl,
  AbstractControl,
  Validators,
} from '@angular/forms';
import { NgbTypeahead } from '@ng-bootstrap/ng-bootstrap';
import { Observable, Subject, merge } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  filter,
  map,
  bufferTime,
} from 'rxjs/operators';

import { Frequency } from '../../../app/interfaces/frequency';
import { FREQUENCIES } from '../../../app/mockup/frequencies';
import { ModalService } from 'src/services/modal.service';
import { Order } from 'src/app/interfaces/order';
import { Duration } from 'src/app/interfaces/duration';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
const DURATION_UNITS: string[] = ['Doses', 'Hours', 'Days'];

@Component({
  selector: 'frequency-form',
  templateUrl: './frequency-form.component.html',
  styleUrls: ['../composer-med.component.scss'],
})
export class FrequencyFormComponent implements OnInit {
  @Output() formReady = new EventEmitter<FormGroup>();
  frequencyForm: FormGroup;
  @Input() preferredFrequencies: Frequency[] = [];

  @ViewChild('frequencyInstance', { static: true })
  frequencyInstance: NgbTypeahead;
  focusFrequency$ = new Subject<string>();
  clickFrequency$ = new Subject<string>();

  frequencies: Frequency[] = FREQUENCIES; // TODO API
  order: Order = null; // TODO come from composer
  preferredDurationUnits: string[] = DURATION_UNITS;
  minStartDateTime: string;
  startEvent = new EventEmitter<string>();
  endEvent = new EventEmitter<string>();
  selectedFrequencyName: string = '';
  selectedFrequencyData: Frequency;
  selectedDuration: number;
  selectedDurationUnit: Duration;
  selectedDurationUnitName: string;

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private composerSchedulerService: ComposerSchedulerService
  ) {}

  ngOnInit() {
    // default values
    let selectedStartDateTime: string = '';
    let selectedEndDateTime: string = '';
    let selectedFrequency: string = '';
    let selectedFrequencyData: object = {};
    let selectedDuration: string = '';
    let selectedDurationUnit: string = '';
    if (
      this.order === null ||
      this.order.startTime === null ||
      this.order.startTime === ''
    ) {
      selectedStartDateTime = this.API2displayDateTime(); // now
    } else {
      selectedStartDateTime = this.API2displayDateTime(this.order.startTime);
    }
    if (
      this.order === null ||
      this.order.triageTime == null ||
      this.order.triageTime === ''
    ) {
      this.minStartDateTime =
        selectedStartDateTime.replace(/ .+$/, '') + ' 00:00'; /// TODO starttime - some hours
    } else {
      this.minStartDateTime = this.API2displayDateTime(this.order.triageTime);
    }
    if (this.order !== null) {
      selectedFrequencyData = this.order.frequency;
      selectedFrequency = this.order.frequency.frequencyName;
      selectedEndDateTime = this.order.endTime;
      selectedDuration = this.order.duration.duration.toString();
      selectedDurationUnit = this.order.duration.durationUnit.unitName;
    }
    // form definition
    // TODO date format validator, end >= start validator in case manual entry
    this.frequencyForm = this.fb.group({
      frequency: new FormControl(selectedFrequency, [
        Validators.required,
        this.frequencyValidator,
      ]),
      frequencyData: new FormControl(selectedFrequencyData),
      duration: new FormControl(selectedDuration, [
        // Validators.required,
        this.durationValidator,
        this.durationValidator.bind(this),
      ]),
      durationUnit: new FormControl(selectedDurationUnit, [
        // Validators.required,
        this.durationUnitValidator,
        this.durationUnitValidator.bind(this),
      ]),
      startTime: new FormControl(selectedStartDateTime),
      endTime: new FormControl(selectedEndDateTime),
    });
    // this.formReady.emit(this.frequencyForm);
    this.composerSchedulerService.addFormGroup('frequency', this.frequencyForm);
    this.composerSchedulerService.performFormReset.subscribe(() => {
      if (this.composerSchedulerService.performFormReset.value) {
        this.resetFrequencyForm();
      }
    });
    // reinject modal datetime result in form
    this.startEvent.subscribe((dateTime: string) => {
      this.frequencyForm.patchValue({ startTime: dateTime });
    });
    this.endEvent.subscribe((dateTime: string) => {
      this.frequencyForm.patchValue({ endTime: dateTime });
    });
  }

  resetFrequencyForm() {
    this.selectedFrequencyName = '';
    this.selectedFrequencyData = {};
    this.selectedDuration = null;
    this.selectedDurationUnit = {};
    this.selectedDurationUnitName = '';
  }

  // ****************** Frequency ***************************

  formatFrequency = (frequency: Frequency) => frequency.frequencyName;

  changeSelectedFrequency(frequency: Frequency) {
    // this.frequencyForm.patchValue({ frequency: frequency });
    if (frequency) {
      this.selectedFrequencyData = frequency;
      this.selectedFrequencyName = frequency.frequencyName;
      this.frequencyForm.controls['frequency'].setValue(
        frequency.frequencyName
      );
      this.frequencyForm.controls['frequencyData'].setValue(frequency);
    } else {
      this.selectedFrequencyData = null;
      this.selectedFrequencyName = '';
      this.frequencyForm.controls['frequency'].setValue('');
      this.frequencyForm.controls['frequencyData'].setValue(null);
    }
    // console.log('frequencyThis', this);
  }

  changeSelectedFrequencyByLookup(frequencyName: string): void {
    const matchingFrequency = !frequencyName
      ? null
      : this.frequencies.find(
          (fndFrequency) => fndFrequency.frequencyName === frequencyName
        );
    this.changeSelectedFrequency(matchingFrequency);
  }

  searchFrequency = (text$: Observable<string>) => {
    const debouncedText$ = text$.pipe(
      debounceTime(200),
      distinctUntilChanged()
    );
    const clicksWithClosedPopup$ = this.clickFrequency$.pipe(
      filter(() => !this.frequencyInstance.isPopupOpen())
    );
    const inputFocus$ = this.focusFrequency$;
    const mergeResults = merge(
      debouncedText$,
      inputFocus$,
      clicksWithClosedPopup$
    ).pipe(
      map((term) => {
        let subSet = [];
        if (term) {
          subSet = this.frequencies
            .filter(
              (v) =>
                v.frequencyName.toLowerCase().indexOf(term.toLowerCase()) > -1
            )
            .slice(0, 10);
        } else {
          subSet = this.frequencies.slice(0, 10);
        }
        return subSet.map((node) => node.frequencyName);
      })
    );
    return mergeResults;
  };

  frequencyValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return { error: '** Frequency is required' };
    }
    return null;
  }

  // TO DO: See if these macros are still needed

  lookupFrequency(text$: Observable<string>) {
    return text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      map((term) =>
        term.length < 0
          ? []
          : FREQUENCIES.filter((f) =>
              new RegExp(term, 'mi').test(f.frequencyName)
            ).slice(0, 10)
      )
      // TODO this.frequencies but this is undefined
    );
  }

  // ****************** Duration ***************************

  changeSelectedDuration(duration: number) {
    this.selectedDuration = duration;
    this.frequencyForm.controls['duration'].setValue(duration);
    if (!duration) {
      this.selectedDurationUnit = null;
      this.selectedDurationUnitName = '';
      this.frequencyForm.controls['durationUnit'].setValue(null);
      const durationValidationErrors = this.durationValidator(
        this.frequencyForm.controls['duration']
      );

      this.frequencyForm.controls['duration'].setErrors(
        durationValidationErrors
      );
    }

    const durationUnitValidationErrors = this.durationUnitValidator(
      this.frequencyForm.controls['durationUnit']
    );
    if (durationUnitValidationErrors) {
      this.frequencyForm.controls['durationUnit'].setErrors(
        durationUnitValidationErrors
      );
    }

    this.maybeResetStartEndTimes();
    // console.log('durationUnitValidationErrors', durationUnitValidationErrors);
    // console.log('durationThis', this);
  }

  onDurationUnit(unit: string) {
    // this.frequencyForm.patchValue({ durationUnit: unit });
    this.selectedDurationUnitName = unit;
    this.frequencyForm.controls['durationUnit'].setValue(unit);

    const durationValidationErrors = this.durationValidator(
      this.frequencyForm.controls['duration']
    );
    if (durationValidationErrors) {
      this.frequencyForm.controls['duration'].setErrors(
        durationValidationErrors
      );
    }

    this.maybeResetStartEndTimes();
    // console.log('durationValidationErrors', durationValidationErrors);
    // console.log('DurationUnitThis', this);
  }

  maybeResetStartEndTimes(): void {
    if (
      this.frequencyForm.get('duration').valid &&
      this.frequencyForm.get('durationUnit').valid
    ) {
      this.frequencyForm.controls['startTime'].setValue('');
      this.frequencyForm.controls['endTime'].setValue('');
    }
  }

  durationValidator(control: AbstractControl): { [key: string]: any } | null {
    // console.log('durationControl', control);
    // console.log('durationValidatorThis', this);
    if (
      !control ||
      !this ||
      !this.frequencyForm ||
      this.frequencyForm.get('durationUnit').invalid ||
      (!this.selectedDuration && !this.selectedDurationUnitName)
    ) {
      return null;
    } else if (
      this.frequencyForm.get('durationUnit').valid &&
      (control.value === undefined ||
        control.value === null ||
        control.value === '')
    ) {
      return { error: '** Blank or invalid duration' };
    } else {
      const valueAsString = control.value.toString();
      if (valueAsString.includes('-')) {
        return { error: '** Duration cannot be negative or contain dashes' };
      } else if (valueAsString.includes('+')) {
        return { error: '** Duration cannot contain plus signs' };
      } else if (valueAsString === '0') {
        return { error: '** Duration cannot be 0' };
      } else if (control.value.length > 4) {
        return { error: '** Duration cannot be > 4 characters' };
      }
    }
    return null;
  }

  durationUnitValidator(
    control: AbstractControl
  ): { [key: string]: any } | null {
    if (
      !control ||
      !this ||
      !this.frequencyForm ||
      this.frequencyForm.get('duration').invalid ||
      !this.selectedDuration
    ) {
      return null;
    } else if (
      this.frequencyForm.get('duration').valid &&
      (control.value === undefined ||
        control.value === null ||
        control.value === '')
    ) {
      return {
        error: '** Duration Unit is required when Numeric Duration is defined',
      };
    }
    return null;
  }

  // ****************** Start/End Date ***************************

  API2displayDateTime(val?: string) {
    let time: Date;
    let text: string;
    if (typeof val === 'undefined' || val === null || val === '') {
      time = new Date();
      text =
        ('0' + (time.getMonth() + 1)).slice(-2) +
        '/' +
        ('0' + time.getDate()).slice(-2) +
        '/' +
        time.getFullYear().toString() +
        ' ' +
        ('0' + time.getHours()).slice(-2) +
        ':' +
        ('0' + time.getMinutes()).slice(-2);
    } else {
      let regexp = /(....)-(..)-(..)T(..):(..)/;
      let match = regexp.exec(val);
      text = `${match[2]}/${match[3]}/${match[1]} ${match[4]}:${match[5]}`;
    }
    return text;
  }
  onSelectTime(type: string) {
    if (type === 'start') {
      this.modalService.open(
        'date-time-modal',
        {
          dateTime: this.frequencyForm.controls['startTime'].value,
          event: this.startEvent,
          minDateTime: this.minStartDateTime,
        },
        'Start Time'
      );
    } else {
      let dateTime: string;
      if (this.frequencyForm.controls['endTime'].value == '') {
        dateTime = this.frequencyForm.controls['startTime'].value;
      } else {
        dateTime = this.frequencyForm.controls['endTime'].value;
      }
      this.modalService.open(
        'date-time-modal',
        {
          dateTime: dateTime,
          event: this.endEvent,
          minDateTime: this.frequencyForm.controls['startTime'].value,
        },
        'End Time'
      );
    }
    this.maybeResetDuration();
    // console.log('startEndDateTimesThis', this);
  }

  maybeResetDuration(): void {
    if (
      // this.frequencyForm.controls['startTime'].value &&
      // this.frequencyForm.controls['endTime'].value &&
      this.frequencyForm.get('startTime').valid &&
      this.frequencyForm.get('endTime').valid
    ) {
      this.selectedDuration = null;
      this.selectedDurationUnit = null;
      this.selectedDurationUnitName = '';
      this.frequencyForm.controls['duration'].setValue(null);
      this.frequencyForm.controls['durationUnit'].setValue('');
    }
  }
}
