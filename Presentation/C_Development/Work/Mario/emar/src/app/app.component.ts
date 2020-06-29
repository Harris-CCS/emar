import { Component } from '@angular/core';

import { User } from './interfaces/user';

import { USER } from 'src/app/mockup/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title: string = 'emar';
  user: User;

  loginUser() {
    this.user = USER
    return this.user;
  }
}
