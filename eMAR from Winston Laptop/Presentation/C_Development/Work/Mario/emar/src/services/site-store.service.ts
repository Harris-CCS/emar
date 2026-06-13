import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { SiteService } from 'src/services/site.service'
import { UserStoreService } from 'src/services/user-store.service'

import { Site } from 'src/app/interfaces/site'
import { Frequency } from 'src/app/interfaces/frequency'

@Injectable({
  providedIn: 'root'
})
export class SiteStoreService {

  private siteId = this.userStoreService.userSiteId //|| 31
  private userId = this.userStoreService.userId

  constructor(
    private siteService: SiteService,
    private userStoreService: UserStoreService
  ) {
    console.log('SiteStoreService constructor')
    
    this.userStoreService.user$.subscribe(async () => {
      console.log('SiteStore: constructor: subscribed UserStoreService')
      this.siteId = this.userStoreService.userSite.id
      console.log('SiteStore: constructor: siteId:', this.siteId, '*****userId:', this.userId)
      
      if (this.userId && this.siteId) {
        this.site = this.userStoreService.userSite
        this.fetchSiteOptions()
        this.fetchSiteMedicationFrequencies()

        console.log('SiteStore: constructor: site:', this.site)
      }
    })
  }
  
  private readonly _site = new BehaviorSubject<Site>(<Site>{})
  readonly site$ = this._site.asObservable()

  // Site
  get site(): Site {
    return this._site.getValue() || <Site>{}
  }

  set site(val: Site) {
    this._site.next(val)
  }

  get timeZoneName(): string {
    return this.site.timeZoneName || ''
  }
  
  get timeZoneOffset(): string {
    return this.site.timeZoneOffset || ''
  }

  // SiteOptions - Global options
  get antimicrobial_display(): string {
    return this.site.siteOptions.antimicrobial_display || ''
  }

  get host_server_url(): string {
    return this.site.siteOptions.host_server_url || ''
  }

  // SiteOptions - Specific options
  get long_date_format(): string {
    return this.site.siteOptions.long_date_format || ''
  }

  get short_date_format(): string {
    return this.site.siteOptions.short_date_format || ''
  }

  get patient_image_path(): string {
    return this.site.siteOptions.patient_image_path || ''
  }

  get schedule_future_items(): number {
    return this.site.siteOptions.schedule_future_items || null
  }

  get custom_indicators_image_path(): string {
    return this.site.siteOptions.custom_indicators_image_path || ''
  }

  get medinpat(): string {
    return this.site.siteOptions.medinpat || ''
  }

  get medoutpat(): string {
    return this.site.siteOptions.medoutpat || ''
  }

  get rxalert(): number {
    return this.site.siteOptions.rxalert || null
  }
  
  get medpyxis(): string {
    return this.site.siteOptions.medpyxis || ''
  }

  get medexactmatch(): string {
    return this.site.siteOptions.medexactmatch || ''
  }

  get drug_db_vendor(): string {
    return this.site.siteOptions.drug_db_vendor || ''
  }

  get session_timeout(): number {
    return this.site.siteOptions.session_timeout || null
  }

  get session_timeout_url(): string {
    return this.site.siteOptions.session_timeout_url || ''
  }

  get show_dose_form(): string {
    return this.site.siteOptions.show_dose_form || ''
  }

  get show_strength(): string {
    return this.site.siteOptions.show_strength || ''
  }
  
  get popup_on_give(): string {
    return this.site.siteOptions.popup_on_give || ''
  }

  get default_printer_id(): string {
    return this.site.siteOptions.default_printer_id || ''
  }

  // MISC - site level static data can be added to here
  get SiteMedicationFrequencies(): Frequency[] {
    return this.site.medicationFrequencies || []
  }

  async fetchSiteOptions() {
    this.site.siteOptions = await this.siteService.getSiteOptionsFromAPI(this.userId, this.siteId, 'all').toPromise()
    // console.log('SiteStore - fetchSiteOptions: finished: site: ', this.site)
  }

  async fetchSiteMedicationFrequencies() {
    this.site.medicationFrequencies = await this.siteService.getSiteMedicationFrequenciesFromAPI(this.siteId).toPromise()
  }

}