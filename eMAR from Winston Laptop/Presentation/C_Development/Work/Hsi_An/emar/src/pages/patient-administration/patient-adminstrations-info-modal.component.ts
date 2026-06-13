import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Patient } from 'src/app/interfaces/patient';
import { ModalService } from 'src/services/modal.service';

@Component({
  selector: 'app-patient-adminstrations-info-modal',
  templateUrl: './patient-adminstrations-info-modal.component.html',
  styleUrls: ['./patient-adminstrations-info-modal.component.scss']
})
export class PatientAdminstrationsInfoModalComponent  implements OnInit {
  //@Input() modalTitle: string;
  @Input() 
  modalPatientId: number;
  @Input() 
  modalPatientFirstName: string;
  @Input() 
  modalPatientLastName: string;
  @Input() 
  modalMedicine: string;
  @Input() 
  modalMedicineSelectedScheduled: string;
  @Input()
  modalAdministeredMedicines: string[];
  @Output() modalDateTimeSelected:EventEmitter<string>= new EventEmitter(); 
  outputMessage:string="I am child component."  
  constructor(
    private modalService: ModalService
  ) { 
    
  }

  ngOnInit(): void {
    console.log('Begin patient administrations for' +  this.modalPatientId + 
    ' Patient Name:  ' + this.modalPatientFirstName + " " + this.modalPatientLastName
); 

  }
  leave = () => {
    console.log('Leaving patient administrations for' +  this.modalPatientId + 
         ' Patient Name:  ' + this.modalPatientFirstName + " " + this.modalPatientLastName
        // ', Patient Id:  ' + this.patient.id 
      // ' Site Name:  ' + this.siteName +
      // ' Site Id:  ' + this.siteId
    );
    //this.closeModifyOption();
    this.modalService.close('patientAdministrationInfo');
  }
  dateTimeSelectedLeave (expected: any) {
    this.modalDateTimeSelected.emit(expected);  
    console.log('Leaving patient administrations for' +  this.modalPatientId + 
         ' Patient Name:  ' + this.modalPatientFirstName + " " + this.modalPatientLastName
        // ', Patient Id:  ' + this.patient.id 
      // ' Site Name:  ' + this.siteName +
      // ' Site Id:  ' + this.siteId
    );
    //this.closeModifyOption();
    this.modalService.close('patientAdministrationInfo');
  }

}
