import { User } from './user'
import { OverrideReason } from './override-reason'

export interface DrugInteraction {
    id: number,
    interactionDrug1: string,
    interactionDrug2: string,
    severity: string
    orderId1?: number,
    orderTable1?: string,
    orderName1?: string,
    interactionOrderId: number,
    interactionOrderTable: string,
    interactionOrderName: string,
    overrideReason: OverrideReason,
    overrideReasonUser: User,
    overrideReasonDatetime: string
}
