import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { MedOrderService } from 'src/services/med-order.service';
import { Subject } from 'rxjs'
import { takeUntil } from 'rxjs/operators'

import { Medication } from '../../../app/interfaces/medication';

@Component({
  selector: 'simple-order-list',
  templateUrl: './simple-order-list.component.html',
  styleUrls: ['./simple-order-list.component.scss'],
})
export class SimpleOrderListComponent implements OnInit, OnDestroy {
  private tabListTabsFull = [
    'Most Used',
    // '#',
    'A',
    'B',
    'C',
    'D',
    'E',
    'F',
    'G',
    'H',
    'I',
    'J',
    'K',
    'L',
    'M',
    'N',
    'O',
    'P',
    'Q',
    'R',
    'S',
    'T',
    'U',
    'V',
    'W',
    'X',
    'Y',
    'Z',
    '#',
  ];
  displayTabs: Array<Object>;
  // displayItems: Medication[];
  displayItems;
  panelToggle: {};
  comboDetailDisplayToggle: {};

  @Input() listName: string;
  @Input() selectedTab: string;
  @Input() selectedGroup: string;
  @Input() orderOnClick1: any;
  @Input() orderOnClick2: any;
  @Input() tabOnClick: (tab: string, listType: string) => void;
  @Input() set items(data) {
    this.displayItems = data;
    this.comboDetailDisplayToggle = {}

    if (this.listName === 'groups') {
      // this.panelToggle = this.displayItems.reduce((o, key) => Object.assign(o, {[key]: false}), {})

      const idx = 0 //default
      // this.panelToggle = (this.displayItems && this.displayItems.length) ? {
      //   [this.displayItems?.[idx].displayGroupName]: true
      // } : {}
      this.panelToggle = {}
      if (this.selectedGroup) {
        this.panelToggle[this.selectedGroup] = true;
      }
      console.log('Input set items - panelToggle: ', this.panelToggle, this.selectedGroup, this.selectedTab, this.listName)
    }

  }
  // @Input() set tabItems(data) {
  //   this.displayTabs = data.map(
  //     x => ({name: x, isChecked: (x === this.selectedTab)})
  //   );
  // }

  @Input() set tabItems(data) {
    if (this.listName === 'deptPreferredWithTab') { 
      this.tabListTabsFull = this.tabListTabsFull.filter( t => t !== 'Most Used')
    }

    this.displayTabs = this.tabListTabsFull.map((cur) => ({
      name: cur,
      isValid: data.includes(cur),
      isChecked: (cur + this.listName) === this.selectedTab,
    }));
  }
  @Input() auth: boolean;
  @Input() isLoading: boolean = false;
  ngUnsubscribe = new Subject<void>();
  loading:number = -1;


  toggleComboComponents(itemId: any) {
    this.comboDetailDisplayToggle[itemId] = !this.comboDetailDisplayToggle[itemId]
    console.log('Combo Med - toggle - itemId: ', itemId)
  }
  // currentPage: number = 1
  // totalPages: number = 280

  constructor(
    private medOrderService: MedOrderService
  ) {}

  ngOnInit(): void {
    this.panelToggle = {};
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
  // getPageSymbol(current: number) {
  //   return ['Most Used', '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'][current - 1];
  // }

  // pageChange() {
  //   console.log('pageChange click page: ', this.currentPage)

  // }

  toggle(panel: string): void {
    console.log('toggle ME: ', panel, this.panelToggle);
    this.panelToggle[panel] = !this.panelToggle[panel];
    console.log('dddddd',this.panelToggle);
  }
  // nowrap
  //tabLists = ['Most Used', '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];

  // tabChange(newTab) {
  //   console.log('tabChnage click:', newTab)

  //   //this.data = this.data.filter( (med) => med.name.startsWith(newTab));
  //   //console.log('tabChage: data: ', this.data)
  // }
  deleteQuickListItem(itemId: number, iItem: number) {
    this.medOrderService.deleteQuickListItem(itemId);
    this.displayItems[iItem].disabled = true;
  }

  // async get the orders of a group 
  getGroupDetail(iGroup: number) {
    const groupName = this.displayItems[iGroup].displayGroupName;
    if (this.panelToggle[groupName]) {
      this.panelToggle[groupName] = false;
    } else {
      this.loading = iGroup; // display waiting icon in the list of orders area
      this.medOrderService.getGroupDetail(groupName)
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe ( (group) => {
          this.displayItems[iGroup].orders = group.orders;
          this.panelToggle[groupName] = true;
          this.loading = -1;
        }
      );
    }
  }

}
