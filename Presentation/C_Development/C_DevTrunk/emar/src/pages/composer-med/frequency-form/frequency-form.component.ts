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
// import { FREQUENCIES } from '../../../app/mockup/frequencies';
import { ModalService } from 'src/services/modal.service';
import { Order } from 'src/app/interfaces/order';
import { Duration } from 'src/app/interfaces/duration';
import { Unit } from 'src/app/interfaces/unit';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
import { UserStoreService } from 'src/services/user-store.service';
const DURATION_UNITS: string[] = ['Dose(s)', 'Hour(s)', 'Day(s)'];

@Component({
  selector: 'frequency-form',
  templateUrl: './frequency-form.component.html',
  styleUrls: ['../composer-med.component.scss'],
})
export class FrequencyFormComponent implements OnInit {
  @Output() formReady = new EventEmitter<FormGroup>();
  frequencyForm: FormGroup;
  @Input() medComponentId: number;
  @Input() preferredFrequencies: Frequency[] = [];

  @ViewChild('frequencyInstance', { static: true })
  frequencyInstance: NgbTypeahead;
  focusFrequency$ = new Subject<string>();
  clickFrequency$ = new Subject<string>();

  // frequencies: Frequency[] = FREQUENCIES;
  frequencies: Frequency[];
  order: Order = null; // TODO come from composer
  preferredDurationUnits: string[] = DURATION_UNITS;
  minStartDateTime: string;
  startEvent = new EventEmitter<string>();
  endEvent = new EventEmitter<string>();
  selectedFrequencyName: string = '';
  selectedFrequencyData: Frequency = {};
  selectedDuration: number;
  selectedDurationUnit: Unit;
  selectedDurationUnitName: string = '';
  initialStartDateTime: string = '';
  initialEndDateTime: string = '';
  userSiteId: number = null;

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private composerSchedulerService: ComposerSchedulerService,
    private userStoreService: UserStoreService
  ) {
    this.userSiteId = this.userStoreService.userSiteId;
    this.frequencies = this.composerSchedulerService.getSiteMedicationFrequencies(
      this.userSiteId
    );
    // console.log('frequenciesinConstructor', this.frequencies);
  }

  ngOnInit() {
    // default values
    // let selectedStartDateTime: string = '';
    // let selectedEndDateTime: string = '';
    // let selectedFrequency: string = '';
    // let selectedFrequencyData: object = {};
    // let selectedDuration: string = '';
    // let selectedDurationUnit: string = '';
    // if (
    //   this.order === null ||
    //   this.order.startTime === null ||
    //   this.order.startTime === ''
    // ) {
    //   selectedStartDateTime = this.API2displayDateTime(); // now
    // } else {
    //   selectedStartDateTime = this.API2displayDateTime(this.order.startTime);
    // }
    // if (
    //   this.order === null ||
    //   this.order.triageTime == null ||
    //   this.order.triageTime === ''
    // ) {
    //   this.minStartDateTime =
    //     selectedStartDateTime.replace(/ .+$/, '') + ' 00:00'; /// TODO starttime - some hours
    // } else {
    //   this.minStartDateTime = this.API2displayDateTime(this.order.triageTime);
    // }
    // if (this.order !== null) {
    //   selectedFrequencyData = this.order.frequency;
    //   selectedFrequency = this.order.frequency.frequencyName;
    //   selectedEndDateTime = this.order.endTime;
    //   selectedDuration = this.order.duration.duration.toString();
    //   selectedDurationUnit = this.order.duration.durationUnit.unitName;
    // }
    // form definition
    // TODO date format validator, end >= start validator in case manual entry
    this.setDefaults();
    this.frequencyForm = this.fb.group({
      frequency: new FormControl(this.selectedFrequencyName, [
        Validators.required,
        this.frequencyValidator,
      ]),
      frequencyData: new FormControl(this.selectedFrequencyData),
      duration: new FormControl(this.selectedDuration, [
        this.durationValidator,
        this.durationValidator.bind(this),
      ]),
      durationUnit: new FormControl(this.selectedDurationUnit, [
        this.durationUnitValidator,
        this.durationUnitValidator.bind(this),
      ]),
      startTime: new FormControl(this.initialStartDateTime, [
        Validators.required,
        this.startTimeValidator,
        this.startTimeValidator.bind(this),
      ]),
      endTime: new FormControl(this.initialEndDateTime, [
        this.endTimeValidator,
        this.endTimeValidator.bind(this),
      ]),
    });
    // this.formReady.emit(this.frequencyForm);
    this.composerSchedulerService.addFormGroup(
      this.medComponentId,
      'frequency',
      this.frequencyForm
    );

    this.composerSchedulerService.resetComponentMedFormId.subscribe(() => {
      if (
        this.composerSchedulerService.resetComponentMedFormId &&
        this.composerSchedulerService.resetComponentMedFormId.value ===
          this.medComponentId
      ) {
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
    this.setDefaults();
    this.frequencyForm.patchValue({ startTime: this.initialStartDateTime });
    this.frequencyForm.patchValue({ endTime: this.initialEndDateTime });
    // console.log('resetFrequencyThis', this);
  }

  setDefaults(): void {
    this.selectedFrequencyName = '';
    this.selectedFrequencyData = {};
    this.selectedDuration = null;
    this.selectedDurationUnit = {};
    this.selectedDurationUnitName = '';
    this.initialStartDateTime = '';
    this.initialEndDateTime = '';
    if (
      this.order === null ||
      this.order.startTime === null ||
      this.order.startTime === ''
    ) {
      this.initialStartDateTime = this.API2displayDateTime(); // now
    } else {
      this.initialStartDateTime = this.API2displayDateTime(
        this.order.startTime
      );
    }
    if (
      this.order === null ||
      this.order.triageTime == null ||
      this.order.triageTime === ''
    ) {
      this.minStartDateTime =
        this.initialStartDateTime.replace(/ .+$/, '') + ' 00:00'; /// TODO starttime - some hours
    } else {
      this.minStartDateTime = this.API2displayDateTime(this.order.triageTime);
    }
    if (this.order !== null) {
      this.selectedFrequencyData = this.order.frequency;
      this.selectedFrequencyName = this.order.frequency.frequencyName;
      this.initialEndDateTime = this.order.endTime;
      this.selectedDuration = this.order.duration.duration;
      this.selectedDurationUnit = this.order.duration.durationUnit;
      this.selectedDurationUnitName = this.order.duration.durationUnit.unitName;
    }
    // console.log('frequencyDefaultsThis', this);
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
          : // : FREQUENCIES.filter((f) =>
            this.frequencies
              .filter((f) => new RegExp(term, 'mi').test(f.frequencyName))
              .slice(0, 10)
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

    this.maybeResetEndTime();
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

    this.maybeResetEndTime();
    // console.log('durationValidationErrors', durationValidationErrors);
    // console.log('DurationUnitThis', this);
  }

  maybeResetEndTime(): void {
    if (
      this.frequencyForm.get('duration').valid &&
      this.frequencyForm.get('durationUnit').valid
    ) {
      // this.frequencyForm.controls['startTime'].setValue('');
      this.frequencyForm.controls['endTime'].setValue('');
    }
  }

  durationValidator(control: AbstractControl): { [key: string]: any } | null {
    // console.log('durationControl', control);
    // console.log('durationValidatorThis', this);
    if (!this || !this.frequencyForm) {
      return null;
    } else if (this.noDurationOrDateTimesEntered()) {
      return {
        error:
          '** Must select either Duration and Duration Unit, or Start and End Times',
      };
    }
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
    if (!this || !this.frequencyForm) {
      return null;
    } else if (this.noDurationOrDateTimesEntered()) {
      return {
        error:
          '** Must select either Duration and Duration Unit, or Start and End Times',
      };
    }
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
      if (this.frequencyForm.controls['endTime'].value === '') {
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

  startTimeValidator(control: AbstractControl): { [key: string]: any } | null {
    // console.log('durationControlValue', control.value);
    if (!control || !this || !this.frequencyForm) {
      return null;
    } else if (!control.value) {
      return { error: '** Start Time is required' };
    } else if (!this.validDateTime(control.value)) {
      return {
        error:
          'Invalid date/time or date/time format. Must be DD/MM/YYYY HH:MM format.',
      };
    }
    this.maybeResetDuration();
    return null;
  }

  endTimeValidator(control: AbstractControl): { [key: string]: any } | null {
    // console.log('durationControlValue', control.value);
    if (!this || !this.frequencyForm) {
      return null;
    } else if (this.noDurationOrDateTimesEntered()) {
      return {
        error:
          '** Must select either Duration and Duration Unit, or Start and End Times',
      };
    }
    if (!control || !control.value || !this || !this.frequencyForm) {
      return null;
    } else if (!this.validDateTime(control.value)) {
      return {
        error:
          'Invalid date/time or date/time format. Must be DD/MM/YYYY HH:MM format.',
      };
    }
    this.maybeResetDuration();
    return null;
  }

  validDateTime(date: string) {
    const pattern = new RegExp(
      '^(1[0-2]|0[1-9])/(3[01]|[12][0-9]|0[1-9])/[0-9]{4} (2[0-3]|[01]?[0-9]):([0-5]?[0-9])$'
    );
    if (date.search(pattern) === 0) {
      return true;
    }
  }

  maybeResetDuration(): void {
    // console.log('frequencyFormResetDuration', this.frequencyForm);
    if (
      // this.frequencyForm.controls['startTime'].value &&
      // this.frequencyForm.controls['endTime'].value &&
      this.frequencyForm.get('startTime').valid &&
      this.frequencyForm.get('endTime').valid &&
      this.frequencyForm.get('startTime').value &&
      this.frequencyForm.get('endTime').value
    ) {
      // console.log('frequencyFormResetDuration1', this.frequencyForm);
      this.selectedDuration = null;
      this.selectedDurationUnit = null;
      this.selectedDurationUnitName = '';
      this.frequencyForm.controls['duration'].setValue(null);
      this.frequencyForm.controls['durationUnit'].setValue('');
    }
  }

  noDurationOrDateTimesEntered(): boolean {
    // console.log('noDurationOrDateTimesEntered', this.frequencyForm);
    return !this.frequencyForm.get('duration').value &&
      (!this.frequencyForm.get('durationUnit').value ||
        Object.keys(this.frequencyForm.get('durationUnit').value).length ===
          0) &&
      // !this.frequencyForm.get('startTime').value &&
      !this.frequencyForm.get('endTime').value
      ? true
      : false;
  }
}
