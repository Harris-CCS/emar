import { Component, OnInit, Input, HostListener } from '@angular/core';

import { ModalService } from '../../../services/modal.service';
import { ModalHeaderParameters } from '../../../../src/app/interfaces/modalHeaderParameters';
import { BuiltinType } from '@angular/compiler';

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
  @Input() modalHeaderParameters: ModalHeaderParameters;
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
    // console.log('ModalComponent: closebtn:', this.closebtn);
    // console.log('ModalComponent: blocking:', this.blocking);
  }

  /* close - Closes the selected modal */
  close(checkBlocking = false): void {
    this.modalService.close(this.modalId, checkBlocking);
  }

  getModalHeaderParameterValue(
    parameter: string,
    node?: string,
    field?: string
  ): any {
    if (this.modalHeaderParameters && parameter) {
      switch (parameter) {
        case 'label': {
          return this.modalHeaderParameters.label;
        }
        case 'title': {
          return this.modalHeaderParameters.title;
        }
        case 'class': {
          return this.modalHeaderParameters.class[parseInt(node, 10)];
        }
        case 'toolTip': {
          return this.modalHeaderParameters.toolTip;
        }
        case 'onTitleClick': {
          return this.modalHeaderParameters.onTitleClick();
        }
        case 'button': {
          if (node && field) {
            const button = this.modalHeaderParameters.buttons.find(
              (btn) => btn.id === node && btn.hasOwnProperty(field)
            );
            if (button && typeof button[field] === 'function') {
              return button[field]();
            } else {
              return button[field];
            }
          }
          break;
        }
        default: {
          return null;
        }
      }
    }
  }
}
