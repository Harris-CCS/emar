import { Component, Output, EventEmitter, OnInit, Input } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';

import { Frequency } from '../../../app/interfaces/frequency';
import { FREQUENCIES } from '../../../app/mockup/frequencies';
const DURATION_UNITS: string[] = ["Doses", "Hours", "Days"]


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

    constructor(private fb: FormBuilder) {}

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
            map(term => term.length < 1 ? []
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
}