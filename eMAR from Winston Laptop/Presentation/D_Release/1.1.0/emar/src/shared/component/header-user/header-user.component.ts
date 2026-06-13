import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { UserStoreService } from '../../../services/user-store.service';
import { PatientStoreService } from '../../../services/patient-store.service';
import { SiteService } from '../../../services/site.service';
import { SiteOptions } from '../../../app/interfaces/site-options';

import { User } from '../../../app/interfaces/user';
import { Patient } from 'src/app/interfaces/patient';
import { PatientExternalIdData } from 'src/app/interfaces/patient-external-id-data';
import { ThrowStmt } from '@angular/compiler';
// import { NOTIFICATIONS } from '../../../app/mockup/notifications';
import { UserService } from 'src/services/user.service';

@Component({
  selector: 'header-user',
  templateUrl: './header-user.component.html',
  styleUrls: ['./header-user.component.scss'],
})
export class HeaderUserComponent implements OnInit, OnDestroy {
  @Input() user: User;
  @Input() title: string;
  interval: number;
  currentTime: string = '';
  siteUTCOffset: string = '';
  siteOptions: SiteOptions;
  patientId: number;
  patientExternalIdData: PatientExternalIdData;
  patientExternalId: string;
  nbNotifications: number = 0;

  constructor(
    private datePipe: DatePipe,
    private userStoreService: UserStoreService,
    private patientStoreService: PatientStoreService,
    private siteService: SiteService,
    private userService: UserService
  ) {
    this.siteUTCOffset = this.userStoreService.userSite.timeZoneOffset;
    this.currentTime = this.datePipe.transform(Date.now(), 'HH:mm', this.siteUTCOffset);
    this.getNbNotifications(this.userStoreService.userId, this.userStoreService.userSiteId);
  }

  ngOnInit(
  ) {
    this.interval = setInterval(() => {
      this.currentTime = this.datePipe.transform(Date.now(), 'HH:mm', this.siteUTCOffset);
      this.getNbNotifications(this.userStoreService.userId, this.userStoreService.userSiteId)
    }, 10000);
    this.getSiteData();
    // alert(`Ext Id1: ${this.PatientStoreService.extId1}`);
    // console.log('userHeaderThis', this);
  }

  ngOnDestroy() {
    if (this.interval) {
      clearInterval(this.interval);
    }
  }
  getNbNotifications(userId: number, siteId: number) {
    // this.nbNotifications = Math.floor(Math.random()*100);
    this.userService.getNbNotifications(userId, siteId).subscribe( data => {
      if (data) {
        this.nbNotifications = data.total
      } else {
        this.nbNotifications = 0;
      }
    });
  }

  async getSiteData() {
    this.siteOptions = this.siteService.getSiteOptions() || await this.getSiteOptionsFromAPI();
  }

  async getSiteOptionsFromAPI() {
    const siteOptionsAPIResponse: SiteOptions = await this.siteService.getSiteOptionsFromAPI(
      this.userStoreService.userId,
      this.userStoreService.userSiteId,
      'all'
    ).toPromise();
    // console.log('siteOptionsInUserHeader', siteOptionsAPIResponse);
    if (siteOptionsAPIResponse && siteOptionsAPIResponse.host_server_url) {
      this.siteService.setSiteOptions(siteOptionsAPIResponse);
      // console.log('setSiteOptionsInUserHeader', this.siteService.getSiteOptions());
      return siteOptionsAPIResponse;
    } else {
      return null;
    }
  }

  onLogout() {
    this.user = null;
    this.user;
  }

  isDisabledLink(linkDependency: string): boolean {

    if (!this.siteOptions || !this.siteOptions.host_server_url.includes('http')) {
      return true;
    }
    else if (linkDependency === 'hostServerURL') {
      return false;
    } else if (linkDependency === 'patientId') {
      if (!this.patientId) {
        this.patientId = this.patientStoreService.patientId;
      }
      return !this.patientId ? true : false;
    } else if (linkDependency === 'externalPatientId') {
      if (!this.patientExternalId) {
        this.patientExternalIdData = this.patientStoreService.patientExternalIdData;
        if (this.patientExternalIdData) {
          this.patientExternalId = (this.patientExternalIdData.vendor === 'pulsecheck') ?
            this.patientExternalIdData.externalId.split('|').pop() :
            this.patientExternalIdData.externalId;
          // console.log('External Id set in user header', this.patientExternalId, this.patientExternalIdData);
          // console.log('siteOptions', this.siteOptions);
        }
      }
      return (!this.patientExternalIdData || !this.patientExternalId) ? true : false;
    } else {
      return true;
    }

  }

