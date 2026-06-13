import { GivenTemplate } from '../interfaces/given-template';

export const ACTION_TEMPLATE_CANCEL: GivenTemplate  = {
    "id": 2,
    "name": "CancelOrder",
    "promptGroups": [
        {
            "id": 4,
            "name": "CancelReason",
            "displayTitle": "Cancellation Reasons",
            "prompts": [
                {
                    "id": 22,
                    "promptGroupId": 4,
                    "sequence": 1,
                    "prompt": "Symptoms resolved",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 23,
                    "promptGroupId": 4,
                    "sequence": 2,
                    "prompt": "Patient refused",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 24,
                    "promptGroupId": 4,
                    "sequence": 3,
                    "prompt": "Change in medication plan",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                
            ]
        },
        {
            "id": 5,
            "name": "CancelDetails",
            "displayTitle": "",
            "prompts": [
                {
                    "id": 25,
                    "promptGroupId": 5,
                    "sequence": 1,
                    "prompt": "Notes",
                    "type": "MultiLineFreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 27,
                    "promptGroupId": 5,
                    "sequence": 2,
                    "prompt": "Canceled At",
                    "type": "DateTime",
                    "default": 'now',
                    "required": true,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 28,
                    "promptGroupId": 5,
                    "sequence": 3,
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
    "title": "Cancel Order",
    "saveButtonText": "Confirm Cancel",
    "cancelButtonText": "Cancel",
    "link": {
        "href": "http://localhost:51044/api/orders/1/actions/5/templates",
        "rel": "File Results of Template",
        "method": "POST"
    }
}
