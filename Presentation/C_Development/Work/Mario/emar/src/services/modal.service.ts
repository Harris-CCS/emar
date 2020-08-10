import { Injectable, EventEmitter } from '@angular/core';

import { ModalComponent } from '../shared/component/modal/modal.component';

@Injectable({
  providedIn: 'root'
})

export class ModalService {
  private modals: Array<ModalComponent>;
  modalOpening = new EventEmitter<any>();

  constructor() {
    this.modals = [];
  }

  
  /* close - Closes the selected modal by searching for the component and setting isOpen to false */
  close(modalId: string, checkBlocking = false): void {
    let modal = this.findModal(modalId);

    if (modal) {
      if (checkBlocking && modal.blocking) {
        return;
      }
      setTimeout(() => {
        modal.isOpen = false;
      }, 100);
    }
  }

  /* findModal - Locates the specified modal in the modals array */
  findModal(modalId: string): ModalComponent {
    for (let modal of this.modals) {

      if (modal.modalId === modalId) {
        return modal;
      }
    }

    return null;
  }

  /* open - Opens the specified modal based on the suplied modal id */
  open(modalId: string, data?: any, title?: string): void {
    let modal = this.findModal(modalId);

    console.log('ModalService: open: data: ', title, data)
    if (modal) {
      setTimeout(() => {
        modal.data = data;
        modal.modalTitle = title || ' ';
        modal.isOpen = true;
        this.modalOpening.emit(data);
      }, 100);
    }
  }

  /* registerModal - Registers all modal components being used on initialization */
  registerModal(newModal: ModalComponent): void {
    let modal = this.findModal(newModal.modalId);

    // Delete existing to replace the modal
    if (modal) {
      this.modals.splice(this.modals.indexOf(modal), 1);
    }

    this.modals.push(newModal);
  }

  /* retrieveModalData - Retrieve the data object on initialization */
  retrieveModalData(modalId: string): any {
    let modal = this.findModal(modalId);

    if (modal) {
      // console.log('ModalService: retrieveModalData: ', modal.data)
      return modal.data;
    }

    return null;
  }
}