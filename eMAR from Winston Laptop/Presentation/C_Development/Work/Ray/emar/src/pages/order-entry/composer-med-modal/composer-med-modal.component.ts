import {
  Component,
  OnInit,
  Input,
  ViewChild,
  AfterViewInit,
  AfterContentChecked,
  ChangeDetectorRef,
  OnDestroy,
} from '@angular/core';
import { ModalService } from '../../../services/modal.service';
import { ThrowStmt } from '@angular/compiler';
import { ComposerMedComponent } from 'src/pages/composer-med/composer-med.component';
import { ModalHeaderParameters } from '../../../../src/app/interfaces/modalHeaderParameters';
import { MedOrderService } from 'src/services/med-order.service';
import { CartStoreService } from 'src/services/cart-store.service';
import { ComposerSchedulerService } from '../../../services/composer-scheduler.service';
import { UserStoreService } from '../../../services/user-store.service';
import { DoseOption } from '../../../app/interfaces/doseOption';
import {
  NgbAccordion,
  NgbPanelChangeEvent,
  NgbPanel,
} from '@ng-bootstrap/ng-bootstrap';
import { ModalComponent } from 'src/shared/component/modal/modal.component';
import { Subscription } from 'rxjs';

@Component({
  selector: 'composer-med-modal',
  templateUrl: './composer-med-modal.component.html',
  styleUrls: ['./composer-med-modal.component.scss'],
})
export class ComposerMedModalComponent implements OnInit, AfterViewInit, AfterContentChecked, OnDestroy {
  // @Input() modalTitle: string;
  @ViewChild('acc') accordionComponent: NgbAccordion;
  modalTitle: string = '';
  modalHeaderParameters: ModalHeaderParameters = {};
  isModalTitleParamsSet: boolean = false;
  composerMedComponents: Array<ComposerMedComponent>;
  addNewMedComponent: boolean = false;
  overallOrderValid: boolean = false;
  initialData;
  gotData: boolean = false;
  userSiteId: number = null;
  doseOptions: Array<DoseOption> = [];
  activeIds: Array<String> = ['medComponent-0'];
  subModalOpening: Subscription = null;
  subModalClosing: Subscription = null;

  constructor(
    private modalService: ModalService,
    private composerSchedulerService: ComposerSchedulerService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private cdref: ChangeDetectorRef
  ) {
    this.userSiteId = this.userStoreService.userSiteId;
    // console.log('userSiteId', this.userSiteId);
    this.continueOrder = this.continueOrder.bind(this);
    this.cancelOrder = this.cancelOrder.bind(this);
    this.checkOrder = this.checkOrder.bind(this);
    this.validOrder = this.validOrder.bind(this);
  }

