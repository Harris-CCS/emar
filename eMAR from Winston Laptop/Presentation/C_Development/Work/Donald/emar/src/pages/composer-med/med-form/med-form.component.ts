import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  ViewChild,
  ɵConsole,
  OnDestroy
} from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Observable, Subject, merge, Subscription } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  filter,
  map,
} from 'rxjs/operators';
import { ComposerSchedulerService } from '../../../services/composer-scheduler.service';
import { UserStoreService } from '../../../services/user-store.service';
import { ComposerOptions } from '../../../app/interfaces/composerOptions';
import { FormStrength } from '../../../app/interfaces/formStrength';
import { AdministrationInstructions } from '../../../app/interfaces/administrationInstructions';
import { AdministrationInstructionsGroups } from '../../../app/interfaces/administrationInstructionsGroups';
import { NgbTypeahead } from '@ng-bootstrap/ng-bootstrap';
import { Dose } from '../../../app/interfaces/dose';
import { Unit } from '../../../app/interfaces/unit';
import { MedicationUnit } from '../../../app/interfaces/medication-unit';
import { Route } from '../../../app/interfaces/route';
// import { UNITS } from '../../../app/mockup/doseUnits';

@Component({
  selector: 'med-form',
  templateUrl: './med-form.component.html',
  styleUrls: ['./med-form.component.scss'],
})
export class MedFormComponent implements OnInit, OnDestroy {
  @Input() medOptions: ComposerOptions;
  @Input() initLowestMedStrengthData: boolean = true;
  @Input() medComponentId: number;

  @ViewChild('duInstance', { static: true }) duInstance: NgbTypeahead;
  focus$ = new Subject<string>();
  click$ = new Subject<string>();
  @ViewChild('routeInstance', { static: true }) routeInstance: NgbTypeahead;
  focusRoute$ = new Subject<string>();
  clickRoute$ = new Subject<string>();

  medForm: FormGroup;
  // Temp Data for strengths, doses, doseUnits, routes, and priorities - Get these from API service
  // doseUnits: Unit[] = UNITS;
  doseUnits: MedicationUnit[];
  priorities = ['STAT', 'Routine'];
  routeOfAdministrationUnits: Array<Route>;
  //

  selectedFormStrengthOptions: FormStrength;
  selectedRouteOfAdministrationData: Route;

  selectedFormStrengthName: string;
  selectedDose: number = null;
  selectedDoseUnitName: string = '';
  selectedDoseUnitData: MedicationUnit;
  selectedDoseName: string = '';
  selectedRouteName: string;
  selectedPriority: string = 'STAT';
  initialComposerData;
  enteredAdministrationInstructionsText: string = '';
  selectedAdministrationInstructionsData: AdministrationInstructions[] = [];
  userSiteId: number = null;
  subscriptionResetComponentMedFormId: Subscription;
  administrationInstructionsGroups: AdministrationInstructionsGroups = {};

  constructor(
    private fb: FormBuilder,
    private composerSchedulerService: ComposerSchedulerService,
    private userStoreService: UserStoreService
  ) {
    this.userSiteId = this.userStoreService.userSiteId;
    this.doseUnits = this.composerSchedulerService.getSiteMedicationUnits(
      this.userSiteId
    );
    this.routeOfAdministrationUnits = this.composerSchedulerService.getSiteMedicationRouteUnits(this.userSiteId);
    // this.doseUnits = UNITS;
    // console.log('RouteUnits', this.routeOfAdministrationUnits);
  }

