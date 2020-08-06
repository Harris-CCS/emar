import { Component, Output, EventEmitter, OnInit, Input, ɵisDefaultChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, bufferTime } from 'rxjs/operators';
import { NgbTimeStruct, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';

import { Frequency } from '../../../app/interfaces/frequency';
import { FREQUENCIES } from '../../../app/mockup/frequencies';
import { ModalService } from 'src/services/modal.service';
const DURATION_UNITS: string[] = ["Doses", "Hours", "Days"];

@Component({
    selector: 'frequency-form',
    templateUrl: './frequency-form.component.html',
    styleUrls: ['../composer-med.component.scss']
})

export class FrequencyFormComponent implements OnInit {
    @Output() formReady = new EventEmitter<FormGroup>();
    frequencyForm: FormGroup;
    @Input() preferredFrequencies: Frequency[] = [];
    selectedFrequency: Frequency = null; // TODO come from composer
    frequencies: Frequency[] = FREQUENCIES; // TODO API
    selectedDuration: string = ''; // TODO come from composer
    preferredDurationUnits: string[] = DURATION_UNITS;
    selectedStartTime: string = ''; // TODO come from composer
    selectedStartDate: string = ''; // TODO come from composer
    selectedEndTime: string = ''; // TODO come from composer
    selectedEndDate: string = ''; // TODO come from composer

    constructor(private fb: FormBuilder,
        private modalService: ModalService) {}

    ngOnInit() {
        this.frequencyForm = this.fb.group({
            'frequency': new FormControl(null, Validators.required),
            'duration': new FormControl(null),
            'durationUnit': new FormControl(null),
            'startTime': new FormControl(null),
            'endTime': new FormControl(null)
       });
        this.formReady.emit(this.frequencyForm);
    }

    lookupFrequency(text$: Observable<string>) {
        return text$.pipe(
            debounceTime(200),
            distinctUntilChanged(),
            map(term => term.length < 0 ? []
            : FREQUENCIES.filter(f => new RegExp(term, 'mi').test(f.frequencyName)).slice(0, 10))
            // TODO this.frequencies but this is undefined
        )
    }

    formatFrequency = (frequency: Frequency) => frequency.frequencyName

    usePreferredFrequency(frequency: Frequency) {
        this.frequencyForm.patchValue({'frequency': frequency});
    }

    onDurationUnit(unit: string) {
        this.frequencyForm.patchValue({'durationUnit': unit});
    }

    onSelectTime(title: string, time?: string, date?: string) {
        // TODO pretty sure a better way to set the now
        const now: Date = new Date;
        let defaultTime: NgbTimeStruct;
        let defaultDate: NgbDateStruct;
        let arr;
        if (typeof time !== 'undefined' && time !== '') {
            arr = time.split(':');
            defaultTime = {
                hour: +arr[0],
                minute: +arr[1],
                second: 0
            };
        } else {
            defaultTime = {
                hour: now.getHours(),
                minute: now.getMinutes(),
                second: 0
            };
        }
        if (typeof date !== 'undefined' && date !== '') {
            arr = date.split('/');
            defaultDate = {
                year: +arr[2],
                month: +arr[0],
                day: +arr[1]};
        } else {
            defaultDate = {
                year: now.getFullYear(),
                month: now.getMonth()+1,
                day: now.getDate()};
        }
        this.modalService.open('date-time-modal', {time: defaultTime, date: defaultDate}, title);
    }
}