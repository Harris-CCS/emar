import { Component, OnInit, Input } from '@angular/core';
import { Patient } from '../../../app/interfaces/patient';
import { SimpleTableComponent } from '../simple-table/simple-table.component';
import { MedSearchComponent } from '../med-search/med-search.component';

@Component({
  selector: 'header-patient',
  templateUrl: './header-patient.component.html',
  // styleUrls: ['./header-patient.component.scss', '../../../assets/css/site.css']
  styleUrls: ['./header-patient.component.css'],
})
export class HeaderPatientComponent implements OnInit {
  @Input() patient: Patient;
  patientLocation: string;
  patientGenderLetter: string;
  patientGenderText: string;
  currentVitalsArray: {
    label: string;
    value: string;
    indicator: string;
    className: string;
  }[];
  vsOverallIndicatorClass: string = 'indicator-normal';
  // showAllAllergies: boolean = false;
  // showAllMeds: boolean = false;

  vitalsTableStructure: SimpleTableComponent;
  allergiesTableStructure: SimpleTableComponent;
  homeMedsTableStructure: SimpleTableComponent;

  constructor() {}

  ngOnInit(): void {}

  getPatientFullName(): string {
    //console.log('patient', this.patient);
    const fullName: string =
      this.patient.fullName ||
      `${this.patient.firstName} ${
        this.patient.middleName ? `${this.patient.middleName} ` : ''
      }${this.patient.lastName}${
        this.patient.nameSuffix ? `, ${this.patient.nameSuffix}` : ''
      }`;
    return fullName;
  }

  getPatientLocation(): string {
    return (this.patientLocation = `${
      this.patient.wardCode ? `${this.patient.wardCode}` : ''
    }${this.patient.roomBedCode ? `${this.patient.roomBedCode}` : ''}`);
  }

  getPatientUrgencyColor(): string {
    if (this.patient.urgencyColor && this.patient.urgencyColor.includes('#')) {
      return this.patient.urgencyColor;
    } else {
      /*
     'R' => 'Red',
     'Y' => 'Yellow',
     'G' => 'Green',
     'B' => 'Blue',
     'P' => 'Purple',
     'Z' => 'Orange',
     'Q' => 'Pink',
     'K' => 'Gray',
     'X' => 'Black'
*/
      switch (this.patient.urgency) {
        case 'R':
          return '#FF0000';
        case 'G':
          return '#60D760';
        case 'Y':
          return '#FBEC5D';
        case 'B':
          return '#64AAF5';
        case 'P':
          return '#CC33CC';
        case 'Q':
          return '#FFC6FF';
        case 'K':
          return '#C2C7CC';
        case 'Z':
          return '#FC9A39';
        case 'X':
          return '#000';
        default:
          return '#000';
      }
    }
  }

  appendDisplayValue(type: string): string {
    switch (type) {
      case 'weight': {
        if (this.patient.heightInCm) {
          return ` / ${this.patient.weightInKg} kg`;
        } else {
          return `${this.patient.weightInKg} kg`;
        }
      }
      case 'accountNumber': {
        if (this.patient.medicalRecordNumber) {
          return ` / ${this.patient.accountNumber}`;
        } else {
          return `${this.patient.accountNumber}`;
        }
      }
      case 'personId': {
        if (this.patient.medicalRecordNumber || this.patient.accountNumber) {
          return ` / ${this.patient.personId}`;
        } else {
          return `${this.patient.personId}`;
        }
      }
      case 'customId': {
        if (
          this.patient.medicalRecordNumber ||
          this.patient.accountNumber ||
          this.patient.personId
        ) {
          return ` / ${this.patient.customPatientId}`;
        } else {
          return `${this.patient.customPatientId}`;
        }
      }
    }
  }

