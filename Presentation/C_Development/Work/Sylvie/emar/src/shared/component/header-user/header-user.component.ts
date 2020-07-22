import { Component, OnInit, Input } from '@angular/core';

import { User } from '../../../app/interfaces/user';

@Component({
  selector: 'header-user',
  templateUrl: './header-user.component.html',
  styleUrls: ['./header-user.component.scss']
})
export class HeaderUserComponent implements OnInit {
  @Input() user: User;
  @Input() title: string;

 constructor() {}
 ngOnInit() {}

  onLogout() {
    this.user = null;
    this.user
  }

}
