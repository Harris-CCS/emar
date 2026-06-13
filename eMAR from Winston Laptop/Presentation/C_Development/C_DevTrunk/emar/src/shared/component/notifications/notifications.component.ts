import { Component, OnInit } from '@angular/core';
// import { NOTIFICATIONS } from '../../../app/mockup/notifications';
import { Notification } from '../../../app/interfaces/notification';
import { UserService } from 'src/services/user.service';
import { UserStoreService } from 'src/services/user-store.service';

@Component({
  selector: 'notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss'],
})
export class NotificationsComponent {
  notifications: Notification[];

  constructor(
    private userService: UserService,
    private userStoreService: UserStoreService
  ) {}

  ngOnInit(): void {
    // this.notifications = NOTIFICATIONS;
    this.userService.getNotifications(this.userStoreService.userId, this.userStoreService.userSiteId).subscribe( data => {
      // console.log('NOTIFICATIONS', data);
      this.notifications = data;
    })
  }

}