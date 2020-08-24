import { Component, OnInit, Input } from '@angular/core';

import { ModalService } from 'src/services/modal.service';
import { CartService } from 'src/services/cart.service';
import { CartStoreService } from 'src/services/cart-store.service';

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
    private cartService: CartService,
    private cartStoreService: CartStoreService,
  ) {}

  ngOnInit(): void {
    this.isDone = false
  }
  
  getData() {
    return this.modalService.retrieveModalData('deleteAllCartOrder') || {}
  }

  getPatient() {
    return this.getData().patientId || 0
  }

  cancelDelete = () => {
    this.modalService.close('deleteAllCartOrder');
  }

  confirmedDelete = () => {
    console.log('confirmedDelete for parientId: ', this.getPatient());
    this.patientId = this.getPatient();

    this.isProcessing = true;

    setTimeout(() => {
        this.cartStoreService.deleteAllCartOrders(this.getPatient(), 5555)
        //API success
        this.isDone = true;
        setTimeout( () => this.modalService.close('deleteAllCartOrder'), 2000)
      }, 1000)

    // setTimeout( () => {
    //   this.cartService.deleteAllCartOrders(this.getPatient(), 6473).subscribe(
    //     resp => console.log('DELETE RESP: ', resp),
    //     err => console.log('DELETE ERR: ', err)
    //     );
    //   //API success
    //   this.isDone = true;
    //   setTimeout( () => this.modalService.close('deleteAllCartOrder'), 2000)
    // }, 1000)
  }
}
