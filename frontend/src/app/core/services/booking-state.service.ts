import { Injectable, signal } from '@angular/core';
import { Flight, SearchRequest } from '../models/flight.model';
import { BookingResponse } from '../models/booking.model';

/**
 * Holds the in-flight booking context (current search and selected flight) so it can be
 * shared across the Search -> Results -> Booking -> Confirmation routes without exposing
 * pricing data through query parameters.
 */
@Injectable({ providedIn: 'root' })
export class BookingStateService {
  readonly searchRequest = signal<SearchRequest | null>(null);
  readonly results = signal<Flight[]>([]);
  readonly selectedFlight = signal<Flight | null>(null);
  readonly confirmation = signal<BookingResponse | null>(null);

  setSearch(request: SearchRequest, results: Flight[]): void {
    this.searchRequest.set(request);
    this.results.set(results);
  }

  selectFlight(flight: Flight): void {
    this.selectedFlight.set(flight);
  }

  setConfirmation(confirmation: BookingResponse): void {
    this.confirmation.set(confirmation);
  }

  clear(): void {
    this.searchRequest.set(null);
    this.results.set([]);
    this.selectedFlight.set(null);
    this.confirmation.set(null);
  }
}
