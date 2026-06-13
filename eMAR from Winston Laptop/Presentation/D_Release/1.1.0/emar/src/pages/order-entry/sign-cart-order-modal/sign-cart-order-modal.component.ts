import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { FormBuilder, Validator } from '@angular/forms'

import { ModalService } from 'src/services/modal.service';
import { CartService } from 'src/services/cart.service';
import { CartStoreService } from 'src/services/cart-store.service';
import { UserStoreService } from 'src/services/user-store.service';
import { PatientStoreService } from '../../../services/patient-store.service'
import { PatientMedOrderStoreService } from '../../../services/patient-med-order-store.service'
import { Subscription } from 'rxjs';

import { Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, map, tap } from 'rxjs/operators';

@Component({
  selector: 'sign-cart-order-modal',
  templateUrl: './sign-cart-order-modal.component.html',
  styleUrls: ['./sign-cart-order-modal.component.scss']
})
export class SignCartOrderModalComponent implements OnInit, OnDestroy {

  @Input() modalTitle: string;
  patientId: number;
  isDone: boolean = false;
  isProcessing: boolean = false;
  isSuccess: boolean = false
  hasError: boolean = false
  errorMessage: string
  modalSubscribe: Subscription

  orderingSource = ['Attending', 'Ordering Only']
  selectedSource: string

  attendingPhysicianList = []
  orderingOnlyPhysicianList = []

  orderingPhysicians = []
  // orderingOnlyPhysicians = []
  attendingPhysician: string
  selectedPhysician: {id: number, displayName: string} = null
  selectedDrugInteractionReasonId: number = null
  selectedAllergyReactionReasonId: number = null

  selectedDrugInteractionReasonMap: object = {}
  selectedAllergyReactionReasonMap: object = {}
  
  allergyReactionOverrideReasons = []
  drugInteractionOverrideReasons = []

  allergyReactionOrders = []
  drugInteractionOrders = []

  allergyReactionList = []
  allergyReactionListNew = []
  drugInteractionList = []
  drugInteractionListNew = []

  panelToggle = {}
  // panelToggleNew = {}
  panelToggleAllergy = {}
  panelToggleDrug = {}

  homeMedicationsFT = []
  patientAllergiesFT = []

  public model: any;


  get hasValidationErrors() : boolean {
    // console.log('hasValidationErrors', this.selectedPhysician)
    const hasPhysician = !!this.selectedPhysician

    return !hasPhysician
  }

  get errors() : object {
    const errors : any = {}

    if (!this.selectedPhysician) {
      errors.physician = "Physician is required"
    }

    return errors
  }

  get hasBlahErrors() : boolean {
    return Object.keys(this.errors).length > 0
  }

  constructor(
    private modalService: ModalService,
    private cartService: CartService,
    private cartStoreService: CartStoreService,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private patientMedOrderStoreService: PatientMedOrderStoreService,
  ) { }

  ngOnInit(): void {
    // this.isDone = false
    this.modalSubscribe = this.modalService.modalOpening.subscribe(({modalId}) => {
      console.log('---- modal opening', modalId)
      if (modalId === 'signCartOrder') {
        this._init()
        this._prepare()
      }
    })
  }

  ngOnDestroy(): void {
    if (this.modalSubscribe) this.modalSubscribe.unsubscribe() 
  }

  private _init(): void {
    this.isProcessing = false
    this.isSuccess = false
    this.hasError = false
    this.errorMessage = ''
    this.isDone = false
    this.selectedSource  = 'Ordering Physician'
    this.selectedPhysician = null
    this.panelToggle = {}
    // this.panelToggleNew = {}
    this.panelToggleAllergy = {}
    this.panelToggleDrug = {}

    this.selectedAllergyReactionReasonMap = []
    this.selectedDrugInteractionReasonMap = []

    this.orderingPhysicians = []
    this.attendingPhysicianList = []
    this.orderingOnlyPhysicianList = []

    console.log('SignCartOrdersModal - _init() completed')
  }

