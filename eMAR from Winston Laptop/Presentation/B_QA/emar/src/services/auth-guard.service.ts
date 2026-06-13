import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})

export class AuthGuardService implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router,
  ) { }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    if (!this.authService.isLoggedIn) {
      console.log('AuthGuardService: isLoggedIn(NOT AUTH): ', this.authService.isLoggedIn)
      return this.router.createUrlTree(
        ['/notauth', { message: 'No permission to enter'}]
      )
    } else {
      console.log('AuthGuardService: isLoggedIn(YES): ', this.authService.isLoggedIn)
      return true
    }
  }
}
