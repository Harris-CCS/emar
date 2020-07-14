import { Component, OnInit, Input } from '@angular/core';
import { Patient } from '../../../app/interfaces/patient';

@Component({
  selector: 'header-patient',
  templateUrl: './header-patient.component.html',
  // styleUrls: ['./header-patient.component.scss', '../../../assets/css/site.css']
  styleUrls: ['./header-patient.component.css'],
})
export class HeaderPatientComponent implements OnInit {
  @Input() patient: Patient;
  patientFullName: string;
  patientLocation: string;
  patientUrgencyColor: string;
  currentVitalsArray: {
    label: string;
    value: string;
    indicator: string;
    className: string;
  }[];
  vsOverallIndicatorClass: string = 'indicator-normal';
  showAllAllergies: boolean = false;
  showAllMeds: boolean = false;

  constructor() {}

  ngOnInit(): void {}

  getPatientFullName(): string {
    return (this.patientFullName = `${this.patient.firstName} ${
      this.patient.middleName ? `${this.patient.middleName} ` : ''
    }${this.patient.lastName}${
      this.patient.nameSuffix ? `, ${this.patient.nameSuffix}` : ''
    }`);
  }

  getPatientLocation(): string {
    return (this.patientLocation = `${
      this.patient.wardCode ? `${this.patient.wardCode}` : ''
    }${this.patient.roomBedCode ? `${this.patient.roomBedCode}` : ''}`);
  }

  getPatientUrgencyColor(): string {
    switch (this.patient.urgencyColor) {
      case 'R':
        return (this.patientUrgencyColor = '#f32836');
      case 'G':
        return (this.patientUrgencyColor = '#8ecc69');
      case 'Y':
        return (this.patientUrgencyColor = '#fdd95b');
      case 'B':
        return (this.patientUrgencyColor = '#121de7');
      case 'PINK':
        return (this.patientUrgencyColor = '#ffc6ff');
      case 'ORANGE':
        return (this.patientUrgencyColor = '#f79f55');
      default:
        return (this.patientUrgencyColor = '#000');
    }
  }

  getPatientGenderIconStyle(): string {
    switch (this.patient.gender) {
      case 'F':
        return `gender gender-female`;
      case 'M':
        return `gender gender-male`;
      case 'O':
        return '';
      case 'U':
        return '';
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
          return true;
        }
      }
      case 'homeMeds': {
        if (!this.patient.homeMeds || !this.patient.homeMeds.length) {
          return false;
        } else {
          return true;
        }
      }
      default: {
        return false;
      }
    }
  }

  onExpandButtonClick(type: string): void {
    if (type === 'allergies') {
      this.showAllAllergies = !this.showAllAllergies;
    }
    if (type === 'meds') {
      this.showAllMeds = !this.showAllMeds;
    }
  }

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
    }
  }

  logVitalSign(label: string, value: string, indicator: string): void {
    const className = this.getIndicatorClassName(indicator);
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
}
