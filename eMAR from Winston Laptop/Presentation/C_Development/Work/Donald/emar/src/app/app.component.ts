import { Component, OnInit } from '@angular/core';
// import { Subject } from 'rxjs'

import { User } from './interfaces/user';
// import { UserService } from '../services/user.service';
import { UserStoreService} from '../services/user-store.service';

import { USER } from 'src/app/mockup/user';
import { Observable, TimeoutError } from 'rxjs';

import { async } from 'rxjs/internal/scheduler/async';
import { AuthService } from 'src/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  title: string = 'emar';
  user: User;
  // hazLoaded = new Subject<boolean>()
  // hanLoaded : boolean = false

  constructor(
    // private userService: UserService,
    public userStoreService: UserStoreService,
    public authService: AuthService,
  ) {
    // this.loginUser();
  }

  ngOnInit() {
    // this.loginUser();

    // this.hazLoaded.next(true)
    // this.userStoreService.fetchUser()
    // this.userStoreService.user$.subscribe(() => {
    //   console.log('user has finished', this.userStoreService.user)
    //   if (this.userStoreService.user.id) {
    //     this.hanLoaded = true
    //     // this.hazLoaded.next(true)
    //   }
    // })

    // setTimeout(() => {this.hanLoaded = true}, 2000)
  }

  // loginUser() {
  //   // this.user = USER;
  //   const userId: number = 27;
  //   // Mock Data
  //   // this.user = this.userService.getUser(userId);

  //   // API
  //   this.userService.getUser(userId).subscribe((user) => {
  //     this.user = user;
  //   });

  //   /* this.userService.fetchUser(244).subscribe(user => {
  //     console.log('USER');console.log(user)
  //   });
  //   */

  //   return this.user;

  // }
}
