import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { DateTimePipe } from 'src/shared/pipes/dateTime';
import { ModalService } from 'src/services/modal.service';
import { UserStoreService } from '../../../services/user-store.service';
import { AdministrationEvent } from '../../../app/interfaces/administrationEvent';
import { ScheduledAdministration } from '../../../app/interfaces/scheduled-administration';
import { NgbCalendar, NgbPeriod, NgbDate } from '@ng-bootstrap/ng-bootstrap';
import { FormGroup } from '@angular/forms';
import * as moment from 'moment';
import { Subscription } from 'rxjs';
import { ComposerSchedulerService } from '../../../services/composer-scheduler.service';
import { SiteStoreService } from 'src/services/site-store.service';

interface CalendarEvent {
  id: number;
  time: string;
  idPrev?: number;
  idNext?: number;
  disabled?: boolean;
}
interface CalendarDay {
  id?: number;
  name: string;
  day: string;
  month: string;
  events: CalendarEvent[];
}
const DAYLABELS = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];
/*
const MAXWEEKS = 2; // number of weeks displayed
const EVENTS: AdministrationEvent[] = [
  { id: 1, eventDate: 'today', eventTime: '09:00' },
  { id: 2, eventDate: 'today', eventTime: '18:00' },
  { id: 3, eventDate: 'today+1', eventTime: '09:00' },
  { id: 4, eventDate: 'today+1', eventTime: '18:00' },
  { id: 5, eventDate: 'today+2', eventTime: '09:00' },
  { id: 6, eventDate: 'today+2', eventTime: '18:00' },
  { id: 7, eventDate: 'today+3', eventTime: '09:00' },
  { id: 8, eventDate: 'today+3', eventTime: '18:00' },
  { id: 9, eventDate: 'today+4', eventTime: '09:00' },
  { id: 10, eventDate: 'today+4', eventTime: '18:00' },
  { id: 11, eventDate: 'today+5', eventTime: '09:00' },
  { id: 12, eventDate: 'today+5', eventTime: '18:00' },
  { id: 13, eventDate: 'today+6', eventTime: '09:00' },
  { id: 14, eventDate: 'today+6', eventTime: '18:00' },
  { id: 15, eventDate: 'today+7', eventTime: '09:00' },
  { id: 16, eventDate: 'today+7', eventTime: '18:00' },
];

const WEEKS = [
    [
        {name: 'Su', day: '26', month: '4', events: []},
        {name: 'Mo', day: '27', month: '4', events: [{id:1, time:'09:00'}, {id:2, time:'18:00'}]},
        {name: 'Tu', day: '28', month: '4', events: [{id:3, time:'09:00'}, {id:4, time:'18:00'}]},
        {name: 'We', day: '29', month: '4', events: [{id:5, time:'09:00'}, {id:6, time:'18:00'}]},
        {name: 'Th', day: '30', month: '4', events: [{id:7, time:'09:00'}, {id:8, time:'18:00'}]},
        {name: 'Fr', day: '1', month: '5', events: [{id:9, time:'09:00'}, {id:10, time:'18:00'}]},
        {name: 'Sa', day: '2', month: '5', events: [{id:11, time:'09:00'},{id:12, time:18:00'}]}
    ],
    [
        {name: 'Su', day: '3', month: '5', events: [{id:13, time:'09:00'}, {id:14, time:'18:00'}]},
        {name: 'Mo', day: '4', month: '5', events: []},
        {name: 'Tu', day: '5', month: '5', events: []},
        {name: 'We', day: '6', month: '5', events: []},
        {name: 'Th', day: '7', month: '5', events: []},
        {name: 'Fr', day: '8', month: '5', events: []},
        {name: 'Sa', day: '9', month: '5', events: []}
    ]
]
*/

@Component({
  selector: 'calendar-form',
  templateUrl: './calendar-form.component.html',
  styleUrls: [
    './calendar-form.component.scss',
    '../composer-med.component.scss',
  ],
})
export class CalendarFormComponent implements OnInit, OnDestroy {
  weeks: CalendarDay[][];
  events: AdministrationEvent[];
  selectedEvent: AdministrationEvent = null;
  private scheduledAdministrations: Array<ScheduledAdministration>;
  setEvent = new EventEmitter<string>();
  siteUTCOffset: string = '';
  subscriptionOrderFrequencyChanged: Subscription;
  subscriptionResetComponentMedFormId: Subscription;
  subscriptionSetEvent: Subscription;
  @Input() composerMedForm: FormGroup;
  @Input() medComponentId: number;
  @Input() modalId: string;
  moreAdministrations: Boolean = false;
  maxWeeks: number = 2;

