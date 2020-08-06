import { Component, OnInit, Input } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ModalService } from 'src/services/modal.service';
import { NgbTimeStruct, NgbDateStruct, NgbDate } from '@ng-bootstrap/ng-bootstrap';
import { NgbTime } from '@ng-bootstrap/ng-bootstrap/timepicker/ngb-time';

@Component({
  selector: 'date-time-modal',
  templateUrl: './date-time-modal.component.html',
  styleUrls: []
})
export class DateTimeModalComponent implements OnInit {
    dateTimeForm: FormGroup;
    time: NgbTimeStruct;
    date: NgbDateStruct;

    constructor(private modalService: ModalService) {
    }

    ngOnInit(): void {
        this.dateTimeForm = new FormGroup({
            'time': new FormControl(null),
            'date': new FormControl(null)
        });
    }

    getData() {
        return this.modalService.retrieveModalData('date-time-modal') || {};
    }
    
    getTime() {
        return this.getData().time || {};
    }

    getDate() {
        return this.getData().date || {};
    }

    onCancel() {
        this.modalService.close('date-time-modal');
        this.dateTimeForm.reset();
    }

    onNow() {
        // TODO pretty sure a better way
        const now: Date = new Date();
        let defaultDate: NgbDateStruct;
        let defaultTime: NgbTimeStruct;
        defaultTime = {
            hour: now.getHours(),
            minute: now.getMinutes(),
            second: 0
        };
        defaultDate = {
            year: now.getFullYear(),
            month: now.getMonth()+1,
            day: now.getDate()
        };
        console.log(defaultDate)
        this.dateTimeForm.controls['time'].setValue(defaultTime);
        this.dateTimeForm.controls['date'].setValue(defaultDate);
    }

    onSubmit() {
        console.log(this.dateTimeForm); console.log(this.time);
        this.modalService.close('date-time-modal');
        this.dateTimeForm.reset();
    }
}