  async _prepare() {
    const cartOrders = this.cartStoreService.cartOrders
    console.log('_______prepare: cartOrders: ', cartOrders)
    const homeMedications = this.patientStoreService.homeMedications
    console.log('_______prepare: homeMedications: ', homeMedications)
    const patientAllergies = this.patientStoreService.patientAllergies
    console.log('_______prepare: patientAllergies: ', patientAllergies)
    const patientOrders = this.patientMedOrderStoreService.patientMedOrder
    console.log('_______prepare: patientOrders: ', patientOrders)

    this.homeMedicationsFT = homeMedications.filter( med => med.internalDrugId === 'ft')
    console.log('_______prepare: homeMedicationsFT: ', this.homeMedicationsFT)
    this.patientAllergiesFT = patientAllergies.filter( alg => alg.internalDrugId === 'ft')
    console.log('_______prepare: patientAllergiesFT: ', this.patientAllergiesFT)

    const p = this.cartService.getPreCheckoutForSign(this.getPatient(), this.userStoreService.userId)
    const preparation = await p

    this.orderingPhysicians = preparation.orderingPhysicianData.availableOrderingPhysicians
    preparation.orderingPhysicianData.availableOrderingPhysicians.forEach( doc => {

      doc.displayName = `${doc.lastName}, ${doc.firstName}`

      if (doc.orderingOnlyPhysician) {
        this.orderingOnlyPhysicianList.push(doc)
      } else {
        this.attendingPhysicianList.push(doc)
      }
    })

    const interactionMap = preparation.drugInteractionOrders.reduce( (prev, ord) => {
      console.log('*******id:', ord.id)
      ord.orderInteractions.forEach( interaction => {
        const obj = prev[interaction.medicationInteractionId] || {
          medicationInteractionId: interaction.medicationInteractionId,
          interactionOrderName: interaction.drugInteraction.interactionOrderName,
          // patientOrderId: interaction.patientOrderId,

          patientCartOrderId: ord.id,
          patientCartOrderOrdered: 
            interaction.patientCartOrderId ? cartOrders.find( ord => ord.id === ord.id) : null,

          // patientCartOrderId: interaction.patientCartOrderId !== ord.id ? interaction.patientCartOrderId : null,
          // patientCartOrderOrdered: 
          //   interaction.patientCartOrderId !== ord.id ?
          //   interaction.patientCartOrderId ? cartOrders.find( ord => ord.id === interaction.patientCartOrderId) : null
          //   : null,

          // patientHomeMedicationId: interaction.patientHomeMedicationId,
          hasPatientOrderInteraction: interaction.drugInteraction.interactionOrderTable === 'patient_orders',
          hasHomeMedicationInteraction: interaction.drugInteraction.interactionOrderTable === 'patient_home_medications',
          hasPatientCartOrderInteraction: interaction.drugInteraction.interactionOrderTable === 'patient_cart_orders',
          drugInteractions: [],
        }

        obj.drugInteractions.push({
          ...interaction.drugInteraction,
          // isInteractWithPatientOrder: interaction.drugInteraction.interactionOrderTable === 'patient_orders',
          patientOrder: 
            interaction.drugInteraction.interactionOrderTable === 'patient_orders' ? 
            interaction.drugInteraction.interactionOrderId ? patientOrders.find( ord => ord['id'] === interaction.drugInteraction.interactionOrderId) : null
            : null,
          // isInteractWithHomeMedication: interaction.drugInteraction.interactionOrderTable === 'patient_home_medications',
          patientHomeMedication:
            interaction.drugInteraction.interactionOrderTable === 'patient_home_medications' ?
            interaction.drugInteraction.interactionOrderId ? homeMedications.find( ord => ord.id === interaction.drugInteraction.interactionOrderId) : null
            : null,
          // isInteractWithPatientCartOrder: 
          //   interaction.drugInteraction.interactionOrderTable === 'patient_cart_orders' && 
          //   interaction.drugInteraction.interactionOrderId !== interaction.patientCartOrderId,  // not itself, !=== patientCartOrderOrderedId
          // patientCartOrder: 
          //   interaction.drugInteraction.interactionOrderTable === 'patient_cart_orders' && 
          //   interaction.drugInteraction.interactionOrderId !== interaction.patientCartOrderId ?
          //   interaction.drugInteraction.interactionOrderId ? cartOrders.find( ord => ord.id === interaction.drugInteraction.interactionOrderId) : null
          //   : null,

          // isInteractWithPatientCartOrder: 
          //   interaction.drugInteraction.interactionOrderTable === 'patient_cart_orders',  // not itself, !=== patientCartOrderOrderedId
          patientCartOrder: 
            interaction.drugInteraction.interactionOrderTable === 'patient_cart_orders' ?
            interaction.patientCartOrderId ? cartOrders.find( ord => ord.id === interaction.patientCartOrderId) : null
            : null,
        })

        prev[interaction.medicationInteractionId] = obj
      } )

      return prev
    }, {})

    console.log('^^^^^^interactionMap: ', interactionMap)
    this.drugInteractionList = Object.values(interactionMap)

    //added another layer on the top of drugInteractionList to reconstruct the override interactions
    //base on the cart orders instead of medicationInteraction
    const interactionMapNew = this.drugInteractionList.reduce( (prev, interaction) => {
     
      const obj = prev[interaction.patientCartOrderId] || {
        patientCartOrderId: interaction.patientCartOrderId,
        patientCartOrder: interaction.patientCartOrderId ? cartOrders.find( ord => ord.id === interaction.patientCartOrderId) : null,
        interactions: [],
      }
      prev[interaction.patientCartOrderId] = obj
      
      obj.interactions.push(interaction)
    
      return prev
    }, {})

    console.log('^^^^^^^^^^^^^^^interactionMapNew: ', interactionMapNew)
    this.drugInteractionListNew = Object.values(interactionMapNew).filter(({patientCartOrder}) => patientCartOrder)


    const reactionMap = preparation.allergyReactionOrders.reduce( (prev, ord) => {
      ord.allergyReactions.forEach( reaction => {
        const obj = prev[reaction.id] || {
          orderReactionId: reaction.id,
          patientAllergyName: reaction.patientAllergyName,
          allergyReactions: [],
          
          patientCartOrderId: reaction.orderId,
          patientCartOrderOrdered: 
            reaction.orderId ? cartOrders.find( ord => ord.id === reaction.orderId) : null,
          hasPatientOrderReaction: reaction.orderTable === 'patient_orders',
          hasHomeMedicationReaction: reaction.orderTable === 'patient_home_medications',
          hasPatientCartOrderReaction: reaction.orderTable === 'patient_cart_orders',
        }

        obj.allergyReactions.push(reaction)

        prev[reaction.id] = obj
      })

      return prev
    }, {})

    this.allergyReactionList = Object.values(reactionMap)
    console.log('^^^^^^reactionMap: ', reactionMap)

    //added another layer on the top of allergyReactionList to reconstruct the override reactions
    //base on the cart orders instead of order_reaction
    const reactionMapNew = this.allergyReactionList.reduce( (prev, reaction) => {
     
      const obj = prev[reaction.patientCartOrderId] || {
        patientCartOrderId: reaction.patientCartOrderId,
        patientCartOrder: reaction.patientCartOrderId ? cartOrders.find( ord => ord.id === reaction.patientCartOrderId) : null,
        reactions: []
      }
      prev[reaction.patientCartOrderId] = obj
      
      obj.reactions.push(reaction)
    
      return prev
    }, {})

    console.log('^^^^^^^^^^^^^^^reactionMapNew: ', reactionMapNew)
    this.allergyReactionListNew = Object.values(reactionMapNew).filter(({patientCartOrder}) => patientCartOrder)


    this.attendingPhysician = preparation.orderingPhysicianData.patientsErAttendingDoc
    this.allergyReactionOrders = preparation.allergyReactionOrders
    this.drugInteractionOrders = preparation.drugInteractionOrders
    this.allergyReactionOverrideReasons = preparation.allergyReactionOverrideReasons
    this.drugInteractionOverrideReasons = preparation.drugInteractionOverrideReasons
    
    if (this.attendingPhysician) { 
      // default the source (Attending) and physician if attendingPhysician is assigned
      this.changeSource('Attending')
    }

    console.log('----preparation: ', preparation)
    console.log('----orderingPhysicians: ', this.orderingPhysicians)
    console.log('----attendingPhysician: ', this.attendingPhysician)
    console.log('----allergyReactionOverrideReasons: ', this.allergyReactionOverrideReasons)
    console.log('----drugInteractionOverrideReasons: ', this.drugInteractionOverrideReasons)
    console.log('----allergyReactionOrders: ', this.allergyReactionOrders)
    console.log('----drugInteractionOrders: ', this.drugInteractionOrders)
    console.log('----orderingOnlyPhysicianList: ', this.orderingOnlyPhysicianList)
    console.log('----attendingPhysicianList: ', this.attendingPhysicianList)
    console.log('----drugInteractionList: ', this.drugInteractionList)
    console.log('----drugInteractionListNew: ', this.drugInteractionListNew)
    console.log('----allergyReactionList: ', this.allergyReactionList)
    console.log('----allergyReactionListNew: ', this.allergyReactionListNew)

  }

