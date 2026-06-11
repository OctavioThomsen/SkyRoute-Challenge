import { Injectable, signal } from '@angular/core';
import { Flight, SearchRequest, SearchResult } from '../models/flight.model';
import { BookingResponse } from '../models/booking.model';

/**
 * Holds the in-flight booking context (current search and selected flight) so it can be
 * shared across the Search -> Results -> Booking -> Confirmation routes without exposing
 * pricing data through query parameters.
 */
@Injectable({ providedIn: 'root' })
export class BookingStateService {
  readonly searchRequest = signal<SearchRequest | null>(null);
  readonly matches = signal<Flight[]>([]);
  readonly suggestions = signal<Flight[]>([]);
  readonly selectedFlight = signal<Flight | null>(null);
  readonly confirmation = signal<BookingResponse | null>(null);

  setSearch(request: SearchRequest, result: SearchResult): void {
    this.searchRequest.set(request);
    this.matches.set(result.matches);
    this.suggestions.set(result.suggestions);
  }

  selectFlight(flight: Flight): void {
    this.selectedFlight.set(flight);
  }

  setConfirmation(confirmation: BookingResponse): void {
    this.confirmation.set(confirmation);
  }

  clear(): void {
    this.searchRequest.set(null);
    this.matches.set([]);
    this.suggestions.set([]);
    this.selectedFlight.set(null);
    this.confirmation.set(null);
  }
}
