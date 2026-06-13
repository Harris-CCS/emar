import { ComposerOptions } from '../interfaces/composerOptions';
export const COMPOSER_OPTIONS: ComposerOptions[] = [
  {
    brandName: 'Ondansetron Hydrochloride',
    availableFormStrength: [
      {
        id: 1095,
        formStrengthName: '4mg orally disintegrating',
        availableRoutes: [
          {
            id: 1,
            siteId: -1,
            routeName: 'orally',
            pointInTime: true,
          },
          {
            id: 5,
            siteId: -1,
            routeName: 'sublingual',
            pointInTime: true,
          },
        ],
        preferredDoses: [
          {
            doseName: '4 mg',
            dose: 4.0,
            doseUnit: {
              id: 40,
              unitName: 'mg',
            },
          },
          {
            doseName: '8 mg',
            dose: 8.0,
            doseUnit: {
              id: 40,
              unitName: 'mg',
            },
          },
          {
            doseName: '12 mg',
            dose: 12.0,
            doseUnit: {
              id: 40,
              unitName: 'mg',
            },
          },
          {
            doseName: '16 mg',
            dose: 16.0,
            doseUnit: {
              id: 40,
              unitName: 'mg',
            },
          },
          {
            doseName: '20 mg',
            dose: 20.0,
            doseUnit: {
              id: 40,
              unitName: 'mg',
            },
          },
        ],
        preferredRoutes: [
          {
            id: 5,
            siteId: -1,
            routeName: 'sublingual',
            pointInTime: true,
          },
          {
            id: 6,
            siteId: -1,
            routeName: 'sniff',
            pointInTime: true,
          },
          {
            id: 7,
            siteId: -1,
            routeName: 'orally',
            pointInTime: true,
          },
          {
            id: 8,
            siteId: -1,
            routeName: 'puff',
            pointInTime: true,
          },
          {
            id: 10,
            siteId: -1,
            routeName: 'subcutaneously',
            pointInTime: true,
          },
        ],
        preferredFrequencies: [
          {
            id: 7,
            scheduleName: '2 TIMES DAILY',
          },
          {
            id: 5,
            scheduleName: 'Every 6 HOURS',
          },
          {
            id: 1,
            scheduleName: 'ONCE',
          },
          {
            id: 8,
            scheduleName: '3 TIMES DAILY',
          },
          {
            id: 9,
            scheduleName: '4 TIMES DAILY',
          },
          {
            id: 11,
            scheduleName: '8 TIMES DAILY',
          },
        ],
        administrationInstructions: [
          {
            id: 1,
            name: 'Telephone Order',
            description: 'This is a telephone order',
          },
          {
            id: 2,
            name: 'Verbal Order',
            description: 'This is a verbal order',
          },
        ],
      },
      {
        id: 1099,
        formStrengthName: '8mg orally disintegrating',
        availableRoutes: [
          {
            id: 1,
            siteId: -1,
            routeName: 'orally',
            pointInTime: true,
          },
          {
            id: 5,
            siteId: -1,
            routeName: 'sublingual',
            pointInTime: true,
          },
        ],
        preferredDoses: [
          {
            doseName: '8 mg',
            dose: 8.0,
            doseUnit: {
              id: 40,
              unitName: 'mg',
            },
          },
          {
            doseName: '16 mg',
            dose: 16.0,
            doseUnit: {
              id: 40,
              unitName: 'mg',
            },
          },
        ],
        preferredRoutes: [
          {
            id: 5,
            siteId: -1,
            routeName: 'sublingual',
            pointInTime: true,
          },
        ],
        preferredFrequencies: [
          {
            id: 7,
            scheduleName: '2 TIMES DAILY',
          },
          {
            id: 5,
            scheduleName: 'Every 6 HOURS',
          },
          {
            id: 1,
            scheduleName: 'ONCE',
          },
        ],
        administrationInstructions: [
          {
            id: 3,
            name: 'Call In Order',
            description: 'This is a call in order',
          },
          {
            id: 4,
            name: 'Test Order',
            description: 'This is a test order',
          },
        ],
      },
    ],
  },
];
