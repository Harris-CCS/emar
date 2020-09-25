import { User } from './user'
import { OverrideReason } from './override-reason'

export interface DrugInteraction {
    id: number,
    interactionDrug1: string,
    interactionDrug2: string,
    severity: string
    orderId1: number,
    orderTable1: string,
    orderName1: string,
    orderId2: number,
    orderTable2: string,
    orderName2: string,
    overrideReason: OverrideReason,
    overrideReasonUser: User,
    overrideReasonDatetime: string
}
