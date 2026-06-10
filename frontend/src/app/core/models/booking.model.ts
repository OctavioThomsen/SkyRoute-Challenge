export type DocumentType = 'Passport' | 'NationalId';

export interface PassengerDetails {
  fullName: string;
  email: string;
  documentNumber: string;
  documentType: DocumentType;
}

export interface BookingRequest {
  flightNumber: string;
  provider: string;
  origin: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  cabinClass: string;
  pricePerPerson: number;
  totalPrice: number;
  passengers: number;
  passengerDetails: PassengerDetails;
}

export interface BookingResponse {
  bookingReference: string;
  flightNumber: string;
  passengerName: string;
  totalPrice: number;
  createdAt: string;
}
