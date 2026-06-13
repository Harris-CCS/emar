export interface UserRememberedListOrder {
  userId: number;
  siteId: number;
  medicationId: number;
  dose?: number;
  medicationUnitId?: number;
  medicationRouteId?: number;
  priority?: string;
  frequencyId?: number;
  orderNotes?: string;
  duration?: number;
  durationUnitId?: number;
}