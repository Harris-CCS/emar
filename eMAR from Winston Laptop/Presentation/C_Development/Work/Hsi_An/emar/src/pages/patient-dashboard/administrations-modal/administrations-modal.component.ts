import { Component, OnInit, Input, Output, EventEmitter, OnDestroy, ViewChild } from '@angular/core';
import { ModalService } from '../../../services/modal.service';
import { Order, OrderAdministration } from '../../../app/interfaces/order';
import { PatientMedOrderStoreService } from 'src/services/patient-med-order-store.service';

@Component({
    selector: 'administrations-modal',
    templateUrl: './administrations-modal.component.html',
    styleUrls: ['../patient-dashboard.component.scss', '../order-hover/order-hover.component.scss','./administrations-modal.component.scss']
})

export class AdministrationsModalComponent implements OnInit {
    @Input() order: Order;
    @Input() currentTime: string;
    @Input() siteUTCOffset: string; // -06:00
    @Output() modalDateTimeSelected =  new EventEmitter<string>();

    constructor(private modalService: ModalService,
        private patientMedOrderStoreService: PatientMedOrderStoreService) {
    }

    ngOnInit(): void {
    }

    onCancel() {
        this.modalDateTimeSelected.emit(null);
    }

    getOrderAdministrationStatus(administration: OrderAdministration, type: string) {
        return this.patientMedOrderStoreService.getOrderAdministrationStatus(administration, type);
    }

    hasFollowUp(administration: OrderAdministration) {
        return administration.administrationEvents.find(event => event.action.actionId == 7);
    }

    dateTimeSelectedLeave (administration: OrderAdministration) {
        let dateTime: string = administration.administrationDatetime;
        if (dateTime == null)
            dateTime = administration.administrationScheduledDatetime;
        this.modalDateTimeSelected.emit(dateTime);
    }
}