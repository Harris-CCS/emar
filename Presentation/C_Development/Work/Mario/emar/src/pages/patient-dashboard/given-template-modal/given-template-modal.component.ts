import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

import { ModalService } from '../../../services/modal.service';
import { GIVEN_TEMPLATE_EAR, } from '../../../app/mockup/given-template-ear';

@Component({
  selector: 'given-template-modal',
  templateUrl: './given-template-modal.component.html',
  styleUrls: ['./given-template-modal.component.scss']
})
export class GivenTemplateModalComponent implements OnInit {
    givenTemplateForm: FormGroup;
    template = GIVEN_TEMPLATE_EAR;

    constructor(
        private modalService: ModalService) {
    }
    ngOnInit(): void {
        this.givenTemplateForm = new FormGroup({});
        for (let group of this.template.promptGroups) {
            for (let prompt of group.prompts) {
                this.givenTemplateForm.addControl(prompt.id.toString(), new FormControl(prompt.default || null));
            }
        }
    }
    changeChoice(prompt, choice) {

    }

    onSelectTime(prompt) {

    }

    onCancel() {
        this.modalService.close('given-template-modal');
        this.givenTemplateForm.reset();
    }

    onSubmit() {
        console.log(this.givenTemplateForm);
        this.modalService.close('given-template-modal');
        this.givenTemplateForm.reset();
    }
}