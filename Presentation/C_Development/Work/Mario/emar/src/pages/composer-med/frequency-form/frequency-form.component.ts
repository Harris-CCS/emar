import { Component, Output, EventEmitter, OnInit, Input, ɵisDefaultChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, bufferTime } from 'rxjs/operators';

import { Frequency } from '../../../app/interfaces/frequency';
import { FREQUENCIES } from '../../../app/mockup/frequencies';
import { ModalService } from 'src/services/modal.service';
import { Order } from 'src/app/interfaces/order';
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
    frequencies: Frequency[] = FREQUENCIES; // TODO API
    order: Order = null; // TODO come from composer
    preferredDurationUnits: string[] = DURATION_UNITS;
    minStartDateTime: string;
    startEvent = new EventEmitter<string>();
    endEvent = new EventEmitter<string>();

    constructor(private fb: FormBuilder,
        private modalService: ModalService) {}

    ngOnInit() {
        // default values
        let selectedStartDateTime: string = ''
        let selectedEndDateTime: string = '';
        let selectedFrequency: string = '';
        let selectedDuration: string = '';
        let selectedDurationUnit: string = '';
        if (this.order === null || this.order.startTime == null || this.order.startTime == '') {
            selectedStartDateTime = this.API2displayDateTime(); // now
        } else { 
            selectedStartDateTime = this.API2displayDateTime(this.order.startTime);
        }
        if (this.order === null || this.order.triageTime == null || this.order.triageTime === '') {
            this.minStartDateTime = selectedStartDateTime.replace(/ .+$/, '') + ' 00:00';  /// TODO starttime - some hours
        } else {
            this.minStartDateTime = this.API2displayDateTime(this.order.triageTime);
        }
        if (this.order !== null) {
            selectedFrequency = this.order.frequency.frequencyName;
            selectedEndDateTime = this.order.endTime;
            selectedDuration = this.order.duration.duration.toString();
            selectedDurationUnit = this.order.duration.durationUnit.unitName;
        }
        // form definition
        // TODO date format validator, end >= start validator in case manual entry
        this.frequencyForm = this.fb.group({
            'frequency': new FormControl(selectedFrequency, Validators.required),
            'duration': new FormControl(selectedDuration),
            'durationUnit': new FormControl(selectedDurationUnit),
            'startTime': new FormControl(selectedStartDateTime),
            'endTime': new FormControl(selectedEndDateTime)
        });
        this.formReady.emit(this.frequencyForm);
        // reinject modal datetime result in form
        this.startEvent.subscribe((dateTime:string) => {
            this.frequencyForm.patchValue({'startTime': dateTime});
        });
        this.endEvent.subscribe((dateTime: string) => {
            this.frequencyForm.patchValue({'endTime': dateTime});
        });
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

    API2displayDateTime(val?: string) {
        let time: Date;
        let text: string;
        if (typeof val === 'undefined' || val === null || val === '') {
            time = new Date();
            text = ('0' + (time.getMonth() + 1)).slice(-2) + '/' + ('0' + time.getDate()).slice(-2) + '/' + time.getFullYear().toString() + ' ' + ('0' + time.getHours()).slice(-2) + ':' + ('0' + time.getMinutes()).slice(-2);
        } else {
            let regexp = /(....)-(..)-(..)T(..):(..)/;
            let match = regexp.exec(val);
            text = `${match[2]}/${match[3]}/${match[1]} ${match[4]}:${match[5]}`;
        }
        return text;
    }
    onSelectTime(type: string) {
        if (type === 'start') {
            this.modalService.open('date-time-modal',
                {
                    dateTime: this.frequencyForm.controls['startTime'].value,
                    event: this.startEvent,
                    minDateTime: this.minStartDateTime
                },
                'Start Time');
        } else {
            let dateTime: string;
            if (this.frequencyForm.controls['endTime'].value == '') {
                dateTime = this.frequencyForm.controls['startTime'].value;
            } else {
                dateTime = this.frequencyForm.controls['endTime'].value;
            }
            this.modalService.open('date-time-modal',
                {
                    dateTime: dateTime,
                    event: this.endEvent,
                    minDateTime: this.frequencyForm.controls['startTime'].value
                },
                'End Time');
        }
    }
}