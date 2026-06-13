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
    beginingModifiedMedicineName =  "";
    followingModifiedMedicineName = "";
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
    adjustMedicinePopups() {

        let loopIndexNum: number = 0;
        let strLoopIndexNum: string;
        let medicineNameIndexNum: number = -1;
        let lowestLastLocation = 99;
        this.beginingModifiedMedicineName = this.order.medication.displayName;
        this.followingModifiedMedicineName = "";
        if (this.order.medication.displayName.length > 0) {
          let numericNotFound = true;
        while (loopIndexNum < 10 && numericNotFound) {
          strLoopIndexNum = loopIndexNum.toString();
          medicineNameIndexNum = this.order.medication.displayName.indexOf(strLoopIndexNum);
          if (medicineNameIndexNum < lowestLastLocation && medicineNameIndexNum >= 0) {
            lowestLastLocation = medicineNameIndexNum;
          }
          loopIndexNum++;
        }
         
          if (lowestLastLocation > -1) {
            this.beginingModifiedMedicineName = this.order.medication.displayName.substring(0,lowestLastLocation);
            this.followingModifiedMedicineName = this.order.medication.displayName.substring(lowestLastLocation);
            numericNotFound = false;
          }
      
      }
      }
    onSubmit() {
        this.modalService.close('five-rights');
        const routeName = this.order.medicationRoute? this.order.medicationRoute.routeName: 'Default';
        this.adjustMedicinePopups();
        let _orderDuration = " ";
        let _orderFor = " ";
        let _forDuration = " ";
        let _orderDurationUnitName = " ";
        if (this.order.duration != null &&  this.order.durationUnit.name != null )
        {
        _orderFor = "for ";
        _forDuration =  this.order.duration + " ";
        _orderDurationUnitName =  this.order.durationUnit.name;
        }

        if (this.order.duration !== null)
        {
        _orderDuration = this.order.duration + " ";
        }
        let title: string = '<span class="bigger-bolder"><b> ' + this.beginingModifiedMedicineName + '</b></span>' +
        '<span>' +this.followingModifiedMedicineName +'<br> </span>' +
        '<span class="bigger-bolder">' +
        '<b><em> Dose: </em>' + this.order.dose + ' ' + this.order.doseUnit?.unitName + ', ' +
            this.order.medicationRoute?.routeName + ', ' + this.order.frequencySchedule?.scheduleName + ' ' +
            _orderFor +  _forDuration + ' ' + _orderDurationUnitName + '</b>' +
        '</span>';
        this.modalService.open(
            'given-template-modal',
            {
              template: this.template,
              order: this.order,
              buttonText: 'Give' // TODO from template
            },
            title
          );
    }

    ngOnDestroy() {
        console.log('NGONDESTROY 5 RIGHTS');
        this.modalSubscribe.unsubscribe();
    }
}