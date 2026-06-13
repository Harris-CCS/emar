import { Component, OnInit, EventEmitter, Input } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ModalService } from 'src/services/modal.service';
import { NgbTimeStruct, NgbDateStruct, NgbCalendar, NgbDatepicker, NgbDate } from '@ng-bootstrap/ng-bootstrap';
import { pairwise } from 'rxjs/operators';
import * as moment from 'moment';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
import { DatePipe } from '@angular/common';

@Component({
    selector: 'date-time-modal',
    templateUrl: './date-time-modal.component.html',
    styleUrls: ['./date-time-modal.component.scss']
})
export class DateTimeModalComponent implements OnInit {
    @Input() modalId: string;
    dateTimeForm: FormGroup;
    minDateTime: string = ''; //2021-02-02T12:22:00-0600
    minDate: NgbDateStruct = null;
    maxDateTime: string = ''; 
    maxDate: NgbDateStruct = null;
    event: EventEmitter<any>; // use to send the final date time
    navigation: string = "arrows"; // default: select (to see navigation on month and year)
    hourStep: number = 1;
    minuteStep: number = 1;
    canHourUp: boolean = true;
    canHourDown: boolean = true;
    canMinuteUp: boolean = true;
    canMinuteDown: boolean = true;
    canNow: boolean = true;
    format: string; // iso: 2009-12-23T11:12, other: 12/23/2009 11:22
    id: number = null;
    siteUTCOffset: string = null;

    constructor(
        private datePipe: DatePipe,
        private modalService: ModalService,
        private calendarService: NgbCalendar) {
    }

    ngOnInit(): void {
        this.dateTimeForm = new FormGroup({
            'hour': new FormControl(null),
            'minute': new FormControl(null),
            'date': new FormControl(null)
        });
        // the modal gets open
        this.modalService.modalOpening.subscribe(({ data }) => {
            const regExpDateTime = /\d+\/\d+\/\d{4} \d+:\d+/
            const regExpIso = /\d+\-\d+\-\d+.\d+:\d+:.*/
            this.canNow = true;
            // console.log('DATEMODAL',data);
            if (data.siteUTCOffset !== undefined) {
                this.siteUTCOffset = data.siteUTCOffset;
            } else {
                this.siteUTCOffset = null;
            }
            if (data.minDateTime !== undefined && regExpDateTime.test(data.minDateTime)) {
                // no more used
                this.minDateTime = data.minDateTime;
                const arr = this.minDateTime.split(/\/| |:/);
                this.minDate = { 'year': +arr[2], 'month': +arr[0], 'day': +arr[1] };
                this.canNow = !moment().isBefore(moment(this.local2ISO(this.minDateTime)));
            } else if (data.minDateTime !== undefined && regExpIso.test(data.minDateTime)) {
                const mo: moment.Moment = moment(data.minDateTime);
                this.minDateTime = data.minDateTime;
                this.minDate = { 'year': mo.year(), 'month': mo.month() + 1, 'day': mo.date() };
                this.canNow = !moment().isBefore(mo);
            } else {
                this.minDateTime = '';
                this.minDate = null;
            }
            if (data.maxDateTime !== undefined && regExpDateTime.test(data.maxDateTime)) {
                // obsolte does not work
                this.maxDateTime = data.maxDateTime;
                const arr = this.maxDateTime.split(/\/| |:/);
                this.maxDate = { 'year': +arr[2], 'month': +arr[0], 'day': +arr[1] };
            } else if (data.maxDateTime !== undefined && regExpIso.test(data.maxDateTime)) {
                const mo: moment.Moment = moment(data.maxDateTime);
                this.maxDateTime = data.maxDateTime
                this.maxDate = { 'year': mo.year(), 'month': mo.month() + 1, 'day': mo.date() };
                this.canNow = this.canNow && !moment().isAfter(mo);
            } else {
                this.maxDateTime = '';
                this.maxDate = null;
            }
            if (data.dateTime !== undefined && regExpDateTime.test(data.dateTime)) {
                let arr = data.dateTime.split(/\/| |:/);
                this.dateTimeForm.controls['hour'].setValue(arr[3]);
                this.dateTimeForm.controls['minute'].setValue(arr[4]);
                this.dateTimeForm.controls['date'].setValue({ day: +arr[1], month: +arr[0], year: +arr[2] });
                this.disableButtons();
            } else if (data.dateTime !== undefined && regExpIso.test(data.dateTime)) {
                let mo = moment(data.dateTime);
                if (this.siteUTCOffset !== null) mo = mo.utcOffset(parseInt(this.siteUTCOffset));
                this.dateTimeForm.controls['hour'].setValue(this.pad(mo.hour()));
                this.dateTimeForm.controls['minute'].setValue(this.pad(mo.minute()));
                this.dateTimeForm.controls['date'].setValue({ day: mo.date(), month: mo.month() + 1, year: mo.year() });
                this.disableButtons();
            }
            if (data.event !== undefined) {
                this.event = data.event;
            }
            if (data.format !== undefined) {
                this.format = data.format;
            }
            this.id = (data.id !== undefined) ? data.id : null; // we have either a specific event or an id an a generic event
        });
        this.modalId = this.modalId || 'date-time-modal';
    }