  ngOnInit() {
    if (!this.medOptions) {
      alert('No Medication Information');
    } else if (!this.medOptions.availableFormStrength) {
      alert(`No Medication Strengths for ${this.medOptions.brandName}`);
    } else {
      // const formGroupExists: boolean = this.composerSchedulerService.checkComponentFormGroup(this.medComponentId, 'med');
      this.initialComposerData = this.composerSchedulerService.getInitialComposerData();
      if (this.initLowestMedStrengthData) {
        this.initLowestMedicationStrengthOptionData();
      }

      this.medForm = this.fb.group(
        {
          formStrengthOptions: new FormControl(
            this.selectedFormStrengthOptions
          ),
          formStrengthName: new FormControl(this.selectedFormStrengthName),
          dose: new FormControl(this.selectedDose, [
            Validators.required,
            this.doseValidator,
            this.doseValidator.bind(this)
          ]),
          doseUnitName: new FormControl(this.selectedDoseUnitName, [
            Validators.required,
            this.doseUnitValidator,
            this.doseUnitValidator.bind(this),
          ]),
          doseUnitData: new FormControl(this.selectedDoseUnitData),
          // selectedDoseData: new FormControl(this.selectedDoseData),
          routeOfAdministrationData: new FormControl(
            this.selectedRouteOfAdministrationData
          ),
          routeName: new FormControl(this.selectedRouteName, [
            Validators.required,
            this.routeUnitValidator,
          ]),
          priority: new FormControl(this.selectedPriority),
          administrationInstructionsText: new FormControl(null),
          administrationInstructionsData: new FormControl(null),
        }
        // { validators: this.validator.bind(this) }
      );
      // console.log('medOptionsThis', this);
    }
    this.setDefaultValues(),
      this.composerSchedulerService.addFormGroup(
        this.medComponentId,
        'med',
        this.medForm
      );
    this.subscriptionResetComponentMedFormId = this.composerSchedulerService.resetComponentMedFormId.subscribe(() => {
      if (
        this.composerSchedulerService.resetComponentMedFormId &&
        this.composerSchedulerService.resetComponentMedFormId.value ===
        this.medComponentId
      ) {
        this.resetMedForm();
      }
    });
    this.groupAdministrationInstructions();
  }

  ngOnDestroy() {
    this.subscriptionResetComponentMedFormId.unsubscribe();
  }

  initLowestMedicationStrengthOptionData() {
    if (this.medOptions.availableFormStrength[0]) {
      const newFormStrengthId: number = this.medComponentId === 0 ? 0
        // : this.composerSchedulerService.getSelectedFormStrengthId(0, this.medForm.value.formStrengthOptions.medicationId);
        : this.composerSchedulerService.getSelectedFormStrengthId(0);
      // alert(`newSelectedFormStrengthId: ${newFormStrengthId}, medComponentId: ${this.medComponentId}`);
      this.changeSelectedStrength(newFormStrengthId, false);
      // alert('initLowestMedicationStrengthOptionData');
      // console.log('newFormStrengthOptions', this.selectedFormStrengthOptions);
      // this.composerSchedulerService.signalNewFormStrengthSelected(this.selectedFormStrengthOptions.id);
      this.composerSchedulerService.signalNewFormStrengthSelected(this.selectedFormStrengthOptions.medicationId);
    }
  }

  async setDefaultValues() {
    await this.setMedFormDefaultValues();
    // alert('setDefaultValues');
  }

