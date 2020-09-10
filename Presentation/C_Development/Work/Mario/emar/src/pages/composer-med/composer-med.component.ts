import { Component, OnInit, Input } from '@angular/core';
import {
  FormGroup,
  FormControl,
  Validators,
  FormBuilder,
} from '@angular/forms';
import { Observable, of, Subject, BehaviorSubject } from 'rxjs';
import { ModalService } from '../../services/modal.service';
import { MedOrderService } from '../../services/med-order.service';
import { COMPOSER_OPTIONS } from '../../app/mockup/composerOptions';
import { ComposerOptions } from '../../app/interfaces/composerOptions';
import { ComposerSchedulerService } from '../../services/composer-scheduler.service';

import { UserStoreService } from '../../services/user-store.service';
import { PatientStoreService } from '../../services/patient-store.service';
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
  @Input() composerMedForm: FormGroup;
  @Input() composerMedFormIndex: number;
  // options: ComposerOptions = COMPOSER_OPTIONS[0];
  options: ComposerOptions;
  selectedFormStrength = 0;
  isOpen: boolean = false;
  // performFormReset: BehaviorSubject<boolean> = new BehaviorSubject(false);
  title: string = '';
  isMedComponentInvalid: boolean = true;
  private userId = this.userStoreService.userId;
  private patientId = this.patientStoreService.patientId;

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private composerSchedulerService: ComposerSchedulerService
  ) {}

  ngOnInit(): void {
    console.log('composerSchedulerThis', this.composerSchedulerService);
    this.options = this.composerSchedulerService.getBrandNameOptions();
    // console.log('options', this.options);
    if (
      this.composerMedFormIndex === undefined ||
      this.composerMedFormIndex === null
    ) {
      // console.log('onInitMedComponentCreated', this.composerMedForm);
      this.composerMedForm = this.fb.group({});
      this.composerMedFormIndex = this.composerSchedulerService.registerComposerMedComponent(
        this
      );
      // console.log('onInitMedComponent', this.composerMedForm);
      // console.log('onInitMedComposerScheduler', this.composerSchedulerService);
    }
    this.composerSchedulerService.resetComponentMedFormId.subscribe(() => {
      if (
        this.composerSchedulerService.resetComponentMedFormId &&
        this.composerSchedulerService.resetComponentMedFormId.value ===
          this.composerMedFormIndex
      ) {
        this.composerMedForm.reset();
      }
    });
    // console.log('medComponentOptions', this.options);
  }

  isMedComposerFormInvalid(): boolean {
    const isInvalid: boolean = this.composerMedForm.invalid ? true : false;
    // console.log('isMedComponentValid', isInvalid, this.isMedComponentInvalid);
    if (this.isMedComponentInvalid !== isInvalid) {
      this.isMedComponentInvalid = isInvalid;
      this.composerSchedulerService.shouldCheckOverallMedOrderValidity.next(
        true
      );
    }
    return isInvalid;
  }

  isMedComposerPropertyInvalid(property: string): boolean {
    return this.composerMedForm.get(property).invalid ? true : false;
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
    // if (`${this.getMed().allergies}`) {
    //   this.modalService.open(
    //     'interaction-modal',
    //     { order: this.getMed(), type: 'allergies' },
    //     'Allergy Reaction'
    //   );
    // } else {
    //   this.medOrderService.allergiesInteractionChanged.next({});
    // }
    // this.medOrderService.allergiesInteractionChanged.subscribe((reasons) => {
    //   console.log('COMPOSER-MED subscribe allergies', reasons);
    //   if (`${this.getMed().drugs}`) {
    //     this.modalService.open(
    //       'interaction-modal',
    //       { order: this.getMed(), type: 'drugs' },
    //       'Medication Interaction'
    //     );
    //   } else {
    //     this.medOrderService.drugsInteractionChanged.next({});
    //   }
    //   this.medOrderService.drugsInteractionChanged.subscribe((reasons) => {
    //     console.log('COMPOSER-MED subscribe drugs', reasons);
    //     this.saveCartOrder();
    //   });
    // });
    this.saveCartOrder();
  };

  // saveCartOrder(closeModal: boolean = false) {
  saveCartOrder = (closeModal: boolean = false) => {
    if (this.getData().action === 'update') {
      console.log('saveCartOrder: PUT: med: ', this.getData());
      // this.medOrderService.updateCartOrder(this.getMed().med);
      this.cartStoreService.updateCartOrder(
        this.getMed(),
        this.patientId,
        this.userId,
        ''
      );
      console.log(
        `UPDATE order: ${this.getMed().id}  name: ${this.getMed().brandName}`
      );
    } else {
      // this.medOrderService.postCartOrder(this.getMed());
      console.log('saveCartOrder: POST: med: ', this.getData());
      this.cartStoreService.postCartOrder(
        this.getMed(),
        this.patientId,
        this.userId,
        ''
      );
      console.log(
        `ADD order: ${this.getMed().id}  name: ${this.getMed().brandName}`
      );
    }

    if (closeModal) {
      this.modalService.close('medComposer');
    }
    console.log('addToCart from SEARCH NEW: modal closed');
  };
}
