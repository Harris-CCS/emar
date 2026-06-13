import { User } from './user'
import { OverrideReason } from './override-reason'

export interface AllergyReaction {
    id: number,
    patientAllergyId: number,
    orderId: number,
    orderTable: string,
    brandName: string,
    allergyName: string,
    overrideReason: OverrideReason,
    overrideReasonUser: User,
    overrideReasonDatetime: string
}