  constructor(
    private modalService: ModalService,
    private calendarService: NgbCalendar,
    private composerSchedulerService: ComposerSchedulerService,
    private userStoreService: UserStoreService,
    private siteStoreService: SiteStoreService,
    private datePipe: DateTimePipe
  ) {
    this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
  }
  ngOnInit() {
    this.maxWeeks = Math.floor(this.siteStoreService.schedule_future_items / 7);
    this.subscriptionOrderFrequencyChanged = this.composerSchedulerService.orderFrequencyChanged.subscribe(() => {
      if (
        this.composerSchedulerService.orderFrequencyChanged &&
        this.composerSchedulerService.orderFrequencyChanged.value ===
        this.medComponentId
      ) {
        this.compileScheduledAdministrations();
      }

      this.subscriptionResetComponentMedFormId = this.composerSchedulerService.resetComponentMedFormId.subscribe(() => {
        if (
          this.composerSchedulerService.resetComponentMedFormId &&
          this.composerSchedulerService.resetComponentMedFormId.value ===
          this.medComponentId
        ) {
          this.compileScheduledAdministrations();
        }
      });
    });

    this.subscriptionSetEvent = this.setEvent.subscribe((dateTime: string) => {
      // console.log('SELECT',dateTime, this.selectedEvent);
      if (dateTime !== this.selectedEvent.eventDateTimeUTC) {
        this.events = this.events.map((event: AdministrationEvent) => {
          if (event.id === this.selectedEvent.id) {
            const mo: moment.Moment = moment(dateTime).utcOffset(parseInt(this.siteUTCOffset));
            event.eventDate = mo.format('MM/DD/yyyy');
            event.eventTime = mo.format('HH:mm');
            event.eventDateTimeUTC = dateTime; //`${this.datePipe.transform(dateTime, 'dateTimeSeconds')}${this.siteUTCOffset}`;
            this.composerSchedulerService.setOrderScheduledAdministration(this.medComponentId, event.id, event.eventDateTimeUTC);
          }
          return event;
        });
        this.weeks = this.group(this.events);
        this.compileScheduledAdministrations();
        // console.log('newCalendarDateTimeScheduledAdministrations', this.scheduledAdministrations);
      }
    });
    // console.log('CalendarThis', this);
  }

  ngOnDestroy() {
    this.subscriptionResetComponentMedFormId.unsubscribe();
    this.subscriptionOrderFrequencyChanged.unsubscribe();
    this.subscriptionSetEvent.unsubscribe();
  }

  compileScheduledAdministrations() {
    let period: NgbPeriod = 'd';
    this.scheduledAdministrations = this.composerSchedulerService.getOrderScheduledAdministrations(this.medComponentId);
    this.events = this.scheduledAdministrations.map((event: ScheduledAdministration, index) => {
      // console.log('scheduledEvent', event);
      if (event.scheduleDateTime) {
        // const eventDateTime = formatDate(event.scheduleDateTime, 'MM/dd/yyyyTHH:mm:ssZZZZZ', 'en-US')

        // const arr = event.eventDate.split('+');
        // let date: NgbDate = this.calendarService.getToday();
        // if (arr.length > 1) {
        //   date = this.calendarService.getNext(date, period, +arr[1]);
        // }
        return {
          id: index,
          // eventDate:
          //   ('0' + date.month.toString()).slice(-2) +
          //   '/' +
          //   ('0' + date.day.toString()).slice(-2) +
          //   '/' +
          //   date.year.toString(),
          // eventTime: event.eventTime,
          // eventDate: formatDate(event.scheduleDateTime, 'MM/dd/yyyy', 'en-US'),
          // eventTime: formatDate(event.scheduleDateTime, 'HH:mm', 'en-US')
          eventDate: this.datePipe.transform(event.scheduleDateTime, 'dateDisplay', this.siteUTCOffset),
          eventTime: this.datePipe.transform(event.scheduleDateTime, 'time', this.siteUTCOffset),
          eventDateTimeUTC: event.scheduleDateTime
        };
      }
    });
    this.weeks = this.group(this.events);
    // console.log('compileScheduledAdministrationsThis', this);
  }

