import { Hateoas } from './hateoas'
import { ListOrder } from './list-order'

export interface GroupOrderList {
    groups: Array<Group>
}

interface Group {
    groupName: string,
    orders: Array<Order>
}

interface Order extends ListOrder{
    links: Array<Hateoas>
}