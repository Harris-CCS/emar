import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  ViewChild,
  ɵConsole,
} from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Observable, Subject, merge } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  filter,
  map,
} from 'rxjs/operators';
import { ComposerOptions } from '../../../app/interfaces/composerOptions';
import { FormStrength } from '../../../app/interfaces/formStrength';
import { AdministrationInstructions } from '../../../app/interfaces/administrationInstructions';
import { NgbTypeahead } from '@ng-bootstrap/ng-bootstrap';
import { Dose } from '../../../app/interfaces/dose';
import { Unit } from '../../../app/interfaces/unit';
import { Route } from '../../../app/interfaces/route';
import { UNITS } from '../../../app/mockup/doseUnits';

@Component({
  selector: 'med-form',
  templateUrl: './med-form.component.html',
  styleUrls: ['./med-form.component.scss'],
})
export class MedFormComponent implements OnInit {
  @Input() medOptions: ComposerOptions;
  @Input() initLowestMedStrengthData: boolean = true;
  @Output() formReady = new EventEmitter<FormGroup>();

  @ViewChild('duInstance', { static: true }) duInstance: NgbTypeahead;
  focus$ = new Subject<string>();
  click$ = new Subject<string>();
  @ViewChild('routeInstance', { static: true }) routeInstance: NgbTypeahead;
  focusRoute$ = new Subject<string>();
  clickRoute$ = new Subject<string>();

  medForm: FormGroup;
  // Temp Data for strengths, doses, doseUnits, routes, and priorities - Get these from API service
  doseUnits: Unit[] = UNITS;
  priorities = ['STAT', 'Routine'];
  //

  selectedFormStrengthOptions: FormStrength;
  selectedRouteOfAdministrationData: Route;