  getData() {
    return this.modalService.retrieveModalData('signCartOrder') || {}
  }

  getPatient() {
    return this.getData().patientId || 0
  }

  selectedOrderingSourceDisplay() {
      return this.selectedSource
  }
  
  changeSource(newSource: string) {
    this.selectedSource = newSource;
    console.log('changeSource: ', this.selectedSource)
    this.selectedPhysician = this.selectedSource === 'Attending'
      ? this.attendingPhysicianList.find((p) => p.id === this.attendingPhysician) || null
      : null

    this.model = this.selectedPhysician ? this.selectedPhysician.displayName : null
    console.log('changeSource model: ', this.model)
  }

  get selectedPhysicianDisplay() : string {
    // console.log('++++attendingPhysician:', this.attendingPhysician)
    // console.log('++++selectedSource:', this.selectedSource)
    // console.log('++++selectedPhysician:', (this.selectedPhysician === {}))

    // return this.orderingPhysicians.find((p) => p.displayName === this.attendingPhysician)?.displayName || ''
    // if (this.selectedSource === 'Attending' && this.attendingPhysician && Object.keys(this.selectedPhysician).length === 0) {
      // return this.attendingPhysicianList.find((p) => p.id === this.attendingPhysician) || {}
    // } else {
      return this.selectedPhysician?.displayName || ''
    // }
    // return this.orderingPhysicians.find((p) => p.id === this.attendingPhysician)?.displayName || ''
  }

