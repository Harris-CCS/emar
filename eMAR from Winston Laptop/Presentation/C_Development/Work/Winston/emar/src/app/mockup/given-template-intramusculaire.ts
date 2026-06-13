import { GivenTemplate } from '../interfaces/given-template';

export const GIVEN_TEMPLATE_INTRAMUSCULAIRE: GivenTemplate  = {
    "id": 1,
    "name": "Intramuscular",
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
                    "prompt": "IM (Not an antibiotic or immunization)",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "displayChildPromptsValue": "true",
                    "promptChoices": [
                        {
                            "choiceText": "4"
                        },
                        {
                            "choiceText": "5"
                        },
                        {
                            "choiceText": "6"
                        }
                    ],
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
                            "id": 41,
                            "promptId": 4,
                            "sequence": 41,
                            "choiceText": "Left deltoid"
                        },
                        {
                            "id": 42,
                            "promptId": 4,
                            "sequence": 42,
                            "choiceText": "Right deltoid"
                        },
                        {
                            "id": 43,
                            "promptId": 4,
                            "sequence": 43,
                            "choiceText": "Left buttock"
                        },
                        {
                            "id": 44,
                            "promptId": 4,
                            "sequence": 44,
                            "choiceText": "Right buttock"
                        },
                        {
                            "id": 45,
                            "promptId": 4,
                            "sequence": 45,
                            "choiceText": "Left hip"
                        },
                        {
                            "id": 46,
                            "promptId": 4,
                            "sequence": 46,
                            "choiceText": "Right hip"
                        },
                        {
                            "id": 47,
                            "promptId": 4,
                            "sequence": 47,
                            "choiceText": "Left thigh"
                        },
                        {
                            "id": 48,
                            "promptId": 4,
                            "sequence": 48,
                            "choiceText": "Right thigh "
                        },
                        {
                            "id": 49,
                            "promptId": 4,
                            "sequence": 49,
                            "choiceText": "Other IV sites - TODO"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 5,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Amount given",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 6,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Combined with",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 7,
                    "promptGroupId": 1,
                    "sequence": 7,
                    "prompt": "IM antibiotic",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "displayChildPromptsValue": "true",
                    "promptChoices": [
                        {
                            "choiceText": "51"
                        },
                        {
                            "choiceText": "52"
                        },
                        {
                            "choiceText": "53"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 51,
                    "promptGroupId": 1,
                    "sequence": 4,
                    "prompt": "Site",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {
                            "id": 410,
                            "promptId": 4,
                            "sequence": 41,
                            "choiceText": "Left deltoid"
                        },
                        {
                            "id": 420,
                            "promptId": 4,
                            "sequence": 42,
                            "choiceText": "Right deltoid"
                        },
                        {
                            "id": 430,
                            "promptId": 4,
                            "sequence": 43,
                            "choiceText": "Left buttock"
                        },
                        {
                            "id": 440,
                            "promptId": 4,
                            "sequence": 44,
                            "choiceText": "Right buttock"
                        },
                        {
                            "id": 450,
                            "promptId": 4,
                            "sequence": 45,
                            "choiceText": "Left hip"
                        },
                        {
                            "id": 460,
                            "promptId": 4,
                            "sequence": 46,
                            "choiceText": "Right hip"
                        },
                        {
                            "id": 470,
                            "promptId": 4,
                            "sequence": 47,
                            "choiceText": "Left thigh"
                        },
                        {
                            "id": 480,
                            "promptId": 4,
                            "sequence": 48,
                            "choiceText": "Right thigh "
                        },
                        {
                            "id": 490,
                            "promptId": 4,
                            "sequence": 49,
                            "choiceText": "Other IV sites - TODO"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 52,
                    "promptGroupId": 1,
                    "prompt": "Amount given",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 53,
                    "promptGroupId": 1,
                    "prompt": "Combined with",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 8,
                    "promptGroupId": 1,
                    "sequence": 8,
                    "prompt": "IM immunization",
                    "type": "CheckBox",
                    "default": null,
                    "displayChildPromptsValue": "true",
                    "required": false,
                    "promptChoices": [
                        {
                            "choiceText": "601"
                        },
                        {
                            "choiceText": "602"
                        },
                        {
                            "choiceText": "603"
                        },
                        {
                            "choiceText": "604"
                        },
                        {
                            "choiceText": "605"
                        },
                        {
                            "choiceText": "606"
                        },
                        {
                            "choiceText": "607"
                        },
                        {
                            "choiceText": "608"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 601,
                    "promptGroupId": 1,
                    "sequence": 4,
                    "prompt": "Site",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {
                            "id": 641,
                            "promptId": 4,
                            "sequence": 41,
                            "choiceText": "Left deltoid"
                        },
                        {
                            "id": 642,
                            "promptId": 4,
                            "sequence": 42,
                            "choiceText": "Right deltoid"
                        },
                        {
                            "id": 643,
                            "promptId": 4,
                            "sequence": 43,
                            "choiceText": "Left buttock"
                        },
                        {
                            "id": 644,
                            "promptId": 4,
                            "sequence": 44,
                            "choiceText": "Right buttock"
                        },
                        {
                            "id": 645,
                            "promptId": 4,
                            "sequence": 45,
                            "choiceText": "Left hip"
                        },
                        {
                            "id": 646,
                            "promptId": 4,
                            "sequence": 46,
                            "choiceText": "Right hip"
                        },
                        {
                            "id": 647,
                            "promptId": 4,
                            "sequence": 47,
                            "choiceText": "Left thigh"
                        },
                        {
                            "id": 648,
                            "promptId": 4,
                            "sequence": 48,
                            "choiceText": "Right thigh "
                        },
                        {
                            "id": 649,
                            "promptId": 4,
                            "sequence": 49,
                            "choiceText": "Other IV sites - TODO"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 602,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Amount given",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 603,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Combined with",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 604,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Date of publication",
                    "type": "Date",
                    "default": null,
                    "required": false,
                    "isActive": true  
                },
                {
                    "id": 605,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Name of publication",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "isActive": true  
                },
                {
                    "id": 606,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Manufacturer",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "isActive": true  
                },
                {
                    "id": 607,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Lot number",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "isActive": true  
                },
                {
                    "id": 608,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Expiration",
                    "type": "Date",
                    "default": null,
                    "required": false,
                    "isActive": true  
                }
            ],
            "siteId": -1
        },
        {
            "id": 4,
            "name": "Assessment",
            "displayTitle": "Pre-Administration Assessment",
            "prompts": [
                {
                    "id": 400,
                    "promptGroupId": 400,
                    "prompt": "O2 Stat",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {"id": 401,"promptId": 400,"choiceText": "100%"},
                        {"id": 402,"promptId": 400,"choiceText": "99%"},
                        {"id": 403,"promptId": 400,"choiceText": "98%"},
                        {"id": 404,"promptId": 400,"choiceText": "97%"},
                        {"id": 405,"promptId": 400,"choiceText": "96%"},
                        {"id": 406,"promptId": 400,"choiceText": "95%"},
                        {"id": 407,"promptId": 400,"choiceText": "94%"},
                        {"id": 408,"promptId": 400,"choiceText": "93%"},
                        {"id": 409,"promptId": 400,"choiceText": "92%"},
                        {"id": 410,"promptId": 400,"choiceText": "91%"},
                        {"id": 411,"promptId": 400,"choiceText": "90%"},
                        {"id": 412,"promptId": 400,"choiceText": "89%"},
                        {"id": 413,"promptId": 400,"choiceText": "88%"},
                        {"id": 414,"promptId": 400,"choiceText": "87%"},
                        {"id": 415,"promptId": 400,"choiceText": "86%"},
                        {"id": 416,"promptId": 400,"choiceText": "85%"},
                        {"id": 417,"promptId": 400,"choiceText": "84%"},
                        {"id": 418,"promptId": 400,"choiceText": "83%"},
                        {"id": 419,"promptId": 400,"choiceText": "82%"},
                        {"id": 420,"promptId": 400,"choiceText": "81%"},
                        {"id": 421,"promptId": 400,"choiceText": "80%"},
                        {"id": 422,"promptId": 400,"choiceText": ",80%"},
                    ],
                    "isActive": true
                },
                {
                    "id": 401,
                    "promptGroupId": 400,
                    "prompt": "O2 Amount",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                    ],
                    "isActive": true
                },
                {
                    "id": 402,
                    "promptGroupId": 400,
                    "prompt": "O2 Type",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                    ],
                    "isActive": true
                },
                {
                    "id": 403,
                    "promptGroupId": 400,
                    "prompt": "Rhythm",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                    ],
                    "isActive": true
                },
                {
                    "id": 404,
                    "promptGroupId": 400,
                    "prompt": "Ectopy",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                    ],
                    "isActive": true
                },
                {
                    "id": 405,
                    "promptGroupId": 400,
                    "prompt": "St changes",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [

                    ],
                    "isActive": true
                },
                {
                    "id": 406,
                    "promptGroupId": 400,
                    "prompt": "Correct patient, time, route, dose and medication confirmed prior to administration",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 407,
                    "promptGroupId": 400,
                    "prompt": "Patient advised of actions and side-effects prior to administration",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 408,
                    "promptGroupId": 400,
                    "prompt": "Allergies confirmed and medications reviewed prior to administration",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 409,
                    "promptGroupId": 400,
                    "prompt": "All of the above",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 4090,
                            "promptId": 19,
                            "sequence": 0,
                            "choiceText": "406"
                        },
                        {
                            "id": 4091,
                            "promptId": 19,
                            "sequence": 0,
                            "choiceText": "407"
                        },
                        {
                            "id": 4093,
                            "promptId": 19,
                            "sequence": 0,
                            "choiceText": "408"
                        },
                    ],
                    "isActive": true
                },
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
                    "placeholderText": "Myself",
                    "default": "",
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
                    "prompt": "Friend at beside",
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
    "title": "Intramusculaire Give Template",
    "saveButtonText": "Give",
    "link": {
        "href": "http://localhost:51044/api/Template/Give"
    }
}
