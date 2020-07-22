import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ModalService } from '../../services/modal.service';
import { MedOrderService } from '../../services/med-order.service';
import { InteractionModalComponent } from '../order-entry/interaction-modal/interaction-modal.component';

@Component({
  selector: 'composer-med',
  templateUrl: './composer-med.component.html',
  styleUrls: ['./composer-med.component.scss']
})
export class ComposerMedComponent implements OnInit {

  constructor(
    private modalService: ModalService,
    private medOrderService: MedOrderService
  ) {}

  ngOnInit(): void {
    
  }

  getData() {
    return this.modalService.retrieveModalData('medComposer') || {}
  }

  getMed() {
    return this.getData().med || {}
  }

  getActionText() {
    let action = this.getData().action || 'add'
    return action === 'update' ? 'Update Order' : 'Add Order'
  }

  processCartOrder = () => {
    if (`${this.getMed().allergies}`) {
      this.modalService.open('interaction-modal', {order: this.getMed(), type: 'allergies'}, 'Allergy Reaction');
    } else {
      this.medOrderService.allergiesInteractionChanged.next({});
    }
    this.medOrderService.allergiesInteractionChanged.subscribe( (reasons) => {
      console.log("COMPOSER-MED subscribe allergies", reasons);
      if (`${this.getMed().drugs}`) {
        this.modalService.open('interaction-modal', {order: this.getMed(), type: 'drugs'}, 'Medication Interaction' );
      } else {
        this.medOrderService.drugsInteractionChanged.next({});
      }
      this.medOrderService.drugsInteractionChanged.subscribe( (reasons) => {
        console.log("COMPOSER-MED subscribe drugs", reasons);
        this.saveCartOrder();
      });
    });
  }
  saveCartOrder = () => {
    if (this.getData().action === 'update') {
      this.medOrderService.updateCartOrder(this.getMed());
      console.log(`UPDATE order: ${this.getMed().id}  name: ${this.getMed().name}`);
    } else {
      this.medOrderService.postCartOrder(this.getMed());
      console.log(`Add order: ${this.getMed().id}  name: ${this.getMed().name}`);
    }
    
    this.modalService.close('medComposer');
    console.log('addToCart from SEARCH NEW: modal closed')
  }
}
