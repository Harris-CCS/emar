import { Component, OnInit, EventEmitter } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ModalService } from 'src/services/modal.service';
import { NgbTimeStruct, NgbDateStruct, NgbCalendar, NgbDatepicker, NgbDate } from '@ng-bootstrap/ng-bootstrap';
import { pairwise } from 'rxjs/operators';
import * as moment from 'moment';

@Component({
  selector: 'date-time-modal',
  templateUrl: './date-time-modal.component.html',
  styleUrls: ['./date-time-modal.component.scss']
})
export class DateTimeModalComponent implements OnInit {
    dateTimeForm: FormGroup;
    minDateTime: string = '';
    minDate: NgbDateStruct = null;
    maxDateTime: string = '';
    maxDate: NgbDateStruct = null;
    event: EventEmitter<string>; // use to send the final date time
    navigation: string = "arrows"; // default: select (to see navigation on month and year)
    hourStep: number = 1;
    minuteStep: number = 5;
    canHourUp: boolean = true;
    canHourDown: boolean = true;
    canMinuteUp: boolean = true;
    canMinuteDown: boolean = true;
    canNow: boolean = true;

    constructor(
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
        this.modalService.modalOpening.subscribe(data => {
            const regExpDateTime = /\d+\/\d+\/\d{4} \d+:\d+/ 
            if (data.minDateTime !== undefined && regExpDateTime.test(data.minDateTime)) {
                this.minDateTime = data.minDateTime;
                const arr = this.minDateTime.split(/\/| |:/);
                this.minDate = {'year': +arr[2], 'month': +arr[0], 'day': +arr[1]};
                this.canNow = !moment().isBefore(moment(this.local2ISO(this.minDateTime)));
            } else {
                this.minDateTime = '';
                this.minDate = null;
                this.canNow = true;
            }
            if (data.maxDateTime !== undefined && regExpDateTime.test(data.maxDateTime)) {
                this.maxDateTime = data.maxDateTime;
                const arr = this.maxDateTime.split(/\/| |:/);
                this.maxDate = {'year': +arr[2], 'month': +arr[0], 'day': +arr[1]};
            } else {
                this.maxDateTime = '';
                this.maxDate = null;
            }
            if (data.dateTime !== undefined && regExpDateTime.test(data.dateTime)) {
                let arr = data.dateTime.split(/\/| |:/);
                this.dateTimeForm.controls['hour'].setValue(arr[3]);
                this.dateTimeForm.controls['minute'].setValue(arr[4]);
                this.dateTimeForm.controls['date'].setValue({day: +arr[1], month: +arr[0], year: +arr[2]});
                this.disableButtons();
            }
            if (data.event !== undefined) {
                this.event = data.event;
            }
        });
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
        const currentDateTime:moment.Moment = moment(cd.year.toString() + '-' + this.pad(cd.month) + '-' + this.pad(cd.day) + ' ' + h + ':' + m);
        if (this.minDateTime !== '') {
            const minDateTime:moment.Moment = moment(this.local2ISO(this.minDateTime));
            this.canHourDown = !currentDateTime.clone().subtract(this.hourStep, 'hour').isBefore(minDateTime);
            this.canMinuteDown = !currentDateTime.clone().subtract(this.minuteStep, 'minute').isBefore(minDateTime);
        } else {
            this.canHourDown = true;
            this.canMinuteDown = true;
        }
        if (this.maxDateTime !== '') {
            const maxDateTime:moment.Moment = moment(this.local2ISO(this.maxDateTime));
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
        this.modalService.close('date-time-modal');
        this.dateTimeForm.reset();
    }

    onNow() {
        const now: Date = new Date();
        this.dateTimeForm.controls['hour'].setValue(this.pad(now.getHours()));
        this.dateTimeForm.controls['minute'].setValue(this.pad(now.getMinutes()));
        this.dateTimeForm.controls['date'].setValue(this.calendarService.getToday());
    }

    onSubmit() {
        const hour: string = this.dateTimeForm.controls['hour'].value;
        const minute: string = this.dateTimeForm.controls['minute'].value;
        const ngbDate: NgbDateStruct = this.dateTimeForm.controls['date'].value;
        if (hour !== null && minute !== null && ngbDate !== null) {
            const time: string = hour + ':' + minute;
            const date: string = this.pad(ngbDate['month']) + '/' + this.pad(ngbDate['day']) + '/' + ngbDate['year'].toString();
            this.event.emit(date + ' ' + time);
        }
        this.modalService.close('date-time-modal');
        this.dateTimeForm.reset();
        this.event = null;
        this.minDateTime = null;
    }
}
