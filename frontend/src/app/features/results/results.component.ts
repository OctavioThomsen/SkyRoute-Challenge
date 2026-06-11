import { AfterViewInit, Component, computed, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { Flight, SortOption } from '../../core/models/flight.model';
import { getAirport } from '../../core/data/airports';
import { BookingStateService } from '../../core/services/booking-state.service';
import { FlightService } from '../../core/services/flight.service';
import { FlightCardComponent } from '../../shared/components/flight-card/flight-card.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

interface DateSliderItem {
  date: string;              // 'YYYY-MM-DD'
  minPricePerPerson: number | null;
  isSelected: boolean;
}

@Component({
  selector: 'app-results',
  imports: [FlightCardComponent, EmptyStateComponent, DatePipe, CurrencyPipe],
  templateUrl: './results.component.html',
  styleUrl: './results.component.scss'
})
export class ResultsComponent implements AfterViewInit {
  private readonly bookingState = inject(BookingStateService);
  private readonly flightService = inject(FlightService);
  private readonly router = inject(Router);

  @ViewChild('dateSlider') private dateSliderRef!: ElementRef<HTMLElement>;

  readonly searchRequest = this.bookingState.searchRequest;

  // The date the user originally arrived with — kept permanently in the slider
  // so they can see that the chosen date has no flights after switching.
  private readonly anchorDate = this.bookingState.searchRequest()?.departureDate ?? '';

  readonly searching = signal(false);
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

  // Date slider: one chip per date that has a flight on this route with the searched cabin.
  readonly dateSliderItems = computed<DateSliderItem[]>(() => {
    const request = this.searchRequest();
    if (!request) return [];

    const priceMap = new Map<string, number | null>();

    for (const flight of this.bookingState.matches()) {
      const d = flight.departureTime.substring(0, 10);
      const cur = priceMap.get(d);
      if (cur === undefined || cur === null || flight.pricePerPerson < cur) {
        priceMap.set(d, flight.pricePerPerson);
      }
    }
    for (const flight of this.bookingState.suggestions()) {
      if (flight.cabinClass !== request.cabinClass) continue;
      const d = flight.departureTime.substring(0, 10);
      const cur = priceMap.get(d);
      if (cur === undefined || cur === null || flight.pricePerPerson < cur) {
        priceMap.set(d, flight.pricePerPerson);
      }
    }

    // Always keep both the currently selected date and the original anchor date
    // (shown with — if no flights that day).
    if (!priceMap.has(request.departureDate)) {
      priceMap.set(request.departureDate, null);
    }
    if (this.anchorDate && !priceMap.has(this.anchorDate)) {
      priceMap.set(this.anchorDate, null);
    }

    return Array.from(priceMap.entries())
      .map(([date, price]) => ({
        date,
        minPricePerPerson: price,
        isSelected: date === request.departureDate
      }))
      .sort((a, b) => a.date.localeCompare(b.date));
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

  ngAfterViewInit(): void {
    setTimeout(() => this.scrollSelectedIntoView(), 120);
  }

  selectDate(date: string): void {
    const request = this.searchRequest();
    if (!request || date === request.departureDate) return;
    const newRequest = { ...request, departureDate: date };
    this.searching.set(true);
    this.flightService.search(newRequest).subscribe({
      next: (result) => {
        this.bookingState.setSearch(newRequest, result);
        this.searching.set(false);
        setTimeout(() => this.scrollSelectedIntoView(), 50);
      },
      error: () => this.searching.set(false)
    });
  }

  private scrollSelectedIntoView(): void {
    const container = this.dateSliderRef?.nativeElement;
    if (!container) return;
    const selected = container.querySelector('.date-chip--selected') as HTMLElement | null;
    selected?.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
  }
}
