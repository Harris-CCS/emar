import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
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
import { Observable, Subject, merge, Subscription, from, scheduled } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  filter,
  map,
  bufferTime,
} from 'rxjs/operators';
import * as moment from 'moment';
import { Frequency } from '../../../app/interfaces/frequency';
// import { FREQUENCIES } from '../../../app/mockup/frequencies';
import { DateTimePipe } from 'src/shared/pipes/dateTime';
import { ModalService } from 'src/services/modal.service';
import { Order } from 'src/app/interfaces/order';
import { Duration } from 'src/app/interfaces/duration';
import { DurationUnit } from 'src/app/interfaces/duration-unit';
import { ScheduledAdministration } from 'src/app/interfaces/scheduled-administration';
import { CartOrderAdministration } from 'src/app/interfaces/cart-order-administration';
import { Unit } from 'src/app/interfaces/unit';
import { PRNIndication } from 'src/app/interfaces/prn-indication'
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
import { UserStoreService } from 'src/services/user-store.service';
import { SiteStoreService } from 'src/services/site-store.service';
import { connectableObservableDescriptor } from 'rxjs/internal/observable/ConnectableObservable';
import { PatientStoreService } from 'src/services/patient-store.service';

import { PRNIndicationOptions } from 'src/assets/content/PRN_Indication_Options'

const DURATION_UNITS: string[] = ['Dose(s)', 'Hour(s)', 'Day(s)'];

@Component({
  selector: 'frequency-form',
  templateUrl: './frequency-form.component.html',
  styleUrls: ['../composer-med.component.scss'],
})
export class FrequencyFormComponent implements OnInit, OnDestroy {
  @Output() formReady = new EventEmitter<FormGroup>();
  frequencyForm: FormGroup;
  @Input() medComponentId: number;
  @Input() preferredFrequencies: Frequency[] = [];
  @Input() modalId: string;

  @ViewChild('frequencyInstance', { static: true })
  frequencyInstance: NgbTypeahead;
  focusFrequency$ = new Subject<string>();
  clickFrequency$ = new Subject<string>();

  @ViewChild('prnIndicationInstance')
  prnIndicationInstance: NgbTypeahead;
  focusPRNIndication$ = new Subject<string>();
  clickPRNIndication$ = new Subject<string>();

  prnOptions: Array<PRNIndication> = PRNIndicationOptions.sort( (a, b) => (a.optionDescription > b.optionDescription) ? 1 : -1 )
  selectedPRNIndicationData: PRNIndication = {};
  selectedPRNIndicationDescription: string = '';

