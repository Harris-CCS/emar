import { Site } from './site';
import { UserSetting } from './user-setting';

export interface User {
  id?: number;
  typeCode?: string;
  typeDescription?: string;
  name?: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  nameSuffix?: string;
  userInitials?: string;
  displayName?: string;
  displayInitialsIndicator?: boolean;
  // site?: {
  //   id: number;
  //   name: string;
  //   active: boolean;
  //   timeZoneName: string;
  // };
  site?: Site;
  userSettings?: Array<UserSetting>;
}
