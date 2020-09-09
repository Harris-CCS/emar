import { Injectable } from '@angular/core';
import {
  Form,
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Observable, of, Subject, BehaviorSubject } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  tap,
  switchMap,
} from 'rxjs/operators';
import { ComposerMedComponent } from '../pages/composer-med/composer-med.component';
import { ModalService } from '../services/modal.service';
import { MedOrderService } from '../services/med-order.service';
import { CartStoreService } from '../services/cart-store.service';

@Injectable({
  providedIn: 'root',
})
export class ComposerSchedulerService {
  private composerMedComponents: Array<ComposerMedComponent>;
  // composerMedForm: FormGroup;
  // performFormReset: BehaviorSubject<boolean> = new BehaviorSubject(false);
  resetComponentMedFormId: BehaviorSubject<number> = new BehaviorSubject(-1);
  resetAllComponentMedFormIds: BehaviorSubject<boolean> = new BehaviorSubject(
    false
  );
  addNewMedComponent: BehaviorSubject<boolean> = new BehaviorSubject(false);
  newMedComponentAdded: BehaviorSubject<boolean> = new BehaviorSubject(false);
  changeIndication: BehaviorSubject<boolean> = new BehaviorSubject(false);
  changeDiagnosis: BehaviorSubject<boolean> = new BehaviorSubject(false);
  shouldCheckOverallMedOrderValidity: BehaviorSubject<
    boolean
  > = new BehaviorSubject(false);

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private medOrderService: MedOrderService,
    private cartStoreService: CartStoreService
  ) {
    this.composerMedComponents = [];
    // this.composerMedForm = this.fb.group({});
  }

  addNewComposerMedComponent(): void {
    this.addNewMedComponent.next(true);
  }

  registerComposerMedComponent(newMedComponent: ComposerMedComponent): number {
    // let modal = this.findModal(newModal.modalId);

    // if (modal) {
    //   this.modals.splice(this.modals.indexOf(modal), 1);
    // }
    this.composerMedComponents.push(newMedComponent);
    this.newMedComponentAdded.next(true);
    return this.composerMedComponents.length - 1;
  }

  getComposerMedComponents() {
    return !this.composerMedComponents ? [] : this.composerMedComponents;
  }

  // addFormGroup(name: string, form: FormGroup) {
  addFormGroup(id: number, name: string, form: FormGroup) {
    // console.log('addFormGroupParams', id, name, form);
    // if (this.composerMedComponents[id]) {
    this.composerMedComponents[id].composerMedForm.setControl(name, form);
    // console.log('addFormGroup', id, name, form);
    /// console.log('composerMedComponentsThis', this);
    // }
  }

  resetAllComponentMedForms(): void {
    // this.composerMedComponents[id].composerMedForm.reset();
    // this.composerMedComponents[id].performFormReset.next(true);
    this.composerMedComponents.forEach((medComponent, index) => {
      this.resetComponentMedFormById(index);
    });
    this.composerMedComponents = [];
  }

  resetComponentMedFormById(index: number) {
    this.resetComponentMedFormId.next(index);
  }

  removeMedComponent(id: number): void {
    this.composerMedComponents.splice(id, 1);
    // console.log('removeMedComponent', this.composerMedComponents);
  }

  checkOverallMedOrderValidity(): boolean {
    const invalidMedComponent = this.composerMedComponents.find(
      (medComponent) => medComponent.isMedComposerFormInvalid()
    );
    // console.log('checkOverallValidity', invalidMedComponent);
    return invalidMedComponent ? false : true;
  }
}