  assignEMARRoute(location: string): string {
    // if (!this.patientId) {
    //   this.patientId = this.patientStoreService.patientId;
    // }
    if (location) {
      if (location === 'MAR - Dept') {
        return `${window.location.origin}/patients`;
      } else if (this.patientId) {
        switch (location) {
          case 'Med Services': {
            return `${window.location.origin}/patients/${this.patientId}/medservice`;
          }
          case 'MAR - Patient': {
            return `${window.location.origin}/patients/${this.patientId}`;
          }
          default: {
            return '#';
          }
        }
      }
    }
    return '#';
  }

  assignExternalRoute(location: string): string {
    // const url = `http://ros-demo-zx01.picis.com${location}`;
    // if (!this.patientExternalId) {
    //   this.patientExternalIdData = this.patientStoreService.patientExternalIdData;
    //   if (this.patientExternalIdData) {
    //     this.patientExternalId = (this.patientExternalIdData.vendor === 'pulsecheck') ?
    //       this.patientExternalIdData.externalId.split('|').pop() :
    //       this.patientExternalIdData.externalId;
    //     console.log('External Id set in user header', this.patientExternalId, this.patientExternalIdData);
    //     console.log('siteOptions', this.siteOptions);
    //   }
    // }

    if (location && this.siteOptions && this.siteOptions.host_server_url) {
      if (location === 'MainTrackingBoard') {
        return `${this.siteOptions.host_server_url}/index.mpex`;
      } else if (location === 'Help') {
        return `${this.siteOptions.host_server_url}/help/Default_Left.htm#CSHID=index|SkinName=Picis%20Skin|OpenType=Javascript`;
      }
      else if (location === 'Archive') {
        return `${this.siteOptions.host_server_url}/rep01.ibx`;
      }
      else if (location === 'Help') {
        return `${this.siteOptions.host_server_url}/help/Default_Left.htm#CSHID=index|SkinName=Picis%20Skin|OpenType=Javascript`;
      } else if (location === 'My Charts') {
        return `${this.siteOptions.host_server_url}/rep08.ibx`;
      } else if (this.patientExternalId) {
        switch (location) {
          case 'Allergies': {
            return `${this.siteOptions.host_server_url}/ibex1s.ibx?aorm=A&p=${this.patientExternalId}`;
          }
          case 'Chart': {
            return `${this.siteOptions.host_server_url}/ibex70.mpex?f=d&p=${this.patientExternalId}`;
          }
          case 'Current Medications': {
            return `${this.siteOptions.host_server_url}/ibex1s.ibx?aorm=M&p=${this.patientExternalId}`;
          }
          case 'DCI': {
            return `${this.siteOptions.host_server_url}/ibex93.ibx?p=${this.patientExternalId}`;
          }
          case 'Disposition': {
            return `${this.siteOptions.host_server_url}/ibex11.ibx?p=${this.patientExternalId}`;
          }
          case 'Medication Reconciliation': {
            return `${this.siteOptions.host_server_url}/ibex1s.ibx?aorm=R&p=${this.patientExternalId}`;
          }
          case 'Orders': {
            return `${this.siteOptions.host_server_url}/ibex6a.ibx?p=${this.patientExternalId}`;
          }
          case 'Patient Data': {
            return `${this.siteOptions.host_server_url}/ibex44.ibx?p=${this.patientExternalId}`;
          }
          case 'Rx': {
            return `${this.siteOptions.host_server_url}/ibex07.ibx?p=${this.patientExternalId}`;
          }
          default: {
            return '#';
          }
        }
      }
    }
    return '#';
  }

  isSelected(location: string): boolean {
    return window.location.href === this.assignEMARRoute(location) ? true : false
  }
}
