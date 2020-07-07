export interface Order {
    id: number,
    patientId: number,
    startTime: string,
    endTime: string,
    name: string,
    dose: string,
    route: string,
    signedOn: string,
    signedBy: string,
    allergies?: any,
    drugs?: any 
}
