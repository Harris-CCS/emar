import { Component, OnInit } from '@angular/core';

import { User } from './interfaces/user';
import { UserService } from '../services/user.service';

import { USER } from 'src/app/mockup/user';
import { Observable, TimeoutError } from 'rxjs';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { filter, map, mergeMap } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  title: string = 'emar';
  pageTitle$: Observable<string>;
  user: User;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private userService: UserService
  ) {
    // https://stackoverflow.com/questions/49632152/angular-2-how-to-access-active-route-outside-router-outlet
    this.pageTitle$ = this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => activatedRoute),
      map((route) => {
        while (route.firstChild) {
          route = route.firstChild;
        }
        return route;
      }),
      mergeMap((route) => route.data),
      map((data) => (data.hasOwnProperty('title') ? data.title : ''))
    );
    // this.loginUser();
  }

  ngOnInit() {
    this.loginUser();
  }

  loginUser() {
    // this.user = USER;
    const userId: number = 28;
    // Mock Data
    this.user = this.userService.getUser(userId);

    // API
    // this.userService.getUser(userId).subscribe((user) => {
    //   this.user = user;
    // });

    /* this.userService.fetchUser(244).subscribe(user => {
      console.log('USER');console.log(user)
    });
    */
    return this.user;
  }
}
