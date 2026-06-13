import { Component, OnDestroy, OnInit } from '@angular/core';

import { ModalService } from '../../../services/modal.service';
import { Order } from '../../../app/interfaces/order';
import { User } from '../../../app/interfaces/user';
import { GivenTemplate } from '../../../app/interfaces/given-template';
import { Subscription } from 'rxjs';

@Component({
    selector: 'five-rights-modal',
    templateUrl: './five-rights-modal.component.html',
    styleUrls: ['./five-rights-modal.component.scss']
})

export class FiveRightsComponent implements OnInit, OnDestroy {
    patient: User;
    order: Order;
    template: GivenTemplate;
    modalSubscribe: Subscription;
    
    constructor(private modalService: ModalService) {
    }

    ngOnInit(): void {
        this.modalSubscribe = this.modalService.modalOpening.subscribe( modal => {
            console.log('SUBSCRIBE 5 RIGHTS');
            this.patient = modal.data.patient;
            this.order = modal.data.order;
            this.template = modal.data.template;
        });
    }

    onCancel() {
        this.modalService.close('five-rights');
    }

    onSubmit() {
        this.modalService.close('five-rights');
        const routeName = this.order.medicationRoute? this.order.medicationRoute.routeName: 'Default';
        this.modalService.open(
            'given-template-modal',
            {
              template: this.template,
              order: this.order,
              buttonText: 'Give' // TODO from template
            },
            '<span class="bigger-bolder-blue">' + this.order.medication.displayName + '</span> - <span class="bigger-bolder">'+ routeName + '</span>'
          );
    }

    ngOnDestroy() {
        console.log('NGONDESTROY 5 RIGHTS');
        this.modalSubscribe.unsubscribe();
    }
}