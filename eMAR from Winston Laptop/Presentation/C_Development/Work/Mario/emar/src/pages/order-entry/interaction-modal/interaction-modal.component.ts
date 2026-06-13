import { Component, OnInit, Input, Output, ViewChild, EventEmitter, Injectable} from '@angular/core';
import { FormGroup, FormControl, FormArray, FormBuilder, Validators, ValidatorFn } from '@angular/forms';

import { Order } from '../../../app/interfaces/order';
import { ModalService } from 'src/services/modal.service';
import { ThrowStmt } from '@angular/compiler';
import { MedOrderService } from 'src/services/med-order.service';


interface Reason {
  type: string,
  label: string,
  id: string
}

@Component({
  selector: 'interaction-modal',
  templateUrl: './interaction-modal.component.html',
  styleUrls: ['./interaction-modal.component.scss','../../composer-med/composer-med.component.scss']
})

export class InteractionModalComponent implements OnInit {
  interactionForm: FormGroup;
  reasons: Reason[] = [];
  mandatoryReason: boolean = true;

  constructor(private modalService: ModalService,
    private formBuilder: FormBuilder,
    private medOrderService: MedOrderService
    ) { }

  ngOnInit(): void {
    // TODO call API - move  from order-entry to app to call only once
    this.reasons = [];
    this.reasons.push({type: 'drugs', label: 'Patient "being" monitored', id: "D1"});
    this.reasons.push({type: 'drugs', label: "Patient agrees", id: "D2"});
    this.reasons.push({type: 'drugs', label: "Patient tolerated same medication", id: "D3"});
    this.reasons.push({type: 'allergies', label: "Patient tolerates this medication", id: "A1"});
    this.reasons.push({type: 'allergies', label: 'Allergy is not clinically significant', id:'A2'})
    this.mandatoryReason = true;

    let controls: FormControl[] = [];
    for (let reason of this.reasons) {
      controls.push(new FormControl(null));
    }
    this.interactionForm = new FormGroup({
      'rationale': new FormArray(controls),
      'otherReason': new FormControl(null)
    }, { validators: this.atLeastOneValidator.bind(this) } )
  }

  getData() {
    return this.modalService.retrieveModalData('interaction-modal') || {};
  }

  getOrder() {
    return this.getData().order || {};
  }

  getTypeInteraction() {
    return this.getData().type || "";
  }

  atLeastOneValidator(group: FormGroup): {[s:string]: boolean} {
    if (typeof this.interactionForm === 'undefined') {
      return null; //TODO why do I have to add this test
    }
    if (this.atLeastOne()) {
      return null;
    }
    return {'atLeastOne': true};
  }

  atLeastOne() {
    if (this.interactionForm.controls['otherReason'].value !== null && this.interactionForm.controls['otherReason'].value.trim() !== '') return true;
    if (this.interactionForm.controls['rationale'].value === null) return false;
    for (const r of this.interactionForm.controls['rationale'].value) {
      if (r === true) return true;
    }
    return false;
  }

  onSubmit() {
    // console.log('INTERACTION-MODAL submit', this.interactionForm.value);
    this.modalService.close('interaction-modal');
    this.interactionForm.reset();
    if (this.getTypeInteraction() == 'allergies') {
      this.medOrderService.allergiesInteractionChanged.next(this.interactionForm.value);
    } else {
      this.medOrderService.drugsInteractionChanged.next(this.interactionForm.value);
    }
  }
}