  async setMedFormDefaultValues() {
    const initialComposerData: any = this.composerSchedulerService.getInitialComposerData();
    const initialOrderData: any = initialComposerData.med;
    // Strength
    // if (initialOrderData.formStrengthId) {
    //   this.selectedFormStrengthName = initialOrderData.medication.formStrength;
    //   const formStrengthIndex: number = this.medOptions.availableFormStrength.findIndex(
    //     fs => fs.id === initialOrderData.formStrengthId && fs.formStrengthName === initialOrderData.formStrengthName);
    //   if (formStrengthIndex !== -1) {
    //     this.changeSelectedStrength(formStrengthIndex, false);
    //   }
    // }
    if (this.medComponentId === 0) {
      // Dose
      if (initialOrderData.dose === 0 || initialOrderData.dose) {
        this.selectedDose = initialOrderData.dose;
        this.changeSelectedDose(this.selectedDose);
      }
      // Dose Unit
      if (initialOrderData.doseUnit && initialOrderData.doseUnit.unitName) {
        this.changeSelectedDoseUnit(initialOrderData.doseUnit);
      }
      // Route
      if (initialOrderData.medicationRoute && initialOrderData.medicationRoute.routeName) {
        // const routeUnit = this.selectedFormStrengthOptions.availableRoutes.find(
        this.changeSelectedRoute(initialOrderData.medicationRoute);
      }
      // Priority
      if (initialOrderData.priority === 0 || initialOrderData.priority) {
        switch (initialOrderData.priority) {
          case 2: {
            this.selectedPriority = 'STAT';
            this.changeSelectedPriority('STAT');
            break;
          }
          case 4: {
            this.selectedPriority = 'Routine';
            this.changeSelectedPriority('Routine');
            break;
          }
          case 'STAT': {
            this.selectedPriority = 'STAT';
            this.changeSelectedPriority('STAT');
            break;
          }
          case 'Routine': {
            this.selectedPriority = 'Routine';
            this.changeSelectedPriority('Routine');
            break;
          }
          default: {
            // alert(`Invalid/Unknown Priority: ${initialOrderData.priority}`);
            break;
          }
        }
      }
      // Administration Notes
      if (initialOrderData.orderNotes) {
        this.enteredAdministrationInstructionsText = initialOrderData.orderNotes;
        this.saveAdministrationInstructionsText(initialOrderData.orderNotes);
      }
    }
  }

  disableFormStrengthEdit(): boolean {
    return (this.medComponentId !== 0 ||
      this.medOptions.availableFormStrength.length === 0 ||
      this.initialComposerData.source !== 'med-search') ? true : false;
  }

  resetMedForm() {
    this.selectedFormStrengthOptions = {};
    this.selectedRouteOfAdministrationData = {};

    this.selectedFormStrengthName = '';
    this.selectedDose = null;
    this.selectedDoseUnitName = '';
    this.selectedDoseUnitData = null;
    this.selectedDoseName = '';
    this.selectedRouteName = '';
    this.selectedPriority = 'STAT';
    this.enteredAdministrationInstructionsText = '';

    for (const adminInstructions of this
      .selectedAdministrationInstructionsData) {
      const adminInstructionsCheckbox = document.getElementById(
        `${adminInstructions.name}-${adminInstructions.id}`
      ) as HTMLInputElement;
      if (adminInstructionsCheckbox) {
        adminInstructionsCheckbox.checked = false;
      }
    }

    this.medForm.patchValue({ priority: this.selectedPriority });

    this.selectedAdministrationInstructionsData = [];

    const composerMedComponents = this.composerSchedulerService.getComposerMedComponents();

    // if (!this.selectedFormStrengthName && composerMedComponents[0].composerMedForm.value.formStrengthOptions) {
    if (!this.selectedFormStrengthName) {
      const medicationId = composerMedComponents[0].composerMedForm.value.med.formStrengthOptions ?
        composerMedComponents[0].composerMedForm.value.med.formStrengthOptions.medicationId :
        null;
      let formStrengthIndex: number = !medicationId ? -1 :
        this.medOptions.availableFormStrength.findIndex(fs => fs.medicationId === medicationId);
      // alert(`resetValuesMedFormNewIndex: ${formStrengthIndex}`);
      if (formStrengthIndex === -1) {
        formStrengthIndex = 0;
      }
      this.changeSelectedStrength(formStrengthIndex, false);
    }

    // console.log('resetMedFormThis', this);
  }

  // ********** Form Strength ***************************

