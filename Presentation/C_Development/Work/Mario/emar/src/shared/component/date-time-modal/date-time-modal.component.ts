import { Component, OnInit, EventEmitter, ViewChild } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ModalService } from 'src/services/modal.service';
import { NgbTimeStruct, NgbDateStruct, NgbCalendar, NgbDatepicker } from '@ng-bootstrap/ng-bootstrap';
import { pairwise } from 'rxjs/operators';

@Component({
  selector: 'date-time-modal',
  templateUrl: './date-time-modal.component.html',
  styleUrls: []
})
export class DateTimeModalComponent implements OnInit {
    dateTimeForm: FormGroup;
    minDate: NgbDateStruct;
    minTime: NgbTimeStruct;
    event: EventEmitter<string>;
    @ViewChild('dp', { static: true }) datepicker: NgbDatepicker;

    constructor(
        private modalService: ModalService,
        private calendar: NgbCalendar) {
    }

    ngOnInit(): void {
        this.dateTimeForm = new FormGroup({
            'time': new FormControl(null),
            'date': new FormControl(null)
        });
        // the modal gets open
        this.modalService.modalOpening.subscribe(data => {
            const regExpDateTime = /\d+\/\d+\/\d{4} \d+:\d+/ 
            if (data.dateTime !== undefined && regExpDateTime.test(data.dateTime)) {
                let arr = data.dateTime.split(/\/| |:/);
                this.dateTimeForm.controls['time'].setValue({hour: +arr[3], minute: +arr[4]});
                this.dateTimeForm.controls['date'].setValue({day: +arr[1], month: +arr[0], year: +arr[2]});
            }
            if (data.minDateTime !== undefined && regExpDateTime.test(data.minDateTime)) {
                let arr = data.dateTime.split(/\/| |:/);
                this.minDate = {day: +arr[1], month: +arr[0], year: + arr[2]};
                // TODO this.minTime
            }
            if (data.event !== undefined) {
                this.event = data.event;
            }
        });
        // observables have a pairwise
        this.dateTimeForm.controls['time']
        .valueChanges
        .pipe(pairwise())
        .subscribe(([prev, next]: [NgbTimeStruct, NgbTimeStruct]) => {
            // TODO implements minDateTime on hour
            if (next === null) return;
            if (prev.hour == 23 && next.hour == 0) {
                // TODO change date
                const day = this.dateTimeForm.controls['date'].value.day + 1;
                console.log('NavigateUp', this.datepicker, day);
                // TODO
                // this.datepicker.navigateTo({year: 2020, month: 8, day: day}); 
                // this.dateTimeForm.controls['date'].patchValue('day', this.dateTimeForm.controls['date'].value.day + 1);
            }
            if (prev.hour == 0 && next.hour == 23) {
                // TODO change date
                console.log('NavigateDown', this.datepicker);
                // TODO
                // this.datepicker.navigateTo({year: 2020, month: 8, day: this.dateTimeForm.controls['date'].value.day - 1});
                // this.dateTimeForm.controls['date'].patchValue('day', this.dateTimeForm.controls['date'].value.day - 1)
            }
        });
    }

    onCancel() {
        this.modalService.close('date-time-modal');
        this.dateTimeForm.reset();
    }

    onNow() {
        const now: Date = new Date();
        this.dateTimeForm.controls['time'].setValue({
            hour: now.getHours(),
            minute: now.getMinutes(),
            second: 0
        });
        this.dateTimeForm.controls['date'].setValue(this.calendar.getToday());
    }

    onSubmit() {
        const ngbTime: NgbTimeStruct = this.dateTimeForm.controls['time'].value;
        const ngbDate: NgbDateStruct = this.dateTimeForm.controls['date'].value;
        console.log('NGBDATE', ngbDate, ngbTime)
        if (ngbTime !== null && ngbDate !== null) {
            const time: string = ('0' + ngbTime['hour'].toString()).slice(-2) + ':' + ('0' + ngbTime['minute'].toString()).slice(-2);
            const date: string = ('0' + ngbDate['month'].toString()).slice(-2) + '/' + ('0' + ngbDate['day'].toString()).slice(-2) + '/' + ngbDate['year'].toString();
            this.event.emit(date + ' ' + time);
        }
        this.modalService.close('date-time-modal');
        this.dateTimeForm.reset();
        this.event = null;
        this.minDate = null;
        this.minTime = null;
    }
}
