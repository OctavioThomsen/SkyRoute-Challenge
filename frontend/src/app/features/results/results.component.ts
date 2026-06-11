import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Flight, SortOption } from '../../core/models/flight.model';
import { getAirport } from '../../core/data/airports';
import { BookingStateService } from '../../core/services/booking-state.service';
import { FlightCardComponent } from '../../shared/components/flight-card/flight-card.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-results',
  imports: [FlightCardComponent, EmptyStateComponent],
  templateUrl: './results.component.html',
  styleUrl: './results.component.scss'
})
export class ResultsComponent {
  private readonly bookingState = inject(BookingStateService);
  private readonly router = inject(Router);

  readonly searchRequest = this.bookingState.searchRequest;
  readonly sortOption = signal<SortOption>('priceLowToHigh');

  readonly sortOptions: { value: SortOption; label: string }[] = [
    { value: 'priceLowToHigh', label: 'Price: Low to High' },
    { value: 'priceHighToLow', label: 'Price: High to Low' },
    { value: 'durationShortest', label: 'Duration: Shortest First' },
    { value: 'departureTime', label: 'Departure Time' }
  ];

  // Sorting happens entirely in the frontend, with no additional API calls.
  readonly sortedFlights = computed<Flight[]>(() =>
    this.sortFlights(this.bookingState.matches())
  );

  // Suggestions for the same route on other dates/cabins, shown below a divider.
  readonly sortedSuggestions = computed<Flight[]>(() =>
    this.sortFlights(this.bookingState.suggestions())
  );

  readonly routeLabel = computed(() => {
    const request = this.searchRequest();
    if (!request) {
      return '';
    }
    const origin = getAirport(request.origin)?.city ?? request.origin;
    const destination = getAirport(request.destination)?.city ?? request.destination;
    return `${origin} to ${destination}`;
  });

  private sortFlights(source: Flight[]): Flight[] {
    const flights = [...source];
    switch (this.sortOption()) {
      case 'priceHighToLow':
        return flights.sort((a, b) => b.totalPrice - a.totalPrice);
      case 'durationShortest':
        return flights.sort((a, b) => a.durationMinutes - b.durationMinutes);
      case 'departureTime':
        return flights.sort(
          (a, b) => new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime()
        );
      case 'priceLowToHigh':
      default:
        return flights.sort((a, b) => a.totalPrice - b.totalPrice);
    }
  }

  trackFlight(_index: number, flight: Flight): string {
    return `${flight.flightNumber}-${flight.departureTime}-${flight.cabinClass}`;
  }

  onSortChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as SortOption;
    this.sortOption.set(value);
  }

  onSelect(flight: Flight): void {
    this.bookingState.selectFlight(flight);
    this.router.navigate(['/booking']);
  }

  newSearch(): void {
    this.router.navigate(['/search']);
  }
}
