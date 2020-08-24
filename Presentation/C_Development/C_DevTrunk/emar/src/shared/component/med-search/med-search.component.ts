import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, tap, switchMap } from 'rxjs/operators';

import { MedOrderService } from '../../../services/med-order.service';
import { ModalService } from '../../../services/modal.service';

import { Medication } from '../../../app/interfaces/medication';
import { MEDICATIONS } from 'src/app/mockup/medications';

@Component({
  selector: 'med-search',
  templateUrl: './med-search.component.html',
  styleUrls: ['./med-search.component.scss']
})
export class MedSearchComponent implements OnInit {

  model: any;
  searching: boolean = false;
  //searchFailed: boolean = false;
  label: string = '';
  selectedSource: string = 'All'
  sources: string[] = ['Quick List', 'Dept Preferred List', 'Groups', 'Formulary', 'All']

  constructor(
    private medOrderService: MedOrderService,
    private modalService: ModalService,
  ) { }

  ngOnInit(): void {
  }

  changeSource(newSource: string) {
    this.selectedSource = newSource
  }

  /*search = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.searching = true),
      switchMap(term =>
        this.medOrderService.search(term).pipe(
          tap(() => this.searchFailed = false),
          catchError(() => {
            this.searchFailed = true;
            return of([]);
          }))
      ),
      tap(() => this.searching = false)
    )*/

    inputFormat(value: any) {
      return (value.brandName) ? value.brandName : value;
    }

    resultFormat(value: any) {
      return value.brandName;
    }

    search = (text$: Observable<string>, source: string) =>
    text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      //switchMap( (searchText) => this.medOrderService.search(searchText) ),
      //catchError(new ErrorInfo().parseObservableResponseError)
      map(term => term.length < 2 
        ? [] //console.log('selectedSource: ', source = this.selectedSource)
        : MEDICATIONS.filter(m => m.brandName.toLowerCase().indexOf(term.toLowerCase()) > -1).slice(0, 10))
    )

    onSelect($event, input) {
      $event.preventDefault();
      console.log('onSelect: ', $event.item);
      
      //this.medOrderService.postCartOrder($event.item, 'new');
      console.log(`next from NEW: ${$event.item.name}`);
      this.modalService.open('medComposer', {action: 'add', med: $event.item});
      input.value = '';
      input.blur();
    }
}
