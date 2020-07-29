import { ComposerOptions } from '../interfaces/composerOptions';
export const COMPOSER_OPTIONS: ComposerOptions[] =[
    {
        "brandName": "Ondansetron Hydrochloride",
        "availableFormStrength": [
            {
                "id": 1095,
                "formStrengthName": "4mg orally disintegrating",
                 "availableRoutes": [
                    {
                        "id": 1,
                        "siteId": -1,
                            "routeName": "orally",
                            "pointInTime": true
                        },
                        {
                            "id": 5,
                            "siteId": -1,
                            "routeName": "sublingual",
                            "pointInTime": true
                        }
                    ],
                    "preferredDoses": [
                        {
                            "doseName": "4 mg",
                            "dose": 4.0,
                            "doseUnit": {
                                "id": 40,
                                "unitName": "mg"
                            }
                        },
                        {
                            "doseName": "8 mg",
                            "dose": 8.0,
                            "doseUnit": {
                                "id": 40,
                                "unitName": "mg"
                            }
                        }
                    ],
                    "preferredRoutes": [
                        {
                            "id": 5,
                            "siteId": -1,
                            "routeName": "sublingual",
                            "pointInTime": true
                        }
                    ],
                    "preferredFrequencies": [
                        {
                            "id": 7,
                            "frequencyName": "2 TIMES DAILY"
                        },
                        {
                            "id": 5,
                            "frequencyName": "Every 6 HOURS"
                        },
                        {
                            "id": 1,
                            "frequencyName": "ONCE"
                        }
                    ]
                },
                {
                    "id": 1099,
                    "formStrengthName": "8mg orally disintegrating",
                    "availableRoutes": [
                        {
                            "id": 1,
                            "siteId": -1,
                            "routeName": "orally",
                            "pointInTime": true
                        },
                        {
                            "id": 5,
                            "siteId": -1,
                            "routeName": "sublingual",
                            "pointInTime": true
                        }
                    ],
                    "preferredDoses": [
                        {
                            "doseName": "8 mg",
                            "dose": 8.0,
                            "doseUnit": {
                                "id": 40,
                                "unitName": "mg"
                            }
                        },
                        {
                            "doseName": "16 mg",
                            "dose": 16.0,
                            "doseUnit": {
                                "id": 40,
                                "unitName": "mg"
                            }
                        }
                    ],
                    "preferredRoutes": [
                        {
                            "id": 5,
                            "siteId": -1,
                            "routeName": "sublingual",
                            "pointInTime": true
                        }
                    ],
                    "preferredFrequencies": [
                        {
                            "id": 7,
                            "frequencyName": "2 TIMES DAILY"
                        },
                        {
                            "id": 5,
                            "frequencyName": "Every 6 HOURS"
                        },
                        {
                            "id": 1,
                            "frequencyName": "ONCE"
                        }
                    ]
                }
            ]
        }
]