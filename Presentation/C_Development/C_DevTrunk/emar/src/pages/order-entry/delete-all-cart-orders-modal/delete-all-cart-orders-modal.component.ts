import { Component, OnInit, Input } from '@angular/core';

import { ModalService } from 'src/services/modal.service';
import { CartService } from 'src/services/cart.service';
import { CartStoreService } from 'src/services/cart-store.service';
import { UserStoreService } from 'src/services/user-store.service';

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
  isSuccess: boolean = false
  hasError: boolean = false
  errorMessage: string

  constructor(
    private modalService: ModalService,
    private cartService: CartService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
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

  confirmedDelete = async () => {
    console.log('confirmedDelete for parientId: ', this.getPatient());
    this.patientId = this.getPatient();

    this.isProcessing = true;

    try {
      await this.cartStoreService.deleteAllCartOrders(this.getPatient(), this.userStoreService.userId)

      this.isDone = true;
      this.isSuccess = true;
      
      setTimeout( () => this.modalService.close('deleteAllCartOrder'), 2000)

    } catch (err) {
      this.isDone = true;
      this.hasError = true;
      this.errorMessage = `${err.status} ${err.statusText} ${err.error}`
      
      setTimeout( () => this.modalService.close('deleteAllCartOrder'), 2000)
    }

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
