import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Observable, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';
import { UserStoreService } from '../../../services/user-store.service';
import { PatientStoreService } from '../../../services/patient-store.service';
import { AntimicrobialIndication } from 'src/app/interfaces/antimicrobialIndication';
import { Diagnosis } from 'src/app/interfaces/diagnosis';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
import { Patient } from 'src/app/interfaces/patient';

@Component({
  selector: 'detail-form',
  templateUrl: './detail-form.component.html',
  styleUrls: ['../composer-med.component.scss'],
})
export class DetailFormComponent implements OnInit, OnDestroy {
  @Input() medComponentId: number;
  @Input() antimicrobialRequiredIndicator: boolean;
  @Output() formReady = new EventEmitter<FormGroup>();
  detailForm: FormGroup;
  // diagnoses: string[] = ['Hypertension', 'Diabetes', 'Back pain'];
  diagnoses: Array<Diagnosis>;
  // indications: string[] = ['Sepsis', 'Pneumonia'];
  indications: Array<AntimicrobialIndication> = [];
  mandatoryIndication: boolean; //TODO get from service
  selectedDiagnosis: string;
  selectedIndication: any;
  otherIndication: string = '';
  patient: Patient;
  userSiteId: number;
  subscriptionResetComponentMedFormId: Subscription;

  constructor(
    private fb: FormBuilder,
    private composerSchedulerService: ComposerSchedulerService,
    private patientStoreService: PatientStoreService,
    private userStoreService: UserStoreService,
  ) { }

  ngOnInit() {
    this.userSiteId = this.userStoreService.userSiteId;
    this.patient = this.patientStoreService.patient;
    this.diagnoses = this.patient.patientProblems;
    this.indications = this.composerSchedulerService.getSiteMedicationAntimicrobialIndications(this.userSiteId);
    this.mandatoryIndication = (this.medComponentId === 0 && this.antimicrobialRequiredIndicator) ? true : false;
    // console.log('mandatoryIndication', this.mandatoryIndication);
    if (this.mandatoryIndication) {
      this.detailForm = this.fb.group(
        {
          diagnosis: new FormControl(null),
          antimicrobialIndication: new FormControl(null, [
            // Validators.required,
            // TODO: make it not required for now
            // this.indicationValidator,
            // this.indicationValidator.bind(this),
          ]
          ),
          antimicrobialIndicationFreeText: new FormControl(null),
          antimicrobialIndicationDisplayName: new FormControl(null),
        }
      );
    } else {
      this.detailForm = this.fb.group({
        diagnosis: new FormControl(null),
        antimicrobialIndication: new FormControl(null),
        antimicrobialIndicationFreeText: new FormControl(null),
        antimicrobialIndicationDisplayName: new FormControl(null),
      });
    }
    // this.formReady.emit(this.detailForm);
    this.composerSchedulerService.addFormGroup(
      this.medComponentId,
      'detail',
      this.detailForm
    );

    this.subscriptionResetComponentMedFormId = this.composerSchedulerService.resetComponentMedFormId.subscribe(() => {
      if (
        this.composerSchedulerService.resetComponentMedFormId &&
        this.composerSchedulerService.resetComponentMedFormId.value ===
        this.medComponentId
      ) {
        this.resetDetailForm();
      }
    });
    this.setDefaultValues();
    // console.log('detailMedComponentId', this.medComponentId);
  }

  ngOnDestroy() {
    this.subscriptionResetComponentMedFormId.unsubscribe();
  }

  async setDefaultValues() {
    await this.setDetailFormDefaultValues();
  }

  async setDetailFormDefaultValues() {
    if (this.medComponentId === 0) {
      const initialComposerData: any = this.composerSchedulerService.getInitialComposerData();
      const initialOrderData: any = initialComposerData.med;
      if (initialOrderData.patientProblem && initialOrderData.patientProblem.id) {
        // const patientProblem = this.diagnoses.find(prb => prb.id === initialOrderData.patientProblemId);
        this.changeSelectedDiagnosis(initialOrderData.patientProblem);
      }
      if (initialOrderData.antimicrobialIndication && initialOrderData.antimicrobialIndication.id) {
        // const antimicrobialIndicationId = this.indications.find(ind => ind.id === initialOrderData.antimicrobialIndicationId);
        this.changeSelectedIndication(initialOrderData.antimicrobialIndication);
      }
      if (initialOrderData.antimicrobialIndicationText) {
        this.changeSelectedIndication(initialOrderData.antimicrobialIndicationText);
      }
    }
  }

  resetDetailForm(): void {
    this.selectedDiagnosis = null;
    this.selectedIndication = null;
  }

  changeSelectedDiagnosis(diagnosis: Diagnosis) {
    this.selectedDiagnosis = diagnosis ? diagnosis.problemName : null;
    this.detailForm.controls['diagnosis'].setValue(diagnosis);
    // if (diagnosis !== undefined) {
    //   this.composerSchedulerService.changeDiagnosis.next(true);
    // }

    // console.log('changeSelectedDiagnosisThis', this);
  }

  changeSelectedIndication(indication: any) {
    this.selectedIndication = indication;
    if (typeof indication === 'object') {
      this.detailForm.controls['antimicrobialIndication'].setValue(indication);
      this.detailForm.controls['antimicrobialIndicationDisplayName'].setValue(indication.description);
      this.detailForm.controls['antimicrobialIndicationFreeText'].setValue('');
    } else if (typeof indication === 'string') {
      this.detailForm.controls['antimicrobialIndication'].setValue(null);
      this.detailForm.controls['antimicrobialIndicationDisplayName'].setValue(indication);
      this.detailForm.controls['antimicrobialIndicationFreeText'].setValue(indication);
    } else {
      this.detailForm.controls['antimicrobialIndication'].setValue(null);
      this.detailForm.controls['antimicrobialIndicationDisplayName'].setValue('');
      this.detailForm.controls['antimicrobialIndicationFreeText'].setValue('');
    }

    // if (indication !== undefined) {
    //   this.composerSchedulerService.changeIndication.next(true);
    // }

    // console.log('changeSelectedIndicationThis', this);
  }

  indicationValidator(control: AbstractControl): { [key: string]: any } | null {
    if (
      !control ||
      !this ||
      !this.detailForm
    ) {
      return null;
    } else if (!control.value && this.medComponentId === 0 && !this.selectedIndication) {
      return { error: '** Indication is required' };
    }
    return null;
  }
}
