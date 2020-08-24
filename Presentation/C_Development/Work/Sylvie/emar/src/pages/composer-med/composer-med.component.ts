import { Component, OnInit, Input } from '@angular/core';
import {
  FormGroup,
  FormControl,
  Validators,
  FormBuilder,
} from '@angular/forms';

import { ModalService } from '../../services/modal.service';
import { MedOrderService } from '../../services/med-order.service';
import { COMPOSER_OPTIONS } from '../../app/mockup/composerOptions';
import { ComposerOptions } from '../../app/interfaces/composerOptions';
import { ComposerSchedulerService } from '../../services/composer-scheduler.service';

import { CartStoreService } from '../../services/cart-store.service';

@Component({
  selector: 'composer-med',
  templateUrl: './composer-med.component.html',
  styleUrls: ['./composer-med.component.scss'],
})

// Inspiration: https://itnext.io/partial-reactive-form-with-angular-components-443ca06d8419
export class ComposerMedComponent implements OnInit {
  @Input() resetForm: boolean;
  // composerMedForm: FormGroup;
  options: ComposerOptions = COMPOSER_OPTIONS[0]; // TODO API call
  selectedFormStrength = 0;

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService,
    private composerSchedulerService: ComposerSchedulerService
  ) {}

  ngOnInit(): void {
    // this.composerMedForm = this.fb.group({
    //   orderNotes: null, // this is here for test
    // });
  }

  // formInitialized(name: string, form: FormGroup) {
  //   this.composerMedForm.setControl(name, form);
  // }

  isMedComposerFormInvalid(): boolean {
    return this.composerSchedulerService.composerMedForm.invalid ? true : false;
  }

  isMedComposerPropertyInvalid(property: string): boolean {
    return this.composerSchedulerService.composerMedForm.get(property).invalid
      ? true
      : false;
  }

  resetMedComposerForm() {
    this.modalService.close('medComposer');
  }

  getData() {
    return this.modalService.retrieveModalData('medComposer') || {};
  }

  getMed() {
    return this.getData().med || {};
  }

  getActionText() {
    let action = this.getData().action || 'add';
    return action === 'update' ? 'Update Order' : 'Add Order';
  }

  processCartOrder = () => {
    if (`${this.getMed().allergies}`) {
      this.modalService.open(
        'interaction-modal',
        { order: this.getMed(), type: 'allergies' },
        'Allergy Reaction'
      );
    } else {
      this.medOrderService.allergiesInteractionChanged.next({});
    }
    this.medOrderService.allergiesInteractionChanged.subscribe((reasons) => {
      console.log('COMPOSER-MED subscribe allergies', reasons);
      if (`${this.getMed().drugs}`) {
        this.modalService.open(
          'interaction-modal',
          { order: this.getMed(), type: 'drugs' },
          'Medication Interaction'
        );
      } else {
        this.medOrderService.drugsInteractionChanged.next({});
      }
      this.medOrderService.drugsInteractionChanged.subscribe((reasons) => {
        console.log('COMPOSER-MED subscribe drugs', reasons);
        this.saveCartOrder();
      });
    });
    // this.saveCartOrder()
  };

  saveCartOrder = () => {
    if (this.getData().action === 'update') {
      console.log('saveCartOrder: PUT: med: ', this.getData())
      // this.medOrderService.updateCartOrder(this.getMed().med);
      this.cartStoreService.updateCartOrder(this.getMed(), 1, 5555, '')
      console.log(
        `UPDATE order: ${this.getMed().id}  name: ${this.getMed().brandName}`
      );
    } else {
      // this.medOrderService.postCartOrder(this.getMed());
      console.log('saveCartOrder: POST: med: ', this.getData())
      this.cartStoreService.postCartOrder(this.getMed(), 1, 5555, '')
      console.log(
        `ADD order: ${this.getMed().id}  name: ${this.getMed().brandName}`
      );
    }

    this.modalService.close('medComposer');
    console.log('addToCart from SEARCH NEW: modal closed');
  };
}
