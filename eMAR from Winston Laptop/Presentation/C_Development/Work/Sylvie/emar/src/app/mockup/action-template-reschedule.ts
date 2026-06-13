import { GivenTemplate } from '../interfaces/given-template';

export const ACTION_TEMPLATE_RESCHEDULE: GivenTemplate  = {
    "id": 2,
    "name": "Reschedule",
    "promptGroups": [
        {
            "id": 5,
            "name": "RescheduleDetails",
            "displayTitle": "",
            "prompts": [
                {
                    "id": 27,
                    "promptGroupId": 5,
                    "sequence": 1,
                    "prompt": "Reschedule To",
                    "type": "DateTime",
                    "default": 'now',
                    "required": true,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 28,
                    "promptGroupId": 5,
                    "sequence": 2,
                    "prompt": "All future administration times will be updated based on the previously entered frequency.",
                    "type": "Information",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                }
            ]
        },
    ],
    "active": true,
    "title": "Reschedule Order",
    "saveButtonText": "Confirm Reschedule",
    "cancelButtonText": "Cancel",
    "link": {
        "href": "http://localhost:51044/api/orders/1/actions/5/templates",
        "rel": "File Results of Template",
        "method": "POST"
    }
}
