import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'simple-table',
  templateUrl: './simple-table.component.html',
  styleUrls: ['./simple-table.component.scss'],
})
export class SimpleTableComponent implements OnInit {
  @Input() tableStructure: SimpleTableComponent;

  title: string;
  header?: {
    row?: {
      id: number;
      cell: {
        data: string;
        dataType?: string;
        className?: string;
      }[];
    }[];
  };
  body: {
    row?: {
      className?: string;
      id?: number;
      isHeaderRow?: boolean;
      cell?: {
        isHeaderCell?: boolean;
        id?: string;
        className?: string;
        data: string;
        dataType?: string;
        imagePath?: string;
      }[];
    }[];
  };
  params?: {
    pagination: {
      usePagination?: boolean;
      page?: number;
      pageSize?: number;
      collectionSize?: number;
      items?: {}[];
      itemsPerPageChoices?: number[];
    };
  };

  constructor() {}

  // constructor(title: string) {
  //   this.title = title;
  //   this.header = {};
  //   this.body = {};
  // }

  refreshPaginationItems(): void {
    this.tableStructure.params.pagination.items = this.tableStructure.body.row
      .map((item, i) => ({ id: i + 1, ...item }))
      .slice(
        (this.tableStructure.params.pagination.page - 1) *
          this.tableStructure.params.pagination.pageSize,
        (this.tableStructure.params.pagination.page - 1) *
          this.tableStructure.params.pagination.pageSize +
          this.tableStructure.params.pagination.pageSize
      );
  }

  ngOnInit(): void {
    if (this.hasPagination()) {
      if (!this.tableStructure.params.pagination.page) {
        this.tableStructure.params.pagination.page = 1;
      }
      if (!this.tableStructure.params.pagination.collectionSize) {
        this.tableStructure.params.pagination.collectionSize = this.tableStructure.body.row.length;
      }

      if (!this.tableStructure.params.pagination.items) {
        this.tableStructure.params.pagination.items = this.tableStructure.body.row;
      }

      if (
        !this.tableStructure.params.pagination.itemsPerPageChoices ||
        !this.tableStructure.params.pagination.itemsPerPageChoices.length
      ) {
        this.tableStructure.params.pagination.itemsPerPageChoices = [];
        const itemsTotal: number = this.tableStructure.params.pagination.items
          .length;
        const options: number[] = [3, 5, 10, 20, 50];
        for (const opt of options) {
          if (itemsTotal >= opt) {
            this.tableStructure.params.pagination.itemsPerPageChoices.push(opt);
          }
        }
        const optionsLength: number = this.tableStructure.params.pagination
          .itemsPerPageChoices.length;
        if (
          itemsTotal > optionsLength - 1 &&
          this.tableStructure.params.pagination.itemsPerPageChoices.indexOf(
            itemsTotal
          ) === -1
        ) {
          this.tableStructure.params.pagination.itemsPerPageChoices.push(
            itemsTotal
          );
        }
      }
      if (!this.tableStructure.params.pagination.pageSize) {
        this.tableStructure.params.pagination.pageSize = this.tableStructure.params.pagination.itemsPerPageChoices[0];
      }

      this.refreshPaginationItems();
    }
  }

  appendTableHeaderCell(
    appendNewRow: boolean,
    cell: {
      isHeaderCell: boolean;
      data?: any;
      dataType?: string;
      className?: string;
    }
  ): void {
    if (!this.header) {
      this.header = {};
    }
    if (!this.header.row || !this.header.row.length) {
      this.header.row = [];
    }
    let rowId = this.header.row.length === 0 ? 0 : this.header.row.length - 1;

    if (appendNewRow) {
      if (this.header.row.length !== 0) {
        rowId++;
      }
      this.header.row.push({ id: rowId, cell: [] });
    }
    this.header.row[rowId].cell.push({
      data: cell.data,
      dataType: cell.dataType,
      className: cell.className || '',
    });
  }

  appendTableBodyCell(
    appendNewRow: boolean,
    cell: {
      isHeaderCell: boolean;
      data?: any;
      dataType?: string;
      className?: string;
      imagePath?: string;
    }
  ): void {
    if (!this.header) {
      this.header = {};
    }
    if (!this.body) {
      this.body = {};
    }
    if (!this.body.row || !this.body.row.length) {
      this.body.row = [];
    }
    let rowId = this.body.row.length === 0 ? 0 : this.body.row.length - 1;

    if (appendNewRow) {
      if (this.body.row.length !== 0) {
        rowId++;
      }
      this.body.row.push({ id: rowId, cell: [] });
    }
    this.body.row[rowId].cell.push({
      isHeaderCell: cell.isHeaderCell,
      data: cell.data || ' ',
      dataType: cell.dataType,
      className: cell.className,
      imagePath: cell.imagePath,
    });
  }

  tableHasData(): boolean {
    // console.log('tableStructure', this.tableStructure);
    if (!this.tableStructure) {
      return false;
    } else if (!this.tableStructure.header) {
      return false;
    } else if (!this.tableStructure.body) {
      return false;
    } else if (
      !this.tableStructure.header.row &&
      !this.tableStructure.body.row
    ) {
      return false;
    } else {
      return true;
    }
  }

  hasPagination(): boolean {
    if (!this.tableStructure.params) {
      return false;
    } else if (!this.tableStructure.params.pagination) {
      return false;
    } else if (!this.tableStructure.params.pagination.usePagination) {
      this.tableStructure.params.pagination.usePagination = false;
      return false;
    } else {
      return true;
    }
  }
}
