import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

import { ModalService } from '../../../services/modal.service';
import { GIVEN_TEMPLATE_EAR} from '../../../app/mockup/given-template-ear';
import { PromptGroup, Prompt, PromptChoice } from '../../../app/interfaces/given-template';

@Component({
  selector: 'given-template-modal',
  templateUrl: './given-template-modal.component.html',
  styleUrls: ['./given-template-modal.component.scss']
})
export class GivenTemplateModalComponent implements OnInit {
    givenTemplateForm: FormGroup;
    template = GIVEN_TEMPLATE_EAR; // TODO

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
    onSelectChoice(prompt: Prompt, choice: PromptChoice): void {
        this.givenTemplateForm.controls[prompt.id].setValue(choice.id);
    }

    // get the label of the current choice
    getChoice(prompt: Prompt): string {
        const value: string = this.givenTemplateForm.controls[prompt.id].value;
        if (value === null || value === "") return "";
        const choices: PromptChoice[] = prompt.promptChoices.filter((choice) => choice.id.toString() == value);
        if (choices.length <= 0) return "";
        return choices[0].choiceText;
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

    // return the position in an all above group - 0 if not in an all above
    inAllAbove(group: PromptGroup, prompt: Prompt): number {
        let ii: number = 0;
        for (let pr of group.prompts) {
            if (pr.type == "CheckBox" && pr.promptChoices.length) { // All of the above checkbox
                for (let choice of pr.promptChoices) {
                    ii = ii + 1;
                    if (+choice.choiceText == prompt.id) {
                        return ii;
                    }
                }
                ii = 0;
            }
        }
        return ii;
    }

    // Check all the above
    onClickCheckbox(prompt: Prompt): void {
        if (prompt.promptChoices.length) { // All of above
            let checked = this.givenTemplateForm.controls[prompt.id.toString()].value;
            if (checked === null) checked = false;
            for (let choice of prompt.promptChoices) {
                this.givenTemplateForm.controls[choice.choiceText].setValue(!checked);
            }
        }
    }
}