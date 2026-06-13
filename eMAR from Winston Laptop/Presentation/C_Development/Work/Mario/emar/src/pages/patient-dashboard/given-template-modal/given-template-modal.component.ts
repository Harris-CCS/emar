import { Component, OnInit, EventEmitter, ComponentFactoryResolver, OnDestroy, ViewChild, Injectable } from '@angular/core';
import { FormGroup, FormControl, AbstractControl, Validators } from '@angular/forms';
import * as moment from 'moment';

import { ModalService } from '../../../services/modal.service';
import { PromptGroup, Prompt, PromptChoice, GivenTemplate } from '../../../app/interfaces/given-template';
import { UserStoreService } from '../../../services/user-store.service';
import { Order, OrderAdministration } from '../../../app/interfaces/order';
import { User } from '../../../app/interfaces/user';
import { PatientStoreService } from '../../../services/patient-store.service';
import { PatientMedOrderService } from '../../../services/patient-med-order.service';
import { PatientMedOrderStoreService } from '../../../services/patient-med-order-store.service';
import { SiteStoreService } from '../../../services/site-store.service';
import { Subject, Subscription } from 'rxjs';
import { NgbDate, NgbDatepicker, NgbDateAdapter, NgbDateStruct, NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
import { SimplePlaceholderMapper } from '@angular/compiler/src/i18n/serializers/serializer';
import { takeUntil } from 'rxjs/operators';

const TIME_FORMAT = 'HH:mm'; // moment format
const DATE_FORMAT = 'MM/DD/yyyy'; // moment format

interface DateTimeId {
    dateTime: string,
    id: string
}
/* This Service handles how the date is represented in scripts i.e. ngModel */
@Injectable()
export class CustomAdapter extends NgbDateAdapter<string> {
    readonly DELIMITER = '/';
    fromModel(value: string | null): NgbDateStruct | null {
        if (value) {
            let date = value.split(this.DELIMITER);
            // console.log('FROMMODEL',value,{day : parseInt(date[0], 10),month : parseInt(date[1], 10),year : parseInt(date[2], 10)});
            return {
                day : parseInt(date[1], 10),
                month : parseInt(date[0], 10),
                year : parseInt(date[2], 10)
            };
        }
        return null;
    }
    toModel(date: NgbDateStruct | null): string | null {
        // console.log('TOMODEL',date,date ? date.day + this.DELIMITER + date.month + this.DELIMITER + date.year : null);
        return date ? ('0' + date.month.toString()).slice(-2) + this.DELIMITER + ('0' + date.day.toString()).slice(-2) + this.DELIMITER + date.year : null;
    }
}
/* This Service handles how the date is rendered and parsed from keyboard i.e. in the bound input field.*/
@Injectable()
export class CustomDateParserFormatter extends NgbDateParserFormatter {
    readonly DELIMITER = '/';
    parse(value: string): NgbDateStruct | null {
        if (value) {
            let date = value.split(this.DELIMITER);
            // console.log('PARSE',value,{day : parseInt(date[1], 10),month : parseInt(date[0], 10),year : parseInt(date[2], 10)});
            return {
                day : parseInt(date[1], 10),
                month : parseInt(date[0], 10),
                year : parseInt(date[2], 10)
            };
        }
        return null;
    }
    format(date: NgbDateStruct | null): string {
        // console.log('FORMAT',date,date ? ('0' + date.month.toString()).slice(-2) + this.DELIMITER + ('0' + date.day.toString()).slice(-2) + this.DELIMITER + date.year : '');
        return date ? ('0' + date.month.toString()).slice(-2) + this.DELIMITER + ('0' + date.day.toString()).slice(-2) + this.DELIMITER + date.year : '';
    }
}
@Component({
  selector: 'given-template-modal',
  templateUrl: './given-template-modal.component.html',
  styleUrls: ['./given-template-modal.component.scss'],
  providers: [
    {provide: NgbDateAdapter, useClass: CustomAdapter},
    {provide: NgbDateParserFormatter, useClass: CustomDateParserFormatter}
  ]
})
export class GivenTemplateModalComponent implements OnInit, OnDestroy {
    givenTemplateForm: FormGroup = new FormGroup({});
    template: GivenTemplate = {};
    order: Order;
    admin: OrderAdministration;
    givenAtEvent = new EventEmitter<string>();
    orderUsers: User[] = [];
    timeFormat: string = TIME_FORMAT;
    dateFormat: string = DATE_FORMAT;
    triageDateTime: string = '';
    lastGivenDateTime: string = '';
    buttonText: string = '';
    siteUTCOffset: string = ''; // -06:00
    userSubscribe: Subscription = null;
    modalSubscribe: Subscription = null;
    templateSubscribe: Subscription = null;
    notifierTemplateSubscribe = new Subject;
    eventSubscribe: Subscription = null;
    depth: number[]; // number of parents
    margin: number[]; // margin to apply to a child
    utility: boolean[]; // prompt is an utility: information, label, all of the above - not saved in database
    parent: number[]; // direct parent promptid
    line: number[]; // 1: first on line, 2: on line, -1: last on line, 0: alone on line

    promptMap: Object
    hiddenPromptChild: Array<any>

    constructor(
        private modalService: ModalService,
        private userStoreService: UserStoreService,
        private patientStoreService: PatientStoreService,
        private siteStoreService: SiteStoreService,
        private patientMedOrderService : PatientMedOrderService,
        private patientMedOrderStoreService: PatientMedOrderStoreService) {
    }
    ngOnInit(): void {
        /*
        this.userSubscribe = this.userStoreService.user$.subscribe ( data => {
            console.log('SUBSCRIBE TEMPLATE');
            // this.displayUserName = typeof data.id === 'undefined'? '':
            //    data.displayInitialsIndicator? data.userInitials: data.lastName + ', ' + data.firstName;
            // TODO uncomment when  API to be ready
            // this.timeFormat = typeof data.timeFormat === 'undefined' ? TIME_FORMAT: data.timeFormat;
            // this.dateFormat = typeof data.dateFormat === 'undefined' ? DATE_FORMAT: data.dateFormat;
        });
        */
        this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
        this.dateFormat = this.siteStoreService.long_date_format
        this.triageDateTime = this.patientStoreService.visitStartDateTime;
        this.modalSubscribe = this.modalService.modalOpening.subscribe( modal => {
            // console.log('SUBSCRIBE TEMPLATE', moment().format('HH:mm:ss'));
            const data = modal.data;
            if (typeof data.template === 'undefined') return; // this is not a template popup, perhaps a date popup
            this.template = data.template;
            
            this.givenTemplateForm = new FormGroup({});
            this.promptMap = {}
            this.hiddenPromptChild = []

            for (let group of this.template.promptGroups) {
                for (let prompt of group.prompts) {
                    if (prompt.promptChildren.length && prompt.displayChildPromptsValue) {
                        this.promptMap[group.id] = this.promptMap[group.id] || {}
                        this.promptMap[group.id][prompt.id] = this.promptMap[group.id][prompt.id] || {} 

                        for (let promptChildID of prompt.promptChildren) {
                            const childPrompt = group.prompts.find(p => p.id === promptChildID)
                            if (childPrompt?.required) {
                                this.hiddenPromptChild.push(Number(promptChildID))

                                this.promptMap[group.id][prompt.id][promptChildID] = true
                            }
                        }

                        // Object.assign(this.promptMap[group.id][prompt.id],
                        //     ...prompt.promptChildren.map((key) => ({ [key]: group.prompts.filter( p => p.id === key)[0].required }))
                        // )
                    }
                }
            }
            // console.log('=================setPromptChildrenValidators: hiddenPromptChild: ', this.hiddenPromptChild)


            for (let group of this.template.promptGroups) {
                for (let prompt of group.prompts) {
                    let def = prompt.default;
                    if (def == 'now') { // TODO delete when no more mockup
                        def = moment().format();
                    }
                    /* if (def === "<user>") {
                        def = this.displayUserName;
                    } */

                    if (prompt.type.toLowerCase() == 'checkbox' && prompt.required) {
                        const validator: FormControl = new FormControl(def, [this.checkBoxValidator])
                        this.givenTemplateForm.addControl(prompt.id.toString(), validator);
                    } else {
                        const validator: FormControl = prompt.type == 'Date'? new FormControl(def, this.validatorDate): 
                        //prompt.type == 'DateTime'? new FormControl(def, this.validatorDateTime):
                        new FormControl(def, (prompt.required && !this.hiddenPromptChild.includes(prompt.id)) ? Validators.required : null);
                        this.givenTemplateForm.addControl(prompt.id.toString(), validator);
                    }
                }
            }
            this.buildExtra();
            this.setPromptChildrenValidators()
            const now = moment();
            this.order = data.order;
            this.admin = data.admin;
            this.lastGivenDateTime = this.getLastGivenDateTime();
            this.orderUsers = this.getOrderUsers(this.order);
        });
    // reinject modal datetime result in form
    this.eventSubscribe = this.givenAtEvent.subscribe( (obj: DateTimeId) => {
        // console.log('SUBSCRIBE TEMPLATE');
        this.givenTemplateForm.controls[obj.id].setValue(obj.dateTime);
      });
    }

    setPromptChildrenValidators() {
        // console.log('=================setPromptChildrenValidators: promptMap:', this.promptMap)

        for (const group in this.promptMap) {
            for (const promptParent in this.promptMap[group]) {
                for (const promptChild in this.promptMap[group][promptParent]) {
                    if (this.promptMap[group][promptParent][promptChild]) { //child prompt is required
                        const promptChildControl = this.givenTemplateForm.get(promptChild)

                        this.givenTemplateForm.get(promptParent).valueChanges.subscribe(
                            parent => {
                                if (parent) {  //checkbox chceked, value changed to true
                                    promptChildControl.setValidators([Validators.required])
                                } else {
                                    promptChildControl.setValidators(null)
                                }

                                promptChildControl.updateValueAndValidity()
                            })
                    }
                }
            }
        }
    }

    // build exta information to speed up
    buildExtra() {
        console.log('BUILDEXTRA');
        this.depth = [];
        this.margin = [];
        this.utility = [];
        this.parent = [];
        this.line = [];
        let inLine: boolean = false;
        let previous: number = -1;
        this.template.promptGroups.forEach(function(group, ig) {
            group.prompts.forEach(function(prompt, ip) {
                if (prompt.placeholderText!== null && prompt.placeholderText.includes('~~')) {
                    const l = this.template.promptGroups[ig].prompts[ip].placeholderText.split('~~');
                    this.template.promptGroups[ig].prompts[ip].placeholderText = l[0];
                    this.template.promptGroups[ig].prompts[ip].params = l.slice(1);
                } else {
                    this.template.promptGroups[ig].prompts[ip].params = [];
                }
                if (this.depth[prompt.id] == undefined) {
                    this.depth[prompt.id] = 0;
                }
                this.utility[prompt.id] = (prompt.type == 'Information') || (prompt.type == 'Label')
                    || (typeof prompt.promptChildren !== 'undefined' && prompt.promptChildren.length
                        && (typeof prompt.displayChildPromptsValue == 'undefined' || prompt.displayChildPromptsValue == null));
                if (prompt.promptChildren.length > 0 && prompt.displayChildPromptsValue !== null) {
                    for (let child of prompt.promptChildren) {
                        this.depth[child] = this.depth[prompt.id] + 1;
                        this.parent[child] = prompt.id;
                    }
                }
                if (prompt.isOnNewline) {
                    if (inLine) { // end a line
                        if (previous >= 0) this.line[previous] = -1;
                        inLine = false;
                    }
                } else {
                    if (!inLine) { // start of line
                        if (previous >= 0) this.line[previous] = 1
                        inLine = true;
                    } 
                }
                this.line[prompt.id] = inLine? 2: 0;
                previous = prompt.id;
            }, this);
            if (inLine && previous >=0) {// end the line
                this.line[previous] = -1;
            }
        }, this);
        for (let i in this.depth) {
            this.margin[i] = (this.depth[i] > 0)? (this.depth[i] - 1 ) * 5: 0;
        }
        // console.log('DEPTH', this.depth, this.line);
    }
    
    onSelectChoice(prompt: Prompt, choice: PromptChoice): void {
        // console.log('onSelectChoice: prompt: ', prompt, '  choice: ', choice)
        this.givenTemplateForm.controls[prompt.id].setValue(choice.choiceText);
    }

    // get the label of the current choice
    getChoice(prompt: Prompt): string {
        const value: string = this.givenTemplateForm.controls[prompt.id].value;
        if (value === null || value === "") return "";
        const choices: PromptChoice[] = prompt.promptChoices.filter((choice) => choice.choiceText == value);
        if (choices.length <= 0) return "";
        return choices[0].choiceText;
    }

    /* when clicking on the button to show datetime popup */
    onPickupTime(prompt: Prompt) {
        let minDateTime = '';
        let maxDateTime = '';
        if (prompt.params.length) {
            if (prompt.params.includes('afterTriage')) minDateTime = this.triageDateTime;
            if (prompt.params.includes('future')) minDateTime = moment().utcOffset(parseInt(this.siteUTCOffset)).format();
            if (prompt.params.includes('past')) maxDateTime = moment().utcOffset(parseInt(this.siteUTCOffset)).format();
            if (prompt.params.includes('afterGiven')) minDateTime = this.lastGivenDateTime;
        }
        let title = prompt.prompt;
        const n = title.indexOf('~~');
        if (n > 0) title = title.substring(0, n);
        this.modalService.open(
            'date-time-modal',
            {
              dateTime: this.givenTemplateForm.controls[prompt.id].value,
              event: this.givenAtEvent,
              format: 'iso',
              minDateTime: minDateTime,
              maxDateTime: maxDateTime,
              id: prompt.id.toString(),
              siteUTCOffset: this.siteUTCOffset
            },
            '<span class="bigger-bolder-blue">' + title +'</span>'
          );
    }
    getLastGivenDateTime() {
        let minDateTime: string = '';
        if (this.admin === null) {
            this.order.orderAdministrations.forEach(function(admin) {
                // TODO: perhaps check administration is not stopped
                if (minDateTime === '') {
                    minDateTime = admin.administrationDatetime;
                } else if (moment(admin.administrationDatetime).isBefore(moment(minDateTime))) {
                    minDateTime = admin.administrationDatetime;
                }
            });
            // console.log('MINDATETIMEORDER', minDateTime);
        } else {
            minDateTime = this.admin.administrationDatetime;
            // console.log('MINDATETIMEADMIN', minDateTime);
        }
        return minDateTime;
    }

    /* when changing the datetime in the input */
    onChangeTime(prompt: Prompt, event) {
        let time: string = event.target.value.trim();
        const pattern = /\d+:\d\d +\d+\/\d+\/\d\d\d\d/;
        const pattern2 = /\d+\/\d+\/\d\d\d\d +\d+:\d\d/;
        const pattern3 = /\d+:\d\d/;
        if (!time.match(pattern) && !time.match(pattern2) && !time.match(pattern3)) {
            this.givenTemplateForm.controls[prompt.id].setErrors({valid: false});
            // console.log('TIMEBADFORMAT',time);
            return;
        } 
        if (!time.includes(' ')) {
            time = time + ' ' + moment().utcOffset(parseInt(this.siteUTCOffset)).format(DATE_FORMAT);
        }
        
        if (time.match(pattern)) {
            // console.log('TIMEATTEMPT',time);
            const mo = moment(time + this.siteUTCOffset, TIME_FORMAT + ' ' + DATE_FORMAT + 'ZZ');
            //utcOffset(-6) 2021-01-27T10:40:00-05:00 => 2021-01-27T09:40:00-06:00
            if (mo.isValid()) {
                if (this.testDateTime(mo, prompt.params)) {
                    // console.log('TIME',mo.format());
                    this.givenTemplateForm.controls[prompt.id].setValue(mo.format());
                    this.givenTemplateForm.controls[prompt.id].setErrors(null);
                    return;
                }
            }
        } else if (time.match(pattern2)) {
            const mo2 = moment(time + this.siteUTCOffset, DATE_FORMAT + ' ' + TIME_FORMAT + 'ZZ');
            if (mo2.isValid()) {
                if (this.testDateTime(mo2, prompt.params)) {
                    // console.log('TIME2,mo2.format());')
                    this.givenTemplateForm.controls[prompt.id].setValue(mo2.format());
                    this.givenTemplateForm.controls[prompt.id].setErrors(null);
                    return;
                }
            }
        }
        // console.log('TIMEFAILS',time);
        this.givenTemplateForm.controls[prompt.id].setErrors({valid: false});
    }

    testDateTime(mo: moment.Moment, params: string[]) {
        if (params.includes('afterTriage') && mo.isBefore(this.triageDateTime)) return false;
        if (params.includes('future') && mo.isBefore(moment())) return false;
        if (params.includes('past') && mo.isAfter(moment())) return false;
        if (params.includes('afterGiven') && mo.isBefore(this.lastGivenDateTime)) return false;
        return true;
    }

    onCancel() {
        this.modalService.close('given-template-modal');
        this.givenTemplateForm.reset();
    }

    onSubmit() {
        let formObj= {};
        Object.entries(this.givenTemplateForm.controls).forEach(entry => {
            const [key, value] = entry;
            if (!this.isUtility(key)) {
                formObj[key] = value.value;
            }
        });
        console.log('SUBMIT',formObj);
        this.patientMedOrderService.updateRequest.emit(true);
        this.templateSubscribe = this.patientMedOrderService.postTemplate(this.template, formObj)
        .pipe(takeUntil(this.notifierTemplateSubscribe))
        .subscribe(data => {
            console.log('RESULT POST TEMPLATE', data);
            console.log('SUBSCRIBE TEMPLATE');
            if (typeof data.updatedOrder !== 'undefined' && data.updatedOrder !== null) {
                this.patientMedOrderService.refreshRequest.emit(data.updatedOrder);
            } else {
                this.patientMedOrderService.refreshRequest.emit(null);
            }

            //update patient current order
            this.patientMedOrderStoreService.fetchPatientMedOrder(this.patientStoreService.patientId)
        });
        this.modalService.close('given-template-modal');
        this.givenTemplateForm.reset();
    }

    // return the position in an all above group - 0 if not in an all above
    // or type=prompt return the id on the all of above
    inAllAbove(group: PromptGroup, prompt: Prompt, type: string): number {
        let ii: number = 0;
        for (let pr of group.prompts) {
            if (pr.type == "CheckBox") {
                // old version
                if (typeof pr.promptChoices !== 'undefined' && pr.promptChoices.length && (typeof pr.displayChildPromptsValue == 'undefined' || pr.displayChildPromptsValue == null)) { // All of the above checkbox
                    for (let choice of pr.promptChoices) {
                        ii = ii + 1;
                        if (+choice.choiceText == prompt.id) {
                            return (type == 'prompt')? pr.id: ii;
                        }
                    }
                    ii = 0;
                }
                // new version
                if (typeof pr.promptChildren !== 'undefined' && pr.promptChildren.length && (typeof pr.displayChildPromptsValue == 'undefined' || pr.displayChildPromptsValue == null)) {
                    for (let choice of pr.promptChildren) {
                        ii = ii + 1;
                        if (+choice == prompt.id) {
                            return (type == 'prompt')? pr.id: ii;
                        }
                    }
                    ii = 0;
                }
            }
        }
        return ii;
    }

    /* test if a prompt is a 'All of the above' or an Information*/
    isUtility(key: string): boolean {
        /*
        let is: boolean = false;
        this.template.promptGroups.forEach (promptGroup => {
            if (!is) {
                promptGroup.prompts.forEach(prompt => {
                    if (!is && prompt.id === +key) {
                        is = prompt.type == 'Information'
                            || (typeof prompt.promptChildren !== 'undefined' && prompt.promptChildren.length
                                && (typeof prompt.displayChildPromptsValue == 'undefined' || prompt.displayChildPromptsValue == null));
                    }
                });
            }
        });
        return is;
        */
       return this.utility[key];
    }

    isHidden(group: PromptGroup, prompt: Prompt): boolean {
        /*
        for (let pr of group.prompts) {
            // displayChildPromptsValue named before displayIf
            if (typeof pr.displayChildPromptsValue !== 'undefined' && pr.displayChildPromptsValue !== null && pr.displayChildPromptsValue !== '') {
                if (typeof pr.promptChoices !== 'undefined') {
                    for (let choice of pr.promptChoices) {
                        if (+choice.choiceText == prompt.id) {
                            // TODO more way to test DisplayChildPromptsValue - for now only cheched checkbox
                            return !this.givenTemplateForm.controls[pr.id].value;
                        }
                    }
                 }
                if (typeof pr.promptChildren !== 'undefined') {
                    for (let choice of pr.promptChildren) {
                        if (+choice == prompt.id) {
                            // TODO more way to test DisplayChildPromptsValue - for now only cheched checkbox
                            return !this.givenTemplateForm.controls[pr.id].value;
                        }
                    }
                }
            }
        }
        return false;
        */
       return this.depth[prompt.id] > 0 && !this.givenTemplateForm.controls[this.parent[prompt.id]].value
    }
    inHidden(group: PromptGroup, prompt: Prompt): boolean {
        /*
        for (let pr of group.prompts) {
            if (typeof pr.displayChildPromptsValue !== 'undefined' && pr.displayChildPromptsValue !== null && pr.displayChildPromptsValue !== '') {
                if (typeof pr.promptChoices !== 'undefined') {
                    for (let choice of pr.promptChoices) {
                        if (+choice.choiceText == prompt.id) {
                            return true;
                        }
                    }
                }   
                if (typeof pr.promptChildren !== 'undefined') {
                    for (let choice of pr.promptChildren) {
                        if (+choice == prompt.id) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
        */
       return this.depth[prompt.id] > 0;
    }

    // Check all the above
    onClickCheckbox(group: PromptGroup, prompt: Prompt, event: any): void {
        let checked = event.target.checked;
        let id: number;
        if (prompt.promptChildren?.length && (typeof prompt.displayChildPromptsValue == 'undefined' || prompt.displayChildPromptsValue == null || prompt.displayChildPromptsValue == '')) { // All of above
            for (let choice of prompt.promptChildren) {
                this.givenTemplateForm.controls[choice].setValue(checked);
            }
        } else if (!checked && (id = this.inAllAbove(group, prompt, 'prompt'))) { // uncheck the all of above button
            this.givenTemplateForm.controls[id].setValue(false);
        } else if (!checked && prompt.displayChildPromptsValue !== '') { // need to clear all the depending chlidren
            for (let choice of prompt.promptChildren) {
                console.log('fffff', this.givenTemplateForm.controls[choice].value)
                this.givenTemplateForm.controls[choice].setValue('');
            }
        }
    }

    // when clicking on a user
    onSelectNotified(user: User, prompt: Prompt) {
        // val contains the list of selected userIds separated by comma
        const val: string = this.givenTemplateForm.controls[prompt.id].value;
        let users: string[] = (val === null || val === '')? []: val.split(',');
        const i: number = users.indexOf(user.id.toString());
        if (i >= 0) {
            users = users.filter( value => +value !== user.id);
        } else {
            users.push(user.id.toString());
        }
        this.givenTemplateForm.controls[prompt.id].setValue(users.toString());
    }

    nbNotifiedUsers(prompt: Prompt): number {
        const val: string = this.givenTemplateForm.controls[prompt.id].value;
        return (val === null || val === '')? 0: val.replace(/[^,]/g, '').length + 1;
    }
    isNotifiedUser(user: User, prompt: Prompt) {
        const val: string = ',' + this.givenTemplateForm.controls[prompt.id].value + ',';
        const id = ',' + user.id.toString() + ',';
        return val.includes(id);
    }

    // return all the users that can be notified on an order
    getOrderUsers(order: Order): User[] {
        let users: User[] = [];
        if (typeof order.orderingPhysicianUser !== undefined && order.orderingPhysicianUser != null && order.orderingPhysicianUser.id != null) {
            users.push(order.orderingPhysicianUser);
        }
        if (typeof order.addUser !== undefined && order.addUser != null && order.addUser.id != null) {
            users.push(order.addUser);
        }
        order.orderAdministrations.forEach( admin => {
            if (typeof admin.administeringUser !== undefined && admin.administeringUser != null && admin.administeringUser.id != null) {
                users.push(admin.administeringUser);
            }
            if (typeof admin.stopUser !== undefined && admin.stopUser != null && admin.stopUser.id != null) {
                users.push(admin.stopUser);
            }
            if (typeof admin.acknowledgeUser !== undefined && admin.acknowledgeUser != null && admin.acknowledgeUser.id != null) {
                users.push(admin.acknowledgeUser);
            }
        });
        // TODO more users.....
        /// console.log('ORDERUSERS', users)
        return users.sort(function(a,b) { return a.lastName.localeCompare(b.lastName)})
            .filter(function(el,i,a) { return !i || el.id !== a[i-1].id});
    }

    validatorDate(control: AbstractControl): { [key: string]: any } | null {
        if (!control || control.value === undefined || control.value === null || control.value.trim() == '') return null;
        return moment(control.value, DATE_FORMAT).isValid()? null : {error: 'Invalid date'};
    }
    
    checkBoxValidator(control: AbstractControl): { [key: string]: any } | null {
        return (control.value === true) ? null : {error: 'Required input'};
    }

    trackByFn(index, item) {
        return item.id; // unique id corresponding to the item
    }

    ngOnDestroy() {
        console.log('NGONDESTROY TEMPLATE');
        // if (this.userSubscribe !== null) this.userSubscribe.unsubscribe();
        if (this.modalSubscribe !== null) this.modalSubscribe.unsubscribe();
        // if (this.templateSubscribe !== null) this.templateSubscribe.unsubscribe();
        this.notifierTemplateSubscribe.next();
        this.notifierTemplateSubscribe.complete();
        if (this.eventSubscribe !== null) this.eventSubscribe.unsubscribe();
    }
}
