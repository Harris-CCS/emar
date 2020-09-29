import { Site } from './site'
import { MedicationDetail } from './medication-detail'

export interface Medication {
    id: number,
    site?: Site,
    drugId?: string,
    displayName?: string,
    drugVendor?: string,
    medicationDetails?: Array<MedicationDetail>,

    // the following items will be removed once we dont use mock data for the med search
    packagingId?: string,
    brandName?: string,
    drugStrength?: string,
    dose?: string,
    route?: string,
    activeId?: string,
    activeName?: string
}
