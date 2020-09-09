import { Component, OnInit, Input } from '@angular/core';

import { Medication } from '../../../app/interfaces/medication';

@Component({
  selector: 'simple-order-list',
  templateUrl: './simple-order-list.component.html',
  styleUrls: ['./simple-order-list.component.scss'],
})
export class SimpleOrderListComponent implements OnInit {
  private tabListTabsFull = [
    'Most Used',
    '#',
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
  ];
  displayTabs: Array<Object>;
  // displayItems: Medication[];
  displayItems: Array<{
    displayGroupName: string
  }>;
  panelToggle: {};

  @Input() listName: string;
  @Input() selectedTab: string;
  @Input() orderOnClick1: any;
  @Input() orderOnClick2: any;
  @Input() tabOnClick: any;
  @Input() set items(data) {
    this.displayItems = data;

    if (this.listName === 'groups') {
      // this.panelToggle = this.displayItems.reduce((o, key) => Object.assign(o, {[key]: false}), {})

      const idx = 0 //default
      this.panelToggle = (this.displayItems && this.displayItems.length) ? {
        [this.displayItems?.[idx].displayGroupName]: true
      } : {}
      console.log('Input set items - panelToggle: ', this.panelToggle)
    }
  }
  // @Input() set tabItems(data) {
  //   this.displayTabs = data.map(
  //     x => ({name: x, isChecked: (x === this.selectedTab)})
  //   );
  // }

  @Input() set tabItems(data) {
    this.displayTabs = this.tabListTabsFull.map((cur) => ({
      name: cur,
      isValid: data.includes(cur),
      isChecked: cur === this.selectedTab,
    }));
  }

  // currentPage: number = 1
  // totalPages: number = 280

  constructor() {}

  ngOnInit(): void {
    this.panelToggle = {};
  }

  // getPageSymbol(current: number) {
  //   return ['Most Used', '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'][current - 1];
  // }

  // pageChange() {
  //   console.log('pageChange click page: ', this.currentPage)

  // }

  toggle(panel: string) {
    console.log('toggle ME: ', panel);
    this.panelToggle[panel] = !this.panelToggle[panel];
  }
  // nowrap
  //tabLists = ['Most Used', '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];

  // tabChange(newTab) {
  //   console.log('tabChnage click:', newTab)

  //   //this.data = this.data.filter( (med) => med.name.startsWith(newTab));
  //   //console.log('tabChage: data: ', this.data)
  // }
}
