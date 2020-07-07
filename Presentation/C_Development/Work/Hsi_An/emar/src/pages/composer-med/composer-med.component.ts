import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ModalService } from '../../services/modal.service';
import { MedOrderService } from '../../services/med-order.service';

@Component({
  selector: 'composer-med',
  templateUrl: './composer-med.component.html',
  styleUrls: ['./composer-med.component.scss']
})
export class ComposerMedComponent implements OnInit {

  constructor(
    private modalService: ModalService,
    private medOrderService: MedOrderService,
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
