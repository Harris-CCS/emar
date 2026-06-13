import { GivenTemplate } from '../interfaces/given-template';

export const ACTION_TEMPLATE_DELETE: GivenTemplate  = {
    "id": 2,
    "name": "Delete",
    "promptGroups": [
        {
            "id": 1,
            "name": "Delete",
            "displayTitle": "Confirm delete",
            "prompts": [
            ]
        },
        {
            "id": 2,
            "name": "Generic",
            "displayTitle": " ",
            "prompts": [
            ]
        }
    ],
    "active": true,
    "title": "Delete Template",
    "saveButtonText": "Confirm delete",
    "cancelButtonText": "Cancel",
    "link": {
        "href": "http://localhost:51044/api/Template/Delete"
    }
}