  ngOnInit(): void {
    this.subModalOpening = this.modalService.modalOpening.subscribe(({ data }) => {
      // Get initial data from source (quick list, department list, group, or new order)
      this.initialData = data;
      this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
      // console.log('initialData', this.initialData);
      // console.log('initialDataBrandName', this.initialData.med.brandName);
      // console.log('dataBrandName', data);

      this.composerSchedulerService
        .getBrandNameOptionsFromAPI(this.userSiteId, encodeURIComponent(this.initialData.med.medication.displayName),null,null,null,null)
        .subscribe((response) => {
          if (
            this.initialData &&
            this.initialData.med &&
            this.initialData.med.medication.displayName
          ) {
            this.composerSchedulerService.setBrandNameOptions(response);
            const getBrandNameOptions = this.composerSchedulerService.getBrandNameOptions();
            // console.log(
            //   'Set Brand Name Options: ',
            //   this.composerSchedulerService
            // );
            // console.log('Get Brand Name Options: ', getBrandNameOptions);

            if (
              this.composerSchedulerService.getBrandNameOptions() &&
              this.composerSchedulerService.getSiteMedicationFrequencies(
                this.userStoreService.userSiteId
              ) &&
              this.composerSchedulerService.getSiteMedicationUnits(
                this.userStoreService.userSiteId
              )
            ) {
              this.gotData = true;
              // console.log('gotData', this.gotData);
            }
          }
        });

      this.composerSchedulerService
        // .getDosingOptionsFromAPI('00173044202')
        // .getDosingOptionsFromAPI(this.initialData.med.medication.drugId)
        .getDosingOptionsFromAPI(2340)
        .subscribe((response) => {
          this.composerSchedulerService.setDosingOptions(response);
          this.doseOptions = this.composerSchedulerService.getDosingOptions();
        });
    });

    // Site Frequencies
    this.composerSchedulerService
      .getSiteMedicationFrequenciesFromAPI(this.userStoreService.userSiteId)
      .subscribe((response) => {
        if (typeof this.userStoreService.userSiteId === 'number') {
          this.composerSchedulerService.setSiteMedicationFrequencies(
            this.userStoreService.userSiteId,
            response
          );

          // const getMedicationFrequencies = this.composerSchedulerService.getSiteMedicationFrequencies(
          //   this.userSiteId
          // );
          // console.log(
          //   `Set Frequencies for SiteId: ${this.userSiteId}`,
          //   this.composerSchedulerService
          // );
          // console.log(
          //   `Get Frequencies for SiteId: ${this.userSiteId}`,
          //   getMedicationFrequencies
          // );
        }
      });
    // Site Medication Units
    this.composerSchedulerService
      .getSiteMedicationUnitsFromAPI(this.userStoreService.userSiteId)
      .subscribe((response) => {
        if (typeof this.userStoreService.userSiteId === 'number') {
          this.composerSchedulerService.setSiteMedicationUnits(
            this.userStoreService.userSiteId,
            response
          );
          // const getMedicationUnits = this.composerSchedulerService.getSiteMedicationUnits(
          //   this.userSiteId
          // );
          // console.log(
          //   `Set Units for SiteId: ${this.userSiteId}`,
          //   this.composerSchedulerService
          // );
          // console.log(
          //   `Get Units for SiteId: ${this.userSiteId}`,
          //   getMedicationUnits
          // );
        }
      });
    this.subModalClosing = this.modalService.modalClosing.subscribe( (modal: ModalComponent) => {
      // if (this.modalService.modalClosed.value === 'medComposer') {
      if (modal.modalId === 'medComposer') {
        this.composerSchedulerService.resetAllComponentMedForms();
        this.resetMedModal();
      }
    });
    this.composerSchedulerService.shouldCheckOverallMedOrderValidity.subscribe(
      () => {
        if (
          this.composerSchedulerService.shouldCheckOverallMedOrderValidity.value
        ) {
          this.checkOverallOrdersValidity();
        }
      }
    );
    if (
      (!this.composerMedComponents ||
        this.composerMedComponents.length === 0) &&
      this.getMed()
    ) {
      this.composerSchedulerService.addNewComposerMedComponent();
      // console.log('componentAddAttempt', this.composerMedComponents);
    }
    this.composerSchedulerService.addNewMedComponent.subscribe(() => {
      if (this.composerSchedulerService.addNewMedComponent.value) {
        // console.log('addNewComponentMedEventHeard');
        this.addNewMedComponent = true;
      }
    });
    this.composerSchedulerService.newMedComponentAdded.subscribe(() => {
      if (this.composerSchedulerService.newMedComponentAdded.value) {
        this.addNewMedComponent = false;
        this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
        this.accordionComponent.collapseAll();
        this.activeIds = [`medComponent-${this.composerMedComponents.length - 1}`];
        // console.log('ComponentAddedMedEventHeard', this.composerMedComponents);
        this.composerSchedulerService.shouldCheckOverallMedOrderValidity.next(
          true
        );
        // console.log('ComponentAddedCheckArray');
      }
    });
    this.composerSchedulerService.changeDiagnosis.subscribe(() => {
      if (this.composerSchedulerService.changeDiagnosis.value) {
        this.setModalHeaderParameter(
          'diagnosis',
          'Diagnosis: ',
          this.composerMedComponents[0].composerMedForm.value.detail.diagnosis
        );
      }
    });
    this.composerSchedulerService.changeIndication.subscribe(() => {
      if (this.composerSchedulerService.changeIndication.value) {
        this.setModalHeaderParameter(
          'indication',
          'Indication: ',
          this.composerMedComponents[0].composerMedForm.value.detail
            .antimicrobialIndication
        );
      }
    });
  }