  getPatientGenderIconStyle(): string {
    switch (this.patient.gender) {
      case 'F':
        this.patientGenderText = 'Sex: Female';
        return `gender gender-female`;
      case 'M':
        this.patientGenderText = 'Sex: Male';
        return `gender gender-male`;
      case 'O':
        this.patientGenderLetter = this.patient.gender;
        this.patientGenderText = 'Sex: Other';
        return 'gender gender-other-unknown';
      case 'U':
        this.patientGenderLetter = this.patient.gender;
        this.patientGenderText = 'Sex: Unknown';
        return 'gender gender-other-unknown';
      default:
        return '';
    }
  }

  getCustomIndicatorImage(indicator: any): string {
    return `../../../assets/img/${indicator.imageName}`;
  }

  checkForData(category: string): boolean {
    switch (category) {
      case 'vitals': {
        this.maybeBuildCurrentVitalsArray();
        if (!this.currentVitalsArray || !this.currentVitalsArray.length) {
          return false;
        } else {
          return true;
        }
      }
      case 'allergies': {
        if (!this.patient.allergies || !this.patient.allergies.length) {
          return false;
        } else {
          this.maybeBuildAllergiesTableStructure();
          return true;
        }
      }
      case 'homeMeds': {
        if (!this.patient.homeMeds || !this.patient.homeMeds.length) {
          return false;
        } else {
          this.maybeBuildHomeMedsTableStructure();
          return true;
        }
      }
      default: {
        return false;
      }
    }
  }

  // onExpandButtonClick(type: string): void {
  //   if (type === 'allergies') {
  //     this.showAllAllergies = !this.showAllAllergies;
  //   }
  //   if (type === 'meds') {
  //     this.showAllMeds = !this.showAllMeds;
  //   }
  // }

  getVitalSignIconPath() {
    if (this.vsOverallIndicatorClass === 'indicator-high') {
      return `../../../assets/icon/vitals_high.svg`;
    } else {
      return `../../../assets/icon/vitals_mid.svg`;
    }
  }

  maybeBuildCurrentVitalsArray() {
    if (!this.currentVitalsArray || !this.currentVitalsArray.length) {
      this.currentVitalsArray = [];
      if (this.patient.vsSystolic && this.patient.vsDiastolic) {
        this.logVitalSign(
          'Blood Pressure: ',
          `${this.patient.vsSystolic}/${this.patient.vsDiastolic}`,
          this.patient.vsBloodPressureIndicator
        );
      }
      if (this.patient.vsPulse) {
        this.logVitalSign(
          'Mean Arterial Pressure (MAP)',
          this.patient.vsMap,
          this.patient.vsMapLevel
        );
      }
      if (this.patient.vsPulse) {
        this.logVitalSign(
          'Pulse: ',
          this.patient.vsPulse,
          this.patient.vsPulseIndicator
        );
      }
      if (this.patient.vsRespiratory) {
        this.logVitalSign(
          'Respiratory: ',
          this.patient.vsRespiratory,
          this.patient.vsRespiratoryIndicator
        );
      }
      if (this.patient.vsTemperature) {
        this.logVitalSign(
          'Temperature: ',
          this.patient.vsTemperature,
          this.patient.vsTemperatureIndicator
        );
      }
      if (this.patient.vsEndTidal) {
        this.logVitalSign(
          'End Tidal: ',
          this.patient.vsEndTidal,
          this.patient.vsEndTidalLevel
        );
      }
      if (this.patient.vsOxygenSaturation) {
        this.logVitalSign(
          'Oxygen Saturation: ',
          this.patient.vsOxygenSaturation,
          this.patient.vsOxygenSaturationIndicator
        );
      }
      if (this.patient.vsPainScale) {
        this.logVitalSign(
          'Pain Scale: ',
          this.patient.vsPainScale,
          this.patient.vsPainScaleIndicator
        );
      }
      // console.log('vitalSignsTable', this.vitalsTableStructure);
    }
  }

