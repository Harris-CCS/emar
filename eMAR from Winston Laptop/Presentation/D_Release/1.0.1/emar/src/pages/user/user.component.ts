import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs'
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { filter, map, mergeMap } from 'rxjs/operators';

import { UserStoreService} from '../../services/user-store.service';
import { SiteStoreService } from 'src/services/site-store.service'
import { MyPatientsStoreService } from 'src/services/my-patients-store.service'
import { AllPatientsStoreService } from 'src/services/all-patients-store.service'
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'pages-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {
  hasLoaded : boolean = false
  pageTitle$: Observable<string>;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    public userStoreService: UserStoreService,
    public authService: AuthService,
    private siteStoreService: SiteStoreService,
    // private myPatientsStoreService: MyPatientsStoreService,
    // private allPatientsStoreService: AllPatientsStoreService,
  ) { }

  ngOnInit(): void {
    console.log('UserComponent.ngOnInit')

    console.log('router: ', this.router.events)
    // https://stackoverflow.com/questions/49632152/angular-2-how-to-access-active-route-outside-router-outlet
    this.pageTitle$ = this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => this.activatedRoute),
      map((route) => {
        while (route.firstChild) {
          route = route.firstChild;
        }
        return route;
      }),
      mergeMap((route) => route.data),
      map((data) => (data.hasOwnProperty('title') ? data.title : ''))
    );
    
    this.userStoreService.user$.subscribe(() => {
      console.log('user has finished', this.userStoreService.user)
      if (this.userStoreService.user.id) {
        this.hasLoaded = true
        this.authService.login()
        // this.hazLoaded.next(true)
      }
    })

    this.userStoreService.fetchUser()
  }

}
