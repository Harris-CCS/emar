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
  patientAgeText: string;

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

  getPatientAgeSexDisplay(): string {
    return (this.patientAgeText = `${
      this.patient.age ? `${this.patient.age} ` : ''
    }${this.patient.ageUnits ? `${this.patient.ageUnits} ` : ''}`);
  }

  getPatientGenderIconStyle(): string {
    let iconFolderPath: string = 'gender';
    switch (this.patient.gender) {
      case 'F':
        return `${iconFolderPath} gender-female`;
      case 'M':
        return `${iconFolderPath} gender-male`;
      case 'O':
        return '';
      case 'U':
        return '';
      default:
        return '';
    }
  }

  checkForData(category: string): boolean {
    if (category === 'vitals') {
      return true;
    } else if (category === 'allergies') {
      return false;
    } else if (category === 'homeMeds') {
      return false;
    }
  }

  getVitalSignIconPath() {}
}