  ngAfterViewInit() {
    // console.log('ngAfterViewInit', this.accordionComponent);
    if (this.accordionComponent) {
      this.accordionComponent.panelChange.subscribe(
        (panelChangeEvent: NgbPanelChangeEvent) => {
          // console.log('this.accordion', this.accordionComponent);
          // console.log('panelChangeEvent', panelChangeEvent);
          // if (!panelChangeEvent.nextState && panelChangeEvent.panelId) {
          // this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
          if (panelChangeEvent.panelId) {
            // console.log(
            //   'this.composerMedComponents',
            //   this.composerMedComponents
            // );
            // console.log('panelChange');
            this.accordionComponent.panels.forEach((panel) => {
              // console.log('panel: ', panel.id, panel.isOpen);
              if (panel.isOpen) {
                const medComponentId: number = parseInt(
                  panel.id.split('-').pop(),
                  10
                );
                // this.activeIds = [`medComponent-${medComponentId}`];
                this.setMedComponentText(medComponentId);
                // console.log(
                //   'updatedMedComponent',
                //   this.composerMedComponents[medComponentId].title
                // );
                // console.log('updatedPanel: ', panel.id, panel);
              }
            });
          }
        }
      );
    }
  }

  ngAfterContentChecked() {
    // console.log('contentChecked');
    this.cdref.detectChanges();
  }

  getPanelCardClass(): string {
    return (this.composerMedComponents.length === 1) ? 'hide-panel' : '';
  }

  async checkOverallOrdersValidity() {
    await this.composerSchedulerService.checkOverallMedOrderValidity()
      .then(res => {
        this.overallOrderValid = res;
      });

  }

  // gotMedData(): boolean {
  //   console.log('this.gotData', this.gotData);
  //   this.gotData =
  //     this.gotData
  // ||
  // (this.composerSchedulerService.getBrandNameOptions() &&
  //   this.composerSchedulerService.getSiteMedicationFrequencies(
  //     this.userSiteId
  //   ) &&
  //   this.composerSchedulerService.getSiteMedicationUnits(this.userSiteId))
  //       ? true
  //       : false;

  //   return this.gotData;
  // }

  resetMedModal(): void {
    this.modalTitle = ' ';
    this.isModalTitleParamsSet = false;
    this.modalHeaderParameters = {};
    this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
    this.composerSchedulerService.addNewComposerMedComponent();
    // console.log('medModalReset');
  }

  resetComposerMedComponent(id): void {
    // alert(`resetComposerMedComponent - ${id}`);
    this.composerSchedulerService.resetComponentMedFormById(id);
  }

  getData() {
    return this.modalService.retrieveModalData('medComposer') || {};
  }

  getMed() {
    return this.getData().med || {};
  }

  getLatestMedComponentDOMId(): string {
    // this.addNewMedComponent = false;
    // console.log('addNewMedComponent', this.addNewMedComponent);
    return !this.composerMedComponents ||
      this.composerMedComponents.length === 0
      ? 'medComponent-0'
      : `medComponent-${this.composerMedComponents.length}`;
  }

  setMedComponentText(id: number): string {
    // console.log('setMedComponentText0', this.composerMedComponents, id);
    if (this.composerMedComponents && this.composerMedComponents[id]) {
      const data = this.composerMedComponents[id].composerMedForm.value;
      // console.log('setMedComponentData', this.composerMedComponents[id]);
      let textLine1: string;
      let textLine2: string;

      // Check Text Line #1
      textLine1 = `${this.getCompData(data, 'dose')} ${this.getCompData(
        data,
        'duration'
      )} ${this.getCompData(data, 'route')} ${this.getCompData(
        data,
        'priority'
      )} ${this.getCompData(data, 'frequency')}`
        //   ${this.getCompData(data, 'diagnosis')} ${this.getCompData(
        //   data,
        //   'indication'
        // )} `
        .split('  ')
        .join(' ');
      textLine1 = new RegExp(`^[\s]$`).test(textLine1) ? '' : textLine1;
      // Check Text Line #2
      textLine2 = `${this.getCompData(data, 'startTime')} ${this.getCompData(
        data,
        'endTime'
      )}`
        .split('  ')
        .join(' ');
      textLine2 = new RegExp(`^[\s]$`).test(textLine2) ? '' : textLine2;
      // console.log('textline1', textLine1);
      // console.log('textline2', textLine2);

      this.composerMedComponents[id].title =
        textLine1 || textLine2 ? `${textLine1}\n${textLine2}` : `Order #${id}`;
      return this.composerMedComponents[id].title;
    } else {
      return `Order #${id}`;
    }
  }

