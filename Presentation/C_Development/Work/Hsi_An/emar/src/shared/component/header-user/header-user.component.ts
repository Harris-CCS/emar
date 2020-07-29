import { Component, OnInit, Input } from '@angular/core';

import { User } from '../../../app/interfaces/user';

@Component({
  selector: 'header-user',
  templateUrl: './header-user.component.html',
  styleUrls: ['./header-user.component.scss'],
})
export class HeaderUserComponent implements OnInit {
  @Input() user: User;
  @Input() title: string;

  constructor() {}
  ngOnInit() {}

  onLogout() {
    this.user = null;
    this.user;
  }

  assignExternalRoute(location: string, launchInNewTab?: boolean): string {
    const url = `http://ros-demo-zx01.picis.com${location}`;
    if (launchInNewTab) {
      window.open(url, '_blank');
    } else {
      return url;
    }
  }
}
