import { Injectable, EventEmitter } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';

import { ModalComponent } from '../shared/component/modal/modal.component';

@Injectable({
  providedIn: 'root',
})
export class ModalService {
  private modals: Array<ModalComponent>;
  // eventEmitter should be used with @output so not in a service
  // modalOpening = new EventEmitter<any>();
  // modalClosing = new EventEmitter<any>();
  modalOpening = new Subject<ModalComponent>();
  modalClosing = new Subject<ModalComponent>();
  // modalClosed: BehaviorSubject<string> = new BehaviorSubject('');

  constructor() {
    this.modals = [];
  }

  /* close - Closes the selected modal by searching for the component and setting isOpen to false */
  close(modalId: string, checkBlocking = false, incognito: boolean = false): void {
    // console.log('closeModal', modalId);
    let modal = this.findModal(modalId);

    if (modal) {
      if (checkBlocking && modal.blocking) {
        return;
      }
      setTimeout(() => {
        modal.isOpen = false;
      }, 100);
      if (modal.closebtn) {
        if (!incognito) {
          this.modalClosing.next(modal);
        }
        modal.modalTitle = ' ';
        modal.data = {};
      }
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

    // console.log('ModalService: open: data: ', title, data);
    if (modal) {
      setTimeout(() => {
        modal.data = data;
        modal.modalTitle = title || ' ';
        modal.isOpen = true;
        this.modalOpening.next(modal);
      }, 100);
      // console.log('modalStored', this.modals);
    }
    // console.log('modalList', this.modals);
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

  assignModalHeaderParameters(modalId: string, parameters: any): void {
    const modal = this.findModal(modalId);
    if (modal && parameters) {
      modal.modalTitle = parameters.name;
      modal.modalHeaderParameters = parameters;
      // console.log('modalHeaderParameters', modal);
    }
  }

  modalIsOpen(modalId: string) {
    const modal = this.findModal(modalId);
    return modal ? modal.isOpen : false;
  }
}