  selectedFormStrengthName: string;
  selectedDose: number = null;
  selectedDoseUnitName: string = '';
  selectedDoseUnitData: Unit;
  selectedDoseName: string = '';
  selectedRouteName: string;
  selectedPriority: string = 'STAT';
  enteredAdministrationInstructionsText: string = '';
  selectedAdministrationInstructionsData: AdministrationInstructions[] = [];

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
    if (!this.medOptions) {
      alert('No Medication Information');
    } else if (!this.medOptions.availableFormStrength) {
      alert(`No Medication Strengths for ${this.medOptions.brandName}`);
    } else {
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
          ]),
          doseUnitName: new FormControl(this.selectedDoseUnitName, [
            Validators.required,
            this.doseUnitValidator,
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
      // console.log('medOptions', this.medOptions);
      // console.log('thisInit', this);
      // console.log('medForm', this.medForm);
    }
  }

  initLowestMedicationStrengthOptionData(): void {
    if (this.medOptions.availableFormStrength[0]) {
      // this.assignSelectedMedStrengthParameters(0);
      this.changeSelectedStrength(0);
    }
  }

  assignSelectedMedStrengthParameters(medStrengthIndex: number): void {
    if (this.medForm) {
      this.resetMedForm();
      this.changeSelectedStrength(medStrengthIndex);
    } else {
      this.selectedFormStrengthOptions = this.medOptions.availableFormStrength[
        medStrengthIndex
      ];
    }
  }

  resetMedForm() {
    this.selectedFormStrengthOptions = {};
    this.selectedRouteOfAdministrationData = {};

    this.selectedFormStrengthName = '';
    this.selectedDose = null;
    this.selectedDoseUnitName = '';
    this.selectedDoseUnitData = {};
    this.selectedDoseName = '';
    this.selectedRouteName = '';
    this.selectedPriority = 'STAT';
    this.enteredAdministrationInstructionsText = '';
    this.selectedAdministrationInstructionsData = [];

    this.medForm.reset();
    // console.log('resetMedForm', this.medForm);
  }

  // ********** Form Strength ***************************

  changeSelectedStrength(strengthIndex: number) {
    if (
      this.selectedFormStrengthName &&
      this.medOptions.availableFormStrength[strengthIndex].formStrengthName ===
        this.selectedFormStrengthName
    ) {
      console.log('do nothing');
    } else {
      if (this.medForm) {
        this.resetMedForm();
      }

      this.selectedFormStrengthOptions = this.medOptions.availableFormStrength[
        strengthIndex
      ];
      this.selectedFormStrengthName = this.selectedFormStrengthOptions.formStrengthName;

      if (this.medForm) {
        this.medForm.controls['formStrengthOptions'].setValue(
          this.selectedFormStrengthOptions
        );

        this.medForm.controls['formStrengthName'].setValue(
          this.selectedFormStrengthOptions.formStrengthName
        );
      }
    }
    // console.log('thisStrength', this);
  }

  // ********** Dose/Unit ***************************

  changeSelectedDose(dose: any, source?: string) {
    if (source === 'happyButton' && typeof dose === 'object') {
      this.selectedDose = dose.dose;
      this.changeSelectedDoseUnit(dose.doseUnit);
      this.medForm.controls['dose'].setValue(dose.dose);
    } else {
      this.selectedDose = dose;
      this.medForm.controls['dose'].setValue(dose);
    }

    this.changeSelectedDoseName();

    // console.log('thisChangeDoseNumber', this);
    // console.log('medForm', this.medForm);
  }

  changeSelectedDoseUnit(unit: Unit) {
    // console.log('changebyUnitObject', unit);
    if (unit) {
      this.selectedDoseUnitData = unit;
      this.selectedDoseUnitName = unit.unitName;
      this.changeSelectedDoseName();
      this.medForm.controls['doseUnitName'].setValue(unit.unitName);
      this.medForm.controls['doseUnitData'].setValue(unit);
    } else {
      this.selectedDoseUnitData = null;
      this.selectedDoseUnitName = '';
      this.selectedDoseName = '';
      this.medForm.controls['doseUnitName'].setValue('');
      this.medForm.controls['doseUnitData'].setValue(null);
    }

    // console.log('thisChangeDoseUnit', this);
    // console.log('medForm', this.medForm);
  }

  changeSelectedDoseUnitByLookup(unitName: string): void {
    // console.log('ChangeByUnitLookup', unitName);
    const matchingUnit = !unitName
      ? null
      : UNITS.find((fndUnit) => fndUnit.unitName === unitName);
    this.changeSelectedDoseUnit(matchingUnit);
    // console.log('thisChangeDoseUnitByLookup', this);
    // console.log('medForm', this.medForm);
  }

  changeSelectedDoseName(): void {
    if (!this.selectedDose || !this.selectedDoseUnitName) {
      this.selectedDoseName = '';
    } else {
      this.selectedDoseName = `${this.selectedDose}\u202F${this.selectedDoseUnitName}`;
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
          subSet = UNITS.filter(
            (v) => v.unitName.toLowerCase().indexOf(term.toLowerCase()) > -1
          ).slice(0, 10);
        } else {
          subSet = UNITS.slice(0, 10);
        }
        return subSet.map((node) => node.unitName);
      })
    );
    return mergeResults;
  };

  // TODO: Check if these methods are needed

  lookupUnit(text$: Observable<string>) {
    return text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      map((term) =>
        term.length < 1
          ? []
          : UNITS.filter((unit) =>
              new RegExp(term, 'mi').test(unit.unitName)
            ).slice(0, 10)
      )
      // TODO this.units but this is undefined
    );
  }

  formatUnit = (unit: Unit) => unit.unitName;

  getSelectedDoseUnitName(): string {
    alert(this.selectedDoseUnitName);
    return this.selectedDoseUnitName;
  }

  // ********** Route Of Administration ***************************

  changeSelectedRoute(route: Route) {
    // console.log('ChangeByRouteLookup', route);
    if (route) {
      this.selectedRouteOfAdministrationData = route;
      this.selectedRouteName = route.routeName;
      this.medForm.controls['routeName'].setValue(route.routeName);
      this.medForm.controls['routeOfAdministrationData'].setValue(route);
    } else {
      this.selectedRouteOfAdministrationData = null;
      this.selectedRouteName = '';
      this.medForm.controls['routeName'].setValue('');
      this.medForm.controls['routeOfAdministrationData'].setValue(null);
    }
    // console.log('thisRouteObject', this);
    // console.log('medForm', this.medForm);
  }

  changeSelectedRouteByLookup(routeName: string): void {
    // console.log('routeString', routeName);
    const matchingRoute = !routeName
      ? null
      : this.selectedFormStrengthOptions.availableRoutes.find(
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
          subSet = this.selectedFormStrengthOptions.availableRoutes
            .filter(
              (v) => v.routeName.toLowerCase().indexOf(term.toLowerCase()) > -1
            )
            .slice(0, 10);
        } else {
          subSet = this.selectedFormStrengthOptions.availableRoutes.slice(
            0,
            10
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
    // console.log('thisPriorityString', this);
    // console.log('medForm', this.medForm);
  }

  // ********** Administration Instructions ***************************

  saveAdministrationInstructionsText(text: string): void {
    if (text) {
      this.enteredAdministrationInstructionsText = text;
      this.medForm.controls['administrationInstructionsText'].setValue(text);
    } else {
      this.enteredAdministrationInstructionsText = '';
      this.medForm.controls['administrationInstructionsText'].setValue('');
    }

    // console.log('thisAdminInstructions', this);
    // console.log('medForm', this.medForm);
  }

  updateSelectedAdminInstructions(
    instructions: AdministrationInstructions
  ): void {
    if (instructions.id && instructions.text) {
      const checked = (document.getElementById(
        `${instructions.name}-${instructions.id}`
      ) as HTMLInputElement).checked;
      if (checked) {
        // Selection needs to be added. Update selected Administration Instructions Pre-Set Data
        this.selectedAdministrationInstructionsData.push(instructions);
        this.medForm.controls['administrationInstructionsData'].setValue(
          this.selectedAdministrationInstructionsData
        );
        // Update selected Administration Instructions Overall Text
        this.enteredAdministrationInstructionsText = !this
          .enteredAdministrationInstructionsText
          ? instructions.text
          : `${this.enteredAdministrationInstructionsText} ${instructions.text}`;
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
    // console.log('enteredAdminText', this.enteredAdministrationInstructionsText);
    // console.log('medForm', this.medForm);
  }

  // ********** Validators ***************************

  doseValidator(control: AbstractControl): { [key: string]: any } | null {
    console.log('controlValue', control.value);
    if (!control.value) {
      return { error: '** Dose is required' };
    } else if (control.value.toString().includes('-')) {
      return { error: '** Dose cannot be negative or contain dashes' };
    } else if (control.value.length > 4) {
      return { error: '** Dose cannot be > 4 characters' };
    }
    return null;
  }

  doseUnitValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
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