    changeHour(delta: number) {
        let hour: number = +this.dateTimeForm.controls['hour'].value + delta;
        const currentDate: NgbDate = this.dateTimeForm.controls['date'].value;
        if (delta > 0 && hour > 23) {
            hour = hour - 24;
            this.dateTimeForm.controls['date'].setValue(this.calendarService.getNext(currentDate, 'd', 1));
        }
        if (delta < 0 && hour < 0) {
            hour = hour + 24;
            this.dateTimeForm.controls['date'].setValue(this.calendarService.getPrev(currentDate, 'd', 1));
        }
        this.dateTimeForm.controls['hour'].setValue(this.pad(hour));
        this.disableButtons();
    }

    changeMinute(delta: number) {
        let minute: number = +this.dateTimeForm.controls['minute'].value + delta;
        if (delta > 0 && minute > 59) {
            minute = minute - 60;
            this.changeHour(+1);
        }
        if (delta < 0 && minute < 0) {
            minute = minute + 60;
            this.changeHour(-1);
        }
        this.dateTimeForm.controls['minute'].setValue(this.pad(minute));
        this.disableButtons();
    }

    disableButtons() {
        const cd: NgbDate = this.dateTimeForm.controls['date'].value;
        const h = this.dateTimeForm.controls['hour'].value;
        const m = this.dateTimeForm.controls['minute'].value;
        const currentDateTime: moment.Moment = moment(cd.year.toString() + '-' + this.pad(cd.month) + '-' + this.pad(cd.day) + ' ' + h + ':' + m + this.siteUTCOffset);
        // console.log('DISABLE', currentDateTime.format(),",",this.minDateTime)
        if (this.minDateTime !== '') {
            const minDateTime: moment.Moment = moment(this.minDateTime);
            this.canHourDown = !currentDateTime.clone().subtract(this.hourStep, 'hour').isBefore(minDateTime);
            this.canMinuteDown = !currentDateTime.clone().subtract(this.minuteStep, 'minute').isBefore(minDateTime);
        } else {
            this.canHourDown = true;
            this.canMinuteDown = true;
        }
        if (this.maxDateTime !== '') {
            const maxDateTime: moment.Moment = moment(this.maxDateTime);
            this.canHourUp = currentDateTime.clone().add(this.hourStep, 'hour').isBefore(maxDateTime);
            this.canMinuteUp = currentDateTime.clone().add(this.minuteStep, 'minute').isBefore(maxDateTime);
        } else {
            this.canHourUp = true;
            this.canMinuteUp = true;
        }
    }

    // add a 0 if number is only one digit
    pad(n: number): string {
        return ('0' + n.toString()).slice(-2);
    }

    // transform MM/DD/YYYY HH:MM to YYYY-MM-DD HH:MM
    local2ISO(dateTime: string) {
        let regexp = /(..)\/(..)\/(....) (..):(..)/;
        let match = regexp.exec(dateTime);
        return `${match[3]}-${match[1]}-${match[2]} ${match[4]}:${match[5]}`;
    }

    onCancel() {
        // this.modalService.close('date-time-modal');
        this.modalService.close(this.modalId);
        this.dateTimeForm.reset();
    }

    onNow() {
        let now: moment.Moment = moment();
        if (this.siteUTCOffset !== null) now = now.utcOffset(parseInt(this.siteUTCOffset));
        this.dateTimeForm.controls['hour'].setValue(this.pad(now.hour()));
        this.dateTimeForm.controls['minute'].setValue(this.pad(now.minute()));
        const ngbDate: NgbDateStruct = {'year': now.year(), 'month': now.month()+1, 'day': now.date()};
        this.dateTimeForm.controls['date'].setValue(ngbDate);
        this.disableButtons();
    }

    onSubmit() {
        const hour: string = this.dateTimeForm.controls['hour'].value;
        const minute: string = this.dateTimeForm.controls['minute'].value;
        const ngbDate: NgbDateStruct = this.dateTimeForm.controls['date'].value;
        if (hour !== null && minute !== null && ngbDate !== null) {
            const text: string = ngbDate.year.toString() + '-' + this.pad(ngbDate.month) + '-' + this.pad(ngbDate.day) + 'T' + hour + ':' + minute + ':00.0000000' + this.siteUTCOffset;
            let mo: moment.Moment = moment(text);
            const dateTime = (this.format == 'iso')? mo.format(): mo.format('MM/DD/yyyy HH:mm');
            if (this.id == null) {
                this.event.emit(dateTime);
            } else {
                this.event.emit({ "dateTime": dateTime, "id": this.id });
            }
        }
        // this.modalService.close('date-time-modal');
        this.modalService.close(this.modalId);
        this.dateTimeForm.reset();
        this.event = null;
        this.minDateTime = null;
    }
}
