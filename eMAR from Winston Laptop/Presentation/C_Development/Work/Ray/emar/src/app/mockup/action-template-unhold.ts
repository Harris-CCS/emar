import { GivenTemplate } from '../interfaces/given-template';

export const ACTION_TEMPLATE_UNHOLD: GivenTemplate  = {
    "id": 2,
    "name": "Unhold",
    "promptGroups": [
        {
            "id": 1,
            "name": "Unhold",
            "displayTitle": "Unhold Reasons",
            "prompts": [
                {
                    "id": 1,
                    "promptGroupId": 1,
                    "sequence": 1,
                    "prompt": "Vital signs improved",
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
                    "prompt": "Patient currently in department",
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
                    "prompt": "Patient consents",
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
                    "prompt": "Pain not controlled at present",
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
                    "prompt": "Received order confirmation",
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
                    "prompt": "Returned to department",
                    "type": "Checkbox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                }
            ]
        },
        {
            "id": 2,
            "name": "Generic",
            "displayTitle": "",
            "prompts": [
                {
                    "id": 8,
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
                    "id": 9,
                    "promptGroupId": 2,
                    "sequence": 9,
                    "prompt": "Hold At",
                    "type": "DateTime",
                    "default": 'now',
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 10,
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
    "title": "Unhold Template",
    "saveButtonText": "Unhold",
    "cancelButtonText": "Cancel",
    "link": {
        "href": "http://localhost:51044/api/Template/Unhold"
    }
}