  getCompData(data: any, name: string): string {
    // console.log('getCompData', data);
    switch (name) {
      case 'dose': {
        if (!data.med.dose || !data.med.doseUnitName) {
          return '';
        } else {
          return `${data.med.dose} ${data.med.doseUnitName}`;
        }
      }
      case 'route': {
        if (!data.med.routeName) {
          return '';
        } else {
          return `${data.med.routeName}`;
        }
      }
      case 'priority': {
        if (!data.med.priority) {
          return '';
        } else {
          return `${data.med.priority}`;
        }
      }
      case 'diagnosis': {
        if (!data.detail.diagnosis) {
          return '';
        } else {
          return `${data.detail.diagnosis}`;
        }
      }
      case 'indication': {
        if (!data.detail.antimicrobialIndication) {
          return '';
        } else {
          return `${data.detail.antimicrobialIndication}`;
        }
      }
      case 'frequency': {
        if (!data.frequency.frequency) {
          return '';
        } else {
          return `${data.frequency.frequency}`;
        }
      }
      case 'duration': {
        if (!data.frequency.duration || !data.frequency.durationUnit) {
          return '';
        } else {
          return `[${data.frequency.duration} ${data.frequency.durationUnit}]`;
        }
      }
      case 'startTime': {
        if (!data.frequency.startTime) {
          return '';
        } else {
          return `Start On: ${data.frequency.startTime}`;
        }
      }
      case 'endTime': {
        if (!data.frequency.endTime) {
          return '';
        } else {
          return `End On: ${data.frequency.endTime}`;
        }
      }
      default: {
        return '';
      }
    }
  }

  isMedComponentPanelSelected(id: string): boolean {
    const panel = this.accordionComponent.panels.find((pn) => pn.id === id);
    // console.log('isPanelOpen', panel && panel.isOpen);
    return (panel && panel.isOpen) || false;
  }

  setModalHeaderParameter(id: string, label: string, value: string) {
    // console.log('setModalHeaderParameterStart', id, label, value);
    if (this.modalHeaderParameters && value !== undefined) {
      let fieldIndex: number;
      if (!this.modalHeaderParameters.fields) {
        this.modalHeaderParameters.fields = [];
      }
      const field = this.modalHeaderParameters.fields.find((fld, index) => {
        if (fld.id === id) {
          fieldIndex = index;
          return true;
        }
      });
      if (field) {
        this.modalHeaderParameters.fields.splice(fieldIndex, 1, {
          id,
          label,
          value,
        });
      } else {
        this.modalHeaderParameters.fields.push({
          id,
          label,
          value,
        });
      }
      // console.log(`setModalHeaderParameter ${id}`, this.modalHeaderParameters);
    }
  }