  // frequencies: Frequency[] = FREQUENCIES;
  frequencies: Frequency[];
  order: Order = null; // TODO come from composer
  preferredDurationUnits: Array<DurationUnit>;
  availableAdministrations: Array<ScheduledAdministration> = [];
  scheduledAdministrations: Array<ScheduledAdministration> = [];
  minStartDateTime: string;
  startEvent = new EventEmitter<string>();
  endEvent = new EventEmitter<string>();
  selectedFrequencyName: string = '';
  selectedFrequencyData: Frequency = {};
  selectedDuration: number;
  selectedDurationUnit: DurationUnit;
  selectedDurationUnitName: string = '';
  initialStartDateTime: string = '';
  initialEndDateTime: string = '';
  userSiteId: number = null;
  userId: number;
  siteUTCOffset: string;
  subscriptionResetComponentMedFormId: Subscription;
  subscriptionStartDateTimeEvent: Subscription;
  subscriptionEndDateTimeEvent: Subscription;

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private composerSchedulerService: ComposerSchedulerService,
    private userStoreService: UserStoreService,
    private datePipe: DateTimePipe,
    private siteStoreService: SiteStoreService,
    private patientStoreService: PatientStoreService,
  ) {
    this.userSiteId = this.userStoreService.userSiteId;
    this.userId = this.userStoreService.userId;
    this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
    this.frequencies = this.composerSchedulerService.getSiteMedicationFrequencies(
      this.userSiteId
    );
    this.preferredDurationUnits = this.composerSchedulerService.getDurationUnits();

    // console.log('++++++++++ SiteStore: SiteMedicationFrequencies: ', this.siteStoreService.SiteMedicationFrequencies)
    // console.log('++++++++++ frequencies: ', this.frequencies)
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
    //   this.order.beginDatetime === null ||
    //   this.order.beginDatetime === ''
    // ) {
    //   selectedStartDateTime = this.API2displayDateTime(); // now
    // } else {
    //   selectedStartDateTime = this.API2displayDateTime(this.order.beginDatetime);
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
    //   selectedFrequency = this.order.frequency.scheduleName;
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
      prnIndicationDescription: new FormControl(this.selectedPRNIndicationDescription, [
        Validators.required,
        Validators.maxLength(100),
        this.prnIndicationValidator.bind(this),
      ]),
      // prnIndicationData: new FormControl(this.prnOptions),
      prnIndicationData: new FormControl(this.selectedPRNIndicationData),
      durationUnit: new FormControl(this.selectedDurationUnit, [
        this.durationUnitValidator,
        this.durationUnitValidator.bind(this),
      ]),
      startTime: new FormControl(this.initialStartDateTime, [ // mm/dd/yyyy hh:mm format
        Validators.required,
        this.startTimeValidator,
        this.startTimeValidator.bind(this),
      ]),
      startTimeUTC: new FormControl(moment(this.initialStartDateTime + this.siteUTCOffset, 'MM/DD/YYYY HH:mmZZ').format(), // yyyy-mm-ddZhh:mm:ss-06:00
        // [
        // Validators.required,
        // this.startTimeValidator,
        // this.startTimeValidator.bind(this),
        // ]
      ),
      endTime: new FormControl(this.initialEndDateTime,
        [
          this.endTimeValidator,
          this.endTimeValidator.bind(this),
        ]
      ),
      endTimeUTC: new FormControl(this.initialEndDateTime==''?'':moment(this.initialEndDateTime + this.siteUTCOffset, 'MM/DD/YYYY HH:mmZZ').format(),
        // [
        //   this.endTimeValidator,
        //   this.endTimeValidator.bind(this),
        // ]
      ),
      availableAdministrations: new FormControl(this.availableAdministrations),
      scheduledAdministrations: new FormControl(this.scheduledAdministrations),

    });
    this.setDefaultValues(),
      // this.formReady.emit(this.frequencyForm);
      this.composerSchedulerService.addFormGroup(
        this.medComponentId,
        'frequency',
        this.frequencyForm
      );
    this.subscriptionResetComponentMedFormId = this.composerSchedulerService.resetComponentMedFormId.subscribe(() => {
      if (
        this.composerSchedulerService.resetComponentMedFormId &&
        this.composerSchedulerService.resetComponentMedFormId.value ===
        this.medComponentId
      ) {
        this.resetFrequencyForm();
      }
    });

    // reinject modal datetime result in form
    this.subscriptionStartDateTimeEvent = this.startEvent.subscribe((dateTime: string) => {
      this.frequencyForm.patchValue({ startTime: moment(dateTime).utcOffset(parseInt(this.siteUTCOffset)).format('MM/DD/YYYY HH:mm') });
      this.frequencyForm.patchValue({ startTimeUTC: dateTime}); //`${this.datePipe.transform(dateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` });
      console.log ('DATETIME',dateTime,this.frequencyForm.get('startTime').value, this.frequencyForm.get('startTimeUTC').value)
      this.updateAdministrations(this.selectedFrequencyData, dateTime, this.frequencyForm.get('endTimeUTC').value, 'startTimeSelectButton');
    });
    this.subscriptionEndDateTimeEvent = this.endEvent.subscribe((dateTime: string) => {
      this.frequencyForm.patchValue({ endTime: moment(dateTime).utcOffset(parseInt(this.siteUTCOffset)).format('MM/DD/YYYY HH:mm') });
      this.frequencyForm.patchValue({ endTimeUTC: dateTime }); //`${this.datePipe.transform(dateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` });
      this.updateAdministrations(this.selectedFrequencyData, this.frequencyForm.get('startTimeUTC').value, dateTime, 'endTimeSelectButton');
    });

    // if (this.initialStartDateTime) {
    //   this.frequencyForm.patchValue({ startTimeUTC: `${this.datePipe.transform(this.initialStartDateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` });
    // }
    // if (this.initialEndDateTime) {
    //   this.frequencyForm.patchValue({ endTimeUTC: `${this.datePipe.transform(this.initialEndDateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` });
    // }


    // console.log('frequencyInitThis', this);
  }

  async setDefaultValues() {
    await this.setFrequencyDefaultValues();
  }

  async setFrequencyDefaultValues() {
    if (this.medComponentId === 0) {
      const initialComposerData: any = this.composerSchedulerService.getInitialComposerData();
      const initialOrderData: any = initialComposerData.med;
      // console.log('initialOrderData', initialOrderData);
      // Frequency
      if (initialOrderData.frequencySchedule && initialOrderData.frequencySchedule.scheduleName) {
        // this.changeSelectedFrequency(initialOrderData.frequencySchedule);

        this.selectedFrequencyData = initialOrderData.frequencySchedule;
        this.selectedFrequencyName = initialOrderData.frequencySchedule.scheduleName;
        this.frequencyForm.controls['frequency'].setValue(
          initialOrderData.frequencySchedule.scheduleName
        );
        this.frequencyForm.controls['frequencyData'].setValue(initialOrderData.frequencySchedule);
      }
      // PRN Indication
      this.resetPRNIndicationValidators(initialOrderData.frequencySchedule?.prn ? initialOrderData.frequencySchedule?.prn : false)
      // initialOrderData.prnIndication = 'fever'
      if (initialOrderData.prnIndication) {
        this.changeSelectedPRNIndication({'id': 0, 'optionDescription': initialOrderData.prnIndication})
      }

      // Duration
      if (initialOrderData.duration === 0 || initialOrderData.duration) {
        this.selectedDuration = initialOrderData.duration;
        this.changeSelectedDuration(this.selectedDuration);
      }
      // Duration Unit
      if (initialOrderData.durationUnitId) {

        const durationUnit = this.preferredDurationUnits.find(
          pu => pu.id === initialOrderData.durationUnitId);
        if (durationUnit) {
          this.selectedDurationUnitName = initialOrderData.durationUnit;
          this.selectedDurationUnit = durationUnit;

          this.onDurationUnit(this.selectedDurationUnit);
        }
      }
      // Start Date and Time
      if (initialOrderData.beginDatetime) {
        this.initialStartDateTime = `${this.datePipe.transform(initialOrderData.beginDatetime, 'dateTimeDisplay', this.siteUTCOffset)}`;
        this.frequencyForm.patchValue({ startTime: this.initialStartDateTime });
        this.frequencyForm.patchValue({ startTimeUTC: initialOrderData.beginDatetime });
      } else {
        const now: string = Date.now().toString(); // 1613843137449
        this.initialStartDateTime = this.datePipe.transform(now, 'dateTimeDisplay', this.siteUTCOffset);
        this.frequencyForm.patchValue({ startTime: this.initialStartDateTime }); // 02/20/2021 11:25
        // this.frequencyForm.patchValue({ startTimeUTC: this.datePipe.transform(now, 'UTC', this.siteUTCOffset) }); // 2021-02-20T11:25:14-06:00
        this.frequencyForm.patchValue({ startTimeUTC: moment(this.initialStartDateTime + this.siteUTCOffset, 'MM/DD/YYYY HH:mmZZ').format() }); // 2021-02-20T11:25:00-06:00
      }
      // End Date and Time
      if (initialOrderData.endDatetime && !initialOrderData.duration && !initialOrderData.durationUnitId) {
        this.initialEndDateTime = `${this.datePipe.transform(initialOrderData.endDatetime, 'dateTimeDisplay', this.siteUTCOffset)}`;
        this.frequencyForm.patchValue({ endTime: this.initialEndDateTime });
        this.frequencyForm.patchValue({ endTimeUTC: initialOrderData.endDatetime });
      } else {
        this.initialEndDateTime = null;
        this.frequencyForm.patchValue({ endTime: null });
        this.frequencyForm.patchValue({ endTimeUTC: null });
      }
      
      if ( initialComposerData.action === 'repeat' || initialComposerData.action === 'modify' ) {

        this.updateAdministrations(
          initialOrderData.frequencySchedule,
          this.frequencyForm.get('startTimeUTC').value,
          this.frequencyForm.get('endTimeUTC').value,
          'initialOrderInit');
      }
        
      // Scheduled Administrations
      if (initialOrderData.cartOrderAdministrations && initialOrderData.cartOrderAdministrations.length > 0) {
        // this.scheduledAdministrations = initialOrderData.cartOrderAdministrations.map(admin => {
        //   return {
        //     pointInTime: admin.pointInTime,
        //     scheduleDateTime: admin.administrationScheduledDatetime,
        //     stopDateTime: admin.stopScheduledDatetime
        //   }
        // });
        // Need to reorder these administrations because they come in out of order from API from time to time.
        // The system needs these to be in order every time.
        let orderedScheduledAdministrations: Array<ScheduledAdministration> = [];
        initialOrderData.cartOrderAdministrations.forEach(admin => {
          if (admin.administrationScheduledDatetime) {
            const scheduledAdminNode: ScheduledAdministration = {
              pointInTime: admin.pointInTime,
              scheduleDateTime: admin.administrationScheduledDatetime,
              stopDateTime: admin.stopScheduledDatetime
            };
            const index: number = orderedScheduledAdministrations.findIndex(osa => osa.scheduleDateTime > admin.administrationScheduledDatetime);
            if (index === -1) {
              orderedScheduledAdministrations.push(scheduledAdminNode);
            } else {
              orderedScheduledAdministrations.splice(index, 0, scheduledAdminNode);
            }
          }
        });
        this.scheduledAdministrations = [...orderedScheduledAdministrations];
        // console.log('scheduledAdminsReceived', initialOrderData.cartOrderAdministrations);
        // console.log('scheduledAdmins', this.scheduledAdministrations);
        // this.composerSchedulerService.setOrderScheduledAdministrations(this.medComponentId, this.scheduledAdministrations);
        if (initialOrderData.frequencySchedule && initialOrderData.frequencySchedule.id) {
          const beginDateTimeFormatted = this.frequencyForm.get('startTime').value ? `${this.datePipe.transform(this.frequencyForm.get('startTime').value, 'dateTimeSeconds')}${this.siteUTCOffset}` : '';
          const endDateTimeFormatted = this.frequencyForm.get('endTime').value ? `${this.datePipe.transform(this.frequencyForm.get('endTime').value, 'dateTimeSeconds')}${this.siteUTCOffset}` : '';
          this.availableAdministrations = await this.composerSchedulerService.getFrequencyAdministrationsFromAPI
            (this.userId, this.userSiteId, initialOrderData.frequencySchedule.id, beginDateTimeFormatted, endDateTimeFormatted).toPromise();
          // console.log('availableScheduledAdministrations', this.availableAdministrations);
          this.frequencyForm.patchValue({ availableAdministrations: this.availableAdministrations });
          this.frequencyForm.patchValue({ scheduledAdministrations: this.scheduledAdministrations });
          this.composerSchedulerService.signalOrderFrequencyChanged(this.medComponentId);
        }
        // if (initialOrderData.frequencySchedule) {
        // this.updateAdministrations(initialOrderData.frequencySchedule, this.frequencyForm.value.startTime, this.frequencyForm.value.endTime)
        // this.composerSchedulerService.signalOrderFrequencyChanged(this.medComponentId);
        // console.log('getScheduledAdmins', this.composerSchedulerService.getOrderScheduledAdministrations(this.medComponentId));
      }
      if (initialComposerData.action === 'add') {
        this.updateAdministrations(
          initialOrderData.frequencySchedule,
          this.frequencyForm.get('startTimeUTC').value,
          this.frequencyForm.get('endTimeUTC').value,
          'initialOrderInit');
      }
      // console.log('FrequencyDefaultValues', this);
    }
  }

  ngOnDestroy() {
    this.subscriptionResetComponentMedFormId.unsubscribe();
    this.subscriptionStartDateTimeEvent.unsubscribe();
    this.subscriptionEndDateTimeEvent.unsubscribe();
  }

  resetFrequencyForm() {
    this.setDefaults();
    this.frequencyForm.patchValue({ startTime: this.initialStartDateTime });
    if (this.initialStartDateTime) {
      this.frequencyForm.patchValue({ startTimeUTC: `${this.datePipe.transform(this.initialStartDateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` });
    } else {
      this.frequencyForm.patchValue({ startTimeUTC: null });
    }
    this.frequencyForm.patchValue({ endTime: this.initialEndDateTime });
    if (this.initialEndDateTime) {
      this.frequencyForm.patchValue({ endTimeUTC: `${this.datePipe.transform(this.initialEndDateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` });
    } else {
      this.frequencyForm.patchValue({ endTimeUTC: null });

    }
    this.frequencyForm.patchValue({ availableAdministrations: null });
    this.frequencyForm.patchValue({ scheduledAdministrations: null });
    // console.log('resetFrequencyThis', this);
  }

  setDefaults(): void {
    this.selectedFrequencyName = '';
    this.selectedFrequencyData = {};
    this.selectedPRNIndicationDescription = ''
    this.selectedPRNIndicationData = {};
    this.selectedDuration = null;
    this.selectedDurationUnit = null;
    this.selectedDurationUnitName = '';
    this.initialStartDateTime = this.datePipe.transform(Date.now().toString(), 'dateTimeDisplay', this.siteUTCOffset);
    this.initialEndDateTime = '';
    this.minStartDateTime = this.patientStoreService.visitStartDateTime;
    // TODO siteUTCoffset this.datePipe.transform(Date.now().toString(), 'dateTimeDisplay', this.siteUTCOffset);


    // if (
    //   this.order === null ||
    //   this.order.beginDatetime === null ||
    //   this.order.beginDatetime === ''
    // ) {
    //   // this.initialStartDateTime = this.API2displayDateTime(); // now
    //   this.initialStartDateTime = this.datePipe.transform(Date.now().toString(), 'dateTimeDisplay', this.siteUTCOffset);
    // } else {
    //   // this.initialStartDateTime = this.API2displayDateTime(
    //   //   this.order.beginDatetime
    //   // );
    //   this.initialStartDateTime = this.datePipe.transform(this.order.beginDatetime, 'dateTimeDisplay');
    // }
    // if (
    //   this.order === null ||
    //   this.order.triageTime == null ||
    //   this.order.triageTime === ''
    // ) {
    //   this.minStartDateTime =
    //     this.initialStartDateTime.replace(/ .+$/, '') + ' 00:00'; /// TODO starttime - some hours
    // } else {
    //   // this.minStartDateTime = this.API2displayDateTime(this.order.triageTime);
    //   this.minStartDateTime = this.datePipe.transform(this.order.triageTime, 'dateTimeDisplay');
    // }
    // if (this.order !== null) {
    //   this.selectedFrequencyData = this.order.frequency;
    //   this.selectedFrequencyName = this.order.frequency.scheduleName;
    //   this.initialEndDateTime = this.order.endDatetime;
    //   this.selectedDuration = this.order.duration.duration;
    //   this.selectedDurationUnit = this.order.duration.durationUnit;
    //   this.selectedDurationUnitName = this.order.duration.durationUnit.name;
    // }
    // console.log('frequencyDefaultsThis', this);
  }

  // ****************** Frequency ***************************

  formatFrequency = (frequency: Frequency) => frequency.scheduleName;

  changeSelectedFrequency(frequency: Frequency) {
    // this.frequencyForm.patchValue({ frequency: frequency });
    const frequencyUnit = this.frequencies.find(fr => fr.id === frequency.id && fr.scheduleName === frequency.scheduleName);
    if (frequencyUnit) {
      if (!this.frequencyForm.value.frequencyData || !this.frequencyForm.value.frequencyData.id ||
        this.frequencyForm.value.frequencyData.id !== frequency.id) {
        this.updateAdministrations(frequency, this.frequencyForm.get('startTimeUTC').value, this.frequencyForm.get('endTimeUTC').value, 'changeSelectedFrequency');
      }
      this.selectedFrequencyData = frequencyUnit;
      this.selectedFrequencyName = frequencyUnit.scheduleName;
      this.frequencyForm.controls['frequency'].setValue(
        frequencyUnit.scheduleName
      );
      this.frequencyForm.controls['frequencyData'].setValue(frequencyUnit);
    } else {
      this.selectedFrequencyData = null;
      this.selectedFrequencyName = '';
      this.frequencyForm.controls['frequency'].setValue('');
      this.frequencyForm.controls['frequencyData'].setValue(null);
    }
    // console.log('frequencyThis', this);
    this.maybeResetEndTime();
    this.resetPRNIndicationValidators(frequency.prn)
  }

  changeSelectedFrequencyByLookup(frequencyName: string): void {
    const matchingFrequency = !frequencyName
      ? null
      : this.frequencies.find(
        (fndFrequency) => fndFrequency.scheduleName === frequencyName
      );
    this.changeSelectedFrequency(matchingFrequency);
  }

  // begin and end time in utc
  async updateAdministrations(frequency: Frequency, beginDateTime?: string, endDateTime?: string, location?: string) {
    if (frequency &&
      frequency.id &&
      // this.frequencyForm.get('frequency').valid) {
      this.frequencyForm.get('startTime').valid &&
      this.frequencyForm.get('endTime').valid) {
      // alert(`Frequency Id: ${frequency.id} `);
      // alert(`Location: ${location} `);
      // const beginDateTimeFormatted = beginDateTime? `${this.datePipe.transform(beginDateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` : '';
      // const endDateTimeFormatted = endDateTime? `${this.datePipe.transform(endDateTime, 'dateTimeSeconds')}${this.siteUTCOffset}` : '';
      // API accepts only time in server timezone 
      // const beginDateTimeFormatted = moment(beginDateTime).isValid() ? moment(beginDateTime).utcOffset(parseInt(this.siteUTCOffset)).format() :'';
      const beginDateTimeFormatted = beginDateTime? moment(beginDateTime).utcOffset(parseInt(this.siteUTCOffset)).format() :'';
      const endDateTimeFormatted = moment(endDateTime, moment.ISO_8601).isValid() ? moment(endDateTime).utcOffset(parseInt(this.siteUTCOffset)).format() :'';
      // alert(`Begin Date / Time: ${ beginDateTimeFormatted } `);
      // alert(`End Date / Time: ${ endDateTimeFormatted } `);
      this.availableAdministrations = await this.composerSchedulerService.getFrequencyAdministrationsFromAPI
        (this.userId, this.userSiteId, frequency.id, beginDateTimeFormatted, endDateTimeFormatted).toPromise();
      this.scheduledAdministrations = !this.availableAdministrations ? [] : [...this.availableAdministrations];
      // const patchValue = (this.availableAdministrations && this.availableAdministrations.length > 0) ? this.availableAdministrations : null;
      this.frequencyForm.controls.availableAdministrations.patchValue(this.availableAdministrations);
      this.frequencyForm.controls.scheduledAdministrations.patchValue(this.scheduledAdministrations);

      if (this.selectedDuration && this.selectedDurationUnit) {
        this.updateScheduledAdministrationsDuration(this.selectedDuration, this.selectedDurationUnit, true);
      }
      this.composerSchedulerService.signalOrderFrequencyChanged(this.medComponentId);

      // console.log('scheduledAdministrations', this.scheduledAdministrations);
      // console.log('frequencyFormThis', this);
    }
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
                v.scheduleName.toLowerCase().indexOf(term.toLowerCase()) > -1
            )
            .slice(0, 100);
        } else {
          subSet = this.frequencies.slice(0, 100);
        }
        return subSet.map((node) => node.scheduleName);
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
            .filter((f) => new RegExp(term, 'mi').test(f.scheduleName))
            .slice(0, 100)
      )
      // TODO this.frequencies but this is undefined
    );
  }

  // ****************** PRN Indication ***************************
  prnIndicationValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return { error: '** PRN indication is required' };
    } else {
      if (control.value.length > 100) {
        return { error: '** PRN indication only allows 100 characters' };
      }
    }
    return null;
  }

  changeSelectedPRNIndication(indication: PRNIndication) {
    console.log('changeSelectedPRNIndication: ', indication)
    // const option = this.prnOptions.find(o => o.optionDescription === indication.optionDescription);
    if (indication) {
      this.selectedPRNIndicationData = indication;
      this.selectedPRNIndicationDescription = indication.optionDescription;
      this.frequencyForm.controls['prnIndicationDescription'].setValue(indication.optionDescription);
      this.frequencyForm.controls['prnIndicationData'].setValue(indication);
    } else {
      this.selectedPRNIndicationData = null;
      this.selectedPRNIndicationDescription = '';
      this.frequencyForm.controls['prnIndicationDescription'].setValue('');
      this.frequencyForm.controls['prnIndicationData'].setValue(null);
    }
    // console.log('PRNIndicationThis', this);
  }

  changeSelectedPRNIndicationByLookup(optionDesc: string): void {
    console.log('PRN OPTION--------', optionDesc);
    let matchingPRNIndication = !optionDesc
      ? null
      : this.prnOptions.find(
        (o) => o.optionDescription === optionDesc
      );

    if (!matchingPRNIndication) {
      matchingPRNIndication = {
        'id': 0,
        'optionDescription': optionDesc 
      }
    }
    this.changeSelectedPRNIndication(matchingPRNIndication);
  }

  searchPRNIndication = (text$: Observable<string>) => {
    const debouncedText$ = text$.pipe(
      debounceTime(200),
      distinctUntilChanged()
    );
    const clicksWithClosedPopup$ = this.clickPRNIndication$.pipe(
      filter(() => !this.prnIndicationInstance.isPopupOpen())
    );
    const inputFocus$ = this.focusPRNIndication$;
    const mergeResults = merge(
      debouncedText$,
      inputFocus$,
      clicksWithClosedPopup$
    ).pipe(
      map((term) => {
        let subSet = [];
        if (term) {
          subSet = this.prnOptions
            .filter(
              (v) => v.optionDescription.toLowerCase().indexOf(term.toLowerCase()) > -1
            )
            .slice(0, 100);
        } else {
          subSet = this.prnOptions.slice(
            0,
            100
          );
        }
        return subSet.map((node) => node.optionDescription);
      })
    );
    return mergeResults;
  };

  resetPRNIndicationValidators(isPrn: boolean): void {
    if (isPrn) {
      this.frequencyForm.controls['prnIndicationDescription'].enable()
      this.frequencyForm.controls['prnIndicationDescription'].setValue('')
      this.frequencyForm.controls['prnIndicationDescription'].setValidators([Validators.required, Validators.maxLength(100), this.prnIndicationValidator])
    } else {
      this.frequencyForm.controls['prnIndicationDescription'].disable()
      this.frequencyForm.controls['prnIndicationDescription'].setValue('')
      this.frequencyForm.controls['prnIndicationData'].setValue(null)
      this.frequencyForm.controls['prnIndicationDescription'].clearValidators()
    }
    this.frequencyForm.controls['prnIndicationDescription'].updateValueAndValidity()
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
    this.updateScheduledAdministrationsDuration(this.selectedDuration, this.selectedDurationUnit);
    // console.log('durationUnitValidationErrors', durationUnitValidationErrors);
    // console.log('durationThis', this);
  }

  onDurationUnit(unit: DurationUnit) {
    // this.frequencyForm.patchValue({ durationUnit: unit });
    this.selectedDurationUnitName = unit.name;
    this.selectedDurationUnit = unit;
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
    this.updateScheduledAdministrationsDuration(this.selectedDuration, this.selectedDurationUnit);
    // console.log('durationValidationErrors', durationValidationErrors);
    // console.log('DurationUnitThis', this);
  }

  maybeResetEndTime(): void {
    if (
      this.frequencyForm.get('duration').valid && this.frequencyForm.get('duration').value !== null &&
      this.frequencyForm.get('durationUnit').valid
    ) {
      // this.frequencyForm.controls['startTime'].setValue('');
      this.frequencyForm.controls['endTime'].setValue('');
      this.frequencyForm.controls['endTimeUTC'].setValue(null);
    }
  }

  updateScheduledAdministrationsDuration(duration: number, durationUnit: DurationUnit, fromSchAdmin?: boolean) {
    // alert(`UpdateScheduledAdministrationsDuration0`);
    if (duration &&
      durationUnit &&
      this.frequencyForm.get('duration').valid &&
      this.frequencyForm.get('durationUnit').valid) {
      // alert(`UpdateScheduledAdministrationsDuration1`);
      this.scheduledAdministrations = [];

      // console.log('updateScheduledAdministrationDuration', duration, durationUnit, this.availableAdministrations);

      if (!this.selectedDurationUnit.durationInMinutes) {
        this.scheduledAdministrations = !this.availableAdministrations ? [] : this.availableAdministrations.slice(0, this.selectedDuration);
        // console.log('doseAdministrations', this.scheduledAdministrations);
      } else {
        const beginDateTimeUTC: string = (this.frequencyForm.get('startTime').value && this.frequencyForm.get('startTime').valid) ?
          this.frequencyForm.controls['startTimeUTC'].value :
          this.datePipe.transform(Date.now().toString(), 'UTC', this.siteUTCOffset);
        // console.log('beginDateTimeUTC0', beginDateTimeUTC);
        const calculatedEndDateTimeUTC = (this.frequencyForm.get('endTime').value && this.frequencyForm.get('endTime').valid) ?
          this.frequencyForm.get('endTimeUTC').value :
          `${this.datePipe.transform(
            moment.utc(beginDateTimeUTC).add(duration * durationUnit.durationInMinutes, 'minutes').toString(),
            'dateTimeSeconds',
            this.siteUTCOffset
          )
          }${this.siteUTCOffset}`;
        // console.log('beginDateTimeUTC', beginDateTimeUTC);
        // console.log('calculatedEndDateTimeUTC', calculatedEndDateTimeUTC);
        // console.log('updateAdminThis', this);
        this.scheduledAdministrations = !this.availableAdministrations ? [] :
        this.availableAdministrations.filter(
          // admin => beginDateTimeUTC <= admin.scheduleDateTime && calculatedEndDateTimeUTC >= admin.scheduleDateTime
          admin => moment(admin.scheduleDateTime).isBetween(beginDateTimeUTC, calculatedEndDateTimeUTC, null, '[)')
          );
        // console.log('updatedScheduledAdministrations', this.scheduledAdministrations);

      }
      this.frequencyForm.patchValue({ scheduledAdministrations: this.scheduledAdministrations });
      // console.log('durationScheduledAdmins', this.scheduledAdministrations, this);
      this.composerSchedulerService.setOrderScheduledAdministrations(this.medComponentId, this.scheduledAdministrations);
      this.composerSchedulerService.signalOrderFrequencyChanged(this.medComponentId);
    } else if (!duration && !durationUnit && !fromSchAdmin) {
      this.updateAdministrations(this.selectedFrequencyData, this.frequencyForm.get('startTimeUTC').value, this.frequencyForm.get('endTimeUTC').value, 'durationNoDurationSet')
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
      } else if (valueAsString.includes('.')) {
        return { error: '** Duration cannot be a decimal value' };
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
    // if (!this || !this.frequencyForm) {
    //   return null;
    // } else if (this.noDurationOrDateTimesEntered()) {
    //   return {
    //     error:
    //       '** Must select either Duration and Duration Unit, or Start and End Times',
    //   };
    //
    if (!control ||
      !this ||
      !this.frequencyForm ||
      this.frequencyForm.get('duration').invalid ||
      !this.selectedDuration
    ) {
      return null;
    } else if (
      this.frequencyForm.get('duration').valid && this.selectedDuration &&
      (control.value === undefined ||
        control.value === null ||
        control.value === '' ||
        Object.keys(this.frequencyForm.get('durationUnit').value).length ===
        0)
    ) {
      return {
        error: '** Duration Unit is required when Numeric Duration is selected',
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
      text = `${match[2]} /${match[3]}/${match[1]} ${match[4]}: ${match[5]} `;
    }
    return text;
  }

  onSelectTime(type: string) {
    if (type === 'start') {
      this.modalService.open(
        // 'date-time-modal',
        this.modalId,
        {
          dateTime: this.frequencyForm.controls['startTimeUTC'].value,
          event: this.startEvent,
          minDateTime: this.minStartDateTime,
          format: 'iso',
          siteUTCOffset: this.siteUTCOffset,
        },
        'Start Time'
      );
    } else {
      let dateTime: string;
      if (this.frequencyForm.controls['endTime'].value === '' || this.frequencyForm.controls['endTime'].value === null) {
        dateTime = this.frequencyForm.controls['startTimeUTC'].value;
      } else {
        dateTime = this.frequencyForm.controls['endTimeUTC'].value;
      }
      this.modalService.open(
        // 'date-time-modal',
        this.modalId,
        {
          dateTime: dateTime,
          event: this.endEvent,
          minDateTime: this.frequencyForm.controls['startTimeUTC'].value,
          format: 'iso',
          siteUTCOffset: this.siteUTCOffset,
        },
        'End Time'
      );
    }
    // this.maybeResetDuration();
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
          'Invalid date/time format. Must be mm/dd/yyyy hh:mm format.',
      };
    }
    else {
      const error: string = this.checkForStartStopDateSequenceError('startDateTime', control.value);
      if (error) {
        return {
          error
        };
      }
    }
    this.maybeResetDuration();
    return null;
  }

  endTimeValidator(control: AbstractControl): { [key: string]: any } | null {
    // console.log('durationControlValue', control.value);
    if (!this || !this.frequencyForm || !control || !control.value) {
      return null;
    } else if (!this.validDateTime(control.value)) {
      return {
        error:
          'Invalid date/time format. Must be mm/dd/yyyy hh:mm format.',
      };
    }
    else {
      const error: string = this.checkForStartStopDateSequenceError('endDateTime', control.value);
      if (error) {
        return {
          error
        };
      }
    }
    // else if (this.noDurationOrDateTimesEntered()) {
    //   return {
    //     error:
    //       '** Must select either Duration and Duration Unit, or Start and End Times',
    //   };
    // }
    // if (!control || !control.value || !this || !this.frequencyForm) {
    //   return null;
    // } else if (!this.validDateTime(control.value)) {
    //   return {
    //     error:
    //       'Invalid date/time or date/time format. Must be DD/MM/YYYY HH:MM format.',
    //   };
    // }
    this.maybeResetDuration();
    return null;
  }

  validDateTime(date: string) {
    const pattern = new RegExp(
      // '^(1[0-2]|0[1-9])/(3[01]|[12][0-9]|0[1-9])/[0-9]{4} (2[0-3]|[01]?[0-9]):([0-5]?[0-9])$'
      '^(1[0-2]|0[1-9])/(3[01]|[12][0-9]|0[1-9])/[0-9]{4} ([0-2][0-9]:[0-5][0-9])$'
    );
    if (date.search(pattern) === 0) {
      return true;
    }
  }

  checkForStartStopDateSequenceError(field: string, value: string): string {
    const startDateTime: string = this.frequencyForm.controls['startTime'].value;
    const endDateTime: string = this.frequencyForm.controls['endTime'].value;
    // alert(`StartStopDateSequenceErrorValue: ${ value } `);
    // alert(`StartStopDateSequenceErrorNow: ${ this.datePipe.transform(Date.now().toString(), 'dateTimeDisplay', this.siteUTCOffset) } `);
    
    if (!value) {
      return null;
    }
    // else if (moment(value).isBefore(moment(this.datePipe.transform(Date.now().toString(), 'dateTimeDisplay', this.siteUTCOffset)))) {
    else if (moment(this.datePipe.transform(value, 'dateTime'))
      .isBefore(moment(this.datePipe.transform(this.patientStoreService.visitStartDateTime, 'dateTime', this.siteUTCOffset)))) {
      return field === 'startDateTime' ? 'Start Time cannot be before visit arrival.' :
        'Stop Date/Time cannot be from the past.';
    } else if (
      !startDateTime ||
      !this.frequencyForm.controls['startTime'].valid ||
      !endDateTime ||
      !this.frequencyForm.controls['endTime'].valid) {
      return null;
    }
    // else if (moment(startDateTime).isAfter(moment(endDateTime))) {
    else if (moment(this.datePipe.transform(startDateTime, 'dateTime')).isAfter(moment(this.datePipe.transform(endDateTime, 'dateTime')))) {
      return field === 'startDateTime' ? 'Start Date/Time cannot be after Stop Date/Time.' :
        'Stop Date cannot be before Start Date/Time';
    }
    return null;
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
      !this.frequencyForm.get('startTime').value
      ? true
      : false;
  }

  onChangeStartTime(event) {
    if (this.validDateTime(event.target.value)) {
      this.frequencyForm.controls['startTimeUTC'].setValue(moment(event.target.value + this.siteUTCOffset, 'MM/DD/YYYY HH:mmZZ').format());
    }
  }
  onChangeEndTime(event) {
    if (this.validDateTime(event.target.value)) {
      this.frequencyForm.controls['endTimeUTC'].setValue(moment(event.target.value + this.siteUTCOffset, 'MM/DD/YYYY HH:mmZZ').format());
    }

  }
}