  changePhysician(newPhysician: any) {
    this.selectedPhysician = newPhysician;
    console.log('selectedPhysician: ', this.selectedPhysician)
  }

  onItemChangeDrug(item, medicationInteractionId) {
    console.log('onItemChangeDrug: ', item)

    this.selectedDrugInteractionReasonMap[medicationInteractionId] = item.id
  }
  
  onItemChangeAllergy(item, orderReactionId) {
    console.log('onItemChangeAllergy: ', item)

    this.selectedAllergyReactionReasonMap[orderReactionId] = item.id
  }

  toggle(panel: string) {
    console.log('toggle ME..: ', panel);
    this.panelToggle[panel] = !this.panelToggle[panel];
  }
  
  toggleNew(panel: string, type: string) {
    console.log('toggleNew panel: ', panel, ' type: ', type);
    if (type === 'allergy') {
      this.panelToggleAllergy[panel] = !this.panelToggleAllergy[panel];
    } else { //type === drug
      this.panelToggleDrug[panel] = !this.panelToggleDrug[panel];
    }
    // this.panelToggleNew[panel] = !this.panelToggleNew[panel];
  }

  // changeOrderingOnlyPhysician(newPhysician: string) {
  //   this.selectedPhysician = newPhysician;
  //   console.log('changeOrderingOnlyPhysician: ', this.selectedPhysician)
  // }

  cancelSign = () => {
    this.modalService.close('signCartOrder');
  }

