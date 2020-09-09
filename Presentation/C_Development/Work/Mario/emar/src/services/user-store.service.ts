import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

import { User } from 'src/app/interfaces/user';
import { UserService } from './user.service';


interface Site {
  id: number,
  name: string,
  active: boolean,
  timeZoneName: string
}

@Injectable({
  providedIn: 'root'
})
export class UserStoreService {

  extId: number = 10620 //14893

  constructor(private userService: UserService) { 
    this.fetchUser(this.extId)
  }

  private readonly _user = new BehaviorSubject<User>(<User>{site:{}})
  readonly user$ = this._user.asObservable()
  readonly userSite$ = this.user$.pipe(
    map(user => user?.site) 
  )

  get user(): User {
    return this._user.getValue() || <User>{}
  }

  set user(val: User) {
    this._user.next(val)
  }

  get userId(): number {
    console.log('UserStore: userId: ', this._user.getValue().id)
    return this._user.getValue().id || 5205 //2729
    // return 27
  }

  set userId(val: number) {
    // this._user.next(val)
  }

  get userSite(): Site {
    const value = this._user.getValue() || {site: <Site>{}}
    console.log('UserStore: userSite: ', value.site)
    return value.site
  }

  get userSiteId(): number {
    console.log('UserStore: userSiteId: ', this.userSite.id)
    return this.userSite.id || 16
    // return 12
  }

  async fetchUser(extId) {
    this.user = await this.userService.getUserByExtId(extId).toPromise()

    console.log('UserStore - fetchUser: ', this.user)
  }
}

