import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgbAccordion, NgbPanelChangeEvent, NgbPanel } from '@ng-bootstrap/ng-bootstrap';
import { DoseOption } from '../../../app/interfaces/doseOption';
import { SimpleTableComponent } from '../simple-table/simple-table.component';

@Component({
  selector: 'dosing-info',
  templateUrl: './dosing-info.component.html',
  styleUrls: ['./dosing-info.component.scss'],
})
export class DosingInfoComponent implements OnInit {
  @Input() doseOptions: Array<DoseOption>;
  @ViewChild('acc') accordionComponent: NgbAccordion;
  sortedDosingOptions = [];
  dosingInfoTableStructure: SimpleTableComponent;

  constructor() { }

  ngOnInit(): void {
    // console.log('doseOptions', this.doseOptions);
    this.sortDosingData();
  }

  isPanelSelected(id: string): boolean {
    if (this.accordionComponent && this.accordionComponent.panels) {
      const panel = this.accordionComponent.panels.find((pn) => pn.id === id);
      // console.log('isPanelOpen', panel && panel.isOpen);
      return (panel && panel.isOpen) || false;
    } else {
      return false;
    }
  }

  sortDosingData(): void {
    for (const option of this.doseOptions) {
      const routeDescription: string =
        option.routeDescription || 'NO ROUTE DEFINED';
      let routeOptionIndex: number = this.sortedDosingOptions.findIndex(
        (sortedOption) => sortedOption.routeDescription && sortedOption.routeDescription === routeDescription);
      if (routeOptionIndex === -1) {
        routeOptionIndex = !this.sortedDosingOptions.length ? 0 : this.sortedDosingOptions.length;
        this.sortedDosingOptions.push({
          routeDescription,
          routeDescriptionDisplay: routeDescription === 'NO ROUTE DEFINED' ? routeDescription : `ROUTE: ${routeDescription}`,
          options: [],
        });
      }
      this.sortedDosingOptions[routeOptionIndex].options.push(option);
      // console.log('sortedOptions', this.sortedDosingOptions);
    }
  }

  getTableStructure(index: number): SimpleTableComponent {
    if (!this.sortedDosingOptions[index].tableStructure) {
      // *********************************************
      const itemsPerPageOptions = [];
      const pagination = (this.sortedDosingOptions[index].options.length > 10) ? true : false;
      if (pagination) {
        let pageItems: number = 10;
        while (pageItems < this.sortedDosingOptions[index].options.length) {
          if (this.sortedDosingOptions[index].options.length > pageItems) { itemsPerPageOptions.push(pageItems); }
          pageItems += 10;
        }
        itemsPerPageOptions.push(this.sortedDosingOptions[index].options.length);
      }
      const tableStructure = new SimpleTableComponent();
      tableStructure.title = `${this.sortedDosingOptions[index].routeDescription} Dosing Information`;
      tableStructure.params = {
        pagination: {
          usePagination: pagination,
          itemsPerPageChoices: itemsPerPageOptions,
        },
      };
      // Table Headers
      tableStructure.appendTableHeaderCell(true, {
        isHeaderCell: true,
        data: 'Type/Range',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Age',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Weight',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Condition',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Renal',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Low',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'High',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Unit',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Max Frequency',
      });
      // Table Data
      for (const option of this.sortedDosingOptions[index].options) {
        tableStructure.appendTableBodyCell(true, {
          isHeaderCell: true,
          data: option.typeDescription,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: option.ageDdescription,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: option.weightDescription,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: option.condition1Description,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: option.renalDescription,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: true,
          data: option.amountLow,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: true,
          data: option.amountHigh,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: true,
          data: option.unitDoseAbbreviation,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: true,
          data: option.maxFrequency,
          dataType: 'string',
          className: 'align-center',
        });
      }
      //**********************************************
      this.sortedDosingOptions[index].tableStructure = tableStructure;
    }
    return this.sortedDosingOptions[index].tableStructure;
  }
}
