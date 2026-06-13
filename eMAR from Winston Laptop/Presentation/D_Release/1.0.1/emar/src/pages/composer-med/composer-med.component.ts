import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import {
  FormGroup,
  FormControl,
  Validators,
  FormBuilder,
} from '@angular/forms';
import { Observable, of, Subject, BehaviorSubject, Subscription } from 'rxjs';
import { ModalService } from '../../services/modal.service';
import { MedOrderService } from '../../services/med-order.service';
import { COMPOSER_OPTIONS } from '../../app/mockup/composerOptions';
import { ComposerOptions } from '../../app/interfaces/composerOptions';
import { ComposerSchedulerService } from '../../services/composer-scheduler.service';

import { UserStoreService } from '../../services/user-store.service';
import { PatientStoreService } from '../../services/patient-store.service';
import { CartStoreService } from '../../services/cart-store.service';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';
import { NgbCalendar } from '@ng-bootstrap/ng-bootstrap';
import { ScheduledAdministration } from 'src/app/interfaces/scheduled-administration';

@Component({
  selector: 'composer-med',
  templateUrl: './composer-med.component.html',
  styleUrls: ['./composer-med.component.scss'],
})

// Inspiration: https://itnext.io/partial-reactive-form-with-angular-components-443ca06d8419
export class ComposerMedComponent implements OnInit, OnDestroy {
  @Input() resetForm: boolean;
  // composerMedForm: FormGroup;
  @Input() composerMedForm: FormGroup;
  @Input() composerMedFormIndex: number;
  // options: ComposerOptions = COMPOSER_OPTIONS[0];
  options: ComposerOptions;
  selectedFormStrengthId: number = 0;
  isOpen: boolean = false;
  // performFormReset: BehaviorSubject<boolean> = new BehaviorSubject(false);
  title: string = '';
  isMedComponentInvalid: boolean = true;
  private userId: number;
  private userSiteId: number;
  private patientId = this.patientStoreService.patientId;
  modalId: string;
  addOrderToQuickListResultText: string;
  subscriptionResetComponentMedFormId: Subscription;
  subscriptionNewFormStrengthSelected: Subscription;

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private composerSchedulerService: ComposerSchedulerService,
    private ngbCalendar: NgbCalendar,
  ) {
    this.userId = this.userStoreService.userId;
    this.userSiteId = this.userStoreService.userSiteId;
  }

  ngOnInit(): void {
    // console.log('composerSchedulerThis', this.composerSchedulerService);
    this.options = this.composerSchedulerService.getBrandNameOptions();
    // this.options.antimicrobialIndicationRequired = true;
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
    this.subscriptionResetComponentMedFormId = this.composerSchedulerService.resetComponentMedFormId.subscribe(() => {
      if (
        this.composerSchedulerService.resetComponentMedFormId &&
        this.composerSchedulerService.resetComponentMedFormId.value ===
        this.composerMedFormIndex
      ) {
        this.composerMedForm.reset();
      }
    });

    this.subscriptionNewFormStrengthSelected = this.composerSchedulerService.newFormStrengthSelected.subscribe(() => {
      if (this.composerSchedulerService.newFormStrengthSelected.value !== -1 && this.options && this.options.availableFormStrength) {
        const newFormStrengthMedicationId = this.composerSchedulerService.newFormStrengthSelected.value;
        const composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
        let newFormStrengthId: number = null;
        const selectedFormStrength = composerMedComponents[0].options.availableFormStrength.find(
          (fs, index) => {
            if (fs.medicationId === newFormStrengthMedicationId) {
              newFormStrengthId = index;
              return fs;
            }
          }
        );
        this.selectedFormStrengthId = newFormStrengthId;

        // alert(`ComposerMedComponentNewFormStrengthId: ${this.selectedFormStrengthId}`);
      }
      // console.log('newFormStrengthSelected');
    });

    // console.log('medComponentOptions', this.options);

    this.modalId = `date-time-modal-composer-order-panel-${this.composerMedFormIndex}`;
  }

  // ngOnChanges(changes: SimpleChanges) {
  //   console.log('OnChanges');
  //   this.composerMedForm.markAsTouched();
  // }

  ngOnDestroy() {
    this.subscriptionResetComponentMedFormId.unsubscribe();
    this.subscriptionNewFormStrengthSelected.unsubscribe();
  }

  isMedComposerFormInvalid(): boolean {
    // const isInvalid: boolean = (this.composerMedForm.status && (this.composerMedForm.status === 'INVALID' || this.composerMedForm.invalid)) ? true : false;
    const isInvalid: boolean = (this.composerMedForm.status &&
      this.composerMedForm.status === 'VALID' &&
      Object.keys(this.composerMedForm.value).length > 0) ? false : true;
    // console.log('isMedComponentValid', Object.keys(this.composerMedForm.value).length, isInvalid, this.isMedComponentInvalid, this.composerMedForm);
    // console.log('composerMedFormValidity', this.composerMedForm);
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

  invalidOrder(): boolean {
    return this.composerSchedulerService.isMedComposerFormInvalid(this.composerMedFormIndex);
  }

  singleOrderOnly(): boolean {
    return this.composerSchedulerService.getComposerMedComponents().length === 1 ? true : false;
  }

  async addOrderToQuickList(): Promise<string> {
    this.addOrderToQuickListResultText = '...'
    const result = await this.composerSchedulerService.saveOrderToUserQuickList(this.composerMedFormIndex, this.userId, this.userSiteId);
    this.addOrderToQuickListResultText = result ? 'Order Added to User Quick List Successfully' : 'Unable to add order to User Quick List';
    return this.addOrderToQuickListResultText;
  }

  resetMedComposerForm() {
    this.modalService.close('medComposer');
  }

  getScheduledAdministrations(): Array<ScheduledAdministration> {
    return this.composerMedForm.value ? this.composerMedForm.value.frequency.scheduledAdministrations : [];

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
