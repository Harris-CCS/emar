import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgbAccordion, NgbPanelChangeEvent, NgbPanel } from '@ng-bootstrap/ng-bootstrap';
import { PatientOrderInteraction } from '../../../app/interfaces/patient-order-interaction';
import { SimpleTableComponent } from '../simple-table/simple-table.component';

@Component({
  selector: 'patient-order-interactions',
  templateUrl: './patient-order-interactions.component.html',
  styleUrls: ['./patient-order-interactions.component.scss']
})
export class PatientOrderInteractionsComponent implements OnInit {
  @Input() patientOrderInteractions: Array<PatientOrderInteraction>;
  @Input() includeInteractionType: string;
  @ViewChild('acc') accordionComponent: NgbAccordion;
  sortedOrderInteractions = [];
  interactionsTableStructure: SimpleTableComponent;

  constructor() { }

  ngOnInit(): void {
    this.sortInteractions();
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

  sortInteractions(): void {
    /*

    Severity Identifiers per API determined by drug vendor:

    FDB:
        UNDETERMINED = 5,
        MODERATE = 6,
        SEVERE = 7,
        CONTRAINDICATED = 8 
    Multum:
        MINOR = 1,
        MODERATE = 2,
        SEVERE = 3,
        ALLERGY = 4 

    */
    for (const node of this.patientOrderInteractions) {
      const interactionsTypeStructure = this.includeInteractionType === 'drug' ? node.interactions : node.reactions;
      for (const interaction of interactionsTypeStructure) {
        if (this.includeInteractionType.includes(interaction.type)) {
          const severityId: string = interaction.severity_id || 'No Severity Id';
          let insertIndex: number = this.sortedOrderInteractions.findIndex(
            sortedOrderInteraction => (
              ((sortedOrderInteraction.severityId === '0' || sortedOrderInteraction.severityId) &&
                sortedOrderInteraction.severityId === severityId) ||
              (!sortedOrderInteraction.severityId && severityId === 'No Severity Id')
            )
          );
          if (insertIndex === -1) {
            const newObject = {
              severityId,
              severityText: interaction.sevtxt,
              items: [],
            };

            insertIndex = this.sortedOrderInteractions.findIndex(soI => soI.severityId > severityId);
            if (insertIndex === -1) {
              insertIndex = this.sortedOrderInteractions.length === 0 ? 0 : this.sortedOrderInteractions.length;
            }
            this.sortedOrderInteractions.splice(insertIndex, 0, newObject);

          }
          this.sortedOrderInteractions[insertIndex].items.push(interaction);
        }
      }
      // console.log('sortedOrderInteractions', this.sortedOrderInteractions);
    }

    if (this.sortedOrderInteractions.length > 0) {
      this.sortedOrderInteractions = this.sortedOrderInteractions.reverse();
      // console.log('reversedSortedOrderInteractions', this.sortedOrderInteractions);
    }
  }

  getTableStructure(index: number): SimpleTableComponent {
    if (!this.sortedOrderInteractions[index].tableStructure) {
      // *********************************************
      const itemsPerPageOptions = [];
      const pagination = (this.sortedOrderInteractions[index].items.length > 10) ? true : false;
      if (pagination) {
        let pageItems: number = 10;
        while (pageItems < this.sortedOrderInteractions[index].items.length) {
          if (this.sortedOrderInteractions[index].items.length > pageItems) { itemsPerPageOptions.push(pageItems); }
          pageItems += 10;
        }
        itemsPerPageOptions.push(this.sortedOrderInteractions[index].items.length);
      }
      const tableStructure = new SimpleTableComponent();
      tableStructure.title = `${this.sortedOrderInteractions[index].severityText}`;
      tableStructure.params = {
        pagination: {
          usePagination: pagination,
          itemsPerPageChoices: itemsPerPageOptions,
        },
      };
      // Table Headers
      tableStructure.appendTableHeaderCell(true, {
        isHeaderCell: true,
        data: 'Interaction',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Description',
      });
      tableStructure.appendTableHeaderCell(false, {
        isHeaderCell: true,
        data: 'Source',
      });
      // Table Data
      for (const item of this.sortedOrderInteractions[index].items) {
        tableStructure.appendTableBodyCell(true, {
          isHeaderCell: true,
          data: item.dname2 || item.drug,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: item.interaction,
          dataType: 'string',
          className: 'align-center',
        });
        tableStructure.appendTableBodyCell(false, {
          isHeaderCell: false,
          data: item.sourceTable2 || item.sourceTable,
          dataType: 'string',
          className: 'align-center',
        });
      }
      // **********************************************
      this.sortedOrderInteractions[index].tableStructure = tableStructure;
    }
    return this.sortedOrderInteractions[index].tableStructure;
  }

}