  setModalHeaderParameters(): string {
    // this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
    // console.log('composerMedComponentsSMT', this.composerMedComponents);
    if (
      !this.isModalTitleParamsSet &&
      this.getData().med &&
      this.doseOptions.length > 0
    ) {
      this.modalHeaderParameters = {
        label: 'New Order: ',
        title: this.getData().med.brandName,
        class: ['dialog-title', 'order-med-name'],
        toolTip: 'Dosing Information',
        onTitleClick: this.onTitleClick,
        popoverName: 'dosingInfo',
        popoverData: this.doseOptions,
        // popoverData: `<p>Popover Content!</p>`,
        buttons: [
          {
            id: 'quicklist',
            name: 'Add To Quick List',
            onClick: this.addOrderToQuickList,
            toolTip: 'Add Order To Quick List',
          },
          {
            id: 'continue',
            name: 'Continue',
            onClick: this.continueOrder,
            toolTip: 'Create additional order component',
          },
          {
            id: 'cancel',
            name: 'Cancel Order',
            onClick: this.cancelOrder,
            toolTip: 'Cancel Order',
          },
          {
            id: 'checkOrder',
            name: this.getActionText(),
            onClick: this.checkOrder,
            toolTip: 'Add Order to Cart',
          },
          {
            id: 'validOrder',
            name: 'Valid Order',
            onClick: this.validOrder,
            toolTip: 'Order Validity/Status',
          },
        ],
      };
      this.modalService.assignModalHeaderParameters(
        'medComposer',
        this.modalHeaderParameters
      );
      // this.modalTitle = medData.brandName;
      this.isModalTitleParamsSet = true;
      // console.log('modalHeaderParameters', this.modalHeaderParameters);
    }
    return this.modalTitle;
  }

  onTitleClick(): void {
    console.log('Dosage Table to go here when clicked!');
  }

  addOrderToQuickList(): void {
    alert('Add To Quick List!');
  }

  continueOrder(): void {
    // console.log('continueThis', this);
    this.composerSchedulerService.addNewComposerMedComponent();
    this.cdref.detectChanges();
    // console.log('componentAddAttemptFromContinueButton');
  }

  cancelOrder(): void {
    // this.composerSchedulerService.resetAllComponentMedForms();
    this.modalService.close('medComposer');
  }

  checkOrder(): void {
    // alert('Check Order!');
    this.composerMedComponents.forEach((medComponent, index) => {
      const closeModal: boolean =
        this.composerMedComponents.length - 1 === 0 ||
        index === this.composerMedComponents.length - 1;
      medComponent.saveCartOrder(closeModal);
      // console.log('processCartOrder');
    });
  }

  validOrder(): string {
    // alert('Valid Order!');
    // console.log('overallOrderValid', this.overallOrderValid);
    return this.overallOrderValid ? 'VALID' : 'INVALID';
  }

  isMedComposerFormInvalid(id: number) {
    // console.log('isMedComposerFormInvalid', id, this.composerMedComponents);
    return this.composerMedComponents[id].isMedComposerFormInvalid();
  }

  getMedComponentValidityText(id: number): string {
    return this.isMedComposerFormInvalid(id) ? 'INVALID' : 'VALID';
  }

  removeMedComponent(id: number): void {
    // console.log('removeComposerMedComponentsStart', this.composerMedComponents);
    this.composerSchedulerService.removeMedComponent(id);
    this.composerMedComponents = this.composerSchedulerService.getComposerMedComponents();
    if (this.composerMedComponents.length === 1) {
      this.activeIds = [`medComponent-${this.composerMedComponents.length - 1}`];
    }
    this.composerSchedulerService.shouldCheckOverallMedOrderValidity.next(true);
    // console.log('removeComposerMedComponentsDone', this.composerMedComponents);
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
      console.log('saveCartOrder: PUT: med: ', this.getData());
      // this.medOrderService.updateCartOrder(this.getMed().med);
      this.cartStoreService.updateCartOrder(this.getMed(), 1, 5555, '');
      console.log(
        `UPDATE order: ${this.getMed().id}  name: ${this.getMed().brandName}`
      );
    } else {
      // this.medOrderService.postCartOrder(this.getMed());
      console.log('saveCartOrder: POST: med: ', this.getData());
      this.cartStoreService.postCartOrder(this.getMed(), 1, 5555, '');
      console.log(
        `ADD order: ${this.getMed().id}  name: ${this.getMed().brandName}`
      );
    }

    this.modalService.close('medComposer');
    console.log('addToCart from SEARCH NEW: modal closed');
  };

  handleToggle(event: Event) {
    // console.log('handleToggleEvent', event);
    // alert('handle toggle!');
  }

  ngOnDestroy(): void {
    if (this.subModalOpening !== null) this.subModalOpening.unsubscribe();
    if (this.subModalClosing !== null) this.subModalClosing.unsubscribe();
  }
}
