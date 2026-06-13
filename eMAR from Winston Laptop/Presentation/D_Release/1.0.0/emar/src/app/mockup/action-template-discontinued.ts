import { GivenTemplate } from '../interfaces/given-template';

export const ACTION_TEMPLATE_DISCONTINUED: GivenTemplate  = {
    "id": 2,
    "name": "Discontinued",
    "promptGroups": [
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
    "title": "Discontinued Template",
    "saveButtonText": "Discontinued",
    "cancelButtonText": "Cancel",
    "link": {
        "href": "http://localhost:51044/api/Template/Discontinued"
    }
}
