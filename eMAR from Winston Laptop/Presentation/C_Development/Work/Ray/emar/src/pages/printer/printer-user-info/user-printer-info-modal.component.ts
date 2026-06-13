import { Component, OnInit, Input } from '@angular/core';
import { ModalService } from 'src/services/modal.service';
import { CartService } from 'src/services/cart.service';
import { CartStoreService } from 'src/services/cart-store.service';
import { UserStoreService } from 'src/services/user-store.service';
import { SiteStoreService } from 'src/services/site-store.service';
import { PrinterService } from 'src/services/printer.service';
import { PrinterInformation } from '../../../app/interfaces/printer-information';
import html2pdf from 'html2pdf.js';
import html2canvas from 'html2canvas';
import { PatientStoreService } from 'src/services/patient-store.service';
import { Patient } from 'src/app/interfaces/patient';
import { PatientMedOrderService } from 'src/services/patient-med-order.service';
import { PatientMedOrderStoreService } from 'src/services/patient-med-order-store.service';
import { Order } from 'src/app/interfaces/order';
import { createImmediatelyInvokedFunctionExpression } from 'typescript';

// called from PatientDashboardComponent

@Component({
  selector: 'app-user-printer-info-modal',
  templateUrl: './user-printer-info-modal.component.html',
  styleUrls: ['./user-printer-info-modal.component.scss']
})
export class UserPrinterInfoModalComponent implements OnInit {
  @Input() modalTitle: string;
  @Input() patient: Patient;
  patientId: number;
  isDone: boolean = false;
  isProcessing: boolean = false;
  isSuccess: boolean = false
  hasError: boolean = false
  errorMessage: string;
  userId: number;
  defaultSitePrinter: any;
  siteId: any;
  siteName: any;
  userDisplayName: any;
  userLastPrinterUsedId: any;
  documentType: any;
  printerInformation: any;
  private userUrl = 'api/devices/devices';
  zoom: number;
  zoomAdjustment: any;
  reportContainer: any;
  reportSection: any;
  reportSectionTitle: any;
  reportSectionTime: any;
  parentNodeReportPart1: any;
  parentNodeReportPart2: any;
  parentNodeReportPart1Clone: any;
  parentNodeReportPart2Clone: any;
  imageWidth: number;
  windowInnerWidth: number;
  imageHeight: number;
  imageX: number;
  imageY: number;
  windowOuterHeight: number;
  windowOuterWidth: number;
  adjustedZoom: number;
  xxxx: string;
  lastPrinterUsedDescription: string;
  printerAddressType: string;
  printAddress: string;
  printerId: number;
  printerDescription: string;
  printType: number;
  printRoute = [];
  eligiblePrintDestinations = [];
  patientAllergies: string;
  allPatientOrders: object = {};
  orders: Order[];
  printRteNameValueId = [];
  printerSelectList = [];
  newPageCount: number;
  maxMedicinePageLineCount: number = 6;
  initialMaxMedicinePageLineCount: number = 6;
  originalPxSize: number = 12;
  updatedPxSize: number = 18;
  timingsAllColsWidthPercent: number = .98;
  firstPageInd: boolean = true;
  iconIteractionsSize: number = 0.0;
  totalMedicinesFound: number = 0;
  additionalRowsDueToAlleries = 0;
  footerRowAdded: boolean = false;

  header1AnchorTimingsLeft: number = 436;   // offset from th + d + pd-relaative 386 + 50 TEXT PRIMNMARY
  header2AnchorTimingsLeft: number = 810;   // selectColumnsIdMedicine: medicine + std + prn + ACT.
  nbrOfPatientAllergies: number = 0;
  selectColumnsIdMedicineMaxWidth: number = 624;
  selectColumnsIdMedicineWidthDiff: number = 0;
  selectColumnsTextPrimaryWidth: number = 0;
  selectLargeColHeaderForActivityWidthLeft: number = 0;
  selectColumsNewActivitySize: number = 260;
  colHeadingLeftAdjustedBegin: number = 0;
  colSpacePixelsLeft: number = 0;
  colSpacePixelsAdjustLeft: number = 0;
  colSpacePixelsRight: number = 1;
  HeightLinesOfMedicineDescription: number = 5;
  foundLastPrinterUsed: boolean = false;

  constructor(
    private modalService: ModalService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private printerService: PrinterService,
    private siteStoreService: SiteStoreService,
    private patientStoreService: PatientStoreService,
    private patientMedOrderStoreService: PatientMedOrderStoreService,
    private patientMedOrderService: PatientMedOrderService,

  ) {

  }

  ngOnInit(): void {
    this.isProcessing = false
    this.isSuccess = false
    this.hasError = false
    this.errorMessage = ''
    this.isDone = false
    this.userId = this.userStoreService.userId;
    this.userDisplayName = (this.userStoreService.user.displayName) ? this.userStoreService.user.displayName : " not assigned";
    this.userLastPrinterUsedId = (this.userStoreService.LAST_USED_PRINTER) ? this.userStoreService.LAST_USED_PRINTER : " not located";
 //   this.defaultSitePrinter = (this.siteStoreService.default_printer_id) ? this.siteStoreService.default_printer_id : 0;
    this.siteId = (this.siteStoreService.site.id) ? this.siteStoreService.site.id : 0;
    this.siteName = (this.siteStoreService.site.name) ? this.siteStoreService.site.name : " not assigned";
    this.documentType = "Send to PulseMail";
    this.printerService.getPrinterInfo(this.siteId, this.userId).subscribe(data => {
      this.printerInformation = data;
      let lPix = 0;
      let lPrn = 0;
      let fPrn = false;


      // alert( this.printerInformation.length);
      // console.log("..Info. Print Service: !!!!!!!!!!!!!!!begin!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      // let articles = document.getElementsByTagName('*');
      // for (let i = 0; i <  articles.length; i++) {
      //   console.log(articles[i]);
      // }
      // console.log("..Info. Print Service: !!!!!!!!!!!!!!!end!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      // while(lPrn < this.printerInformation.length && !fPrn) {

      // Retrieve printer information from the services noted at the beginning of ngoninit
      this.lastPrinterUsedDescription = "_____________________";
      // while (lPix < this.printerInformation.length ) {
      //   if (this.foundLastPrinterUsed) { break }
      //   if (this.printerInformation[lPrn].isLastUsed == true) {
      //       this.userLastPrinterUsedId = this.printerInformation[lPix].id;
      //       this.lastPrinterUsedDescription = this.printerInformation[lPix].description.trim();
      //       this.foundLastPrinterUsed = true;
      //   }
      //   lPix++
      // }
      var printRteTemp = [];
      // Loop through selection and populate the list
      while (lPrn < this.printerInformation.length) {
      //  alert(this.printerInformation[lPrn].isLastUsed 
      //   + " ... " + this.printerInformation[lPrn].description
      //   + " ... " + this.printerInformation[lPrn].id
      //   + " ... " + this.userLastPrinterUsedId.trim());

        if (this.printerInformation[lPrn].id == this.userLastPrinterUsedId.trim()

          && this.printerInformation[lPrn].description.length > 0) {
          this.lastPrinterUsedDescription = this.printerInformation[lPrn].description.trim();
        }
        
        if ((this.printerInformation[lPrn].address != null
          || this.printerInformation[lPrn].address.length > 6)
          && this.printerInformation[lPrn].deviceType.toUpperCase() == "I") {
            if (this.printerInformation[lPrn].description == this.lastPrinterUsedDescription){
              this.documentType = "Send to printer";
            }
          this.printerAddressType = "Send to printer";
          this.printRteNameValueId.push({
            type: "Send to printer"
            , name: this.printerInformation[lPrn].description
            , id: this.printerInformation[lPrn].id
          });
          fPrn = true;
          printRteTemp.push("Send to printer");
          this.printerSelectList.push(this.printerInformation[lPrn].description.trim());
          this.printType = PrintType.ipPrinter;
          this.printAddress = this.printerInformation[lPrn].address;
        }
        else if ((this.printerInformation[lPrn].address == null
          || this.printerInformation[lPrn].address.length < 6)
          && this.printerInformation[lPrn].deviceType.toUpperCase() == "I") {
            if (this.printerInformation[lPrn].description == this.lastPrinterUsedDescription){
              this.documentType = "Invalid";
            }
          this.printerAddressType = "Invalid";
          this.printRteNameValueId.push({
            type: "Invalid"
            , name: this.printerInformation[lPrn].description
            , id: this.printerInformation[lPrn].id
          });
          this.printType = PrintType.invalid;
          this.printAddress = " ";
        }
        else if (this.printerInformation[lPrn].deviceType.toUpperCase() == "D") {
          if (this.printerInformation[lPrn].description == this.lastPrinterUsedDescription){
            this.documentType = "Send to PulseMail";
          }
          this.printerAddressType = "Send to PulseMail";
          this.printRteNameValueId.push({
            type: "Send to PulseMail"
            , name: this.printerInformation[lPrn].description
            , id: this.printerInformation[lPrn].id
          });
          fPrn = true;
          printRteTemp.push("Send to PulseMail");
          this.printerSelectList.push(this.printerInformation[lPrn].description.trim());
          this.printType = PrintType.pdfPrinter;
          this.printAddress = this.printerInformation[lPrn].address;
        }
        else if ((this.printerInformation[lPrn].address != null
          || this.printerInformation[lPrn].address.length > 1)
          && this.printerInformation[lPrn].deviceType.toUpperCase() == "W") {
            if (this.printerInformation[lPrn].description == this.lastPrinterUsedDescription){
              this.documentType = "Send to file";
            }
          this.printerAddressType = "Send to file";
          this.printRteNameValueId.push({
            type: "Send to file"
            , name: this.printerInformation[lPrn].description
            , id: this.printerInformation[lPrn].id
          });
          fPrn = true;
          printRteTemp.push("Send to file");
          this.printerSelectList.push(this.printerInformation[lPrn].description.trim());
          this.printType = PrintType.file;
          this.printAddress = this.printerInformation[lPrn].address;
        }
        else {
          this.documentType = "Invalid";
          this.printerAddressType = "Invalid";
          this.printType = PrintType.invalid;
        }


        lPrn++;
      }
      var iDev = 0;
      for (iDev = 0; iDev < printRteTemp.length; iDev++) {
        if (!this.printRoute.includes(printRteTemp[iDev])) {
          this.printRoute.push(printRteTemp[iDev]);
        }
      }
      console.log("..Info. Print Service: Selected printer: " + this.lastPrinterUsedDescription +
        "  at address or file: " + this.printerAddressType);
      if (this.printerInformation.length < 1) { this.lastPrinterUsedDescription = "no designated printer"; }
      if (!fPrn) { this.lastPrinterUsedDescription = "select a printer"; }
      if (fPrn && this.lastPrinterUsedDescription.length == 0) { this.lastPrinterUsedDescription = "select a printer" };
    });
    console.log("..Info. Print Service: UserPrinterInfoModal - _init() completed");
  }