  confirmedSign = async () => {
    console.log('confirmedSign for parientId: ', this.getPatient(), 
      this.selectedDrugInteractionReasonMap, 
      this.selectedPhysician, 
      this.selectedAllergyReactionReasonMap
    );
    this.patientId = this.getPatient();

    this.isProcessing = true;

    try {

      const drugInteractionOverrideRationalia = []
      // for (const medicationInteractionId in this.selectedDrugInteractionReasonMap) {
      //   drugInteractionOverrideRationalia.push({
      //     medicationInteractionId,
      //     overrideReasonId: String(this.selectedDrugInteractionReasonMap[medicationInteractionId]),
      //   })
      // }
      for (const cartorder of this.drugInteractionListNew) {
        const commonValue = !!this.panelToggleDrug[cartorder.patientCartOrderId]

        if (commonValue) {
          const lastInteraction = cartorder.interactions[cartorder.interactions.length - 1]
          const lastOverrideReasonId = this.selectedDrugInteractionReasonMap[lastInteraction.medicationInteractionId]

          if (lastOverrideReasonId) {
            for (const {medicationInteractionId} of cartorder.interactions) {
              drugInteractionOverrideRationalia.push({
                medicationInteractionId: String(medicationInteractionId),
                overrideReasonId: String(lastOverrideReasonId),
              })
            }
          }
        } else {
          for (const {medicationInteractionId} of cartorder.interactions) {
            const overrideReasonId = this.selectedDrugInteractionReasonMap[medicationInteractionId]

            if (overrideReasonId) {
              drugInteractionOverrideRationalia.push({
                medicationInteractionId: String(medicationInteractionId),
                overrideReasonId: String(overrideReasonId),
              }) 
            }
          }
        }
      }

      console.log('********drugInteractionOverrideRationalia', drugInteractionOverrideRationalia)

      const allergyReactionOverrideRationalia = []
      // for (const orderReactionId in this.selectedAllergyReactionReasonMap) {
      //   allergyReactionOverrideRationalia.push({
      //     orderReactionId,
      //     overrideReasonId: String(this.selectedAllergyReactionReasonMap[orderReactionId])
      //   })
      // }
      for (const cartorder of this.allergyReactionListNew) {
        const commonValue = !!this.panelToggleAllergy[cartorder.patientCartOrderId]

        if (commonValue) {
          const lastReaction = cartorder.reactions[cartorder.reactions.length - 1]
          const lastOverrideReasonId = this.selectedAllergyReactionReasonMap[lastReaction.orderReactionId]

          if (lastOverrideReasonId) {
            for (const {orderReactionId} of cartorder.reactions) {
              allergyReactionOverrideRationalia.push({
                orderReactionId: String(orderReactionId),
                overrideReasonId: String(lastOverrideReasonId),
              })
            }
          }
        } else {
          for (const {orderReactionId} of cartorder.reactions) {
            const overrideReasonId = this.selectedAllergyReactionReasonMap[orderReactionId]

            if (overrideReasonId) {
              allergyReactionOverrideRationalia.push({
                orderReactionId: String(orderReactionId),
                overrideReasonId: String(overrideReasonId),
              }) 
            }
          }
        }
      }

      console.log('********allergyReactionOverrideRationalia:', allergyReactionOverrideRationalia)


      const body = {
        "orderingPhysicianUserId": this.selectedPhysician.id, 
        drugInteractionOverrideRationalia, 
        allergyReactionOverrideRationalia
      }
      console.log('********POST BODY:', body)

      await this.cartStoreService.postAllCartOrders(
        this.patientId, 
        this.userStoreService.userId, 
        // {"orderingPhysicianUserId": this.selectedPhysician.id},
        body
      )
      
      this.isDone = true;
      // this.isSuccess = true;
      
      // setTimeout( () => {
        this.modalService.close('signCartOrder')
        setTimeout(()=> {this._init()}, 100)
      // }, 2000)

    } catch (err) {
      this.isDone = true;
      this.hasError = true;
      this.errorMessage = `${err.status} ${err.statusText} ${err.error}`
      
      setTimeout( () => {
        this.modalService.close('signCartOrder')
        setTimeout(()=> {this._init()}, 100)
      }, 2000)
    }
  }
  
  /* ordering physiscan typeahead search function set */
  searchAttending = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      map(term => term === '' ? []
        : this.attendingPhysicianList.filter(p => p.displayName.toLowerCase().indexOf(term.toLowerCase()) > -1)
      ),
      tap(() => this.selectedPhysician = null),
      // tap(() => console.log('searchAttending'))
    )

  searchOrderingOnly = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      map(term => term === '' ? []
        : this.orderingOnlyPhysicianList.filter(p => p.displayName.toLowerCase().indexOf(term.toLowerCase()) > -1)
      ),
      tap(() => this.selectedPhysician = null),
      // tap(() => console.log('searchOrderingOnly'))
    )

  inputFormat(value: any) {
    // console.log('inputFormat value: ', value)
    return value;
  }

  onSelect($event, input) {
    $event.preventDefault();
    // console.log('onSelect: ', $event.item);
    // console.log('next from NEW: ', $event.item);

    input.value =  $event.item.displayName;
    
    this.changePhysician($event.item)

    input.blur();
  }

  rt(value: any) {
    return value.displayName;
  }


}