  // group events by day and week
  // Hypothesis: the first event is the first one in time - otherwise need to fi nd the first event
  group(events: AdministrationEvent[]): CalendarDay[][] {
    let weeks: CalendarDay[][] = [];
    let iDay: number;
    let iWeek: number = 0;
    let first: NgbDate;
    let date: NgbDate;
    let arr: string[];
    this.moreAdministrations = false;
    for (let ii = 0; ii < events.length; ++ii) {
      let event = events[ii];
      arr = event.eventDate.split('/');
      date = new NgbDate(+arr[2], +arr[0], +arr[1]);
      iDay = this.calendarService.getWeekday(date); // 1=Mon ... 7=Sun
      if (iDay == 7) {
        iDay = 0;
      }
      // console.log("EVENT",ii,weeks.length,iDay,iWeek)
      if (weeks.length < this.maxWeeks) { // build empty weeks array
        if (iDay == 0) {
          first = date;
        } else {
          first = this.calendarService.getPrev(date, 'd', iDay);
        }
        for (let w = 0; w < this.maxWeeks; w++) {
          weeks[w] = [];
          for (let d = 0; d < 7; d++) {
            weeks[w][d] = {
              name: DAYLABELS[d],
              day: ('0' + first.day.toString()).slice(-2),
              month: ('0' + first.month.toString()).slice(-2),
              events: [],
            };
            first = this.calendarService.getNext(first, 'd', 1);
          }
        }
      } else if (iWeek < this.maxWeeks && +weeks[iWeek][iDay].day !== +arr[1]) {
        iWeek = iWeek + 1;
      }
      if (iWeek < this.maxWeeks) {
        // do not display events outside the displayed weeks
        // console.log('eventDateTimeBefore', `${event.eventDate} ${event.eventTime}`);
        // console.log('eventDateTimeFromPipe', event.eventDateTimeUTC);
        // console.log('eventDateTimeMoment', moment(event.eventDateTimeUTC));
        weeks[iWeek][iDay].events.push({
          time: event.eventTime,
          id: event.id,
          idPrev: ii === 0 ? -1 : events[ii - 1].id,
          idNext: ii === events.length - 1 ? -1 : events[ii + 1].id,
          disabled: !moment().isBefore(moment(event.eventDateTimeUTC)),
        });
      } else {
        this.moreAdministrations = true;
      }
    }
    // console.log('WEEKS', weeks);
    return weeks;
  }

  // transform MM/DD/YYYY HH:MM to YYYY-MM-DD HH:MM
  local2ISO(dateTime: string) {
    // console.log('DDDDDDDD', dateTime);
    let regexp = /(..)\/(..)\/(....) (..):(..)/;
    let match = regexp.exec(dateTime);
    return `${match[3]}-${match[1]}-${match[2]} ${match[4]}:${match[5]}`;
  }

  // they change the time - for instance: they want 9:00 and 18:00 but today as a first day it will be special as the patient arrive at 10:00
  // or they know that for a day the patient will have a special exam and will be in another unit.
  onSelectTime(event: CalendarEvent) {
    const elt: AdministrationEvent = this.events.find(
      ({ id }) => id === event.id
    );
    if (typeof elt !== 'undefined') {
      let minDateTimeUTC: string = '';
      let maxDateTimeUTC: string = '';
      if (event.idPrev >= 0) {
        const eltPrev: AdministrationEvent = this.events.find(
          ({ id }) => id === event.idPrev
        );
        minDateTimeUTC = eltPrev.eventDateTimeUTC;
      } else {
        minDateTimeUTC = this.composerMedForm.controls['frequency'].value.startTimeUTC;
      }
      if (event.idNext >= 0) {
        const eltNext: AdministrationEvent = this.events.find(
          ({ id }) => id === event.idNext
        );
        maxDateTimeUTC = eltNext.eventDateTimeUTC;
      } else {
        maxDateTimeUTC = this.composerMedForm.controls['frequency'].value.endTimeUTC;
      }

      this.selectedEvent = elt;
      this.modalService.open(
        this.modalId,
        {
          dateTime: elt.eventDateTimeUTC,
          event: this.setEvent,
          format: 'iso',
          siteUTCOffset: this.siteUTCOffset,
          minDateTime: minDateTimeUTC,
          maxDateTime: maxDateTimeUTC,
        },
        'Administration'
      );
    }
  }
}
