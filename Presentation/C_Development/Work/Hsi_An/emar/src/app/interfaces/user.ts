export interface User {
  id: number;
  typeCode?: string;
  typeDescription?: string;
  name?: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  nameSuffix?: string;
  userInitials?: string;
  displayInitialsIndicator?: boolean;
  site: {
    id: number;
    name: string;
    active: boolean;
    timeZoneName: string;
  };
}