  changeSelectedStrength(strengthIndex: number, callReset: boolean = false) {
    if (
      this.selectedFormStrengthName &&
      this.medOptions.availableFormStrength[strengthIndex].formStrengthName ===
      this.selectedFormStrengthName
    ) {
      ('');
    } else {
      if (this.medForm && callReset) {
        // this.composerSchedulerService.resetForm();
        // alert('resetFormCallReset');
        this.composerSchedulerService.resetComponentMedFormById(
          this.medComponentId
        );
        this.composerSchedulerService.removeContinuationMedComponents();
        // this.composerSchedulerService.signalNewFormStrengthSelected(this.medOptions.availableFormStrength[strengthIndex].id);
        this.composerSchedulerService.signalNewFormStrengthSelected(this.medOptions.availableFormStrength[strengthIndex].medicationId);
      }

      this.selectedFormStrengthOptions = this.medOptions.availableFormStrength[
        strengthIndex
      ];
      this.selectedFormStrengthOptions.administrationInstructions = this.medOptions.administrationInstructions;

      this.selectedFormStrengthName = this.selectedFormStrengthOptions.formStrengthName;

      if (this.medForm) {
        this.medForm.controls['formStrengthOptions'].setValue(
          this.selectedFormStrengthOptions
        );

        this.medForm.controls['formStrengthName'].setValue(
          this.selectedFormStrengthOptions.formStrengthName
        );
      }
      // alert(`strengthIndex1: ${strengthIndex} ${this.selectedFormStrengthOptions.formStrengthName}`);

    }
    // console.log('changeStrengthThis', this);
  }

  // ********** Dose/Unit ***************************

  changeSelectedDose(dose: any, source?: string) {
    // console.log('doseValue', dose, source);

    if (source === 'happyButton' && typeof dose === 'object') {
      this.selectedDose = dose.dose;
      this.changeSelectedDoseUnit(dose.doseUnit);
      this.medForm.controls['dose'].setValue(dose.dose);
    } else {
      this.selectedDose = dose;
      this.medForm.controls['dose'].setValue(dose);
    }

    this.changeSelectedDoseName();

    // console.log('changeDoseNumberThis', this);
  }

  changeSelectedDoseUnit(unit: MedicationUnit) {
    // console.log('changebyUnitObject', unit, this.doseUnits, this);
    const doseUnit = this.doseUnits.find(du => du.id === unit.id && du.unitName === unit.unitName);
    if (doseUnit) {
      this.selectedDoseUnitData = doseUnit;
      this.selectedDoseUnitName = doseUnit.unitName;
      this.changeSelectedDoseName();
      this.medForm.controls['doseUnitName'].setValue(doseUnit.unitName);
      this.medForm.controls['doseUnitData'].setValue(doseUnit);
    } else {
      this.selectedDoseUnitData = null;
      this.selectedDoseUnitName = '';
      this.selectedDoseName = '';
      this.medForm.controls['doseUnitName'].setValue('');
      this.medForm.controls['doseUnitData'].setValue(null);
    }

    // console.log('changeDoseUnitThis', this);
    // console.log(
    //   'changeDoseUnitMedComponentsThis',
    //   this.composerSchedulerService
    // );
  }

  changeSelectedDoseUnitByLookup(unitName: string): void {
    // console.log('ChangeByUnitLookup', unitName);
    const matchingUnit = !unitName
      ? null
      : // : UNITS.find((fndUnit) => fndUnit.unitName === unitName);
      // this.doseUnits.find((fndUnit) => fndUnit.printName === unitName || fndUnit.unitName === unitName);
      this.doseUnits.find((fndUnit) => fndUnit.unitName === unitName);
    this.changeSelectedDoseUnit(matchingUnit);
    // console.log('changeDoseUnitByLookupThis', this);
  }

  changeSelectedDoseName(): void {
    if (!this.selectedDose || !this.selectedDoseUnitName) {
      this.selectedDoseName = '';
    } else {
      // this.selectedDoseName = `${this.selectedDose}\u202F${this.selectedDoseUnitName}`;
      this.selectedDoseName = `${this.selectedDose} ${this.selectedDoseUnitName}`;
    }
  }