  selectChangePrinterHandler(printList: any) {
    // user has selected a printer from the drop down, html is passing $event
    var target = printList.target.innerHTML;
    this.lastPrinterUsedDescription = target;
    let lPrn = 0;
    let fPrn = false;
    while (lPrn < this.printerInformation.length && !fPrn) {
      if (this.printerInformation[lPrn].description.trim() == target.trim()) {
        if (this.printerInformation[lPrn].deviceType.toUpperCase() == "D") {
          this.printerAddressType = "Send to PulseMail";
          this.printType = PrintType.pdfPrinter;
          this.printerId = this.printerInformation[lPrn].id;
          this.printerDescription = this.printerInformation[lPrn].description;
          this.printAddress = this.printerInformation[lPrn].address;
        }
        else if (this.printerInformation[lPrn].deviceType.toUpperCase() == "W") {
          this.printerAddressType = "Send to file";
          this.printType = PrintType.file;
          this.printerId = this.printerInformation[lPrn].id;
          this.printerDescription = this.printerInformation[lPrn].description;
          this.printAddress = this.printerInformation[lPrn].address;

        }
        else if (this.printerInformation[lPrn].deviceType.toUpperCase() == "I") {
          this.printerAddressType = "Send to printer";
          this.printType = PrintType.ipPrinter;
          this.printerId = this.printerInformation[lPrn].id;
          this.printerDescription = this.printerInformation[lPrn].description;
          this.printAddress = this.printerInformation[lPrn].address;

        }
        fPrn = true;
      }

      else {
        this.printerAddressType = "Invalid";
        this.printType = PrintType.invalid;
      }
      lPrn++;
    }
    console.log("..Info. Print Service: Selected printer: " + this.lastPrinterUsedDescription +
      " ... printer id: " + this.printerId +
      " ... this description: " + this.printerInformation[lPrn - 1].description +
      " ... target: " + target +
      " ...  at address or file: " + this.printerAddressType)
  }
  selectChangeDocumentTyperHandler(documentList: any) {
    //user has selected a type of document to be printed using $event
   // alert("Hello");
    var iPType = 0;
    for (iPType = 0; iPType < this.printerSelectList.length; iPType++) {
      this.printerSelectList[iPType] = " ";
    }

    var target = documentList.target.innerHTML;
    this.documentType = target.trim();
    var iDType = 0;
    for (iDType = 0; iDType < this.printRteNameValueId.length; iDType++) {
      // alert(this.printRteNameValueId[iDType].type);
      if (this.printRteNameValueId[iDType].type == this.documentType) {
        this.printerSelectList[iDType] = this.printRteNameValueId[iDType].name;
      }
      else {
        this.printerSelectList[iDType] = "zzzzzzzzzz";
      }
    }
    if (this.printerSelectList.length > 0) {
      this.printerSelectList.sort();
    }
    var idx1 = this.printerSelectList.indexOf("zzzzzzzzzz");
    if (idx1 !== -1) {
      this.printerSelectList.splice(idx1, this.printerSelectList.length);
      // var idx2=0;
      // for (idx2 = idx1; idx2 < this.printerSelectList.length;idx2++ )
      // {this.printerSelectList.splice(idx2, 1);}
    }
    // console.log(this.printerSelectList);
  }
  printReport(pageId: string) {
    let locateLastPrinter
    = (this.userStoreService.LAST_USED_PRINTER) ? this.userStoreService.LAST_USED_PRINTER : "0";
 
    if (this.printerId == null) {
      try {
      this.printerId = parseInt(locateLastPrinter);
      }
      catch (error)
      
       {this.printerId = 0;}
    }
   
    if (this.lastPrinterUsedDescription != "_____________________") {
      console.log('..Info. Print Service: Printing patient document: ' +
        ' Document Type: ' + this.documentType +
        ', Page Identifier: ' + pageId +
        ', User Name:  ' + this.userDisplayName +
        ', User Id:  ' + this.userId +
        ', Site Name:  ' + this.siteName +
        ', Site Id:  ' + this.siteId
      );
      this.modalService.close('userPrinterInfo');
      var reportType: string = this.documentType.toLowerCase();
      switch (this.printType) {
        case PrintType.pdfPrinter: {
          console.log('..Info. Print Service: User has selected to pdf print');
          this.createReportPdf(pageId);
          break;
        }
        case PrintType.ipPrinter: {
          console.log('..Info. Print Service: User has selected to pdf export');
          this.createReportPdf(pageId);
          break;
        }
        case PrintType.file: {
          console.log('..Info. Print Service: User has selected to pdf file');
          this.createReportPdf(pageId);
          break;
        }
        case PrintType.jpegPrinter: {
          console.log('..Info. Print Service: User has selected to jpeg print');
          this.createReportJpeg(pageId);
          break;
        }
        case PrintType.jpegExport: {
          console.log('..Info. Print Service: User has selected to jpeg export');
          this.createReportJpeg(pageId);
          break;
        }
        case PrintType.tiffPrinter: {
          console.log('..Info. Print Service: User has selected to tiff print');
          break;
        }
        case PrintType.tiffExport: {
          console.log('..Info. Print Service: User has selected to pdf tiff export');
          break;
        }
        case PrintType.pngPrinter: {
          console.log('..Info. Print Service: User has selected to png print');
          break;
        }
        case PrintType.pngExport: {
          console.log('..Info. Print Service: User has selected to png export');
          break;
        }
        default: {
          console.log('..Info. Print Service: Printing patient document error. Improper document type for: ' +
            ' Document Type: ' + this.documentType +
            ', Page Identifier: ' + pageId)
        }
          break;
      }
    }
    else { console.log("..Info. Print Service: User has not selecteda printer destination"); }
  }
  printReportxJpeg(pageId: string) {
    const ePageId = "#" + pageId;
    var reportSection = document.querySelector(ePageId);
    //  alert("height: "+ reportSection.scrollHeight + "     width: " + reportSection.scrollWidth);
    const imgConverted = document.querySelector(ePageId) as HTMLImageElement;
    const rptCanvas2 = document.querySelector(ePageId) as HTMLCanvasElement;
    html2canvas(document.querySelector(ePageId)).then(

      canvas => {

        var imgData = canvas.toDataURL("image/jpeg", 1.0),
          imageTimeout: 15000;
        html2pdf(reportSection, {
          jsPDF: {

            format: 'a4',
            orientation: "landscape",
            height: reportSection.scrollHeight,
            width: reportSection.scrollWidth
          },
          imageType: 'image/jpeg',
          output: './pdf/generate.jpeg'
        });
      }
    )
  }

