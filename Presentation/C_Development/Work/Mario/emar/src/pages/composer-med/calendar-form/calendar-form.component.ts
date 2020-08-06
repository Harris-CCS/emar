import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { formatCurrency } from '@angular/common';
import { ModalService } from 'src/services/modal.service';

interface Day {
    name: string;
    day: string;
    events: string[];
}

const WEEKS = [
    [
        {name: 'Su', day: '26', month: '4', events: []},
        {name: 'Mo', day: '27', month: '4', events: ['09:00', '18:00']},
        {name: 'Tu', day: '28', month: '4', events: ['09:00', '18:00']},
        {name: 'We', day: '29', month: '4', events: ['09:00', '18:00']},
        {name: 'Th', day: '30', month: '4', events: ['09:00', '18:00']},
        {name: 'Fr', day: '1', month: '5', events: ['09:00', '18:00']},
        {name: 'Sa', day: '2', month: '5', events: ['09:00', '18:00']}
    ],
    [
        {name: 'Su', day: '3', month: '5', events: ['09:00', '18:00']},
        {name: 'Mo', day: '4', month: '5', events: []},
        {name: 'Tu', day: '5', month: '5', events: []},
        {name: 'We', day: '6', month: '5', events: []},
        {name: 'Th', day: '7', month: '5', events: []},
        {name: 'Fr', day: '8', month: '5', events: []},
        {name: 'Sa', day: '9', month: '5', events: []}
    ]
]

@Component({
    selector: 'calendar-form',
    templateUrl: './calendar-form.component.html',
    styleUrls: ['./calendar-form.component.scss', '../composer-med.component.scss']
})
export class CalendarFormComponent implements OnInit {
    weeks: Day[][];

    constructor(private modalService: ModalService) {
    }
    ngOnInit() {
        this.weeks = WEEKS;
    }

    onSelectTime(title: string) {
        this.modalService.open('date-time-modal',{}, title);
    }
}