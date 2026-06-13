import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable, of, Subject, Subscription } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  tap,
  switchMap,
} from 'rxjs/operators';

import { MedOrderService } from '../../../services/med-order.service';
import { ComposerSchedulerService } from '../../../services/composer-scheduler.service';
import { ModalService } from '../../../services/modal.service';

import { Medication } from '../../../app/interfaces/medication';
import { MEDICATIONS } from 'src/app/mockup/medications';

@Component({
  selector: 'med-search',
  templateUrl: './med-search.component.html',
  styleUrls: ['./med-search.component.scss'],
})
export class MedSearchComponent implements OnInit, OnDestroy {
  model: any;
  searching: boolean = false;
  searchFailed: boolean = false;
  label: string = '';
  selectedSource: string = '';
  // sources: string[] = [
  //   'Quick List',
  //   'Dept Preferred List',
  //   'Groups',
  //   'Formulary',
  //   'All',
  // ];
  sources = []
  searchOptionsSubscribe: Subscription

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private medOrderService: MedOrderService,
    private modalService: ModalService,
    private composerSchedulerService: ComposerSchedulerService,
  ) {
    
    this.searchOptionsSubscribe = this.medOrderService.getMedicationSearchOptions().subscribe((o) => {
      console.log('MedSeatchOptions: ', o)
      // EMAR 552 - Display Formulary if present as the default, else take normal default.

      // if (o.all) { this.sources.push({ value: 'All', display: 'All' }) }
      // if (o.deptpreferred) { this.sources.push({ value: 'DepartmentPreferredListItem', display: 'Dept Preferred List' }) }
      // if (o.formulary) { this.sources.push({ value: 'FormularyItem', display: 'Formulary' }) }
      // if (o.groups) { this.sources.push({ value: 'GroupRememberedOrder', display: 'Groups' }) }
      // if (o.userquicklist) { this.sources.push({ value: 'UserQuickListItem', display: 'Quick List' }) }

      // this.selectedSource = this.sources[0] ? this.sources[0].value : ''

      if (o.all) { this.sources.push({ value: 'All', display: 'All' }) }
      if (o.deptpreferred) { this.sources.push({ value: 'DepartmentPreferredListItem', display: 'Dept Preferred List' }) }
      if (o.formulary) {
        this.sources.push({ value: 'FormularyItem', display: 'Formulary' });
        this.selectedSource = this.sources[(this.sources.length === 1 ? 0 : this.sources.length - 1)].value;
      }
      if (o.groups) { this.sources.push({ value: 'GroupRememberedOrder', display: 'Groups' }) }
      if (o.userquicklist) { this.sources.push({ value: 'UserQuickListItem', display: 'Quick List' }) }
      this.selectedSource = (!this.sources || !this.sources.length) ? '' : this.selectedSource || this.sources[0].value;
      console.log('MedSeatchOptions: sources: ', this.sources)
    })

    this.selectedSource = (!this.selectedSource && this.sources[0]) ? this.sources[0].value : this.selectedSource;
  }

  ngOnInit(): void { }

  ngOnDestroy(): void {
    if (this.searchOptionsSubscribe) this.searchOptionsSubscribe.unsubscribe()
  }

  changeSource(newSource: string) {
    this.selectedSource = newSource;
    console.log('selectedSource: ', this.selectedSource)

    this.model = ''
    this.searchFailed = false
    this.searching = false
  }

  selectedSourceDisplay() {
    return this.sources.find((source) => source.value === this.selectedSource)?.display || ''
  }

  search = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.searching = true),
      switchMap(term =>
        // this.medOrderService.searchHttp(term, this.selectedSource).pipe(
        this.medOrderService.brandNameSearch(term, this.selectedSource).pipe(
          tap(() => this.searchFailed = false),
          catchError(() => {
            console.log('---------')
            this.searchFailed = true;
            return of([]);
          }))
      ),
      tap(() => this.searching = false)
    )

  inputFormat(value: any) {
    // return value.brandName ? value.brandName : value;
    return value;
  }

  resultFormat(value: any) {
    return value.brandName;
    // return value;
  }

  // search = (text$: Observable<string>, source: string) =>
  //   text$.pipe(
  //     debounceTime(200),
  //     distinctUntilChanged(),
  //     //switchMap( (searchText) => this.medOrderService.search(searchText) ),
  //     //catchError(new ErrorInfo().parseObservableResponseError)
  //     map((term) =>
  //       term.length < 2
  //         ? [] //console.log('selectedSource: ', source = this.selectedSource)
  //         : MEDICATIONS.filter(
  //           (m) => m.brandName.toLowerCase().indexOf(term.toLowerCase()) > -1
  //         ).slice(0, 10)
  //     )
  //   );

  onSelect($event, input) {
    $event.preventDefault();
    // console.log('onSelect: ', $event.item);

    //this.medOrderService.postCartOrder($event.item, 'new');
    // console.log(`next from NEW: ${$event.item.name}`);
    console.log('next from NEW: ', $event.item);
    // this.modalService.open('medComposer', {
    //   action: 'add',
    //   source: 'med-search',
    //   med: $event.item,
    // });
    this.launchMedComposer($event.item.brandName, $event.item);
    input.value = '';
    input.blur();
  }

  launchMedComposer(medBrandName: string, medData: object): void {
    this.composerSchedulerService.setInitialComposerData({ action: 'add', source: 'med-search', med: medData });
    const convertedMedBrandName: string = encodeURIComponent(medBrandName);
    this.router.navigate(['new-order', convertedMedBrandName],
      {
        // state: { data: { medData } },
        queryParams: {},
        relativeTo: this.route
      });
  }
}
