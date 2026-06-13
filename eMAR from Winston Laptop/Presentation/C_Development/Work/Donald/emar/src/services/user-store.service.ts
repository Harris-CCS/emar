import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

import { User } from 'src/app/interfaces/user';
import { UserService } from './user.service';
import { Site } from '../app/interfaces/site';
import { UserSetting } from '../app/interfaces/user-setting';

// interface Site {
//   id: number;
//   name: string;
//   active: boolean;
//   timeZoneName: string;
//   timeZoneOffset?: string;
// }

@Injectable({
  providedIn: 'root',
})
export class UserStoreService {

  /* ibex.drs.num */
  // extId: number = 10620 //14893
  extId: number
  /* emar.users.id */
  emarUserId: number

  constructor(private userService: UserService) {
    console.log('UserStoreService constructor')
    if (this.extId) {
      this.fetchUserByExtId(this.extId)
    }
  }

  private readonly _user = new BehaviorSubject<User>(<User>{ site: {} });
  readonly user$ = this._user.asObservable();
  readonly userSite$ = this.user$.pipe(map((user) => user?.site));
  readonly userSettings$ = this.user$.pipe(map((user) => user?.userSettings))
  private readonly _users = new BehaviorSubject<User[]>([]);
  readonly users$ = this._users.asObservable();

  get user(): User {
    return this._user.getValue() || <User>{};
  }

  set user(val: User) {
    this._user.next(val);
  }

  get users(): User[] {
    return this._users.getValue() || [];
  }

  set users(val: User[]) {
    this._users.next(val);
  }


  get userDeptPageFilter(): string {
    return sessionStorage.getItem('deptFilter')
  }
  
  set userDeptPageFilter(filter: string) {
    sessionStorage.setItem('deptFilter', String(filter))
  }


  get userId(): number {
    // console.log('UserStore: userId: ', this._user.getValue().id)
    // return this._user.getValue().id
    // return this._user.getValue().id || 5205 //2729
    // return 27
    return Number(sessionStorage.getItem('userId'))
  }

  set userId(userId: number) {
    sessionStorage.setItem('userId', String(userId))
    // this._user.next(val)
  }

  //currently selected department in PCED when user redirects to eMAR
  get departmentCode(): string {
    return sessionStorage.getItem('departmentCode')
  }

  set departmentCode(departmentCode: string) {
    sessionStorage.setItem('departmentCode', departmentCode)
  }
  get browser(): string {
    return sessionStorage.getItem('browser');
  }
  set browser(browser: string) {
    sessionStorage.setItem('browser', browser);
  }

  //currently selected ward in PCED when user redirects to eMAR
  get wardCode(): string {
    return sessionStorage.getItem('wardCode')
  }

  set wardCode(wardCode: string) {
    sessionStorage.setItem('wardCode', wardCode)
  }

  get userSite(): Site {
    const value = this._user.getValue() || { site: <Site>{} };
    // console.log('UserStore: userSite: ', value.site);
    return value.site;
  }

  get userSiteId(): number {
    // console.log('UserStore: userSiteId: ', this.userSite.id)
    return this.userSite.id
    // return this.userSite.id || 16
    // return 12
  }

  get userSiteTimeZoneOffset(): string {
    // console.log('UserStore: userTimeZoneOffset: ', this.userSite.timeZoneOffset);
    return this.userSite.timeZoneOffset;
  }

  get userSettings(): UserSetting[] {
    const value = this._user.getValue() || { userSettings: [] };
    // console.log('UserStore: userSite: ', value.site);
    return value.userSettings;
  }

  // get MEDICATION_SERVICES(): UserSetting[] {
  //   return this.userSettings.filter(setting => setting.settingDescription === 'MEDICATION_SERVICES')
  // }

  // Controls who has access to EMAR  allowable values Read (R) or Write (W) or Exclude (E)
  get MEDICATION_SERVICES(): string {
    return this.userSettings.filter(setting => setting.settingDescription === 'MEDICATION_SERVICES')[0].settingValue
  }
  
  // Y = "Full Name", I = "Last Name, First Initial", N = "Anonymous"
  get PATIENT_NAME_DISPLAY(): string {
    return this.userSettings.filter(setting => setting.settingDescription === 'PATIENT_NAME_DISPLAY')[0].settingValue
  }
  
  // E = "Entry Time", A = "Administration Time"
  get PATIENT_PAGE_SORT(): string {
    return this.userSettings.filter(setting => setting.settingDescription === 'PATIENT_PAGE_SORT')[0].settingValue
  }
  
  // B = "Bed", P = "Patient Name", E ="Event Time"
  get DEPARTMENT_PAGE_SORT(): string {
    return this.userSettings.filter(setting => setting.settingDescription === 'DEPARTMENT_PAGE_SORT')[0].settingValue
  }
  
  // A = "All Patients", M = "My Patients", V = "Pharmacist Verification Needed"
  get DEPARTMENT_PAGE_FILTERING(): string {
    return this.userSettings.filter(setting => setting.settingDescription === 'DEPARTMENT_PAGE_FILTERING')[0].settingValue
  }

  // Device ID from devices table
  get LAST_USED_PRINTER(): string {
    return this.userSettings.filter(setting => setting.settingDescription === 'LAST_USED_PRINTER')[0].settingValue
  }


  async fetchUser() {
    console.log('UserStore - fetchUser: 0', this.userId)
    if (this.userId) {
      this.user = await this.userService.getUser(this.userId).toPromise()
    }

    console.log('UserStore - fetchUser: ', this.user)
  }

  async fetchUserByExtId(extId) {
    this.user = await this.userService.getUserByExtId(extId).toPromise()

    console.log('UserStore - fetchUserByExtId: ', this.user)
  }

  async fetchUsers() {
    this.users = await this.userService.getUsers().toPromise();
  }
}
