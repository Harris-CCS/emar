import { Component, OnInit, Input, HostListener } from '@angular/core';

import { ModalService } from '../../../services/modal.service';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss'],
})
export class ModalComponent implements OnInit {
  isOpen: boolean = false;

  @Input() closebtn: boolean;
  @Input() modalId: string;
  @Input() modalTitle: string = '';
  @Input() blocking: boolean;
  @Input() data: any;
  @Input() action: string;
  @HostListener('document:keyup', ['$event'])

  /* keyup - Checks keys entered for the 'esc' key, attached to hostlistener */
  keyup(event: KeyboardEvent): void {
    if (event.keyCode === 27) {
      this.modalService.close(this.modalId, true);
    }
  }

  constructor(private modalService: ModalService) {}

  /* ngOnInit - Initiated when component loads */
  ngOnInit() {
    this.modalService.registerModal(this);
    console.log('ModalComponent: closebtn:', this.closebtn);
    console.log('ModalComponent: blocking:', this.blocking);
  }

  /* close - Closes the selected modal */
  close(checkBlocking = false): void {
    this.modalService.close(this.modalId, checkBlocking);
  }
}
