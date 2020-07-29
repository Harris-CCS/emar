import { Component, Output, EventEmitter, OnInit, Input } from '@angular/core';
import { FormBuilder, FormGroup, FormControl } from '@angular/forms';

import { Frequency } from '../../../app/interfaces/frequency';
import { FormStrength } from '../../../app/interfaces/formStrength';
import { FREQUENCIES } from '../../../app/mockup/frequencies';

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

    constructor(private fb: FormBuilder) {}

    ngOnInit() {
        this.frequencyForm = this.fb.group({
            'frequency': new FormControl(null)
        });
        this.formReady.emit(this.frequencyForm);
    }

    changeSelectedFrequency(frequency: Frequency) {
        this.selectedFrequency = frequency;
        this.frequencyForm.controls['frequency'].setValue(frequency.frequencyName);  // TODO id?
    }
}