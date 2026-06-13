import { GivenTemplate } from '../interfaces/given-template';

export const ACTION_TEMPLATE_FOLLOWUP: GivenTemplate  = {
    "id": 9,
    "name": "FollowUp",
    "promptGroups": [
        
        
        // {
        //     "id": 2,
        //     "name": "Emotional",
        //     "displayTitle": "",
        //     "sequence": 3,
        //     "prompts": [
        //         {
        //             "id": 9,
        //             "promptGroupId": 2,
        //             "sequence": 1,
        //             "prompt": "Emotional support needed and given ",
        //             "type": "CheckBox",
        //             "default": null,
        //             "required": false,
        //             "promptChildren": [],
        //             "promptChoices": [],
        //             "isActive": true
        //         },
        //         {
        //             "id": 10,
        //             "promptGroupId": 2,
        //             "sequence": 2,
        //             "prompt": "Tolerated Procedure",
        //             "type": "DropDownListBox",
        //             "default": null,
        //             "required": false,
        //             "promptChildren": [],
        //             "promptChoices": [
        //                 {
        //                     "id": 7,
        //                     "promptId": 10,
        //                     "sequence": 1,
        //                     "choiceText": "Well"
        //                 },
        //                 {
        //                     "id": 8,
        //                     "promptId": 10,
        //                     "sequence": 2,
        //                     "choiceText": "With Difficulty"
        //                 },
        //                 {
        //                     "id": 9,
        //                     "promptId": 10,
        //                     "sequence": 3,
        //                     "choiceText": "Uncooperative"
        //                 }
        //             ],
        //             "isActive": true
        //         },
        //         {
        //             "id": 11,
        //             "promptGroupId": 2,
        //             "sequence": 3,
        //             "prompt": "Additional Staff Required",
        //             "type": "DropDownListBox",
        //             "default": null,
        //             "required": false,
        //             "promptChildren": [],
        //             "promptChoices": [
        //                 {
        //                     "id": 10,
        //                     "promptId": 11,
        //                     "sequence": 1,
        //                     "choiceText": "1 additional staff"
        //                 },
        //                 {
        //                     "id": 11,
        //                     "promptId": 11,
        //                     "sequence": 2,
        //                     "choiceText": "2 additional staff"
        //                 },
        //                 {
        //                     "id": 12,
        //                     "promptId": 11,
        //                     "sequence": 3,
        //                     "choiceText": "3 additional staff"
        //                 },
        //                 {
        //                     "id": 13,
        //                     "promptId": 11,
        //                     "sequence": 4,
        //                     "choiceText": "4 additional staff"
        //                 }
        //             ],
        //             "isActive": true
        //         },
        //         {
        //             "id": 12,
        //             "promptGroupId": 2,
        //             "sequence": 4,
        //             "prompt": "Reason",
        //             "type": "DropDownListBox",
        //             "default": null,
        //             "required": false,
        //             "promptChildren": [],
        //             "promptChoices": [
        //                 {
        //                     "id": 14,
        //                     "promptId": 12,
        //                     "sequence": 1,
        //                     "choiceText": "Age"
        //                 },
        //                 {
        //                     "id": 15,
        //                     "promptId": 12,
        //                     "sequence": 2,
        //                     "choiceText": "Combative"
        //                 },
        //                 {
        //                     "id": 16,
        //                     "promptId": 12,
        //                     "sequence": 3,
        //                     "choiceText": "Confused"
        //                 },
        //                 {
        //                     "id": 17,
        //                     "promptId": 12,
        //                     "sequence": 4,
        //                     "choiceText": "Distraction"
        //                 },
        //                 {
        //                     "id": 18,
        //                     "promptId": 12,
        //                     "sequence": 5,
        //                     "choiceText": "Uncooperative"
        //                 }
        //             ],
        //             "isActive": true
        //         },
        //         {
        //             "id": 13,
        //             "promptGroupId": 2,
        //             "sequence": 5,
        //             "prompt": "Administered by",
        //             "type": "FreeText",
        //             "default": null,
        //             "required": false,
        //             "promptChildren": [],
        //             "promptChoices": [],
        //             "isActive": true
        //         }
        //     ]
        // },
        {
            "id": 18,
            "name": "GeneralAssessment",
            "displayTitle": "General Assessment",
            "prompts": [
                {
                    "id": 388,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Symptoms",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 41,
                            "promptId": 88,
                            "sequence": 41,
                            "choiceText": "Si"
                        },
                        {
                            "id": 42,
                            "promptId": 88,
                            "sequence": 42,
                            "choiceText": "?"
                        },
                        {
                            "id": 43,
                            "promptId": 88,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 389,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Pain",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 390,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Heart rate",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 41,
                            "promptId": 88,
                            "sequence": 41,
                            "choiceText": "Oui"
                        },
                        {
                            "id": 42,
                            "promptId": 88,
                            "sequence": 42,
                            "choiceText": "?"
                        },
                        {
                            "id": 43,
                            "promptId": 88,
                            "sequence": 43,
                            "choiceText": "Non"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 391,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Blood pressure",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 392,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Temperature",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 41,
                            "promptId": 88,
                            "sequence": 41,
                            "choiceText": "Si"
                        },
                        {
                            "id": 42,
                            "promptId": 88,
                            "sequence": 42,
                            "choiceText": "?"
                        },
                        {
                            "id": 43,
                            "promptId": 88,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 393,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Nausea",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 394,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Vomiting",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 395,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Rash",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 396,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Respiratory rate",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 397,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Respiratory effort",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 398,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Breath sounds improved",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 399,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Mental status",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "Yes"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "UNK"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "No"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 400,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Urine output",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 44,
                            "promptId": 89,
                            "sequence": 41,
                            "choiceText": "有"
                        },
                        {
                            "id": 45,
                            "promptId": 89,
                            "sequence": 42,
                            "choiceText": "?"
                        },
                        {
                            "id": 46,
                            "promptId": 89,
                            "sequence": 43,
                            "choiceText": "沒有"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 401,
                    "promptGroupId": 18,
                    "sequence": 1,
                    "prompt": "Constipation",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 544,
                            "promptId": 401,
                            "sequence": 41,
                            "choiceText": "はい"
                        },
                        {
                            "id": 545,
                            "promptId": 401,
                            "sequence": 42,
                            "choiceText": "?"
                        },
                        {
                            "id": 546,
                            "promptId": 401,
                            "sequence": 43,
                            "choiceText": "番号"
                        }
                    ],
                    "isActive": true
                }
            ]
        },
        {
            "id": 11,
            "name": "siteInspection",
            "displayTitle": "Site Inspection",
            "prompts": [
                {
                    "id": 8,
                    "promptGroupId": 11,
                    "sequence": 1,
                    "prompt": "Swelling",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 123,
                            "promptId": 8,
                            "sequence": 41,
                            "choiceText": "▲"
                        },
                        {
                            "id": 124,
                            "promptId": 8,
                            "sequence": 42,
                            "choiceText": "😐"
                        },
                        {
                            "id": 125,
                            "promptId": 8,
                            "sequence": 43,
                            "choiceText": "▼"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 9,
                    "promptGroupId": 11,
                    "sequence": 2,
                    "prompt": "Drainage",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 123,
                            "promptId": 8,
                            "sequence": 41,
                            "choiceText": "▲"
                        },
                        {
                            "id": 124,
                            "promptId": 8,
                            "sequence": 42,
                            "choiceText": "😐"
                        },
                        {
                            "id": 125,
                            "promptId": 8,
                            "sequence": 43,
                            "choiceText": "▼"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 10,
                    "promptGroupId": 11,
                    "sequence": 3,
                    "prompt": "Bleeding",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 123,
                            "promptId": 10,
                            "sequence": 41,
                            "choiceText": "▲"
                        },
                        {
                            "id": 124,
                            "promptId": 10,
                            "sequence": 42,
                            "choiceText": "😐"
                        },
                        {
                            "id": 125,
                            "promptId": 10,
                            "sequence": 43,
                            "choiceText": "▼"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 11,
                    "promptGroupId": 11,
                    "sequence": 3,
                    "prompt": "Bruising",
                    "type": "threeStateRadioButton",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 123,
                            "promptId": 10,
                            "sequence": 41,
                            "choiceText": "▲"
                        },
                        {
                            "id": 124,
                            "promptId": 10,
                            "sequence": 42,
                            "choiceText": "😐"
                        },
                        {
                            "id": 125,
                            "promptId": 10,
                            "sequence": 43,
                            "choiceText": "▼"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 12,
                    "promptGroupId": 11,
                    "sequence": 2,
                    "prompt": "No S/S of allergic reaction",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 13,
                    "promptGroupId": 11,
                    "sequence": 3,
                    "prompt": "Dressing applied",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 14,
                    "promptGroupId": 11,
                    "sequence": 4,
                    "prompt": "Warm compress applied",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 15,
                    "promptGroupId": 11,
                    "sequence": 4,
                    "prompt": "All normal",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                }
            ]
        },
        {
            "id": 1,
            "name": "IV",
            "displayTitle": "IV",
            "prompts": [
                {
                    "id": 22,
                    "promptGroupId": 1,
                    "sequence": 1,
                    "prompt": "Titrating to patient response",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "displayChildPromptsValue": "true",
                    "promptChoices": [
                        {
                            "choiceText": "145"
                        },
                        {
                            "choiceText": "146"
                        }
                    ],
                    "isActive": true
                },
                // {
                //     "id": 144,
                //     "promptGroupId": 1,
                //     "sequence": 1,
                //     "prompt": "Site",
                //     "type": "DropDownListBox",
                //     "default": null,
                //     "required": true,
                //     "promptChoices": [
                //         {
                //             "id": 41,
                //             "promptId": 4,
                //             "sequence": 41,
                //             "choiceText": "Left deltoid"
                //         },
                //         {
                //             "id": 42,
                //             "promptId": 4,
                //             "sequence": 42,
                //             "choiceText": "Right deltoid"
                //         },
                //         {
                //             "id": 43,
                //             "promptId": 4,
                //             "sequence": 43,
                //             "choiceText": "Left buttock"
                //         },
                //         {
                //             "id": 44,
                //             "promptId": 4,
                //             "sequence": 44,
                //             "choiceText": "Right buttock"
                //         },
                //         {
                //             "id": 45,
                //             "promptId": 4,
                //             "sequence": 45,
                //             "choiceText": "Left hip"
                //         },
                //         {
                //             "id": 46,
                //             "promptId": 4,
                //             "sequence": 46,
                //             "choiceText": "Right hip"
                //         },
                //         {
                //             "id": 47,
                //             "promptId": 4,
                //             "sequence": 47,
                //             "choiceText": "Left thigh"
                //         },
                //         {
                //             "id": 48,
                //             "promptId": 4,
                //             "sequence": 48,
                //             "choiceText": "Right thigh "
                //         },
                //         {
                //             "id": 49,
                //             "promptId": 4,
                //             "sequence": 49,
                //             "choiceText": "Other IV sites - TODO"
                //         }
                //     ],
                //     "isActive": true
                // },
                {
                    "id": 145,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Dose increased",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 146,
                    "promptGroupId": 1,
                    "sequence": 6,
                    "prompt": "Dose decreased",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                }
            ]
        },
        {
            "id": 1,
            "name": "StopTime",
            "displayTitle": "Stop Time",
            "prompts": [
                {
                    "id": 2,
                    "promptGroupId": 1,
                    "sequence": 1,
                    "prompt": "Infusion Discontinued",
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
                    "prompt": "",
                    "type": "DateTime",
                    "default": '2020-12-22T09:00:30.6634061-06:00',
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 5,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Totaling - HOURS",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {
                            "id": 41,
                            "promptId": 4,
                            "sequence": 41,
                            "choiceText": "1 hour"
                        },
                        {
                            "id": 42,
                            "promptId": 4,
                            "sequence": 42,
                            "choiceText": "2 hours"
                        },
                        {
                            "id": 43,
                            "promptId": 4,
                            "sequence": 43,
                            "choiceText": "3 hours"
                        },
                        {
                            "id": 44,
                            "promptId": 4,
                            "sequence": 44,
                            "choiceText": "5 hours"
                        },
                        {
                            "id": 45,
                            "promptId": 4,
                            "sequence": 45,
                            "choiceText": "10 hours"
                        },
                        {
                            "id": 46,
                            "promptId": 4,
                            "sequence": 46,
                            "choiceText": "15 hours"
                        },
                        {
                            "id": 47,
                            "promptId": 4,
                            "sequence": 47,
                            "choiceText": "20 hours"
                        },
                        {
                            "id": 48,
                            "promptId": 4,
                            "sequence": 48,
                            "choiceText": "> 23 hours"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 6,
                    "promptGroupId": 1,
                    "sequence": 5,
                    "prompt": "Totaling - MINS",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {
                            "id": 49,
                            "promptId": 6,
                            "sequence": 41,
                            "choiceText": "5 minutes"
                        },
                        {
                            "id": 50,
                            "promptId": 6,
                            "sequence": 42,
                            "choiceText": "10 minutes"
                        },
                        {
                            "id": 43,
                            "promptId": 6,
                            "sequence": 43,
                            "choiceText": "15 minutes"
                        },
                        {
                            "id": 51,
                            "promptId": 4,
                            "sequence": 44,
                            "choiceText": "20 minutes"
                        },
                        {
                            "id": 52,
                            "promptId": 6,
                            "sequence": 45,
                            "choiceText": "25 minutes"
                        },
                        {
                            "id": 53,
                            "promptId": 6,
                            "sequence": 46,
                            "choiceText": "30 minutes"
                        },
                        {
                            "id": 54,
                            "promptId": 6,
                            "sequence": 47,
                            "choiceText": "35 minutes"
                        },
                        {
                            "id": 55,
                            "promptId": 4,
                            "sequence": 48,
                            "choiceText": "40 minutes"
                        },
                        {
                            "id": 56,
                            "promptId": 6,
                            "sequence": 49,
                            "choiceText": "45 minutes"
                        },
                        {
                            "id": 57,
                            "promptId": 6,
                            "sequence": 50,
                            "choiceText": "50 minutes"
                        },
                        {
                            "id": 58,
                            "promptId": 6,
                            "sequence": 51,
                            "choiceText": "55 minutes"
                        }
                    ],
                    "isActive": true
                },
                // {
                //     "id": 6,
                //     "promptGroupId": 1,
                //     "sequence": 6,
                //     "prompt": "Combined with",
                //     "type": "FreeText",
                //     "default": null,
                //     "required": false,
                //     "promptChoices": [],
                //     "isActive": true
                // },
                {
                    "id": 1,
                    "promptGroupId": 1,
                    "sequence": 2,
                    "prompt": "Stop time unknown",
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
                    "prompt": "Continued upon transfer",
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
                    "sequence": 8,
                    "prompt": "",
                    "type": "DateTime",
                    "default": '2020-12-22T09:00:30.6634061-06:00',
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 52,
                    "promptGroupId": 1,
                    "sequence": 9,
                    "prompt": "Totaling - HOURS",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {
                            "id": 41,
                            "promptId": 52,
                            "sequence": 41,
                            "choiceText": "1 hour"
                        },
                        {
                            "id": 42,
                            "promptId": 52,
                            "sequence": 42,
                            "choiceText": "2 hours"
                        },
                        {
                            "id": 43,
                            "promptId": 52,
                            "sequence": 43,
                            "choiceText": "3 hours"
                        },
                        {
                            "id": 44,
                            "promptId": 52,
                            "sequence": 44,
                            "choiceText": "5 hours"
                        },
                        {
                            "id": 45,
                            "promptId": 52,
                            "sequence": 45,
                            "choiceText": "10 hours"
                        },
                        {
                            "id": 46,
                            "promptId": 52,
                            "sequence": 46,
                            "choiceText": "15 hours"
                        },
                        {
                            "id": 47,
                            "promptId": 52,
                            "sequence": 47,
                            "choiceText": "20 hours"
                        },
                        {
                            "id": 48,
                            "promptId": 52,
                            "sequence": 48,
                            "choiceText": "> 23 hours"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 53,
                    "promptGroupId": 1,
                    "sequence": 10,
                    "prompt": "Totaling - MINS",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": true,
                    "promptChoices": [
                        {
                            "id": 60,
                            "promptId": 53,
                            "sequence": 41,
                            "choiceText": "5 minutes"
                        },
                        {
                            "id": 61,
                            "promptId": 53,
                            "sequence": 42,
                            "choiceText": "10 minutes"
                        },
                        {
                            "id": 62,
                            "promptId": 53,
                            "sequence": 43,
                            "choiceText": "15 minutes"
                        },
                        {
                            "id": 63,
                            "promptId": 4,
                            "sequence": 44,
                            "choiceText": "20 minutes"
                        },
                        {
                            "id": 64,
                            "promptId": 53,
                            "sequence": 45,
                            "choiceText": "25 minutes"
                        },
                        {
                            "id": 65,
                            "promptId": 53,
                            "sequence": 453,
                            "choiceText": "30 minutes"
                        },
                        {
                            "id": 66,
                            "promptId": 53,
                            "sequence": 47,
                            "choiceText": "35 minutes"
                        },
                        {
                            "id": 67,
                            "promptId": 4,
                            "sequence": 48,
                            "choiceText": "40 minutes"
                        },
                        {
                            "id": 68,
                            "promptId": 53,
                            "sequence": 49,
                            "choiceText": "45 minutes"
                        },
                        {
                            "id": 69,
                            "promptId": 53,
                            "sequence": 50,
                            "choiceText": "50 minutes"
                        },
                        {
                            "id": 70,
                            "promptId": 53,
                            "sequence": 51,
                            "choiceText": "55 minutes"
                        }
                    ],
                    "isActive": true
                },
                // {
                //     "id": 8,
                //     "promptGroupId": 1,
                //     "sequence": 8,
                //     "prompt": "IM immunization",
                //     "type": "CheckBox",
                //     "default": null,
                //     "displayChildPromptsValue": "true",
                //     "required": false,
                //     "promptChoices": [
                //         {
                //             "choiceText": "601"
                //         },
                //         {
                //             "choiceText": "602"
                //         },
                //         {
                //             "choiceText": "603"
                //         },
                //         {
                //             "choiceText": "604"
                //         },
                //         {
                //             "choiceText": "605"
                //         },
                //         {
                //             "choiceText": "606"
                //         },
                //         {
                //             "choiceText": "607"
                //         },
                //         {
                //             "choiceText": "608"
                //         }
                //     ],
                //     "isActive": true
                // },
                // {
                //     "id": 601,
                //     "promptGroupId": 1,
                //     "sequence": 4,
                //     "prompt": "Site",
                //     "type": "DropDownListBox",
                //     "default": null,
                //     "required": true,
                //     "promptChoices": [
                //         {
                //             "id": 641,
                //             "promptId": 4,
                //             "sequence": 41,
                //             "choiceText": "Left deltoid"
                //         },
                //         {
                //             "id": 642,
                //             "promptId": 4,
                //             "sequence": 42,
                //             "choiceText": "Right deltoid"
                //         },
                //         {
                //             "id": 643,
                //             "promptId": 4,
                //             "sequence": 43,
                //             "choiceText": "Left buttock"
                //         },
                //         {
                //             "id": 644,
                //             "promptId": 4,
                //             "sequence": 44,
                //             "choiceText": "Right buttock"
                //         },
                //         {
                //             "id": 645,
                //             "promptId": 4,
                //             "sequence": 45,
                //             "choiceText": "Left hip"
                //         },
                //         {
                //             "id": 646,
                //             "promptId": 4,
                //             "sequence": 46,
                //             "choiceText": "Right hip"
                //         },
                //         {
                //             "id": 647,
                //             "promptId": 4,
                //             "sequence": 47,
                //             "choiceText": "Left thigh"
                //         },
                //         {
                //             "id": 648,
                //             "promptId": 4,
                //             "sequence": 48,
                //             "choiceText": "Right thigh "
                //         },
                //         {
                //             "id": 649,
                //             "promptId": 4,
                //             "sequence": 49,
                //             "choiceText": "Other IV sites - TODO"
                //         }
                //     ],
                //     "isActive": true
                // },
                // {
                //     "id": 602,
                //     "promptGroupId": 1,
                //     "sequence": 5,
                //     "prompt": "Amount given",
                //     "type": "FreeText",
                //     "default": null,
                //     "required": false,
                //     "promptChoices": [],
                //     "isActive": true
                // },
                // {
                //     "id": 603,
                //     "promptGroupId": 1,
                //     "sequence": 6,
                //     "prompt": "Combined with",
                //     "type": "FreeText",
                //     "default": null,
                //     "required": false,
                //     "promptChoices": [],
                //     "isActive": true
                // },
                // {
                //     "id": 604,
                //     "promptGroupId": 1,
                //     "sequence": 6,
                //     "prompt": "Date of publication",
                //     "type": "Date",
                //     "default": null,
                //     "required": false,
                //     "isActive": true  
                // },
                // {
                //     "id": 605,
                //     "promptGroupId": 1,
                //     "sequence": 6,
                //     "prompt": "Name of publication",
                //     "type": "FreeText",
                //     "default": null,
                //     "required": false,
                //     "isActive": true  
                // },
                // {
                //     "id": 606,
                //     "promptGroupId": 1,
                //     "sequence": 6,
                //     "prompt": "Manufacturer",
                //     "type": "FreeText",
                //     "default": null,
                //     "required": false,
                //     "isActive": true  
                // },
                // {
                //     "id": 607,
                //     "promptGroupId": 1,
                //     "sequence": 6,
                //     "prompt": "Lot number",
                //     "type": "FreeText",
                //     "default": null,
                //     "required": false,
                //     "isActive": true  
                // },
                // {
                //     "id": 608,
                //     "promptGroupId": 1,
                //     "sequence": 6,
                //     "prompt": "Expiration",
                //     "type": "Date",
                //     "default": null,
                //     "required": false,
                //     "isActive": true  
                // }
            ],
            "siteId": -1
        },
        {
            "id": 3,
            "name": "Safety",
            "displayTitle": "Safety Interventions",
            "sequence": 4,
            "prompts": [
                {
                    "id": 13,
                    "promptGroupId": 3,
                    "sequence": 1,
                    "prompt": "Advised no ambulate w/o help",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    // "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 14,
                    "promptGroupId": 3,
                    "sequence": 2,
                    "prompt": "Patient in position of comfort",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    // "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 15,
                    "promptGroupId": 3,
                    "sequence": 3,
                    "prompt": "Side rails up",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    // "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 16,
                    "promptGroupId": 3,
                    "sequence": 4,
                    "prompt": "Cart in lowest position",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    // "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 17,
                    "promptGroupId": 3,
                    "sequence": 5,
                    "prompt": "Call light in reach",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    // "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 18,
                    "promptGroupId": 3,
                    "sequence": 6,
                    "prompt": "All of the above",
                    // "type": "CheckBoxCheckChildren",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    // "promptChildren": [
                    //     13,
                    //     14,
                    //     15,
                    //     16,
                    //     17
                    // ],
                    "promptChoices": [
                        {
                            "id": 19,
                            "promptId": 18,
                            "sequence": 0,
                            "choiceText": "13"
                        },
                        {
                            "id": 20,
                            "promptId": 18,
                            "sequence": 0,
                            "choiceText": "14"
                        },
                        {
                            "id": 21,
                            "promptId": 18,
                            "sequence": 0,
                            "choiceText": "15"
                        },
                        {
                            "id": 22,
                            "promptId": 18,
                            "sequence": 0,
                            "choiceText": "16"
                        },
                        {
                            "id": 23,
                            "promptId": 18,
                            "sequence": 0,
                            "choiceText": "17"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 19,
                    "promptGroupId": 3,
                    "sequence": 7,
                    "prompt": "Emotional support needed and given",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 20,
                    "promptGroupId": 3,
                    "sequence": 8,
                    "prompt": "Family at bedside",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 21,
                    "promptGroupId": 3,
                    "sequence": 9,
                    "prompt": "Friend at beside",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 22,
                    "promptGroupId": 3,
                    "sequence": 10,
                    "prompt": "Call light in reach",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 23,
                    "promptGroupId": 3,
                    "sequence": 11,
                    "prompt": "Attending physician aware",
                    "type": "CheckBox",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 24,
                    "promptGroupId": 3,
                    "sequence": 12,
                    "prompt": "Physican Name:",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                }
            ]
        },
        {
            "id": 23,
            "name": "vitalSigns",
            "displayTitle": "Vital Signs",
            "sequence": 4,
            "prompts": [
                {
                    "id": 35,
                    "promptGroupId": 23,
                    "sequence": 1,
                    "prompt": "BP (Systolic)",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 36,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "BP (Diastolic)",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 37,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 37,
                            "sequence": 1,
                            "choiceText": "Well"
                        },
                        {
                            "id": 78,
                            "promptId": 37,
                            "sequence": 2,
                            "choiceText": "With Difficulty"
                        },
                        {
                            "id": 79,
                            "promptId": 37,
                            "sequence": 3,
                            "choiceText": "Uncooperative"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 38,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 38,
                            "sequence": 1,
                            "choiceText": "Left Arm"
                        },
                        {
                            "id": 78,
                            "promptId": 38,
                            "sequence": 2,
                            "choiceText": "Right Arm"
                        },
                        {
                            "id": 79,
                            "promptId": 38,
                            "sequence": 3,
                            "choiceText": "Left Butt"
                        }
                    ],
                    "isActive": true
                },


                // {
                //     "id": 39,
                //     "promptGroupId": 23,
                //     "sequence": 1,
                //     "prompt": "BP (Systolic)",
                //     "type": "FreeText",
                //     "default": null,
                //     "required": false,
                //     "promptChoices": [],
                //     "isActive": true
                // },
                {
                    "id": 40,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "Pulse",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 41,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 41,
                            "sequence": 1,
                            "choiceText": "Sitting"
                        },
                        {
                            "id": 78,
                            "promptId": 41,
                            "sequence": 2,
                            "choiceText": "Standing"
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 42,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 42,
                            "sequence": 1,
                            "choiceText": "PulseCheck #1"
                        },
                        {
                            "id": 78,
                            "promptId": 42,
                            "sequence": 2,
                            "choiceText": "PulseCheck #2"
                        },
                        {
                            "id": 79,
                            "promptId": 42,
                            "sequence": 3,
                            "choiceText": "PulseCheck #3"
                        }
                    ],
                    "isActive": true
                },


                {
                    "id": 43,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "Temperature",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 44,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 44,
                            "sequence": 1,
                            "choiceText": "Oral"
                        },
                        {
                            "id": 78,
                            "promptId": 44,
                            "sequence": 2,
                            "choiceText": "Rectal",
                        },
                        {
                            "id": 79,
                            "promptId": 44,
                            "sequence": 3,
                            "choiceText": "Ear",
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 45,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 45,
                            "sequence": 1,
                            "choiceText": "Sitting"
                        },
                        {
                            "id": 78,
                            "promptId": 45,
                            "sequence": 2,
                            "choiceText": "Lying down"
                        }
                    ],
                    "isActive": true
                },



                {
                    "id": 46,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "MAP",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 47,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 47,
                            "sequence": 1,
                            "choiceText": "MAP #1"
                        },
                        {
                            "id": 78,
                            "promptId": 47,
                            "sequence": 2,
                            "choiceText": "MAP #2",
                        },
                        {
                            "id": 79,
                            "promptId": 47,
                            "sequence": 3,
                            "choiceText": "MAP #3>",
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 48,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 48,
                            "sequence": 1,
                            "choiceText": "Clear and acceptable"
                        },
                        {
                            "id": 78,
                            "promptId": 48,
                            "sequence": 2,
                            "choiceText": "MAP 1%"
                        },
                        {
                            "id": 79,
                            "promptId": 48,
                            "sequence": 2,
                            "choiceText": "MAP > 100%"
                        }
                    ],
                    "isActive": true
                },


                

                {
                    "id": 49,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "Respiratory",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 50,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 50,
                            "sequence": 1,
                            "choiceText": "Room Air"
                        },
                        {
                            "id": 78,
                            "promptId": 50,
                            "sequence": 2,
                            "choiceText": "Room Vacuum",
                        },
                        {
                            "id": 79,
                            "promptId": 50,
                            "sequence": 3,
                            "choiceText": "Room Neg Pressure",
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 51,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 51,
                            "sequence": 1,
                            "choiceText": "RESP NO"
                        },
                        {
                            "id": 78,
                            "promptId": 51,
                            "sequence": 2,
                            "choiceText": "RESP YES"
                        }
                    ],
                    "isActive": true
                },





                {
                    "id": 52,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "PAIN",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 53,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 53,
                            "sequence": 1,
                            "choiceText": "Continuous"
                        },
                        {
                            "id": 78,
                            "promptId": 53,
                            "sequence": 2,
                            "choiceText": "Pain Attributes",
                        },
                        {
                            "id": 79,
                            "promptId": 53,
                            "sequence": 3,
                            "choiceText": "Not in pain",
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 54,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 54,
                            "sequence": 1,
                            "choiceText": "Pain > 10%"
                        },
                        {
                            "id": 78,
                            "promptId": 54,
                            "sequence": 2,
                            "choiceText": "Pain > 50%"
                        }
                    ],
                    "isActive": true
                },



                {
                    "id": 55,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "O2 SAT",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 56,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "on",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },




                {
                    "id": 57,
                    "promptGroupId": 23,
                    "sequence": 2,
                    "prompt": "END-TIDAL CO2",
                    "type": "FreeText",
                    "default": null,
                    "required": false,
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 58,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 58,
                            "sequence": 1,
                            "choiceText": "End-Tidal. 1"
                        },
                        {
                            "id": 78,
                            "promptId": 58,
                            "sequence": 2,
                            "choiceText": "End-Tidal. 2",
                        },
                        {
                            "id": 79,
                            "promptId": 58,
                            "sequence": 3,
                            "choiceText": "End-Tidal. 3",
                        }
                    ],
                    "isActive": true
                },
                {
                    "id": 59,
                    "promptGroupId": 23,
                    "sequence": 3,
                    "prompt": "",
                    "type": "DropDownListBox",
                    "default": null,
                    "required": false,
                    "promptChoices": [
                        {
                            "id": 77,
                            "promptId": 59,
                            "sequence": 1,
                            "choiceText": "End-Tidal. @4"
                        },
                        {
                            "id": 78,
                            "promptId": 59,
                            "sequence": 2,
                            "choiceText": "End-Tidal. @5"
                        }
                    ],
                    "isActive": true
                }



            ]
        },
        {
            "id": 17,
            "name": "GenericGive",
            "displayTitle": "",
            "sequence": 5,
            "prompts": [
                {
                    "id": 63,
                    "promptGroupId": 17,
                    "sequence": 1,
                    "prompt": "Notes",
                    "type": "MultiLineFreeText",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                {
                    "id": 62,
                    "promptGroupId": 17,
                    "sequence": 2,
                    "prompt": "Documented At",
                    "type": "DateTime",
                    "default": "2020-11-17T09:00:30.6634061-06:00",
                    "required": true,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                },
                // {
                //     "id": 59,
                //     "promptGroupId": 17,
                //     "sequence": 3,
                //     "prompt": "Self Administered",
                //     "type": "CheckBox",
                //     "default": null,
                //     "required": false,
                //     "promptChildren": [],
                //     "promptChoices": [],
                //     "isActive": true
                // },
                // {
                //     "id": 61,
                //     "promptGroupId": 17,
                //     "sequence": 4,
                //     "prompt": "Patient Supplied",
                //     "type": "CheckBox",
                //     "default": null,
                //     "required": false,
                //     "promptChildren": [],
                //     "promptChoices": [],
                //     "isActive": true
                // },
                {
                    "id": 60,
                    "promptGroupId": 17,
                    "sequence": 5,
                    "prompt": "Notify",
                    "type": "Notify",
                    "default": null,
                    "required": false,
                    "promptChildren": [],
                    "promptChoices": [],
                    "isActive": true
                }
            ]
        }
    ],
    "active": true,
    "title": "Follow Up",
    "saveButtonText": "Enter",
    "cancelButtonText": "Cancel",
    "link": {
        "href": "http://localhost:51044/api/orders/administrations/100/actions/2/templates/2",
        "rel": "File Results of Template",
        "method": "POST"
    }
}
