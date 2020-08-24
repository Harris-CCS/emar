import { Hateoas } from './hateoas'
import { ListOrder } from './list-order'

export interface DeptPreferredList extends ListOrder{
    links: Array<Hateoas>;
}
