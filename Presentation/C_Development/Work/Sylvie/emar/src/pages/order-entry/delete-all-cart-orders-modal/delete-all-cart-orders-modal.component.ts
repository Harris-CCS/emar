import { Component, OnInit, Input } from '@angular/core';

import { ModalService } from 'src/services/modal.service';
import { MedOrderService } from 'src/services/med-order.service';

@Component({
  selector: 'app-delete-all-cart-orders-modal',
  templateUrl: './delete-all-cart-orders-modal.component.html',
  styleUrls: ['./delete-all-cart-orders-modal.component.scss']
})
export class DeleteAllCartOrdersModalComponent implements OnInit {

  @Input() modalTitle: string;
  patientId: number;
  isDone: boolean = false;
  isProcessing: boolean = false;


  constructor(
    private modalService: ModalService,
    private medOrdService: MedOrderService,
  ) {}

  ngOnInit(): void {}
  
  getData() {
    return this.modalService.retrieveModalData('deleteAllCartOrder') || {}
  }

  getPatient() {
    return this.getData().patientId || 0
  }

  cancelDeleteAllCartOrders = () => {
    this.modalService.close('deleteAllCartOrder');
  }

  deleteAllCartOrders = () => {
    console.log('deleteAllCartOrders for parientId: ', this.getPatient());
    this.patientId = this.getPatient();

    this.isProcessing = true;
    //mock
    setTimeout( () => {
      this.medOrdService.removeAllCartOrder(this.getPatient());
      //API success
      this.isDone = true;
      setTimeout( () => this.modalService.close('deleteAllCartOrder'), 2000)
    }, 1000)
  }
}