  logVitalSign(label: string, value: string, indicator: string): void {
    const className = this.getIndicatorClassName(indicator);
    // Table Structure
    if (!this.vitalsTableStructure) {
      this.vitalsTableStructure = new SimpleTableComponent();
      this.vitalsTableStructure.title = 'Current Vital Signs';
      if (this.patient.vsDateTime) {
        this.vitalsTableStructure.appendTableHeaderCell(true, {
          isHeaderCell: true,
          data: 'Last Taken: ',
          className: 'left-align',
        });
        this.vitalsTableStructure.appendTableHeaderCell(false, {
          isHeaderCell: true,
          data: this.patient.vsDateTime,
          dataType: 'date',
          className: 'left-align',
        });
      }
      this.vitalsTableStructure.params = {
        pagination: {
          usePagination: false,
        },
      };
    }
    this.vitalsTableStructure.appendTableBodyCell(true, {
      isHeaderCell: true,
      data: label,
      dataType: 'string',
    });
    this.vitalsTableStructure.appendTableBodyCell(false, {
      isHeaderCell: false,
      data: value,
      dataType: 'string',
      className,
    });
    const vsObject = {
      label,
      value,
      indicator,
      className,
    };
    this.currentVitalsArray.push(vsObject);
  }

  getIndicatorClassName(indicator: any): string {
    if (indicator === '0') {
      if (this.vsOverallIndicatorClass === 'indicator-normal') {
        this.vsOverallIndicatorClass = 'indicator-low';
      }
      return 'indicator-low';
    } else if (indicator === '2') {
      this.vsOverallIndicatorClass = 'indicator-high';
      return 'indicator-high';
    } else {
      return 'indicator-normal';
    }
  }

  // Allergies

  maybeBuildAllergiesTableStructure() {
    if (!this.allergiesTableStructure) {
      this.allergiesTableStructure = new SimpleTableComponent();
      this.allergiesTableStructure.title = 'Current Allergies';
      this.allergiesTableStructure.params = {
        pagination: {
          usePagination: true,
        },
      };
      // Table Headers
      this.allergiesTableStructure.appendTableHeaderCell(true, {
        isHeaderCell: true,
        data: 'Name',
      });
      this.allergiesTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Reaction',
      });
      this.allergiesTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Severity',
      });
      this.allergiesTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Source',
      });
      this.allergiesTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Comment',
      });
      // Table Data
      for (const alg of this.patient.allergies) {
        this.allergiesTableStructure.appendTableBodyCell(true, {
          isHeaderCell: true,
          data: alg.name,
          dataType: 'string',
        });
        this.allergiesTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: alg.reaction,
          dataType: 'string',
        });
        this.allergiesTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: alg.severity,
          dataType: 'string',
        });
        this.allergiesTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: alg.source || ' ',
          dataType: 'string',
        });
        this.allergiesTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: alg.comment,
          dataType: 'string',
        });
      }
      // console.log('allergies', this.allergiesTableStructure);
    }
  }

  // Home Medications
  maybeBuildHomeMedsTableStructure() {
    if (!this.homeMedsTableStructure) {
      this.homeMedsTableStructure = new SimpleTableComponent();
      this.homeMedsTableStructure.title = 'Home Medications';
      this.homeMedsTableStructure.params = {
        pagination: {
          usePagination: true,
        },
      };
      // Table Headers
      this.homeMedsTableStructure.appendTableHeaderCell(true, {
        isHeaderCell: true,
        data: 'Name',
      });
      this.homeMedsTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Dose',
      });
      this.homeMedsTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Route',
      });
      this.homeMedsTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Schedule',
      });
      this.homeMedsTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Last Taken',
      });
      this.homeMedsTableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Comment',
      });
      // Table Data
      for (const med of this.patient.homeMeds) {
        this.homeMedsTableStructure.appendTableBodyCell(true, {
          isHeaderCell: true,
          data: med.name,
          dataType: 'string',
        });
        this.homeMedsTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: med.dose,
          dataType: 'string',
        });
        this.homeMedsTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: med.route,
          dataType: 'string',
        });
        this.homeMedsTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: med.schedule,
          dataType: 'string',
        });
        this.homeMedsTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: med.lastTaken || ' ',
          dataType: 'date',
        });
        this.homeMedsTableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: med.comment || ' ',
          dataType: 'string',
        });
      }
      // console.log('homeMeds', this.homeMedsTableStructure);
    }
  }
}
