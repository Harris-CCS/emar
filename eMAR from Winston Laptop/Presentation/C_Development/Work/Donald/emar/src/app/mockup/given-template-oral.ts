import { GivenTemplate } from '../interfaces/given-template';

export const GIVEN_TEMPLATE_ORAL: GivenTemplate  = {
    "id": 2,
    "name": "Oral",
    "promptGroups": [
        {
            "id": 1,
            "name": "Medication",
            "displayTitle": "Medication",
            "prompts": [
                {
                    "id": 1,
                    "promptGroupId": 1,
                    "sequence": 1,
                    "prompt": "Verbal order read back and verified",
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
                    "prompt": "Amount given",
                    "type": "FreeText",
                    "default": null,
                    "required": true,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 3,
                    "promptGroupId": 1,
                    "sequence": 3,
                    "prompt": "Administration of this medication is documented elsewhere in chart",
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
                    "prompt": "Site",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {
                            "id": 1,
                            "promptId": 4,
                            "sequence": 1,
                            "choiceText": "P.O."
                        },
                        {
                            "id": 2,
                            "promptId": 4,
                            "sequence": 2,
                            "choiceText": "S.L."
                        },
                        {
                            "id": 3,
                            "promptId": 4,
                            "sequence": 3,
                            "choiceText": "Buccal"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 105,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Medication crushed prior to administration",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 106,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Mixed in",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 1057,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Patient vomited during or soon after administration",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 5,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Correct patient, time, route, dose and medication confirmed prior to administration",
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
                    "prompt": "Patient advised of actions and side-effects prior to administration",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 7,
                    "promptGroupId": 1,
                    "sequence": 7,
                    "prompt": "Allergies confirmed and medications reviewed prior to administration",
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
                    "prompt": "All of the above",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 4,
                            "promptId": 8,
                            "sequence": 0,
                            "choiceText": "5"
                        },
                        {
                            "id": 5,
                            "promptId": 8,
                            "sequence": 0,
                            "choiceText": "6"
                        },
                        {
                            "id": 6,
                            "promptId": 8,
                            "sequence": 0,
                            "choiceText": "7"
                        }
                    ],
                    "isActive": true
                }
            ],
            "siteId": -1
        },
        {
            "id": 2,
            "name": "Emotional",
            "displayTitle": " ",
            "prompts": [
                {
                    "id": 9,
                    "promptGroupId": 2,
                    "sequence": 1,
                    "prompt": "Emotional support needed and given ",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 11,
                    "promptGroupId": 2,
                    "sequence": 2,
                    "prompt": "Tolerated Procedure",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 7,
                            "promptId": 11,
                            "sequence": 1,
                            "choiceText": "Well"
                        },
                        {
                            "id": 8,
                            "promptId": 11,
                            "sequence": 2,
                            "choiceText": "With Difficulty"
                        },
                        {
                            "id": 9,
                            "promptId": 11,
                            "sequence": 3,
                            "choiceText": "Uncooperative"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 12,
                    "promptGroupId": 2,
                    "sequence": 3,
                    "prompt": "Additional Staff Required",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 10,
                            "promptId": 12,
                            "sequence": 1,
                            "choiceText": "1 additional staff"
                        },
                        {
                            "id": 11,
                            "promptId": 12,
                            "sequence": 2,
                            "choiceText": "2 additional staff"
                        },
                        {
                            "id": 12,
                            "promptId": 12,
                            "sequence": 3,
                            "choiceText": "3 additional staff"
                        },
                        {
                            "id": 13,
                            "promptId": 12,
                            "sequence": 4,
                            "choiceText": "4 additional staff"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 13,
                    "promptGroupId": 2,
                    "sequence": 4,
                    "prompt": "Reason",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 14,
                            "promptId": 13,
                            "sequence": 1,
                            "choiceText": "Age"
                        },
                        {
                            "id": 15,
                            "promptId": 13,
                            "sequence": 2,
                            "choiceText": "Combative"
                        },
                        {
                            "id": 16,
                            "promptId": 13,
                            "sequence": 3,
                            "choiceText": "Confused"
                        },
                        {
                            "id": 17,
                            "promptId": 13,
                            "sequence": 4,
                            "choiceText": "Distraction"
                        },
                        {
                            "id": 18,
                            "promptId": 13,
                            "sequence": 5,
                            "choiceText": "Uncooperative"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 14,
                    "promptGroupId": 2,
                    "sequence": 5,
                    "prompt": "Administered by",
                    "type": "FreeText",
                    "default": "",
                    "placeholderText": "Myself",
                    "required": true,
                    "promptChoices": [],
                    "isActive": true
                }
            ],
            "siteId": -1
        },
        {
            "id": 3,
            "name": "Safety",
            "displayTitle": "Safety Interventions",
            "prompts": [
                {
                    "id": 15,
                    "promptGroupId": 3,
                    "sequence": 1,
                    "prompt": "Patient in position of comfort",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 16,
                    "promptGroupId": 3,
                    "sequence": 2,
                    "prompt": "Side rails up",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 17,
                    "promptGroupId": 3,
                    "sequence": 3,
                    "prompt": "Cart in lowest position",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 18,
                    "promptGroupId": 3,
                    "sequence": 4,
                    "prompt": "Family at bedside",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 19,
                    "promptGroupId": 3,
                    "sequence": 5,
                    "prompt": "All of the above",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 19,
                            "promptId": 19,
                            "sequence": 0,
                            "choiceText": "15"
                        },
                        {
                            "id": 20,
                            "promptId": 19,
                            "sequence": 0,
                            "choiceText": "16"
                        },
                        {
                            "id": 21,
                            "promptId": 19,
                            "sequence": 0,
                            "choiceText": "17"
                        },
                        {
                            "id": 22,
                            "promptId": 19,
                            "sequence": 0,
                            "choiceText": "18"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 20,
                    "promptGroupId": 3,
                    "sequence": 6,
                    "prompt": "Friend at bedside",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 21,
                    "promptGroupId": 3,
                    "sequence": 7,
                    "prompt": "Call light in reach",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 22,
                    "promptGroupId": 3,
                    "sequence": 8,
                    "prompt": "",
                    "type": "MultiLineFreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": false
                }
            ],
            "siteId": -1
        },
        {
            "id": 2,
            "name": "Generic",
            "displayTitle": "",
            "prompts": [
                {
                    "id": 38,
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
                    "id": 39,
                    "promptGroupId": 2,
                    "sequence": 9,
                    "prompt": "Given At",
                    "type": "DateTime",
                    "default": 'now',
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 40,
                    "promptGroupId": 2,
                    "sequence": 10,
                    "prompt": "Self Administered",
                    "type": "CheckBox",
                    "default": "",
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 41,
                    "promptGroupId": 2,
                    "sequence": 11,
                    "prompt": "Patient Supplied",
                    "type": "CheckBox",
                    "default": "",
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 42,
                    "promptGroupId": 2,
                    "sequence": 12,
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
    "title": "Oral Give Template",
    "saveButtonText": "Give",
    "link": {
        "href": "http://localhost:51044/api/Template/Oral"
    }
}