  createReportJpeg(pageId: string) {
    const ePageId = "#" + pageId;

    html2canvas(document.querySelector(ePageId)).then(canvas => {
      document.body.appendChild(canvas);
      const rptCanvas2 = document.getElementsByTagName("canvas")[0] as HTMLCanvasElement;
      const dataURI = rptCanvas2.toDataURL("image/jpeg", 1.0);
      console.log('..Info. Print Service: ' + dataURI);
      rptCanvas2.remove();
    });
    //  const ePageId = "#" + pageId;
    const imgConverted = document.querySelector(ePageId) as HTMLImageElement;
    //   const rptCanvas2 = document.querySelector(ePageId) as HTMLCanvasElement;
    //  const dataURI = rptCanvas2.toDataURL();
    //  console.log(dataURI);
    //   imgConverted.src = dataURI;
    //  // Here you can do your POST or upload (see previous methods for either)

    //  document.getElementById("imgConverted").remove();
    //  // document.getElementById("rptCanvas").remove()
    //  /*
    //  Once canvas-to-tiff has been installed:

    //  CanvasToTIFF.toDataURL(canvas, function(uri) {
    //    // uri is a Data-URI that can be used as source for images etc.
    //    // uri = "data:image/tiff;base64, ...etc...";

    //    requires the use of the use of CanvasTo Tiff, refer to method :
    //    createDataReportsTiff
    //  });
    //  */
  }
  createReportPng(pageId: string) {
    const ePageId = "#" + pageId;

    html2canvas(document.querySelector(ePageId)).then(canvas => {
      document.body.appendChild(canvas);
      const rptCanvas2 = document.getElementsByTagName("canvas")[0] as HTMLCanvasElement;
      const dataURI = rptCanvas2.toDataURL();
      console.log('..Info. Print Service: ' + dataURI);
      rptCanvas2.remove();
    });
    //  const ePageId = "#" + pageId;
    const imgConverted = document.querySelector(ePageId) as HTMLImageElement;
    //   const rptCanvas2 = document.querySelector(ePageId) as HTMLCanvasElement;
    //  const dataURI = rptCanvas2.toDataURL();
    //  console.log(dataURI);
    //   imgConverted.src = dataURI;
    //  // Here you can do your POST or upload (see previous methods for either)

    //  document.getElementById("imgConverted").remove();
    //  // document.getElementById("rptCanvas").remove()
    //  /*
    //  Once canvas-to-tiff has been installed:

    //  CanvasToTIFF.toDataURL(canvas, function(uri) {
    //    // uri is a Data-URI that can be used as source for images etc.
    //    // uri = "data:image/tiff;base64, ...etc...";

    //    requires the use of the use of CanvasTo Tiff, refer to method :
    //    createDataReportsTiff
    //  });
    //  */
  }
  async createReportPdf(pageId: string) {
    var fromToDateTime: string = "MM-DD-CCYY HH:MM - MM-DD-CCYY HH:MM";
    var indexOfHeader: number = 0;
    var nbrOfPages = 1;

    if (window.parent != window.top) {
      console.log('..Info. Print Service: ' + 
        'Printing patient document error. Function improperly executed for: ' +
        ' Document Type: ' + this.documentType +
        ', Page Identifier: ' + pageId)
    }
    else {



      var xternDatesTimes = document.querySelectorAll(".printer-dates-times");
      if (xternDatesTimes.length > 1) {
        fromToDateTime = xternDatesTimes[0].innerHTML + " /&nbsp; "
          + xternDatesTimes[xternDatesTimes.length - 1].innerHTML;
      }


      // locate icons in div containing all of medicine information  
      var selectColumnsWithDrugInteractions = document.
        getElementsByClassName("drug-dot");
      var selectColumnsWithAllergyInteractions = document.
        getElementsByClassName("allergy-dot");
        var selectColumnsWithPlaceholder = document.
        getElementsByClassName("placeholder-dot");        
      if (selectColumnsWithDrugInteractions.length > 0) {
        this.iconIteractionsSize = 36.0;
        console.log("..Info. Print Service: Found drug interactions: " 
        + selectColumnsWithDrugInteractions.length);
        var selectColumnsWithAllergyInteractions = document.
          getElementsByClassName("allergy-dot");
        if (selectColumnsWithAllergyInteractions.length > 0) {
          this.iconIteractionsSize = this.iconIteractionsSize * 2;
          console.log("..Info. Print Service: found allergy interactions: " 
          + selectColumnsWithAllergyInteractions.length);
        }
      }
      

      else if (selectColumnsWithAllergyInteractions.length > 0) {
          this.iconIteractionsSize = this.iconIteractionsSize = 36.0;
          console.log("..Info. Print Service: Found allergy interactions: " 
          + selectColumnsWithAllergyInteractions.length);
        }
        else if (selectColumnsWithPlaceholder.length > 0) {
          this.iconIteractionsSize = this.iconIteractionsSize = 36.0;
          console.log("..Info. Print Service: Found place holder: " 
          + selectColumnsWithAllergyInteractions.length);
        }

      let reportSectionAddition = document.createElement("div");
      let divOffset = document.getElementById("orderNameCol0").offsetLeft +
        document.getElementById("orderNameCol0").offsetWidth;
      this.getWindowSize();
      //We use this to get the HTML elements noted in the first part of the parameter list
      // that have an ID that matches the second parameter

      var reportSections = this.locateReportSections("div, thead, th, tr", "page-content-id");
      if (reportSections.length > 0) {
        var reportPart = [];
        // locate the largest container holding the information our report requires
        this.reportContainer = document.getElementById("page-report-container");
        // Start contruction of our report using a division
        this.reportSection = document.createElement("div");
        this.reportSection.id = "reportSection";

        this.reportSectionTitle = document.createElement("div");
        this.reportSectionTitle.id = "reportSectionTitle";
        this.reportSectionTitle.style = "color: red; margin: auto;  font-size: 24px;" +
          "width: 50%; border-top: 2px solid gray; padding: 10px;  text-align: center;";
        this.reportSectionTitle.innerHTML = "Patient Medication Administration Record";

        this.reportSectionTime = document.createElement("div");
        this.reportSectionTime.id = "reportSectionTime";
        this.reportSectionTime.style = "color: black; margin: auto;  font-size: 16px;" +
          "width: 50%; border-bottom: 2px solid gray; padding: 10px;  text-align: center;";
        this.xxxx = "yyyyyy";
        this.reportSectionTime.innerHTML = fromToDateTime;



        // Append it to the report container (note: this will be removed later)
        this.reportContainer.appendChild(this.reportSection);
        // Iterate through all of the html elements found so they can be used to construct our report


        var idx = 0;
        // access patient information                        
        this.patientStoreService.fetchPatient(this.patientStoreService.patientId);
        this.patient = this.patientStoreService.patient;
        while (idx < reportSections.length) {
          // check if this is an add whole selection with matching html id: -aws-
          if (reportSections[idx].id.indexOf("-aws-") > 0) {
            reportPart[idx] = reportSections[idx].cloneNode(true);
            if (reportSections[idx].id.indexOf("-aws-logo") > 0) {
              reportPart[idx].append(this.reportSectionTitle);
              reportPart[idx].append(this.reportSectionTime);
              this.reportSection.append(reportPart[idx]);
              indexOfHeader = idx;
            }
            if (reportSections[idx].id.indexOf("-aws-01") > 0) {
              reportPart[idx].childNodes[0].style.font = "24px Verdana,serif";
              reportPart[idx].childNodes[0].style.fontWeight = "500";
              reportPart[idx].style.height = "42px";
              reportPart[idx].style.padding = "1px";
              reportPart[idx].style.backgroundColor = "white";
            }

            if (reportSections[idx].id.indexOf("-aws-03") > 0) {
              // need to add birthdate to demographic (sex age)   
              // and reverse age and sex
              var reverseAgeSex = (reportPart[idx].childNodes[0].childNodes[0].innerText.
                substr(reportPart[idx].childNodes[0].childNodes[0].innerText.length - 1)
                + "\u00A0"
                + reportPart[idx].childNodes[0].childNodes[0].innerText.
                  substr(0, reportPart[idx].childNodes[0].childNodes[0].innerText.length - 1)
              );
             
              var demoInfoBirthDate = "DOB:\u00A0" +
                "__"  + "/" +
                "__"  + "/" +
                "____"  + "\u00A0";
              if (this.patient.dateOfBirth !== null) {
              if (this.patient.dateOfBirth.length >=10) {
 
                demoInfoBirthDate = "DOB:\u00A0" +
                this.patient.dateOfBirth.substr(5, 2) + "/" +
                this.patient.dateOfBirth.substr(8, 2) + "/" +
                this.patient.dateOfBirth.substr(0, 4) + "\u00A0";
                console.log("..Info. This patient " + this.patient.id + " had date of birth as " 
                + demoInfoBirthDate);
              }
            }
            else {
              console.log("..Info. This patient " + this.patient.id + " had no date of birth" 
              + ". Used default date " + demoInfoBirthDate);
            }
            
              reportPart[idx].childNodes[0].childNodes[0].innerText =
                demoInfoBirthDate + "\u00A0" + reverseAgeSex;

              reportPart[idx].childNodes[0].childNodes[0].style.fontSize = "20px";
              reportPart[idx].childNodes[0].childNodes[0].style.margin = "6px";
              reportPart[idx].childNodes[0].childNodes[1].style.margin = "6px";
              reportPart[idx].style.fontSize = "20px";
              reportPart[idx].style.height = "62px";
              reportPart[idx].style.padding = "1px";
              // reportPart[idx].style.backgroundColor="red";
              // reportPart[idx].style.postition="absolute";
              // reportPart[idx].style.top="20px";
              // reportPart[idx].style.left="300px";

            }
            else if (reportSections[idx].id.indexOf("-aws-02") > 0) {
              reportPart[idx].style.fontSize = "20px";
              reportPart[idx].style.backgroundColor = "white";


              // get allergy information
              // first locate allergy information about the patient

              var allergyItem = "";
              this.nbrOfPatientAllergies = 0;
              console.log("..Info. Print Service: patientStoreService.patientAllergies: "
                + this.patientStoreService.patientAllergies.length);
              if (this.patientStoreService.patientAllergies.length > 0) {
                this.nbrOfPatientAllergies = this.patientStoreService.patientAllergies.length;                
                if (this.patientStoreService.patientAllergies.length >= this.HeightLinesOfMedicineDescription) {
                  // this.nbrOfPatientAllergies = this.patientStoreService.patientAllergies.length;
                  this.additionalRowsDueToAlleries = this.nbrOfPatientAllergies / this.HeightLinesOfMedicineDescription ;
                  if (this.nbrOfPatientAllergies % this.HeightLinesOfMedicineDescription > 0) {
                    this.additionalRowsDueToAlleries++;
                  }
                }
                else if (this.patientStoreService.patientAllergies.length >= 1) {
                  this.additionalRowsDueToAlleries = 1;
                }
                allergyItem = "<table style='border: 1px solid black;" +
                  "font-size: " + this.updatedPxSize +
                  "px; margin-top: 9px; margin-bottom: 6px;'>" +
                  "<tr style='font-size:16px;'><th>Allergy</th><th>Severity</th><th>Reaction</th></tr><tr>";
                for (let i = 0; i < this.patientStoreService.patientAllergies.length; i++) {
                  allergyItem =
                  allergyItem +
                  "<td>" +
                  this.patientStoreService.patientAllergies[i].name + "</td>" +
                  "<td>" +
                  this.patientStoreService.patientAllergies[i].severity + "</td>" +
                  "<td>" +
                  this.patientStoreService.patientAllergies[i].reaction + "</td></tr>"
                }
                this.patientAllergies =
                  allergyItem +
                  "</tr></table>"
              }
              else { this.patientAllergies = " "; }
              reportPart[idx].innerHTML += (this.patientAllergies);
              console.log("..Info. Print Service: additionalRowsDueToAlleries: "
                + this.additionalRowsDueToAlleries);

              // get medications prescribed for the patient
              this.allPatientOrders[this.patient.id] = await this.getPatientOrders(this.patient.id);
              var patientMedicineTableLength = this.allPatientOrders[this.patient.id].length;
            }
            else {

              //           Dont do anything if no data

              // locate the column containing medicine information                        
              var pageReportContainerTbodyTrMedicines = reportPart[idx].
                querySelectorAll("tbody > tr "), iTbTr: number = 0;
              var deleteMedicineRecord: boolean = false;
              this.totalMedicinesFound = pageReportContainerTbodyTrMedicines.length;
              if (pageReportContainerTbodyTrMedicines.length > 0) {
                // get key information about the medicine being administered                          
                var medObjX = 0;
                var medLookup = 0;
                // administeed medicines
                var StringOneToSearchFor = 'ng-reflect-ng-class="object Object"> ';
                // canceled medicines
                var StringTwoToSearchFor = 'class="pd-cancelled"> ';
                // ordered medicine date time  
                var StringThreeToSearchFor = 'Ordered:</i></small> ';
                var moreMedicineInfo = " ";
                // search for medicines in the browser
                for (medObjX = 0; medObjX < pageReportContainerTbodyTrMedicines.length; medObjX++) {
                  var medNameFound = " ";
                  var medOrderedTimeDateFound = "00:00 00/00/0000";
                  moreMedicineInfo = " ... ";
                  var beforeMedicineTdHtml = pageReportContainerTbodyTrMedicines[medObjX].childNodes[0].innerHTML;
                  var str = beforeMedicineTdHtml.replace(/[\[\]']+/g, '');
                  // start search for medicines in the browser
                  var medObjXx = str.search(StringOneToSearchFor);
                  if (medObjXx > 0) {
                    var medObjXy = medObjXx + StringOneToSearchFor.length;
                    if (medObjXy > 0) {
                      // alert("2");
                      var medObjXz = str.indexOf('</span>', medObjXy);
                      if (medObjXz > 0) {


                        // search for this medicine start time for this patient in the browser
                        var medObjYx = str.search(StringThreeToSearchFor);
                        if (medObjYx > 0) {

                          var medObjYy = medObjYx + StringThreeToSearchFor.length;
                          if (medObjYy > 0) {
                            var medObjYz = str.indexOf('. </span>', medObjYy);
                            if (medObjYz > 0) {

                              medNameFound = str.substring(medObjXy, medObjXz);
                              medOrderedTimeDateFound = str.substring(medObjYy, medObjYz);
                              for (medLookup = 0; medLookup < patientMedicineTableLength; medLookup++) {
                                // compare medicine names
                                var tableMedicineLookup = this.allPatientOrders[this.patient.id][medLookup].displayName;
                                var medCompLength = ((medNameFound.length
                                  <= tableMedicineLookup.length)
                                  ? medNameFound.length : tableMedicineLookup.length);
                                if (medCompLength > 60) { medCompLength = 60 }
                                // compare browser ordered date to API add date
                                var medApiAddDateYear = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(0, 4);
                                var medApiAddDateMonth = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(5, 2);
                                var medApiAddDateDay = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(8, 2);
                                var medApiAddTimeHour = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(11, 2);
                                var medApiAddTimeMinute = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(14, 2);
                                var medAddTimeDate = medApiAddTimeHour + ":" + medApiAddTimeMinute + " " +
                                  medApiAddDateMonth + '/' + medApiAddDateDay + '/' + medApiAddDateYear;


                                // console.log("..Info. Print Service: medAddTimeDate: "+ medAddTimeDate + "   to   " + "medOrderedTimeDateFound: " + medOrderedTimeDateFound)
                                if ((medNameFound.substring(0, medCompLength) == this.allPatientOrders[this.patient.id][medLookup]
                                  .displayName.substring(0, medCompLength))
                                  && (medAddTimeDate == medOrderedTimeDateFound)
                                ) {

                                  var medApiBegDateYear = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(0, 4);
                                  var medApiBegDateMonth = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(5, 2);
                                  var medApiBegDateDay = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(8, 2);
                                  var medApiBegTimeHour = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(11, 2);
                                  var medApiBegTimeMinute = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(14, 2);
                                  var medBegTimeDate = medApiBegDateMonth + '/' + medApiBegDateDay + '/' + medApiBegDateYear + " " +
                                    medApiBegTimeHour + ":" + medApiBegTimeMinute;


                                  // commented out the dose/route/frequency because now being done on the screen presentation                          
                                  moreMedicineInfo = "<span> Start date/time: " +
                                    // this.allPatientOrders[this.patient.id][medLookup].displayDose +
                                    // "  " + this.allPatientOrders[this.patient.id][medLookup].displayDoseUnit + 
                                    // ": " + 
                                    // this.allPatientOrders[this.patient.id][medLookup].displayRoute +
                                    // ": " + 
                                    // this.allPatientOrders[this.patient.id][medLookup].displayFrequency +
                                    // "</span><br><span>Start date/time: " + 
                                    medBegTimeDate +
                                    // "  " + 
                                    // this.allPatientOrders[this.patient.id][medLookup].displayFrequency +
                                    "</span> ";
                                }
                                else {
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                  // get key information about the medicine which was canceled                             
                  else if (medObjXx != 0) {

                    var medObjXx = str.search(StringTwoToSearchFor);
                    if (medObjXx > 0) {
                      deleteMedicineRecord = true;
                      var medObjXy = medObjXx + StringTwoToSearchFor.length;
                      if (medObjXy > 0) {

                        var medObjXz = str.indexOf('</span>', medObjXy);
                        if (medObjXz > 0) {


                          // search for this medicine start time for this patient in the browser
                          var medObjYx = str.search(StringThreeToSearchFor);
                          if (medObjYx > 0) {

                            var medObjYy = medObjYx + StringThreeToSearchFor.length;
                            if (medObjYy > 0) {
                              var medObjYz = str.indexOf('. </span>', medObjYy);
                              if (medObjYz > 0) {

                                medNameFound = str.substring(medObjXy, medObjXz);
                                medOrderedTimeDateFound = str.substring(medObjYy, medObjYz);
                                for (medLookup = 0; medLookup < patientMedicineTableLength; medLookup++) {
                                  // copare medicine names
                                  var tableMedicineLookup = this.allPatientOrders[this.patient.id][medLookup].displayName;
                                  var medCompLength = ((medNameFound.length
                                    <= tableMedicineLookup.length)
                                    ? medNameFound.length : tableMedicineLookup.length);
                                  if (medCompLength > 60) { medCompLength = 60 }
                                  // compare browser ordered date to API add date
                                  var medApiAddDateYear = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(0, 4);
                                  var medApiAddDateMonth = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(5, 2);
                                  var medApiAddDateDay = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(8, 2);
                                  var medApiAddTimeHour = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(11, 2);
                                  var medApiAddTimeMinute = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(14, 2);
                                  var medAddTimeDate = medApiAddTimeHour + ":" + medApiAddTimeMinute + " " +
                                    medApiAddDateMonth + '/' + medApiAddDateDay + '/' + medApiAddDateYear;


                                  // console.log("..Info. Print Service: medAddTimeDate: "+ medAddTimeDate + "   to   " + "medOrderedTimeDateFound: " + medOrderedTimeDateFound)
                                  if ((medNameFound.substring(0, medCompLength) == this.allPatientOrders[this.patient.id][medLookup]
                                    .displayName.substring(0, medCompLength))
                                    && (medAddTimeDate == medOrderedTimeDateFound)
                                  ) {

                                    var medApiBegDateYear = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(0, 4);
                                    var medApiBegDateMonth = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(5, 2);
                                    var medApiBegDateDay = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(8, 2);
                                    var medApiBegTimeHour = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(11, 2);
                                    var medApiBegTimeMinute = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(14, 2);
                                    var medBegTimeDate = medApiBegDateMonth + '/' + medApiBegDateDay + '/' + medApiBegDateYear + " " +
                                      medApiBegTimeHour + ":" + medApiBegTimeMinute;


                                    // commented out the dose/route/frequency because now being done on the screen presentation                              
                                    moreMedicineInfo = "<span>Start date/time: " +
                                      // this.allPatientOrders[this.patient.id][medLookup].displayDose +
                                      // "  " + this.allPatientOrders[this.patient.id][medLookup].displayDoseUnit + 
                                      // ": " + 
                                      // this.allPatientOrders[this.patient.id][medLookup].displayRoute +
                                      // ": " + 
                                      // this.allPatientOrders[this.patient.id][medLookup].displayFrequency +
                                      // "</span><br><span>Start date/time: " + 
                                      medBegTimeDate +
                                      // "  " + 
                                      // this.allPatientOrders[this.patient.id][medLookup].displayFrequency +
                                      "</span> ";
                                  }
                                  else {
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                    else { deleteMedicineRecord = false; }
                  }
                  if (!deleteMedicineRecord) {
                    pageReportContainerTbodyTrMedicines[medObjX].childNodes[0].innerHTML
                      = beforeMedicineTdHtml + moreMedicineInfo;
                  }
                  else {
                    pageReportContainerTbodyTrMedicines[medObjX].childNodes[0].innerHTML
                      = beforeMedicineTdHtml + "oops";
                    var eligibleForDelete = pageReportContainerTbodyTrMedicines[medObjX];
                    eligibleForDelete.remove();
                    tableRowLength--;

                  }
                }

              }

              // locate and update the div containing all of medicine information  
              var selectColumnsIdMedicine1 = reportPart[idx].
                getElementsByClassName("pd-relative");
              var locateRow1Col1 = true;
              // locate the adminstration headers and adjust their size
              var selectColumnsIdMedicines = document.
                getElementById("orderNameCol0");
              this.selectColumnsTextPrimaryWidth = selectColumnsIdMedicines.clientWidth;
              var selectColumnsTextPrimary = document.
                getElementsByClassName("text-primary");
              this.selectColumnsTextPrimaryWidth = selectColumnsTextPrimary[1].clientWidth;
              this.selectColumnsIdMedicineWidthDiff = this.selectColumnsIdMedicineMaxWidth - this.selectLargeColHeaderForActivityWidthLeft;
              // total rows found in patients report of medicines
              console.log("..Info. Print Service: Total rows found in patients report of medicines: " + selectColumnsIdMedicine1.length)
              for (var idx6 = 0; idx6 < selectColumnsIdMedicine1.length; idx6++) {
                if (idx6 % 4 == 0 || locateRow1Col1) {
                  // var victory = selectColumnsIdMedicine1[idx6].getBoundingClientRect();
                  // alert("Coordinates: " +victory.left + "px, " + victory.top + "px");
                  // //console.log ("Inner html for medicines row 1 col 1: " + selectColumnsIdMedicine1[idx6].innerHTML);
                  // console.log ("Inner text for medicines row 1 col 1: <" + selectColumnsIdMedicine1[idx6].innerText + ">end") ;
                  // var txtx = selectColumnsIdMedicine1[idx6].innerText;
                  // console.log ("Inner text length for medicines row 1 col 1: " + txtx.length);
                  selectColumnsIdMedicine1[idx6].style.minWidth = this.selectColumnsIdMedicineMaxWidth + "px";
                  selectColumnsIdMedicine1[idx6].style.maxWidth = this.selectColumnsIdMedicineMaxWidth + "px";
 //                 selectColumnsIdMedicine1[idx6].style.background = "lightblue";
                  locateRow1Col1 = false;

                }
                else {
                  // adjust the activity columns to appropriate pixel size
                  selectColumnsIdMedicine1[idx6].style.minWidth = "62px";
                  selectColumnsIdMedicine1[idx6].style.maxWidth = "62px";
  //                selectColumnsIdMedicine1[idx6].style.background = "lightpink";
                }


                //  selectColumnsIdMedicine1[idx6].style.background="red";  
                selectColumnsIdMedicine1[idx6].style.fontSize = this.updatedPxSize + "px";


                var rStyle = selectColumnsIdMedicine1[idx6].getAttribute('style');
                //  console.log("..Info. Print Service: pd-relative style: "+rStyle)
              }
              // var selectColumnsIdMedicine = reportPart[idx].
              // getElementsByClassName("pd-full");
              // for (var idx6=0;idx6 < selectColumnsIdMedicine.length; idx6++) {
              //   // console.log("..Info. Print Service: innerText of pd-relative: " +selectColumnsIdMedicine[idx6].innerHTML)
              //  selectColumnsIdMedicine[idx6].style.background= "orange";
              //  selectColumnsIdMedicine[idx6].style.fontSize= "16px";


              //  var fStyle =selectColumnsIdMedicine[idx6].getAttribute('style');
              //   console.log("..Info. Print Service: full style: "+xStyle)
              // }


              // locate the adminstration headers and adjust their size
              var selectColumnsIdMedicine2 = reportPart[idx].
                getElementsByClassName("pd-line");
              for (var idx6 = 0; idx6 < selectColumnsIdMedicine2.length; idx6++) {
                // selectColumnsIdMedicine2[idx6].style.background= "yellow";
                selectColumnsIdMedicine2[idx6].style.fontSize = this.updatedPxSize + "px";
                // var fStyle =selectColumnsIdMedicine2[idx6].getAttribute('style');
                // console.log("..Info. Print Service: full style: "+fStyle);
              }

              // locate the adminstration headers and adjust their size
              // var selectColumnsIdMedicine3 = document.
              // getElementById("orderNameCol0");
              // alert("orderNameCol0 width: " + selectColumnsIdMedicine3.clientWidth);
              // selectColumnsIdMedicine3.style.background="black";



              // var selectColumnsWithPossibleSched = reportPart[idx].
              // querySelectorAll("table > thead > tr ");
              // if (selectColumnsWithPossibleSched.length > 0){

              // for (var schedObjA=0;schedObjA<selectColumnsWithPossibleSched.length;schedObjA++) {
              //   selectColumnsWithPossibleSched[schedObjA].style.backgroundColor="pink";
              //   var fStyle =selectColumnsWithPossibleSched[schedObjA].getAttribute('style');
              //   console.log("..Info. Print Service: table > thead > tr : "+fStyle)
              // }
              //                       }

              // var selectColumnsWithPossibleSched = reportPart[idx].
              // querySelectorAll("tbody > tr > td > div > div ");
              // if (selectColumnsWithPossibleSched.length > 0){

              // for (var schedObjA=0;schedObjA<selectColumnsWithPossibleSched.length;schedObjA++) {
              //   selectColumnsWithPossibleSched[schedObjA].style.backgroundColor="pink";
              //   // setAttribute("style","background: green;");
              //   var fStyle =selectColumnsWithPossibleSched[schedObjA].getAttribute('style');
              //   console.log("..Info. Print Service: tr > td > div > div: "+fStyle)
              // }
              //                       }

              // locate and update the div containing all of the grid lines to print 8+ hours on the page
              var selectColumnsIdMedicine4 = reportPart[idx].
                getElementsByClassName("ie-special");

              for (var idx6 = 0; idx6 < selectColumnsIdMedicine4.length; idx6++) {
                // alert(selectColumnsIdMedicine4[0].childElementCount);
                // alert (selectColumnsIdMedicine4[idx6].childNodes[1].offsetTop);
                // var rect = selectColumnsIdMedicine4[idx6].childNodes[1].getBoundingClientRect();
                // alert (rect.left + "  " + rect.top+ "  " + rect.width + "  " + rect.height) ;


                // console.log("..Info. Print Service: innerText of pd-relative: " +selectColumnsIdMedicine[idx6].innerHTML)
                //selectColumnsIdMedicine[idx6].setAttribute("style","background: red;");
 //               selectColumnsIdMedicine4[idx6].style.backgroundColor = "lightyellow";
                //                   selectColumnsIdMedicine4[idx6].childNodes[4].style.backgroundColor="orange";
                //  console.log("..Info. Print Service: Viva: " + selectColumnsIdMedicine4[idx6].style);
                var timingsAllColWidth = this.timingsAllColsWidthPercent * 100;
                selectColumnsIdMedicine4[idx6].style.width = timingsAllColWidth + "%";
                //  var fStyle =selectColumnsIdMedicine4[idx6].getAttribute('style');
                //  var xStyle =selectColumnsIdMedicine4[idx6].getBoundingClientRect().width ;
                //  console.log(idx6 + "ie-special style: "+fStyle)
              }

              // locate any scheduled information line for line from the screen                  
              var pageReportContainerTbodyTrSchedules = reportPart[idx].
                querySelectorAll("tbody > tr > td > div > span > div"), iTbTr: number = 0;
              if (pageReportContainerTbodyTrSchedules.length > 0) {
                var schedObjX = 0;
                var StringToSearchFor = 'ng-reflect-ngb-tooltip="';
                for (schedObjX = 0; schedObjX < pageReportContainerTbodyTrSchedules.length; schedObjX++) {
                  var str = pageReportContainerTbodyTrSchedules[schedObjX].innerHTML;
                  var schedObjXx = str.search(StringToSearchFor);
                  if (schedObjXx > 0) {
                    var schedObjXy = schedObjXx + StringToSearchFor.length
                    if (schedObjXy > 0) {
                      var schedObjXz = str.indexOf('"', schedObjXy);
                      if (schedObjXz > 0) {
                        var stringFound = str.substring(schedObjXy, schedObjXz);
                        var medicationScheduleEdited = " ";
                        var borderColor = "black";
                        ;
                        switch (stringFound.toUpperCase().replace(/\s+/g, '')) {
                          case "ACKNOWLEDGED": {
                            medicationScheduleEdited = "A";
                            borderColor = "green";
                            break;
                          }
                          case "ADMINISTERED": {
                            medicationScheduleEdited = "G";
                            borderColor = "green";
                            break;
                          }
                          case "GIVEN": {
                            medicationScheduleEdited = "G";
                            borderColor = "green";
                            break;
                          }
                          case "ONGOING": {
                            medicationScheduleEdited = "O";
                            borderColor = "green";
                            break;
                          }
                          case "SCHEDULED": {
                            medicationScheduleEdited = "S";
                            borderColor = "black";
                            break;
                          }
                          case "DUE": {
                            medicationScheduleEdited = "P";
                            borderColor = "red";
                            break;
                          }
                          case "MISSEDDOSE": {
                            medicationScheduleEdited = "M";
                            borderColor = "gray";
                            break;
                          }
                          case "ONHOLD": {
                            medicationScheduleEdited = "H";
                            borderColor = "gold";
                            break;
                          }
                          default: {
                            medicationScheduleEdited = "?";
                            break;
                          }
                        }
                      }
                    }
                  }
                  else { medicationScheduleEdited = "?" };
                  var html_to_insert = "<span style='border: 1px solid " +
                    borderColor +
                    ";font-weight: 700; font-size: 16px; padding: 2px'>"
                    + medicationScheduleEdited + "</span>";
                  pageReportContainerTbodyTrSchedules[schedObjX].innerHTML = "";
                  pageReportContainerTbodyTrSchedules[schedObjX].insertAdjacentHTML('beforeend', html_to_insert);

                  //                  console.log(pageReportContainerTbodyTrSchedules[schedObjX].innerHTML);
                  //       var tempElement = document.createElement('span');


                  //           for (var att, i = 0, atts = pageReportContainerTbodyTrSchedules[schedObjX].attributes, 
                  //             n = atts.length; i < n; i++){
                  //               att = atts[i];
                  //               console.log("..Info. Print Service: !!!!!!!!!!!!!!!!!!! attr name: " 
                  //               + att.nodeName + "     value: " + att.nodeValue);
                  //               var scheduleInfo= pageReportContainerTbodyTrSchedules[schedObjX].innerHTML
                  //         //      if (schedObjX == 100) {alert()};
                  //         //       nodes.push(att.nodeName);
                  //         //      values.push(att.nodeValue);
                  //         console.log(pageReportContainerTbodyTrSchedules[schedObjX].innerHTML);
                  //  //       var tempElement = document.createElement('span');

                  //           }
                }
              }




              var tableRowLength = 0;
              var tableRow = reportPart[idx].querySelectorAll("tbody > tr"), eligibleRow: number = 1;
              //           Dont do anything if no data
              if (tableRow.length > 0) {
                tableRowLength = tableRow.length;
                // clone the row header in the patient table
                var templateNewRowMask1 = document.getElementById("page-content-id-amsp-01").cloneNode(true);

                // create default array timings just in case there is no entry later on
                var timings = ['00:00', '00:00', '00:00', '00:00',
                  '00:00', '00:00', '00:00', '00:00']
                // check if there are entries in the timings table column (3)
                // They are string hour minute entries

                if (templateNewRowMask1.childNodes.length > 6) {
                  var txt2 = templateNewRowMask1.childNodes[6].textContent.trim();
                  // for (var idx8 = 0; idx8 < templateNewRowMask1.childNodes.length; idx8++ )
                  //   {
                  //   // alert(templateNewRowMask1.nodeName);
                  //   // alert(templateNewRowMask1.childNodes[idx8].nodeType);
                  //   // alert(templateNewRowMask1.childNodes[idx8].nodeValue);
                  //   // alert(templateNewRowMask1.childNodes[idx8].textContent);
                  //   // alert(templateNewRowMask1.childNodes[idx8].parentNode);
                  //   // alert(templateNewRowMask1.childNodes[idx8].getRootNode);

                  //  }

                  // Remove spaces and escape characters and place the results in the timing array
                  var array2 = new Array();
                  array2 = txt2.split(/[ ,]+/);
                  var ii = 0;
                  var ix = 0;
                  for (ii = 0; ii < array2.length; ii++) {
                    timings[ix] = array2[ii];
                    ix++;
                  }
                }
                // create a clone of the first entry in the table row which will be edited later for non-page 1
                var newRow = tableRow[0].cloneNode(true);
                var newHeader = tableRow[0].cloneNode(true);
                var newSubHeader = tableRow[0].cloneNode(true);
                var newFooter = tableRow[0].cloneNode(true);
                newHeader.setAttribute("style", "color: black; font-weight: 500; font-size: 16px; border-style: none; background: white;");
                newSubHeader.setAttribute("style", "color: black; font-weight: 500; font-size: 16px; border-style: none; background: white;");
                newFooter.setAttribute("style", "color: black; font-weight: 500; font-size: 16px; border-style: none; background: white;");
                // parse through the rows applying information and html to each column
                for (let i = 1; i < newHeader.childNodes.length - 1; i++) {
                  newHeader.childNodes[i].innerHTML = " ";
                  newHeader.childNodes[i].setAttribute("style", "border-style: none; background: white");
                  newSubHeader.childNodes[i].innerHTML = " ";
                  newSubHeader.childNodes[i].setAttribute("style", "border-style: none; background: white");
                  newFooter.childNodes[i].innerHTML = " ";
                  newFooter.childNodes[i].setAttribute("style", "border-style: none; background: white");
                }
                var patientName = document.getElementById("page-content-id-aws-01");
                newHeader.childNodes[0].textContent = patientName.innerText;
                newHeader.childNodes[0].setAttribute("style", "color: black;font-size: 24px;");
                newHeader.childNodes[4].textContent = "Patient Medication Administration Record";
                newHeader.childNodes[4].setAttribute("style", "color: red; margin: auto;  font-size: 24px;" +
                  "width: 50%; border-top: 2px solid gray; padding: 10px;  text-align: center;");
                newSubHeader.childNodes[0].textContent = " ";
                var idx3 = fromToDateTime.indexOf("&nbsp;");
                var convertHtmlToTextDate = fromToDateTime.substr(0, idx3) + "\u00A0"
                  + fromToDateTime.substr((idx3 + 6));
                newSubHeader.childNodes[4].textContent = convertHtmlToTextDate;
                newSubHeader.childNodes[4].setAttribute("style", "color: black; margin: auto;  font-size: 16px;" +
                  "width: 50%; border-bottom: 2px solid gray; padding: 10px;  text-align: center;");
                newFooter.childNodes[0].textContent = " ";

                newFooter.childNodes[4].setAttribute("style", "color: black; margin: auto;  font-size: 15px;" +
                  "width: 50%; border-bottom: 2px solid gray; padding: 10px;  text-align: center;");
                newRow.childNodes[0].textContent = "";
                // move spaces into the first column
                for (let i = 1; i < newRow.childNodes.length - 1; i++) {
                  if (i < 4) {
                    // move the inner text into the new row for the first colums (except the first column)
                    newRow.childNodes[i].textContent =
                      templateNewRowMask1.childNodes[i - 1].textContent
                  }
                  else {
                    console.log("..Info. Print Service: additional page heading being contructed. ");
                    // this is the column that has the timings for subsequent pages not first page
                    // using pre tag and innerHTML to space them appropriately from our timings array
                    console.log("..Info. Print Service: secondary colHeadingLeftAdjustedBegin: " + this.colHeadingLeftAdjustedBegin);
                    console.log("..Info. Print Service: secondary selectColumnsIdMedicineWidthDiff: " + this.selectColumnsIdMedicineWidthDiff);
                    console.log("..Info. Print Service: secondary selectColumsNewActivitySize: " + this.selectColumsNewActivitySize);
                    var startFromPos =  this.selectColumnsIdMedicineWidthDiff
                    + this.selectColumsNewActivitySize
                    + this.colHeadingLeftAdjustedBegin
                    - (this.iconIteractionsSize + this.colSpacePixelsAdjustLeft);
                   startFromPos = 866 + this.colSpacePixelsAdjustLeft;
                   console.log("..Info. Print Service: secondary start from position: " + startFromPos ) ;
                   var timingPrefixHtml = '<div id="timingHtmlSecondary" style="font-weight: 600; font-size: 14px;' +
                      'class=page-content-class-aws-55;' +
                      'position:absolute; left:' + startFromPos + 'px'
                      + ';padding: 2px'
                      + '"><span style="min-width:10px;max-width:10px;color: white";">X</span>';
                    var timingSuffixHtml = '</div>';
                    var timingsFound = "";
                    for (var idx7 = 0; idx7 < timings.length; idx7++) {
                      timingsFound = timingsFound + '<span style=" font-size: 14px;' +
                        'margin-left: ' +
                        (this.colSpacePixelsLeft) + 'px;' +
                        'margin-right: ' +
                        (this.colSpacePixelsRight) + 'px;' +
                        '">' + timings[idx7] + '</span>';
                    }
                    newRow.childNodes[i].innerHTML =
                      timingPrefixHtml +
                      timingsFound +
                      timingSuffixHtml;

                    // console.log("..Info. Print Service: Vix" + newRow.childNodes[i].innerHTML);
                    nbrOfPages++;
                  }
                }
                // output row count
                var pageRowCount = 0;
                // determine how many input new rows that mimic the table row addressed previously
                // example: "tbody > tr"  
                // will be used to determine total number of pages
                this.initialMaxMedicinePageLineCount = this.maxMedicinePageLineCount;
                this.maxMedicinePageLineCount = this.maxMedicinePageLineCount
                  - Math.floor (this.additionalRowsDueToAlleries);
                this.newPageCount = Math.floor(tableRowLength
                  / this.maxMedicinePageLineCount);
                var newRowCountRemainder = tableRowLength % this.maxMedicinePageLineCount;
                var intAdditionalRowsDueToAlleries = Math.floor (this.additionalRowsDueToAlleries);
                console.log("..Info. Print Service: page row counts: "
                + "initialMaxMedicinePageLineCount: " + this.initialMaxMedicinePageLineCount
                + " ... newRowCountRemainder: " + newRowCountRemainder 
                + " ... tableRowLength: "   + tableRowLength       
                + " ... maxMedicinePageLineCount: " + this.maxMedicinePageLineCount
                + " ... additionalRowsDueToAlleries: " + this.additionalRowsDueToAlleries);
                // add the extra page to print remainder of medicines
                if ((newRowCountRemainder != 0 
                      && tableRowLength < this.initialMaxMedicinePageLineCount 
                      - intAdditionalRowsDueToAlleries
                      && intAdditionalRowsDueToAlleries > 0)
                      || 
                      (newRowCountRemainder != 0 
                        && tableRowLength > this.initialMaxMedicinePageLineCount )
                      )
                {
                  this.newPageCount++;
                }
                else if (tableRowLength + intAdditionalRowsDueToAlleries  
                  < this.initialMaxMedicinePageLineCount) {
                   this.newPageCount = 1}
                var newHeaderArray = [];
                var newSubHeaderArray = [];
                var newRowArray = [];
                var newFooterArray = [];
                var newFooterForLessThanOnePageArray = [];
                var pageBreakCount = 0;
                var pageFooterCount = 2;
                var nowRpt = new Date;
                var pageDateTime = "Printed: "
                  + ("0" + (nowRpt.getMonth() + 1)).substr(-2)
                  + "/" + ("0" + nowRpt.getDate()).substr(-2)
                  + "/" + nowRpt.getFullYear()
                  + " " + ("0" + nowRpt.getHours()).substr(-2)
                  + ":" + ("0" + nowRpt.getMinutes()).substr(-2)
                  + ":" + ("0" + nowRpt.getSeconds()).substr(-2)


                var iii = 0;
                // lets create some new rows to be used as column headings in our report
                for (iii = 0; iii < this.newPageCount; iii++) {
                  newHeaderArray.push(newHeader.cloneNode(true));
                  newSubHeaderArray.push(newSubHeader.cloneNode(true));
                  newRowArray.push(newRow.cloneNode(true));
                }
                // lets create some new rows to be used as footers in our report if there is one full page or mutiple pages
                for (iii = 0; iii < this.newPageCount; iii++) {
                  newFooter.childNodes[4].innerHTML =
                    "<span style='float: center' margin-top: 3px; margin-bottom: 3px;>"
                    + "Legend:&ensp; A - Acknowledged,&ensp; G - Given,&ensp;  "
                    + "S - Scheduled,&ensp;  P - Past Due,&ensp;  "
                    + "M - Missed Dose,&ensp;  H - OnHold,&ensp; O - OnGoing" + "</span><br>"
                    + "<span style='float: center' margin-top: 3px; margin-bottom: 3px;>"
                    + "Page " + (iii + 1) + " of " + this.newPageCount
                    + "</span>                    <span style='float: right'>" + pageDateTime + "</span>";
                  newFooterArray.push(newFooter.cloneNode(true));
                  newFooterForLessThanOnePageArray.push(newFooter.cloneNode(true));
                  //          alert(newFooter.childNodes[4].textContent);
                }
                // lets create some new rows to be used as footers in our report if there is one full page or mutiple pages
                if (this.newPageCount < 1) {
                  newFooter.childNodes[4].innerHTML =
                    "<span style='float: center' margin-top: 3px; margin-bottom: 3px;>"
                    + "Legend:&ensp; A - Acknowledged,&ensp; G - Given,&ensp;  "
                    + "S - Scheduled,&ensp;  P - Past Due,&ensp;  "
                    + "M - Missed Dose,&ensp;  H - OnHold,&ensp; O - OnGoing" + "</span><br>"
                    + "<span style='float: center' margin-top: 3px; margin-bottom: 3px;>"
                    + "Page 1 of 1" 
                    + "</span>                    <span style='float: right'>" + pageDateTime + "</span>";
                  newFooterForLessThanOnePageArray.push(newFooter.cloneNode(true));
                  //          alert(newFooter.childNodes[4].textContent);
                }                
                var AdditionalItems = 0;
                while (eligibleRow <= tableRowLength) {
                  //     alert("0:" + i);
                  if (eligibleRow % this.maxMedicinePageLineCount == 0) {

                    pageBreakCount++;
                    //  alert("1:" + pageBreakCount);
                    pageRowCount += this.maxMedicinePageLineCount;
                    //               retreive the records from the array and apply them to the 
                    //               subsequent pages: report header, sub header and trailer
                    tableRow[0].parentNode.insertBefore(newRowArray.pop(),
                      tableRow[0].parentNode.childNodes[pageRowCount + pageBreakCount + AdditionalItems]);
                    tableRow[0].parentNode.insertBefore(newHeaderArray.pop(),
                      tableRow[0].parentNode.childNodes[pageRowCount + pageBreakCount + AdditionalItems++]);
                    tableRow[0].parentNode.insertBefore(newSubHeaderArray.pop(),
                      tableRow[0].parentNode.childNodes[pageRowCount + pageBreakCount + AdditionalItems++]);
                    tableRow[0].parentNode.insertBefore(newFooterArray.shift(),
                      tableRow[0].parentNode.childNodes[(pageRowCount + pageBreakCount
                        + (AdditionalItems - pageFooterCount))]).classList.add("printer-page-brake");;
                        console.log("..Info. Print Service: Breaking: " + eligibleRow + "... this.maxMedicinePageLineCount: " + this.maxMedicinePageLineCount);
                  }

                  eligibleRow++;
                }
                if ((eligibleRow - 1) % this.maxMedicinePageLineCount != 0) {
                  var lastRows = tableRowLength % this.maxMedicinePageLineCount;
                  lastRows = (tableRowLength - 1) - lastRows;
                  //  tableRow[lastRows].classList.add("printer-page-brake");
                  if (eligibleRow < this.maxMedicinePageLineCount
                    && newFooterArray.length > 0) {
                    tableRow[0].parentNode.insertBefore(newFooterArray.shift(),
                      tableRow[0].parentNode.childNodes[pageRowCount + lastRows + pageBreakCount])
                      console.log("..Info. Print Service: Eligible row Before: " 
                      + eligibleRow + "... this.maxMedicinePageLineCount: " + this.maxMedicinePageLineCount);
                  }
                  else if (eligibleRow -1 < this.maxMedicinePageLineCount
                    && newFooterArray.length == 0
                    && newFooterForLessThanOnePageArray.length > 0) {
                    tableRow[0].parentNode.insertBefore(newFooterForLessThanOnePageArray.shift(),
                      tableRow[0].parentNode.childNodes[pageRowCount + lastRows + pageBreakCount])
                      console.log("..Info. Print Service:Last trailer resulting from small page: " 
                      + eligibleRow + "... this.maxMedicinePageLineCount: " + this.maxMedicinePageLineCount);
                  }
                  else {
                    // more lines to print so place footer at end using a null for the old node object
                    tableRow[0].parentNode.after(newFooterArray.shift(),
                      tableRow[0].parentNode.childNodes[pageRowCount + (lastRows 
                        + this.maxMedicinePageLineCount) + pageBreakCount])
                    this.firstPageInd = false;
                    console.log("..Info. Print Service: Eligible row After: " + eligibleRow + "... this.maxMedicinePageLineCount: " + this.maxMedicinePageLineCount);
                    console.log("..Info. Print Service: pageRowCount: " +  pageRowCount +  " ... lastRows: " 
                    + lastRows + " ... pageBreakCount: "  + pageBreakCount);
                  }
                }
              }
              let newMessage = reportPart[idx].innerHTML.replace(/undefined/g, ' ');
              reportPart[idx].innerHTML = newMessage;
              this.reportSection.append(reportPart[idx]);

            }

          }
          // check if this is an add selection but hide with matching html id: -ahs-
          if (reportSections[idx].id.indexOf("-ahs-") > 0) {
            reportPart[idx] = reportSections[idx].cloneNode(true);
            reportPart[idx].style.opacity = "0.0";
            this.reportSection.append(reportPart[idx]);
          }
          // check if this is an add selection but modify with matching html id and class: -ams*-
          // this section of code -amsp- is for the patient page only but can be replicated for
          // other pages with minor changes in the selection criteria
          var activityPrefixHtml = '<span style="font-weight: 500;font-size: 18px;margin: 2px;">';
          var activitySuffixHtml = '</span>';
          if (reportSections[idx].id.indexOf("-amsp-") > 0) {
            //locate the column headigs in the orginal web page
            var selectColumnsTimingsX = document.
              getElementsByClassName("ie-special");
            // get its upper top left positionof activity column
            // adjust for margins to place activity headers
            var colHeadingsRect = selectColumnsTimingsX[0].getBoundingClientRect();
            var additionalPixForMargin = 4;
            if (this.nbrOfPatientAllergies < this.HeightLinesOfMedicineDescription 
                && this.nbrOfPatientAllergies > 0 ) {

                  switch (this.nbrOfPatientAllergies) {
                    case 1: {
                      additionalPixForMargin = 20
                      break;
                    }
                    case 2: {
                      additionalPixForMargin = 14
                      break;
                    }
                    case 3: {
                      additionalPixForMargin = 10
                      break;
                    }          
                    case 4: {
                      additionalPixForMargin = 6
                      break;
                    }            
                    default: {
                      additionalPixForMargin = 4
                      break;
                    }
                }
            }
            var positionOfTopActivity = colHeadingsRect.top + ((this.updatedPxSize + additionalPixForMargin) 
              * this.nbrOfPatientAllergies);
            console.log("..Info. Print Service: Coordinates: left: " + colHeadingsRect.left + "px, top: " + colHeadingsRect.top + "px");
            // builds acivity column headings
            // colHeadingsRectLeftAdjusted is the differences between patiens medicine info
            var colHeadingsRectLeftAdjusted = 624 - colHeadingsRect.left;
            reportPart[idx] = reportSections[idx].cloneNode(true);
            reportPart[idx].children[0].innerHTML = activityPrefixHtml + "STAT" + activitySuffixHtml;
            reportPart[idx].children[0].style = "position:absolute; left:" + "624px"
              + ";top:" + positionOfTopActivity + "px"
              + ";padding: 2px"
              + ";width: 64px";
            reportPart[idx].children[1].innerHTML = activityPrefixHtml + "PRN" + activitySuffixHtml;
            reportPart[idx].children[1].style = "position:absolute; left:" + "686px"
              + ";top:" + positionOfTopActivity + "px"
              + ";padding: 2px"
              + ";width: 64px";
            reportPart[idx].children[2].innerHTML = activityPrefixHtml + "ACT." + activitySuffixHtml;
            reportPart[idx].children[2].style = "position:absolute; left:" + "748px"
              + ";top:" + positionOfTopActivity + "px"
              + ";padding: 2px;"
              + ";width: 64px"; // background: lightblue;";
            var txt1 = reportPart[idx].children[3].innerText;
            var timings = ['00:00']
            txt1 = txt1.trim();
            // Remove spaces and escape characters and place the results in the timing array
            var array1 = new Array();
            array1 = txt1.split(/[ ,]+/);
            var ii = 0;
            var ix = 0;
            for (ii = 0; ii < array1.length; ii++) {
              timings[ix] = array1[ii];
              ix++;
            }
            console.log("..Info. Print Service: primary page heading being contructed.");
            // this is the column that has the timings. Note first page
            // using msrgin-left tag and innerHTML to space them appropriately from our timings array

            var cpTimeSize = 36; // 00:00 roboto font
            var cpNbrOfTimingUnits = 12;  // each is 5 minutes totalling 60 minutes

            var cpWidth = selectColumnsTimingsX[0].clientWidth - colHeadingsRectLeftAdjusted;
            // adjust for terminal display size 
            var cpAdjustTerminalPercentSize = 0.0;
            if (selectColumnsTimingsX[0].clientWidth < 1333) {
              console.log("..Info. Print Service: Monitor smaller that normal ");
              cpWidth = cpWidth + (1333 - selectColumnsTimingsX[0].clientWidth);
              cpAdjustTerminalPercentSize = selectColumnsTimingsX[0].clientWidth / 1333;
            }
            else if (selectColumnsTimingsX[0].clientWidth > 1333) {
              console.log("..Info. Print Service: Monitor larger that normal ");
              cpWidth = cpWidth - (1333 - selectColumnsTimingsX[0].clientWidth);
              cpAdjustTerminalPercentSize = selectColumnsTimingsX[0].clientWidth / 1333;
            }

            console.log("..Info. Print Service: * cpAdjustTerminalPercentSize: " + cpAdjustTerminalPercentSize)
            console.log("..Info. Print Service: colHeadingsRectLeftAdjusted: " + colHeadingsRectLeftAdjusted);
            console.log("..Info. Print Service: cpWidth: " + cpWidth);
            console.log(selectColumnsTimingsX[0].clientWidth);

            // number of 5 minute segments, base = 112
            var cpNbrTimeSeg = selectColumnsTimingsX[0].childElementCount;
            // 5 min segs - 9 for blanks / base 12 = 8
            var cpNbrOfTimings = Math.floor((cpNbrTimeSeg - 9) / cpNbrOfTimingUnits);
            console.log("..Info. Print Service: cpNbrOfTimings: " + cpNbrOfTimings);
            // derived by:
            // var cpSizeOfChar = Math.floor(cpWidth / (cpNbrTimeSeg-9)); // 9 is the empty  space left behind by programmer
            // 3. something font size * 12 five minute segments * 8 hoours 
            // var cpSizeOfBaseTimings = (14 * cpTimeSize) // usual 3.5 * 5 the time display * * 8; 
            // cpTimeSize derived from roboto character font for 00:00, cpNbrOfTimings is the number of hours
            var cpAnchorCol = timings.length - 8;
            if (cpAnchorCol > 17) { cpAnchorCol++ };
            var cpSizeOfTimings = (cpTimeSize) * timings.length; // usual 7.0 * 4  + 1 * 4 the time display * timings
            //var cpColTotRemainingSpace = (cpWidth - 9 + cpAnchorCol) - cpSizeOfTimings; // area on screen showing timings 
            // cpwidth for timings, -9 for blank, cpSizeOfTimings is nbr of hrs times pixel size

            var cpColTotRemainingSpace = (cpWidth - 9) - cpSizeOfTimings - this.iconIteractionsSize;
            // var colSpacePixelsRight = ((cpColTotRemainingSpace / cpNbrOfTimings * .1) );
            // var colSpacePixelsRight = ((((cpSizeOfTimings ) / (cpColTotRemainingSpace + colHeadingsRectLeftAdjusted)) / cpNbrOfTimings)*100*4);
            console.log("..Info. Print Service: this.zoomAdjustment/100: " + this.zoomAdjustment / 100);
            console.log("..Info. Print Service: cpColTotRemainingSpace: " + cpColTotRemainingSpace)
            // adjust for zoom factor, start placement of timingss and number of timmings

            var cpColLeftShift = 0;
            if (timings.length <= 2) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 0;
              cpColLeftShift = cpColLeftShift - 58;
            }
            else if (timings.length <= 4) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 0;
              cpColLeftShift = cpColLeftShift - 34;
            }
            else if (timings.length <= 7) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 0;
              cpColLeftShift = cpColLeftShift - 4;
            }
            else if (timings.length <= 11) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 0;
              cpColLeftShift = cpColLeftShift + 4;
            }
            else if (timings.length <= 15) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 3;
              cpColLeftShift = cpColLeftShift + 10;
            }
            else if (timings.length <= 19) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 6;
              cpColLeftShift = cpColLeftShift + 20;
            }
            else if (timings.length <= 23) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 8;
              cpColLeftShift = cpColLeftShift + 28;
            }

            else if (timings.length <= 25) {
              this.colSpacePixelsRight = (
                (cpColTotRemainingSpace + ((cpColTotRemainingSpace * (this.zoomAdjustment / 100) / 8)))
                - (timings.length * 8)) / (timings.length) + 10;
              cpColLeftShift = cpColLeftShift + 38;
            }
            this.colSpacePixelsAdjustLeft = cpColLeftShift;
            console.log("..Info. Print Service: colSpacePixelsRight: " + this.colSpacePixelsRight);
            var largeColHeaderForActivity = document.querySelector("thead > tr > .pd-relative");

            if (largeColHeaderForActivity) {

              // alert("Coordinates: " +victory.left + "px, " + victory.top + "px");
              var largeColHeaderForActivityStartedAt = largeColHeaderForActivity.getBoundingClientRect();

              this.selectLargeColHeaderForActivityWidthLeft = largeColHeaderForActivityStartedAt.left;
            }

            // var nullColumnHeader =  document.
            // getElementsByClassName("pd-null-column");
            // if (nullColumnHeader[0]) {
            // var nullColumnHeaderWidth = nullColumnHeader[0].clientWidth;
            // alert("nullColumnHeaderWidth: " + nullColumnHeaderWidth);
            // }

            console.log("..Info. Print Service: timings.length: " + timings.length);
            console.log("..Info. Print Service: cpSizeOfTimings: " + cpSizeOfTimings)
            console.log("..Info. Print Service: colSpacePixelsRight: " + this.colSpacePixelsRight);

            //   var colHeadingLeftAdjustedBegin = 449+ colHeadingsRectLeftAdjusted - cpAnchorCol;
            if (timings.length >= 4) {
              cpColLeftShift = (Math.floor(timings.length / 4) * 2) + cpColLeftShift;
            }
            console.log("..Info. Print Service: cpColLeftShift: " + cpColLeftShift);
            this.colHeadingLeftAdjustedBegin = 448 + colHeadingsRectLeftAdjusted - cpColLeftShift;
            this.selectColumnsIdMedicineWidthDiff = this.selectColumnsIdMedicineMaxWidth - this.selectLargeColHeaderForActivityWidthLeft;
            console.log("..Info. Print Service: primary colHeadingLeftAdjustedBegin: " + this.colHeadingLeftAdjustedBegin);
            console.log("..Info. Print Service: primary selectColumnsIdMedicineWidthDiff: " + this.selectColumnsIdMedicineWidthDiff);
            console.log("..Info. Print Service: primary selectColumsNewActivitySize: " + this.selectColumsNewActivitySize);

            var timingPrefixHtml = '<div id="timingHtmlPrimary" style="font-weight: 600; font-size: 14px;' +
              'class=page-content-class-aws-55;' +
              'position:absolute'
              + ';top:' + '-36px'

              + ' ;left: ' + this.colHeadingLeftAdjustedBegin + 'px'
              + '"><span style="min-width:10px;max-width:10px;color: white;">X</span>';

            var timingSuffixHtml = '</div>';
            var timingsFound = "";
            for (var idx9 = 0; idx9 < timings.length; idx9++) {
              timingsFound = timingsFound + '<span style=" font-size: 14px;' +
                'margin-left: ' +
                (this.colSpacePixelsLeft) + 'px;' +
                'margin-right: ' +
                (this.colSpacePixelsRight) + 'px;' +

                '">' + timings[idx9] + '</span>';
            }

            reportPart[idx].children[3].innerHTML =
              timingPrefixHtml +
              timingsFound +
              timingSuffixHtml;
            // console.log("..Info. Print Service: Viv" + reportPart[idx].children[3].innerHTML);


            // locate the administrative div to set up the time columns
            //                   reportPart[idx].querySelector(".pd-relative > div").setAttribute("style", "color: back;");

            // first large column used for medicine descriptions
            // var x =   document.querySelector("thead > tr > th");
            // if (x) {console.log("..Info. Print Service: found col header x .. width: " + x.clientWidth)
            // var ratioOrUp = this.updatedPxSize / this.originalPxSize;
            // var nbrOfPxX = Math.round(x.clientWidth * ratioOrUp); 
            // }
            // locate the adminstration headers and adjust their size
            // var selectColumnsIdMedicine3 = document.
            // getElementById("orderNameCol0");
            // alert("orderNameCol0 width: " + selectColumnsIdMedicine3.clientWidth);


            // second large column used for activities (stat, prn, etc)                    
            //   var y =   document.querySelector("thead > tr > .pd-relative");
            //   if (y) {console.log("..Info. Print Service: found col header y .. width: " + y.clientWidth);}

            //   var z = this.reportContainer.clientWidth;
            //   if (z) {console.log("..Info. Print Service: found col header x-z .. width: " + z);}
            //   var colummHeaderSize = z - ( nbrOfPxX +  y.clientWidth)  ;
            //   colummHeaderSize = Math.round(colummHeaderSize * this.adjustedZoom);
            //   var colummHeaderSizeEdit = Math.round(colummHeaderSize) + "px";
            //   console.log("..Info. Print Service: columm Header Size: " + colummHeaderSizeEdit);
            //  var pdLineClass = reportPart[idx].querySelectorAll(".pd-line");
            //  for (var idx7=0; idx7 < 3; idx7++) {
            //       pdLineClass[idx7].style.fontSize="24px";
            //       pdLineClass[idx7].style.background="lightblue";
            //       // alert (colummHeaderSize);
            //    //   pdLineClass[idx7].style="position:absolute; left:" + "10px";
            //       pdLineClass[idx7].width= "30px";
            //  }

            //  var divs = reportPart[idx].querySelectorAll('.pd-relative'), i: number;

            //  for (i = 0; i < divs.length; ++i) {
            //    divs[i].backgroundColor = "pink";
            //   //  alert("found: " + divs[i].innerHTML);
            //    console.log("..Info. Print Service: found: index: " + i + " " + divs[i].innerHTML);
            //  }
            //  reportPart[idx].querySelector(".pd-relative").style.paddingLeft="400px";
            //  reportPart[idx].querySelector(".pd-relative").style.width ="600px";
            //  reportPart[idx].querySelector(".pd-relative").style.backgroundColor="orange";
            //  reportPart[idx].querySelector(".pd-relative")
            // .setAttribute("style", "width: " + colummHeaderSizeEdit);
            // reportPart[idx].querySelector(".pd-relative")
            // .setAttribute("width",  colummHeaderSizeEdit);

          }
          this.reportSection.append(reportPart[idx]);

          idx++;
        }
      }
      var now = new Date;
      var rptFileName = "eMarU" + this.userId + "y" + now.getUTCFullYear()
        + "m" + (now.getUTCMonth() + 1) + "d" + now.getUTCDate()
        + "h" + now.getUTCHours() + "m" + now.getUTCMinutes()
        + "s" + now.getUTCSeconds() + "c" + now.getUTCMilliseconds()
        + ".pdf";

      var utc_offset = now.getTimezoneOffset();
      var utc_diff = 0;
      var utc_diffStr = "";
      if (utc_offset >= 61) { utc_diff = utc_offset / 60; utc_offset = Math.floor(utc_diff); }
      if (utc_offset <= 9) { utc_diffStr = "0" + utc_offset + ":00" } else { utc_diffStr = utc_offset + ":00" }
      //  var rptDateTime = now.getUTCFullYear()
      //  + "-"+(now.getUTCMonth() +1) + "-" + now.getUTCDate()
      //  + " " + now.getUTCHours() + ":" + now.getUTCMinutes()
      //  + ":" + now.getUTCSeconds() + ":" +now.getUTCMilliseconds()
      //  + " -" + utc_diffStr;
      let rptDateTime = now.toISOString()

      var pdfImage: string;
      var exporter = html2pdf().from(this.reportSection).set({
        margin: 2,
        filename: rptFileName,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: {
        scale: 2, logging: true, dpi: 192
        },
        pagebreak: { after: '.printer-page-brake' },
        jsPDF: {
        format: 'a2',
        unit: 'mm',
        orientation: "landscape"
        },
        imageTimeout: 15000,
        imageType: 'image/jpeg'
        }).toPdf();
        exporter.output().then(function(pdf, item) {
        pdfImage = btoa(pdf);
        });

      function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
      }
      async function postPdfNext(r: PrinterService,
        _userId: number,
        _printerId: number,
        _patientId: number,
        _total_pages: number,
        _documentType: string,
        _printerAddressType: string,
        _printAddress: string
      ) {
        console.log('Waiting for report completion...');
        await sleep(6000);
        console.log('Stop waiting for report completion.');

        var reportTitle = r.postPdfBase64(
          _userId,
          _printerId,
          _patientId,
          "MAR Patient Report",
          _documentType,
          rptFileName,
          _printerAddressType,
          _printAddress,
          _total_pages,
          rptDateTime,
          rptDateTime,
          pdfImage
        );
      }
      let documentPageCount = 1;
      if (this.newPageCount > 0) {
        documentPageCount = this.newPageCount;
      }
      postPdfNext(this.printerService,
        this.userId,
        this.printerId,
        this.patient.id,
        documentPageCount,
        this.documentType,
        this.printerAddressType,
        this.printAddress);
      this.reportContainer.removeChild(this.reportSection);
      // // this.reportContainer.removeChild(reportSectionAddition);

    }
  }
  pdfCallback(pdfObject) {
    var number_of_pages = pdfObject.internal.getNumberOfPages();
    var pdf_pages = pdfObject.internal.pages;
    var myFooter = "Footer info";
    for (var i = 1; i < pdf_pages.length; i++) {
      // We are telling our pdfObject that we are now working on this page
      pdfObject.setPage(i);
      // The 10,200 value is only for A4 landscape. You need to define your own for other page sizes
      pdfObject.text("my header text", 10, 10);
    }
  }
  leave = () => {
    console.log('Leaving print patient document for' +
      ' User Name:  ' + this.userDisplayName +
      ' User Id:  ' + this.userId +
      ' Site Name:  ' + this.siteName +
      ' Site Id:  ' + this.siteId
    );
    //this.closeModifyOption();
    this.modalService.close('userPrinterInfo');
  }
  locateReportSections(selectorTag, prefix) {
    /* locate the appropriate page section using html id's
    */
    var items = [];
    //select all tags with the appropriate tag names which is passed to this method via a parameter list
    var myPosts = document.querySelectorAll(selectorTag);
    for (var i = 0; i < myPosts.length; i++) {
      //omitting undefined null check for brevity
      // check if the general id is found within the tag selection
      if (myPosts[i].id.lastIndexOf(prefix, 0) === 0) {
        items.push(myPosts[i]);
      }
    }
    return items;
  }

  getWindowSize() {
    //We use this to get the window and container sizes
    this.zoom = ((window.outerWidth - 10) / window.innerWidth); // -10
    this.zoomAdjustment = (90 - (this.zoom * 100) * -1);
    //    this.adjustedZoom= (100/this.zoom)/100;
    this.imageWidth = window.innerWidth * this.zoom;
    this.imageHeight = window.innerHeight * this.zoom;
    this.imageX = window.outerWidth - window.innerWidth;
    this.imageY = window.outerHeight - window.innerHeight;
    this.windowInnerWidth = window.innerWidth;

    this.windowOuterHeight = window.outerHeight;
    this.windowOuterWidth = window.outerWidth;
    var imageInfo =
      "window.innerHeight: " + window.innerHeight + " ...   " + "\n"
      + "window.innerWidth: " + window.innerWidth + " ...   " + "\n"
      + "window.outerHeight: " + window.outerHeight + " ...   " + "\n"
      + "window.outerWidth: " + window.outerWidth + " ...   " + "\n"
      + "window.innerHeight * this.zoom: " + this.imageHeight + " ...   " + "\n"
      + "window.innerWidth * this.zoom: " + this.imageWidth + " ...   " + "\n"
      + "img x: " + this.imageX + " ...   " + "\n"
      + "img y: " + this.imageY + " ...   " + "\n"
      + "zoom: " + this.zoom

      ;
    console.log(imageInfo);
  }
  openModifyOption() {
    this.lastPrinterUsedDescription = this.lastPrinterUsedDescription;
    document.getElementById('printer-doc-info').style.display = 'inline-block';
    document.body.classList.add('printer-doc-info');
  }
  closeModifyOption() {
    document.getElementById('printer-doc-info').style.display = 'none';
    document.body.classList.remove('printer-doc-info');
  }

  changeProperties = async () => {
    console.log('Modifying print patient document information for' +
      ' User Name:  ' + this.userDisplayName +
      ' User Id:  ' + this.userId +
      ' Site Name:  ' + this.siteName +
      ' Site Id:  ' + this.siteId
    );
    this.openModifyOption();
  }
  async getPatientOrders(patientId: number): Promise<Order[]> {

    let orders = await this.patientMedOrderService.getPatientCurrentOrders(patientId).toPromise()

    orders = orders?.map((order) => ({
      ...order,
      medTable: "emar",
      displayName: order.medication?.displayName,
      displayRoute: order.medicationRoute ? order.medicationRoute.routeName : '',
      displayFrequency: order.frequencySchedule ? order.frequencySchedule.scheduleName : '',
      displayDose: order.dose,
      displayDoseUnit: order.doseUnit ? order.doseUnit.printName : '',
      isComboMed: order.medication?.medicationDetails.length > 1 ? true : false,  // TODO: check the drugId &&
      comboMedDetails: order.medication?.medicationDetails.length > 1 ? order.medication.medicationDetails.map((m) => ({
        brandName: m.brandName,
        dose: m.dose,
        doseUnit: m.doseUnit ? m.doseUnit.printName : ''
      })) : [],
      // allergyReactionsText: order.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
      // drugInteractionsText: order.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
    }));

    return orders
  }
}
enum PrintType {
  pdfPrinter,
  ipPrinter,
  file,
  jpegPrinter,
  jpegExport,
  tiffPrinter,
  tiffExport,
  pngPrinter,
  pngExport,
  invalid
}