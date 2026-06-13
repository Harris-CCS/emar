import { Component, OnInit, Input } from '@angular/core';
import { ModalService } from 'src/services/modal.service';
import { CartService } from 'src/services/cart.service';
import { CartStoreService } from 'src/services/cart-store.service';
import { UserStoreService } from 'src/services/user-store.service';
import { SiteStoreService } from 'src/services/site-store.service';
import { PrinterService } from 'src/services/printer.service';
import { PrinterInformation} from '../../../app/interfaces/printer-information';
import    html2pdf   from 'html2pdf.js';
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
  reportContainer : any;
  reportSection : any;
  reportSectionTitle : any;
  reportSectionTime : any;
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
  newRowCount: number;
  maxMedicinePageLineCount: number = 7;
 
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
    this.defaultSitePrinter = (this.siteStoreService.default_printer_id) ? this.siteStoreService.default_printer_id : 0;
    this.siteId = (this.siteStoreService.site.id) ? this.siteStoreService.site.id : 0;
    this.siteName = (this.siteStoreService.site.name) ? this.siteStoreService.site.name : " not assigned";
    this.documentType= "PDF Printer";
    this.printerService.getPrinterInfo(this.siteId, this.userId).subscribe(data => {
      this.printerInformation =data;
      let lPrn = 0;
      let fPrn = false;
     
 
      // alert( this.printerInformation.length);
      // console.log("!!!!!!!!!!!!!!!begin!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      // let articles = document.getElementsByTagName('*');
      // for (let i = 0; i <  articles.length; i++) {
      //   console.log(articles[i]);
      // }
      // console.log("!!!!!!!!!!!!!!!end!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        // while(lPrn < this.printerInformation.length && !fPrn) {

      // Retrieve printer information from the services noted at the beginning of ngoninit
        var printRteTemp = [];
        while(lPrn < this.printerInformation.length) {    


        if (this.printerInformation[lPrn].id ==  this.userLastPrinterUsedId.trim()

          && this.printerInformation[lPrn].description.length > 0) {
          this.lastPrinterUsedDescription = this.printerInformation[lPrn].description.trim();
          }
          else {this.lastPrinterUsedDescription="_____________________";}
          if ((this.printerInformation[lPrn].address != null
            || this.printerInformation[lPrn].address. length > 6 )  
            && this.printerInformation[lPrn].deviceType.toUpperCase() == "I") {
              this.printerAddressType = "Export Report";
              this.printRteNameValueId.push({type:"Export Report"
                  ,name:this.printerInformation[lPrn].description
                  ,id: this.printerInformation[lPrn].id}); 
              fPrn = true;
              printRteTemp.push("Export Report");
              this.printerSelectList.push(this.printerInformation[lPrn].description.trim());
              this.printType = PrintType.ipPrinter;
              this.printAddress = this.printerInformation[lPrn].address;
                // alert("1");
          }
            else if ((this.printerInformation[lPrn].address == null
            || this.printerInformation[lPrn].address. length < 6 )  
            && this.printerInformation[lPrn].deviceType.toUpperCase() == "I") {
              this.printerAddressType = "Invalid";
              this.printRteNameValueId.push({type:"Invalid"
                ,name:this.printerInformation[lPrn].description
                ,id: this.printerInformation[lPrn].id}); 
              this.printType = PrintType.invalid;
              this.printAddress = " ";
              //  alert("2");
          }
          else if (this.printerInformation[lPrn].deviceType.toUpperCase() == "D") {
              this.printerAddressType = "PDF Printer";
              this.printRteNameValueId.push({type:"PDF Printer"
                ,name:this.printerInformation[lPrn].description
                ,id: this.printerInformation[lPrn].id}); 
              fPrn = true;
              printRteTemp.push("PDF Printer");
              this.printerSelectList.push(this.printerInformation[lPrn].description.trim());
              this.printType = PrintType.pdfPrinter;
              this.printAddress = this.printerInformation[lPrn].address;
          }
          else if ((this.printerInformation[lPrn].address != null
            || this.printerInformation[lPrn].address. length > 1 )  
            && this.printerInformation[lPrn].deviceType.toUpperCase() == "W") {
              this.printerAddressType = "File Directory";
              this.printRteNameValueId.push({type:"File Directory"
                ,name:this.printerInformation[lPrn].description
                ,id: this.printerInformation[lPrn].id}); 
              fPrn = true;
              printRteTemp.push("File Directory");
              this.printerSelectList.push(this.printerInformation[lPrn].description.trim());
              this.printType = PrintType.file;
              this.printAddress = this.printerInformation[lPrn].address;
          }
          else  {
              this.printerAddressType = "Invalid";
              this.printType = PrintType.invalid;
              //  alert("5");
              }
            
             
        lPrn++;
      }
      var iDev=0;
      for (iDev=0; iDev< printRteTemp.length; iDev++) {  
        if (!this.printRoute.includes( printRteTemp[iDev])) {
            this.printRoute.push( printRteTemp[iDev]); 
        }
    }
      console.log("Selected printer: " + this.lastPrinterUsedDescription +
      "  at address or file: " +  this.printerAddressType );
      if (this.printerInformation.length < 1) {this.lastPrinterUsedDescription  = "no designated printer";}
      if(!fPrn) {this.lastPrinterUsedDescription  = "select a printer";}
      if(fPrn && this.lastPrinterUsedDescription.length == 0) {this.lastPrinterUsedDescription  = "select a printer"};
    });
     console.log('UserPrinterInfoModal - _init() completed')
  }

  selectChangePrinterHandler (printList: any) {
    // user has selected a printer from the drop down, html is passing $event
   var target = printList.target.innerHTML;
   this.lastPrinterUsedDescription =target;
   let lPrn = 0;
   let fPrn = false;
   while(lPrn < this.printerInformation.length && !fPrn) {
     if (this.printerInformation[lPrn].description.trim() ==  target.trim()) {
      if (this.printerInformation[lPrn].deviceType.toUpperCase() == "D") {
          this.printerAddressType = "PDF Printer";
          this.printType = PrintType.pdfPrinter;
          this.printerId = this.printerInformation[lPrn].id;
          this.printerDescription = this.printerInformation[lPrn].description;
          this.printAddress = this.printerInformation[lPrn].address;
        }
        else if (this.printerInformation[lPrn].deviceType.toUpperCase() == "W") {
          this.printerAddressType = "File Directory";
          this.printType = PrintType.file;
          this.printerId = this.printerInformation[lPrn].id;
          this.printerDescription = this.printerInformation[lPrn].description;
          this.printAddress = this.printerInformation[lPrn].address;

        }
        else if (this.printerInformation[lPrn].deviceType.toUpperCase() == "I") {
          this.printerAddressType = "Export Report";
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
   console.log("Selected printer: " + this.lastPrinterUsedDescription +
  " ... printer id: " + this.printerId +
  " ... this description: " +  this.printerInformation[lPrn-1].description +
  " ... target: " + target +
   " ...  at address or file: " +  this.printerAddressType )
}
selectChangeDocumentTyperHandler (documentList: any) {
  //user has selected a type of document to be printed using $event
  var iPType = 0;
  for (iPType=0; iPType< this.printerSelectList.length; iPType++) { 
    this.printerSelectList[iPType]=" ";
  }
  
  var target = documentList.target.innerHTML;
  this.documentType=target.trim();
  var iDType = 0;
  for (iDType=0; iDType< this.printRteNameValueId.length; iDType++) { 
    // alert(this.printRteNameValueId[iDType].type);
    if(this.printRteNameValueId[iDType].type == this.documentType) {
      this.printerSelectList[iDType]=this.printRteNameValueId[iDType].name;
    }
    else {
      this.printerSelectList[iDType]="zzzzzzzzzz";
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
    if (this.lastPrinterUsedDescription != "_____________________") {
  console.log('Printing patient document: '+
  ' Document Type: ' +  this.documentType +
    ', Page Identifier: ' + pageId +
    ', User Name:  ' + this.userDisplayName +
    ', User Id:  ' + this.userId +
    ', Site Name:  ' + this.siteName +
    ', Site Id:  ' + this.siteId
    );
  this.modalService.close('userPrinterInfo');
  var reportType:string = this.documentType.toLowerCase();
switch(this.printType) {
   case PrintType.pdfPrinter: {
    console.log('User has selected to pdf print');
    this.createReportPdf(pageId);
      break;
   }
   case PrintType.ipPrinter: {
   console.log('User has selected to pdf export');
    this.createReportPdf(pageId);
      break;
   }
   case PrintType.file: {
    console.log('User has selected to pdf file');
    this.createReportPdf(pageId);
      break;
   }
   case PrintType.jpegPrinter: {
    console.log('User has selected to jpeg print');
    this.createReportJpeg(pageId);
      break;
   }
   case PrintType.jpegExport: {
    console.log('User has selected to jpeg export');
    this.createReportJpeg(pageId);
      break;
   }
   case PrintType.tiffPrinter: {
    console.log('User has selected to tiff print');
       break;
   }
   case PrintType.tiffExport: {
    console.log('User has selected to pdf tiff export');
      break;
   }
   case PrintType.pngPrinter: {
    console.log('User has selected to png print');
      break;
   }
   case PrintType.pngExport: {
    console.log('User has selected to png export');
      break;
   }
   default: {
    console.log('Printing patient document error. Improper document type for: '+
    ' Document Type: ' +  this.documentType +
      ', Page Identifier: ' + pageId)
  }
      break;
   }
  }
  else {console.log("User has not selecteda printer destination");}
}
printReportxJpeg(pageId: string) {
     const ePageId = "#" + pageId;
  var reportSection= document.querySelector(ePageId);
//  alert("height: "+ reportSection.scrollHeight + "     width: " + reportSection.scrollWidth);
 const imgConverted = document.querySelector(ePageId) as HTMLImageElement;
 const rptCanvas2 = document.querySelector(ePageId) as HTMLCanvasElement;
  html2canvas(document.querySelector(ePageId)).then(

    canvas => {

      var imgData = canvas.toDataURL("image/jpeg", 1.0),
      imageTimeout:15000;
      html2pdf(reportSection, {
        jsPDF: {

          format: 'a4',
          orientation: "landscape",
          height:reportSection.scrollHeight,
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
      const rptCanvas2  = document.getElementsByTagName("canvas")[0]  as HTMLCanvasElement;
      const dataURI = rptCanvas2.toDataURL("image/jpeg", 1.0);
      console.log(dataURI);
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
      const rptCanvas2  = document.getElementsByTagName("canvas")[0]  as HTMLCanvasElement;
      const dataURI = rptCanvas2.toDataURL();
      console.log(dataURI);
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
     var nbrOfPages =1;

    if (window.parent != window.top) {
      console.log('Printing patient document error. Function improperly executed for: '+
      ' Document Type: ' +  this.documentType +
        ', Page Identifier: ' + pageId)
    }
    else {



      var xternDatesTimes = document.querySelectorAll(".printer-dates-times");
      if(xternDatesTimes .length > 1) {
        fromToDateTime = xternDatesTimes [0].innerHTML+ " /&nbsp; "
        + xternDatesTimes [xternDatesTimes .length-1].innerHTML;
      }




      let reportSectionAddition = document.createElement("div");
      let divOffset = document.getElementById("orderNameCol0").offsetLeft +
      document.getElementById("orderNameCol0").offsetWidth;
         this.getWindowSize() ;
        //We use this to get the HTML elements noted in the first part of the parameter list
        // that have an ID that matches the second parameter

        var reportSections = this.locateReportSections("div, thead, th, tr","page-content-id");
        if (reportSections.length > 0) {
          var reportPart = [];
          // locate the largest container holding the information our report requires
        this.reportContainer = document.getElementById("page-report-container");
        // Start contruction of our report using a division
        this.reportSection = document.createElement("div");
        this.reportSection.id="reportSection";

        this.reportSectionTitle = document.createElement("div");
        this.reportSectionTitle.id="reportSectionTitle";
        this.reportSectionTitle.style="color: red; margin: auto;  font-size: 24px;" +
        "width: 50%; border-top: 2px solid gray; padding: 10px;  text-align: center;";
        this.reportSectionTitle.innerHTML="Patient Medication Administration Record";

        this.reportSectionTime = document.createElement("div");
        this.reportSectionTime.id="reportSectionTime";
        this.reportSectionTime.style="color: black; margin: auto;  font-size: 16px;" +
        "width: 50%; border-bottom: 2px solid gray; padding: 10px;  text-align: center;";
        this.xxxx="yyyyyy";
        this.reportSectionTime.innerHTML=fromToDateTime;



        // Append it to the report container (note: this will be removed later)
        this.reportContainer.appendChild(this.reportSection);
        // Iterate through all of the html elements found so they can be used to construct our report


        var idx = 0;
// access patient information                        
this.patientStoreService.fetchPatient(this.patientStoreService.patientId);
this.patient = this.patientStoreService.patient;
        while (idx < reportSections.length)
          {
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
                        reportPart[idx].childNodes[0].style.fontWeight="500";
                        reportPart[idx].style.height="42px";
                        reportPart[idx].style.padding="1px";
                        reportPart[idx].style.backgroundColor="white";
                      }

                      if (reportSections[idx].id.indexOf("-aws-03") > 0) {
                        // need to add birthdate to demographic (sex age)   
                        // and reverse age and sex
                        var reverseAgeSex = (reportPart[idx].childNodes[0].childNodes[0].innerText.
                          substr(reportPart[idx].childNodes[0].childNodes[0].innerText.length-1)
                            + "\u00A0"
                            + reportPart[idx].childNodes[0].childNodes[0].innerText.
                            substr(0,reportPart[idx].childNodes[0].childNodes[0].innerText.length-1)
                            );                      
                        var demoInfoBirthDate = "DOB:\u00A0" +
                        this.patient.dateOfBirth.substr(5,2) + "/" +
                        this.patient.dateOfBirth.substr(8,2) + "/" +
                        this.patient.dateOfBirth.substr(0,4) + "\u00A0";
                        reportPart[idx].childNodes[0].childNodes[0].innerText= 
                            demoInfoBirthDate + "\u00A0" + reverseAgeSex;
                 
                        reportPart[idx].childNodes[0].childNodes[0].style.fontSize = "20px";
                        reportPart[idx].childNodes[0].childNodes[0].style.margin = "6px";
                        reportPart[idx].childNodes[0].childNodes[1].style.margin = "6px";
                        reportPart[idx].style.fontSize = "20px";
                        reportPart[idx].style.height="62px";
                        reportPart[idx].style.padding="1px";
                        reportPart[idx].style.backgroundColor="white";

                      }
                      else if (reportSections[idx].id.indexOf("-aws-02") > 0) {
                        reportPart[idx].style.fontSize="20px";
                        reportPart[idx].style.backgroundColor="white";


// get allergy information
// first locate allergy information about the patient

                        var allergyItem ="";
                        if (this.patientStoreService.patientAllergies.length > 0) {
                          allergyItem = "<table style='border: 1px solid black;" +
                          "font-size: 18px; margin-top: 9px; margin-bottom: 6px;'>" +
                           "<tr style='font-size:16px;'><th>Allergy</th><th>Severity</th><th>Reaction</th></tr><tr>";
                        for (let i = 0; i <  this.patientStoreService.patientAllergies.length; i++)
                          { allergyItem = 
                            allergyItem + 
                            "<td>" +
                             this.patientStoreService.patientAllergies[i].name + "</td>" + 
                             "<td>" +
                             this.patientStoreService.patientAllergies[i].severity + "</td>"  +
                            "<td>" +
                             this.patientStoreService.patientAllergies[i].reaction + "</td></tr>"
                          }
                          this.patientAllergies = 
                          allergyItem + 
                          "</tr></table>" 
                        }
                        else {this.patientAllergies = " ";}
                        reportPart[idx].innerHTML +=(this.patientAllergies);


// get medications prescribed for the patient
this.allPatientOrders[this.patient.id]  = await this.getPatientOrders(this.patient.id);
var patientMedicineTableLength = this.allPatientOrders[this.patient.id].length;
                      }                      
                      else {

                        //           Dont do anything if no data

// locate the column containing medicine information                        
                        var pageReportContainerTbodyTrMedicines =  reportPart[idx].
                        querySelectorAll("tbody > tr "), iTbTr: number = 0;
                        var deleteMedicineRecord: boolean = false;
                        if (pageReportContainerTbodyTrMedicines.length > 0){
// get key information about the medicine being administered                          
                          var medObjX = 0;
                          var medLookup = 0;
                          // administeed medicines
                          var StringOneToSearchFor='ng-reflect-ng-class="object Object"> ';
                          // canceled medicines
                          var StringTwoToSearchFor='class="pd-cancelled"> ';
                          // ordered medicine date time  
                          var StringThreeToSearchFor= 'Ordered:</i></small> ';
                          var moreMedicineInfo = " "; 
                          // search for medicines in the browser
                          for (medObjX=0;medObjX<pageReportContainerTbodyTrMedicines.length;medObjX++) 
                          {
                          var medNameFound = " ";
                          var medOrderedTimeDateFound = "00:00 00/00/0000";
                          moreMedicineInfo = " ... ";
                          var beforeMedicineTdHtml = pageReportContainerTbodyTrMedicines[medObjX].childNodes[0].innerHTML;
                          var str =  beforeMedicineTdHtml.replace(/[\[\]']+/g,'');
                          // start search for medicines in the browser
                          var medObjXx = str.search(StringOneToSearchFor);
                          if (medObjXx > 0)
                          {
                          var medObjXy = medObjXx+StringOneToSearchFor.length;
                          if(medObjXy > 0) {
                            // alert("2");
                          var medObjXz = str.indexOf('</span>', medObjXy);
                          if (medObjXz > 0) {


                          // search for this medicine start time for this patient in the browser
                          var medObjYx = str.search(StringThreeToSearchFor);
                          if (medObjYx > 0) 
                          {
 
                            var medObjYy = medObjYx+StringThreeToSearchFor.length;
                            if(medObjYy > 0) {
                            var medObjYz = str.indexOf('. </span>', medObjYy);
                            if (medObjYz > 0) {

                          medNameFound = str.substring(medObjXy,medObjXz);
                          medOrderedTimeDateFound = str.substring(medObjYy,medObjYz);
                          for (medLookup=0;medLookup<patientMedicineTableLength;medLookup++)  {
                          // copare medicine names
                            var tableMedicineLookup = this.allPatientOrders[this.patient.id][medLookup].displayName;
                            var  medCompLength = ((medNameFound.length  
                              <=  tableMedicineLookup.length) 
                            ? medNameFound.length : tableMedicineLookup.length);
                            if(medCompLength > 60 ) {medCompLength  = 60}
                            // compare browser ordered date to API add date
                            var medApiAddDateYear = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(0,4);
                            var medApiAddDateMonth = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(5,2);
                            var medApiAddDateDay = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(8,2);
                            var medApiAddTimeHour = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(11,2);
                            var medApiAddTimeMinute = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(14,2); 
                            var medAddTimeDate =  medApiAddTimeHour + ":" + medApiAddTimeMinute + " " +
                            medApiAddDateMonth + '/' + medApiAddDateDay + '/' + medApiAddDateYear;
                                 

                                // console.log("medAddTimeDate: "+ medAddTimeDate + "   to   " + "medOrderedTimeDateFound: " + medOrderedTimeDateFound)
                            if ((medNameFound.substring(0,medCompLength) == this.allPatientOrders[this.patient.id][medLookup]
                            .displayName.substring(0,medCompLength))
                              && (medAddTimeDate == medOrderedTimeDateFound)
                              ) {

 var medApiBegDateYear = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(0,4);
 var medApiBegDateMonth = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(5,2);
 var medApiBegDateDay = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(8,2);
 var medApiBegTimeHour = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(11,2);
 var medApiBegTimeMinute = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(14,2); 
 var medBegTimeDate =  medApiBegDateMonth + '/' + medApiBegDateDay + '/' + medApiBegDateYear + " "  +
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
                                "</span> " ;
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
                          if (medObjXx > 0)
                          {
                            deleteMedicineRecord = true;
                            var medObjXy = medObjXx+StringTwoToSearchFor.length;
                            if(medObjXy > 0) {
           
                            var medObjXz = str.indexOf('</span>', medObjXy);
                            if (medObjXz > 0) {
      
  
                            // search for this medicine start time for this patient in the browser
                            var medObjYx = str.search(StringThreeToSearchFor);
                            if (medObjYx > 0) 
                            {
       
                              var medObjYy = medObjYx+StringThreeToSearchFor.length;
                              if(medObjYy > 0) {
                              var medObjYz = str.indexOf('. </span>', medObjYy);
                              if (medObjYz > 0) {
  
                            medNameFound = str.substring(medObjXy,medObjXz);
                            medOrderedTimeDateFound = str.substring(medObjYy,medObjYz);
                            for (medLookup=0;medLookup<patientMedicineTableLength;medLookup++)  {
                            // copare medicine names
                              var tableMedicineLookup = this.allPatientOrders[this.patient.id][medLookup].displayName;
                              var  medCompLength = ((medNameFound.length  
                                <=  tableMedicineLookup.length) 
                              ? medNameFound.length : tableMedicineLookup.length);
                              if(medCompLength > 60 ) {medCompLength  = 60}
                              // compare browser ordered date to API add date
                              var medApiAddDateYear = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(0,4);
                              var medApiAddDateMonth = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(5,2);
                              var medApiAddDateDay = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(8,2);
                              var medApiAddTimeHour = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(11,2);
                              var medApiAddTimeMinute = this.allPatientOrders[this.patient.id][medLookup].addDatetime.substr(14,2); 
                              var medAddTimeDate =  medApiAddTimeHour + ":" + medApiAddTimeMinute + " " +
                              medApiAddDateMonth + '/' + medApiAddDateDay + '/' + medApiAddDateYear;
                                   
  
                                  // console.log("medAddTimeDate: "+ medAddTimeDate + "   to   " + "medOrderedTimeDateFound: " + medOrderedTimeDateFound)
                              if ((medNameFound.substring(0,medCompLength) == this.allPatientOrders[this.patient.id][medLookup]
                              .displayName.substring(0,medCompLength))
                                && (medAddTimeDate == medOrderedTimeDateFound)
                                ) {
  
   var medApiBegDateYear = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(0,4);
   var medApiBegDateMonth = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(5,2);
   var medApiBegDateDay = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(8,2);
   var medApiBegTimeHour = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(11,2);
   var medApiBegTimeMinute = this.allPatientOrders[this.patient.id][medLookup].beginDatetime.substr(14,2); 
   var medBegTimeDate =  medApiBegDateMonth + '/' + medApiBegDateDay + '/' + medApiBegDateYear + " "  +
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
                                  "</span> " ;
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
                            else {deleteMedicineRecord = false;}                      
                          }
                      if (!deleteMedicineRecord) {
                      pageReportContainerTbodyTrMedicines[medObjX].childNodes[0].innerHTML 
                      = beforeMedicineTdHtml +  moreMedicineInfo;
                      }
                      else {
                        pageReportContainerTbodyTrMedicines[medObjX].childNodes[0].innerHTML 
                        = beforeMedicineTdHtml +  "oops";
                        var eligibleForDelete = pageReportContainerTbodyTrMedicines[medObjX];
                        eligibleForDelete.remove();
                      }
                    }
                    
                      }

 // locate any scheduled information line for line from the screen                  
                                     var pageReportContainerTbodyTrSchedules =  reportPart[idx].
                                     querySelectorAll("tbody > tr > td > div > span > div"), iTbTr: number = 0;
                                     if (pageReportContainerTbodyTrSchedules.length > 0){
                                      var nodes=[], values=[];
                                      var schedObjX = 0;
                                      var StringToSearchFor='ng-reflect-ngb-tooltip="';
                                      for (schedObjX=0;schedObjX<pageReportContainerTbodyTrSchedules.length;schedObjX++) {
                                        var str= pageReportContainerTbodyTrSchedules[schedObjX].innerHTML;
                                        var schedObjXx = str.search(StringToSearchFor);
                                        if (schedObjXx > 0)
                                        {
                                        var schedObjXy = schedObjXx+StringToSearchFor.length
                                        if(schedObjXy > 0) {
                                        var schedObjXz = str.indexOf('"', schedObjXy);
                                        if (schedObjXz > 0) {
                                        var stringFound = str.substring(schedObjXy,schedObjXz);
                                        var medicationScheduleEdited = " ";
                                        var borderColor = "black";
                                       ;
                                        switch( stringFound.toUpperCase().replace(/\s+/g, '')) {
                                              case "ACKNOWLEDGED": {
                                                medicationScheduleEdited = "A";
                                                borderColor="green";
                                                break;
                                              }
                                              case "ADMINISTERED": {
                                                medicationScheduleEdited = "G";
                                                borderColor="green";
                                                break;
                                              }
                                              case "GIVEN": {
                                                medicationScheduleEdited = "G";
                                                borderColor="green";
                                                break;
                                              }
                                              case "ONGOING": {
                                                medicationScheduleEdited = "O";
                                                borderColor="green";
                                                break;
                                              }                                              
                                              case "SCHEDULED": {
                                                medicationScheduleEdited = "S";
                                                borderColor="black";
                                                break;
                                              }
                                              case "DUE": {
                                                medicationScheduleEdited = "P";
                                                borderColor="red";
                                                break;
                                              }
                                              case "MISSEDDOSE": {
                                                medicationScheduleEdited = "M";
                                                borderColor="gray";
                                                break;
                                              }      
                                              case "ONHOLD": {
                                                medicationScheduleEdited = "H";
                                                borderColor="gold";
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
                                        else {medicationScheduleEdited = "?"};
                                        var html_to_insert = "<span style='border: 1px solid " + 
                                        borderColor +
                                        ";font-weight: 700; font-size: 16px; padding: 2px'>" 
                                        + medicationScheduleEdited + "</span>";
                                        pageReportContainerTbodyTrSchedules[schedObjX].innerHTML="";
                                        pageReportContainerTbodyTrSchedules[schedObjX].insertAdjacentHTML('beforeend', html_to_insert);

                      //                  console.log(pageReportContainerTbodyTrSchedules[schedObjX].innerHTML);
                               //       var tempElement = document.createElement('span');
  
                                        
                            //           for (var att, i = 0, atts = pageReportContainerTbodyTrSchedules[schedObjX].attributes, 
                            //             n = atts.length; i < n; i++){
                            //               att = atts[i];
                            //               console.log("!!!!!!!!!!!!!!!!!!! attr name: " 
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




                        var tableRow = reportPart[idx].querySelectorAll("tbody > tr"), i: number = 1;
             //           Dont do anything if no data
                        if (tableRow.length > 0) {
                          // clone the row header in the patient table
                          var templateNewRowMask1 = document.getElementById("page-content-id-amsp-01").cloneNode(true);
                          // create default array timings just in case there is no entry later on
                          var timings = ['00:00', '00:00', '00:00', '00:00',
                          '00:00', '00:00', '00:00', '00:00']
                          // check if there are entries in the timings table column (3)
                          // They are string hour minute entries
                          if(templateNewRowMask1.childNodes.length > 6) {
                            var  txt1=templateNewRowMask1.childNodes[6].textContent;
                          // Remove spaces and escape characters and place the results in the timing array
                            var array1 = new Array();
                            array1=txt1.split(/[ ,]+/);
                            var ii=0;
                            var ix=0;
                            for (ii=0;ii<array1.length;ii++)
                              {
                                timings[ix] =  array1[ii];
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
                          for (let i = 1; i <  newHeader.childNodes.length-1; i++) {
                            newHeader.childNodes[i].innerHTML = " ";
                            newHeader.childNodes[i].setAttribute("style", "border-style: none; background: white");
                            newSubHeader.childNodes[i].innerHTML = " ";
                            newSubHeader.childNodes[i].setAttribute("style", "border-style: none; background: white");
                            newFooter.childNodes[i].innerHTML = " ";
                            newFooter.childNodes[i].setAttribute("style", "border-style: none; background: white");
                          }
                          var patientName =  document.getElementById("page-content-id-aws-01");
                          newHeader.childNodes[0].textContent = patientName.innerText;
                          newHeader.childNodes[0].setAttribute("style", "color: black;font-size: 24px;");
                          newHeader.childNodes[4].textContent = "Patient Medication Administration Record";
                          newHeader.childNodes[4].setAttribute("style", "color: red; margin: auto;  font-size: 24px;" +
                          "width: 50%; border-top: 2px solid gray; padding: 10px;  text-align: center;");
                          newSubHeader.childNodes[0].textContent = " ";
                          var idx3 = fromToDateTime.indexOf("&nbsp;");
                          var convertHtmlToTextDate = fromToDateTime.substr(0,idx3) + "\u00A0" 
                              + fromToDateTime.substr((idx3 + 6));
                          newSubHeader.childNodes[4].textContent = convertHtmlToTextDate;
                          newSubHeader.childNodes[4].setAttribute("style", "color: black; margin: auto;  font-size: 16px;" +
                          "width: 50%; border-bottom: 2px solid gray; padding: 10px;  text-align: center;");
                          newFooter.childNodes[0].textContent = " ";

                          newFooter.childNodes[4].setAttribute("style", "color: black; margin: auto;  font-size: 15px;" +
                          "width: 50%; border-bottom: 2px solid gray; padding: 10px;  text-align: center;");
                          newRow.childNodes[0].textContent = "";
                          // move spaces into the first column
                          for (let i = 1; i <  newRow.childNodes.length-1; i++) {
                            if (i < 4) {
                          // move the inner text into the new row for the first colums (except the first column)
                            newRow.childNodes[i].textContent =
                            templateNewRowMask1.childNodes[i-1].textContent
                          }
                          else {
                            // this is the column that has the timings
                            // using pre tag and innerHTML to space them appropriately from our timings array
                            var colSpace = "                               ";
                           //   newRow.childNodes[i].classList.add("newRowColumnHeader");
                           //   newRow.childNodes[4].setAttribute("style", "color: blue;");
                              newRow.childNodes[i].innerHTML=
                              '<span style="font-weight: bold;"><pre>  ' +
                              timings[1]+ colSpace + timings[2] + colSpace + timings[3] + colSpace +
                              timings[4]+ colSpace + timings[5] + colSpace + timings[6] + colSpace +
                              timings[7]+ colSpace+ timings[8]
                              '</prev></span>';
                              nbrOfPages++;
                          }
                        }
                          // output row count
                          var pageRowCount = 0;
                          // determine how many input new rows that mimic the table row addressed previously
                          // example: "tbody > tr"
                          this.newRowCount = Math.floor(tableRow.length / this.maxMedicinePageLineCount);
                          var newRowCountRemainder = tableRow.length % this.maxMedicinePageLineCount;
                          if ( newRowCountRemainder !=0) {
                            this.newRowCount++;
                          }
                          var newHeaderArray = [];
                          var newSubHeaderArray = [];
                          var newRowArray = [];
                          var newFooterArray = [];
                          var pageBreakCount =0;
                          var pageFooterCount = 2;
                          var nowRpt = new Date;
                          var pageDateTime = "Printed: "
                          +("0" + (nowRpt.getMonth() +1) ).substr(-2)
                          + "/"+("0" + nowRpt.getDate()).substr(-2)
                          + "/"+ nowRpt.getFullYear()
                          + " "+("0" + nowRpt.getHours()).substr(-2)
                          + ":"+("0" + nowRpt.getMinutes()).substr(-2)                          
                          + ":"+("0" + nowRpt.getSeconds()).substr(-2)     


                          var iii = 0;
                          // lets create some new rows to be used as column headings in our report
                          for (iii = 0; iii < this.newRowCount; iii++)
                          {
                            newHeaderArray.push(newHeader.cloneNode(true));
                            newSubHeaderArray.push(newSubHeader.cloneNode(true));
                            newRowArray.push(newRow.cloneNode(true));
                           }
                           for (iii =0; iii < this.newRowCount; iii++)
                           {
                             newFooter.childNodes[4].innerHTML =  
                             "<span style='float: center' margin-top: 3px; margin-bottom: 3px;>" 
                             + "Legend:&ensp; A - Acknowledged,&ensp; G - Given,&ensp;  " 
                             + "S - Scheduled,&ensp;  P - Past Due,&ensp;  " 
                             + "M - Missed Dose,&ensp;  H - OnHold,&ensp; O - OnGoing" + "</span><br>"
                             + "<span style='float: center' margin-top: 3px; margin-bottom: 3px;>" 
                             + "Page " + (iii +1) + " of " + this.newRowCount 
                             + "</span>                    <span style='float: right'>" + pageDateTime  + "</span>";
                             newFooterArray.push(newFooter.cloneNode(true));
                   //          alert(newFooter.childNodes[4].textContent);
                            }
                          var AdditionalItems = 0;
                          while (i < tableRow.length)
                               {
                           //     alert("0:" + i);
                                 if (i % this.maxMedicinePageLineCount == 0)
                                 {
                                
                                   pageBreakCount++;
                                 //  alert("1:" + pageBreakCount);
                                   pageRowCount +=this.maxMedicinePageLineCount;
                  //               retreive the records from the array and apply them to the 
                  //               subsequent pages: report header, sub header and trailer
                                   tableRow[0].parentNode.insertBefore(newRowArray.pop(),
                                   tableRow[0].parentNode.childNodes[pageRowCount+pageBreakCount+ AdditionalItems]);
                                   tableRow[0].parentNode.insertBefore(newHeaderArray.pop(),
                                   tableRow[0].parentNode.childNodes[pageRowCount+pageBreakCount+ AdditionalItems++]);
                                   tableRow[0].parentNode.insertBefore(newSubHeaderArray.pop(),
                                   tableRow[0].parentNode.childNodes[pageRowCount+pageBreakCount+ AdditionalItems++]);
                                   tableRow[0].parentNode.insertBefore(newFooterArray.shift(),
                                   tableRow[0].parentNode.childNodes[(pageRowCount+pageBreakCount 
                                    + (AdditionalItems-pageFooterCount))]).classList.add("printer-page-brake");;                                    
                                 }
     
                              i++;
                              }
                              if ((i-1) % this.maxMedicinePageLineCount != 0)
                              {
                               var lastRows = tableRow.length % this.maxMedicinePageLineCount ;
                               lastRows = ( tableRow.length-1) - lastRows;
                              //  tableRow[lastRows].classList.add("printer-page-brake");
                              tableRow[0].parentNode.insertBefore(newFooterArray.shift(),
                              tableRow[0].parentNode.childNodes[pageRowCount +lastRows+ pageBreakCount])
                              }
                        }

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
                    if (reportSections[idx].id.indexOf("-amsp-") > 0) {

                      reportPart[idx] = reportSections[idx].cloneNode(true);
                      reportPart[idx].children[3].style.color = "red";

                    reportPart[idx].querySelector(".pd-relative > div").setAttribute("style", "color: back;");
                    var x =   document.querySelector("thead > tr > th");
                    if (x) {console.log("found col header x .. width: " + x.clientWidth)}
                    var y =   document.querySelector("thead > tr > .pd-relative");
                    if (y) {console.log("found col header y .. width: " + y.clientWidth);}

                    var z = this.reportContainer.clientWidth;
                    if (z) {console.log("found col header x-z .. width: " + z);}
                    var colummHeaderSize = z - ( x.clientWidth +  y.clientWidth)  ;
                    colummHeaderSize = colummHeaderSize * this.adjustedZoom;
                    var colummHeaderSizeEdit = colummHeaderSize + "px";
                     reportPart[idx].querySelector(".pd-relative")
                    .setAttribute("style", "width: " + colummHeaderSizeEdit);
                    reportPart[idx].querySelector(".pd-relative")
                    .setAttribute("width",  colummHeaderSizeEdit);
                  }
                    this.reportSection.append(reportPart[idx]);

          idx++;
          }
    }
        var now = new Date;
        var rptFileName = "eMarU"+ this.userId + "y" + now.getUTCFullYear()
         + "m"+(now.getUTCMonth() +1) + "d" + now.getUTCDate()
         + "h" + now.getUTCHours() + "m" + now.getUTCMinutes()
         + "s" + now.getUTCSeconds() + "c" +now.getUTCMilliseconds()
         + ".pdf";
         
         var utc_offset = now.getTimezoneOffset();
         var utc_diff=0;
         var utc_diffStr="";
         if (utc_offset >= 61) {utc_diff = utc_offset/60; utc_offset =Math.floor(utc_diff); }
         if (utc_offset <= 9) {utc_diffStr = "0" + utc_offset + ":00"} else {utc_diffStr = utc_offset + ":00"}
        //  var rptDateTime = now.getUTCFullYear()
        //  + "-"+(now.getUTCMonth() +1) + "-" + now.getUTCDate()
        //  + " " + now.getUTCHours() + ":" + now.getUTCMinutes()
        //  + ":" + now.getUTCSeconds() + ":" +now.getUTCMilliseconds()
        //  + " -" + utc_diffStr;
        let rptDateTime = now.toISOString()

         var pdfImage: string;
          html2pdf(this.reportSection, {
            margin: 2,
            filename: rptFileName,
            image: {type: 'jpeg', quality: 0.98 },
            html2canvas:{ scale:2, logging: true, dpi: 192
            },
            pagebreak: { after: '.printer-page-brake'},
            jsPDF: {
              format: 'a2',
              unit: 'mm',
              orientation: "landscape"
            },
            imageTimeout:15000,
            imageType: 'image/jpeg'
          }).outputPdf().then(function(pdf) {
            pdfImage =btoa(pdf);
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
              _total_pages ,
              rptDateTime,
              rptDateTime,
              pdfImage   
            );
          }      
          postPdfNext(this.printerService,
            this.userId,
            this.printerId,
            this.patient.id,
            this.newRowCount,
            this.documentType,
            this.printerAddressType,
            this.printAddress);
   this.reportContainer.removeChild(this.reportSection);
    // // this.reportContainer.removeChild(reportSectionAddition);

    }
  }
  pdfCallback(pdfObject){
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
    console.log('Leaving print patient document for'+
    ' User Name:  ' + this.userDisplayName +
    ' User Id:  ' + this.userId +
    ' Site Name:  ' + this.siteName +
    ' Site Id:  ' + this.siteId
    );
    //this.closeModifyOption();
    this.modalService.close('userPrinterInfo');
  }
  locateReportSections (selectorTag, prefix) {
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

  getWindowSize()
{
//We use this to get the window and container sizes
    this.zoom = (( window.outerWidth -10 ) / window.innerWidth); // -10
    this.adjustedZoom= (100/this.zoom)/100;
    this.imageWidth = window.innerWidth * this.zoom;
    this.imageHeight = window.innerHeight * this.zoom;
    this.imageX = window.outerWidth - window.innerWidth;
    this.imageY = window.outerHeight - window.innerHeight;
    this.windowInnerWidth = window.innerWidth;

    this.windowOuterHeight = window.outerHeight;
    this.windowOuterWidth = window.outerWidth;
    var imageInfo =
      "window.innerHeight: " + window.innerHeight + " ...   " +  "\n"
    + "window.innerWidth: " + window.innerWidth + " ...   " +  "\n"
    + "window.outerHeight: " + window.outerHeight + " ...   " +  "\n"
    + "window.outerWidth: " + window.outerWidth + " ...   " +  "\n"
    + "window.innerHeight * this.zoom: " + this.imageHeight + " ...   " +  "\n"
    + "window.innerWidth * this.zoom: " + this.imageWidth + " ...   " +  "\n"
    + "img x: " + this.imageX + " ...   " +  "\n"
    + "img y: " + this.imageY + " ...   " +  "\n"
    + "zoom: " + this.zoom

  ;
  console.log(imageInfo);
  }
  openModifyOption() {
    this.lastPrinterUsedDescription = this.lastPrinterUsedDescription ;
    document.getElementById('printer-doc-info').style.display = 'inline-block';
    document.body.classList.add('printer-doc-info');
    }
    closeModifyOption() {
    document.getElementById('printer-doc-info').style.display = 'none';
    document.body.classList.remove('printer-doc-info');
    }

  changeProperties = async () => {
      console.log('Modifying print patient document information for'+
    ' User Name:  ' + this.userDisplayName +
    ' User Id:  ' + this.userId +
    ' Site Name:  ' + this.siteName +
    ' Site Id:  ' + this.siteId
    );
    this.openModifyOption();
   }
   async getPatientOrders(patientId: number) : Promise<Order[]> {
    
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