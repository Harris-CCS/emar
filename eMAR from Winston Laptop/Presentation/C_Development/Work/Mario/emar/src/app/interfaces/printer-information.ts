export interface PrinterInformation{
    id: number;
    siteId: number;
    address?: string;
    description?: string;
    isActive?: boolean;
    printQueueName?: string;
    tray?: string;
    deviceType?: string;
    pclType?: string;
    isLastUsed?: boolean;
  }