  searchDoseUnit = (text$: Observable<string>) => {
    const debouncedText$ = text$.pipe(
      debounceTime(200),
      distinctUntilChanged()
    );
    const clicksWithClosedPopup$ = this.click$.pipe(
      filter(() => !this.duInstance.isPopupOpen())
    );
    const inputFocus$ = this.focus$;
    const mergeResults = merge(
      debouncedText$,
      inputFocus$,
      clicksWithClosedPopup$
    ).pipe(
      map((term) => {
        let subSet = [];
        if (term) {
          // subSet = UNITS.filter(
          subSet = this.doseUnits
            .filter(
              (v) => v.unitName.toLowerCase().indexOf(term.toLowerCase()) > -1
            )
            .slice(0, 100);
        } else {
          // subSet = UNITS.slice(0, 10);
          subSet = this.doseUnits.slice(0, 100);
        }
        // return subSet.map((node) => node.printName || node.unitName);
        return subSet.map((node) => node.unitName);
      })
    );
    return mergeResults;
  };

  // ********** Route Of Administration ***************************

  changeSelectedRoute(route: Route) {
    // console.log('ChangeByRouteLookup', route);
    const routeUnit = this.routeOfAdministrationUnits.find(ru => ru.id === route.id && ru.routeName === route.routeName);
    if (routeUnit) {
      this.selectedRouteOfAdministrationData = routeUnit;
      this.selectedRouteName = routeUnit.routeName;
      this.medForm.controls['routeName'].setValue(routeUnit.routeName);
      this.medForm.controls['routeOfAdministrationData'].setValue(routeUnit);
    } else {
      this.selectedRouteOfAdministrationData = null;
      this.selectedRouteName = '';
      this.medForm.controls['routeName'].setValue('');
      this.medForm.controls['routeOfAdministrationData'].setValue(null);
    }
    // console.log('RouteThis', this);
  }

  changeSelectedRouteByLookup(routeName: string): void {
    // console.log('routeString', routeName);
    const matchingRoute = !routeName
      ? null
      // : this.selectedFormStrengthOptions.availableRoutes.find(
      : this.routeOfAdministrationUnits.find(
        (fndRoute) => fndRoute.routeName === routeName
      );
    this.changeSelectedRoute(matchingRoute);
  }

  searchRouteOfAdmin = (text$: Observable<string>) => {
    const debouncedText$ = text$.pipe(
      debounceTime(200),
      distinctUntilChanged()
    );
    const clicksWithClosedPopup$ = this.clickRoute$.pipe(
      filter(() => !this.routeInstance.isPopupOpen())
    );
    const inputFocus$ = this.focusRoute$;
    const mergeResults = merge(
      debouncedText$,
      inputFocus$,
      clicksWithClosedPopup$
    ).pipe(
      map((term) => {
        let subSet = [];
        if (term) {
          // subSet = this.selectedFormStrengthOptions.availableRoutes
          subSet = this.routeOfAdministrationUnits
            .filter(
              (v) => v.routeName.toLowerCase().indexOf(term.toLowerCase()) > -1
            )
            .slice(0, 100);
        } else {
          // subSet = this.selectedFormStrengthOptions.availableRoutes.slice(
          subSet = this.routeOfAdministrationUnits.slice(
            0,
            100
          );
        }
        return subSet.map((node) => node.routeName);
      })
    );
    return mergeResults;
  };

  // ********** Priority ***************************

  changeSelectedPriority(priority: string) {
    this.selectedPriority = priority;
    this.medForm.controls['priority'].setValue(priority);
    // console.log('priorityThis', this);
  }

  // ********** Administration Instructions ***************************

