import { GivenTemplate } from '../interfaces/given-template';

export const ACTION_TEMPLATE_MISSED_DOSE: GivenTemplate  = {
    "id": 2,
    "name": "Missed Dose",
    "promptGroups": [
        {
            "id": 1,
            "name": "Hold",
            "displayTitle": "Missed Reasons",
            "prompts": [
                {
                    "id": 1,
                    "promptGroupId": 1,
                    "sequence": 1,
                    "prompt": "Vital signs out of range",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 2,
                    "promptGroupId": 1,
                    "sequence": 2,
                    "prompt": "Vital signs stabilized",
                    "type": "Checkbox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 3,
                    "promptGroupId": 1,
                    "sequence": 3,
                    "prompt": "Patient refused",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 4,
                    "promptGroupId": 1,
                    "sequence": 4,
                    "prompt": "Pain controlled at present",
                    "type": "Checkbox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 5,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Symptoms controlled at present",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 6,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Awaiting order confirmation",
                    "type": "Checkbox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 7,
                    "promptGroupId": 1,
                    "sequence": 7,
                    "prompt": "Catheter/tube placement can not be confirmed",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 8,
                    "promptGroupId": 1,
                    "sequence": 8,
                    "prompt": "Administration route unavailable",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 9,
                    "promptGroupId": 1,
                    "sequence": 9,
                    "prompt": "Attending physician aware",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 10,
                    "promptGroupId": 1,
                    "sequence": 7,
                    "prompt": "Out of department",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
            ]
        },
        {
            "id": 2,
            "name": "Generic",
            "displayTitle": "",
            "prompts": [
                {
                    "id": 11,
                    "promptGroupId": 2,
                    "sequence": 8,
                    "prompt": "Notes",
                    "type": "MultiLineFreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 12,
                    "promptGroupId": 2,
                    "sequence": 9,
                    "prompt": "At",
                    "type": "DateTime",
                    "default": 'now',
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 13,
                    "promptGroupId": 2,
                    "sequence": 10,
                    "prompt": "",
                    "type": "Notify",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                }
            ]
        },
    ],
    "active": true,
    "title": "Missed Dose Template",
    "saveButtonText": "Missed Dose",
    "cancelButtonText": "Cancel",
    "link": {
        "href": "http://localhost:51044/api/Template/MissedDose"
    }
}