import { Site } from './site'

export interface OverrideReason {
    id: number,
    siteId: number,
    isMedication: boolean,
    description: string,
    site: Site
}
