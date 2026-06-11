export type CabinClass = 'Economy' | 'Business' | 'FirstClass';

export interface SearchRequest {
  origin: string;
  destination: string;
  departureDate: string;
  passengers: number;
  cabinClass: CabinClass;
}

export interface Flight {
  flightNumber: string;
  provider: string;
  origin: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  cabinClass: CabinClass;
  pricePerPerson: number;
  totalPrice: number;
  isInternational: boolean;
}

export interface SearchResult {
  matches: Flight[];
  suggestions: Flight[];
}

export type SortOption =
  | 'priceLowToHigh'
  | 'priceHighToLow'
  | 'durationShortest'
  | 'departureTime';
