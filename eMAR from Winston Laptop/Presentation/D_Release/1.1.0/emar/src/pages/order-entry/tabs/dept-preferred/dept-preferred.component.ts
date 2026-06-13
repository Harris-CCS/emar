import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs'
import { takeUntil } from 'rxjs/operators'

//import { MEDICATIONS } from '../../../../app/mockup/medications';
import { MedOrderService } from '../../../../services/med-order.service';
import { CartStoreService } from '../../../../services/cart-store.service';
import { UserStoreService } from '../../../../services/user-store.service';
import { PatientStoreService } from '../../../../services/patient-store.service';
import { ModalService } from '../../../../services/modal.service';
import { ComposerSchedulerService } from '../../../../services/composer-scheduler.service';

@Component({
  selector: 'dept-preferred',
  templateUrl: './dept-preferred.component.html',
  styleUrls: ['./dept-preferred.component.scss'],
})
export class DeptPreferredComponent implements OnInit, OnDestroy {
  @Output() isTabValid: EventEmitter<boolean> = new EventEmitter();
  private listContents = [];

  // @Output() isTabValid: EventEmitter<boolean> = new EventEmitter()
  // private listContents = [];
  private userId = this.userStoreService.userId;
  private patientId = this.patientStoreService.patientId;
  @Input() auth: boolean;
  ngUnsubscribe = new Subject<void>();
  isLoading: boolean = false

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private medOrderService: MedOrderService,
    private modalService: ModalService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private composerSchedulerService: ComposerSchedulerService,
  ) { }

  ngOnInit(): void {
    this.getDeptPreferredOrdersList();

    this.medOrderService.refreshRequest_listOrders
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe( e => {
      // console.log('++++subscribe refreshRequest_listOrders - REFRESH DeptPreferredOrdersList e: ', e);
      this.getDeptPreferredOrdersList();
    });
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  deptPreferred() {
    return 'deptpreferredlist';
  }

  deptPreferredOrders() {
    // return this.medOrderService.getDeptPreferredOrdersList();
    return this.listContents;
  }

  getDeptPreferredOrdersList() {
    this.isLoading = true
    this.medOrderService.getDeptPreferredOrdersList()
      .pipe(takeUntil(this.ngUnsubscribe))  
      .subscribe((resp) => {
        if (!resp || resp.length === 0) {
          this.isTabValid.emit(false);
        } else {
          console.log('dept pref has data, resp:', resp);
          this.isTabValid.emit(true);
          this.listContents = resp.map((o) => ({
            ...o,
            displayName: o.medication?.displayName,
            displayRoute: o.medicationRoute ? o.medicationRoute.routeName : '',
            displayFrequency: o.frequencySchedule ? o.frequencySchedule.scheduleName : '',
            displayDose: o.dose,
            displayDoseUnit: o.doseUnit ? o.doseUnit.printName : '',
            isComboMed: o.medication?.medicationDetails.length > 1 ? true : false,
            comboMedDetails: o.medication?.medicationDetails.length > 1 ? o.medication.medicationDetails.map((m) => ({
              brandName: m.brandName,
              dose: m.dose,
              doseUnit: m.doseUnit ? m.doseUnit.printName : ''
            })) : [],
            // allergyReactionsText: o.allergyReactions?.map((alg) => alg.patientAllergyName).join(', '),
            // drugInteractionsText: o.orderInteractions?.map((drug) => drug.drugInteraction.interactionOrderName + ' ( ' + drug.drugInteraction.severity + ' )').join(', ')
          }));
        }
        this.isLoading = false
      });
  }

  addToCart = (med) => {
    console.log('addToCart from deptPreferred list: med: ', med);

    this.cartStoreService.postCartOrderByListOrderId(med, med.id, this.patientId, this.userId, this.deptPreferred());
    console.log(`addToCart from deptPreferred list: ${med.id}  name: ${med.brandName} by userId: ${this.userId}`);
    med.hasBeenAdded = true;
  };


  editOrder = (med) => {
    // this.modalService.open('medComposer', {
    //   action: 'add',
    //   source: 'dept-list',
    //   med,
    // });
    this.launchMedComposer(med.id, med);
    console.log(`editOrder from Dept Preferred list: ${med.displayName}`);
  };

  launchMedComposer(medId: number, medData: object): void {
    this.composerSchedulerService.setInitialComposerData({ action: 'add', source: 'dept-list', med: medData });
    this.router.navigate(['new-order', medId],
      {
        // state: { data: { medData } },
        queryParams: {},
        relativeTo: this.route
      });
  }
}