  groupAdministrationInstructions(): void {
    if (this.selectedFormStrengthOptions.administrationInstructions &&
      this.selectedFormStrengthOptions.administrationInstructions.length > 0) {
      let count: number = 0;
      let groupIndex: number = 0;
      this.administrationInstructionsGroups.groups = [{}];
      this.selectedFormStrengthOptions.administrationInstructions.forEach(instr => {
        if (count === 0 || count === 3) {
          groupIndex++;
          this.administrationInstructionsGroups.groups[groupIndex] = { items: [] };
          count = 0;
        }
        count++;
        this.administrationInstructionsGroups.groups[groupIndex].items.push(instr);
      });
      // console.log('adminInstructionsGroups', this.administrationInstructionsGroups);
    }
  }

  saveAdministrationInstructionsText(text: string): void {
    if (text) {
      this.enteredAdministrationInstructionsText = text;
      this.medForm.controls['administrationInstructionsText'].setValue(text);
    } else {
      this.enteredAdministrationInstructionsText = '';
      this.medForm.controls['administrationInstructionsText'].setValue('');
    }

    // console.log('adminInstructionsThis', this);
  }

  updateSelectedAdminInstructions(
    instructions: AdministrationInstructions
  ): void {
    if (instructions.id && instructions.description) {
      const checked = this.adminInstructionCheckboxChecked(instructions);
      if (checked) {
        // Selection needs to be added. Update selected Administration Instructions Pre-Set Data
        this.selectedAdministrationInstructionsData.push(instructions);
        this.medForm.controls['administrationInstructionsData'].setValue(
          this.selectedAdministrationInstructionsData
        );
        // Update selected Administration Instructions Overall Text
        this.enteredAdministrationInstructionsText = !this
          .enteredAdministrationInstructionsText
          ? instructions.description
          : `${this.enteredAdministrationInstructionsText} ${instructions.description}`;
        this.medForm.controls['administrationInstructionsText'].setValue(
          this.enteredAdministrationInstructionsText
        );
      } else {
        // Selection needs to be removed from the selelected administration pre-set text data
        this.selectedAdministrationInstructionsData = this.selectedAdministrationInstructionsData.filter(
          (inst) => inst.id !== instructions.id
        );
        this.medForm.controls['administrationInstructionsData'].setValue(
          this.selectedAdministrationInstructionsData
        );
      }
    }
    // console.log('enteredAdminTextThis', this);
  }

  adminInstructionCheckboxChecked(instructions: AdministrationInstructions): boolean {
    return (document.getElementById(
      `medComponent-${this.medComponentId}-adminInstruction-${instructions.id}`
    ) as HTMLInputElement).checked ? true : false;
  }

  // ********** Validators ***************************

  doseValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!this || !this.medForm || control === null) {
      return null;
    }
    // case dose=0 to dose empty
    if (control.value === null) {
      if (this.medForm.controls['doseUnitName'].value === '') {
        this.medForm.controls['doseUnitName'].setErrors({error: '** Dose Unit is required' })
      }
      return { error: '** Dose is required'};
    }
    // Format is 12.3 means 12 numbers except dot max, 3 numbers after dot max
    const valueAsString = control.value.toString();
    // Dose unit is only not required when dose=0
    if (this.medForm.controls['doseUnitName'].value === '' && valueAsString !== '0') {
      this.medForm.controls['doseUnitName'].setErrors({error: '** Dose Unit is required' })
    } else {
      this.medForm.controls['doseUnitName'].setErrors(null);
    }
    if (valueAsString === '') {
      return { error: '** Dose is required'};
    }
    if (valueAsString.replace('.','').length > 12) {
      return { error: '** Dose must have less than 12 digits'};
    }
    if (!/^\d+(\.\d\d?\d?)?$/.test(valueAsString)) {
      return { error: '** Dose must be a number with no more than 3 decimals'}; 
    }
    return null;
  }

  doseUnitValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!this || !this.medForm) {
      return null;
    }
    if (!control.value && this.medForm.get('dose').value !== '0') {
      return { error: '** Dose Unit is required' };
    }
    return null;
  }

  routeUnitValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return { error: '** Route Unit is required' };
    }
    return null;
  }
}
