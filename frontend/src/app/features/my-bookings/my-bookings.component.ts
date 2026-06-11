import { Component, inject, OnInit, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { BookingSummary } from '../../core/models/booking.model';
import { Flight } from '../../core/models/flight.model';
import { BookingService } from '../../core/services/booking.service';
import { FlightCardComponent } from '../../shared/components/flight-card/flight-card.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-my-bookings',
  imports: [CurrencyPipe, DatePipe, FlightCardComponent, EmptyStateComponent],
  templateUrl: './my-bookings.component.html',
  styleUrl: './my-bookings.component.scss'
})
export class MyBookingsComponent implements OnInit {
  private readonly bookingService = inject(BookingService);
  private readonly router = inject(Router);

  readonly bookings = signal<BookingSummary[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly expandedRef = signal<string | null>(null);

  ngOnInit(): void {
    this.bookingService.getAll().subscribe({
      next: (list) => {
        this.bookings.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load bookings. Please try again.');
        this.loading.set(false);
      }
    });
  }

  /** Adapts a BookingSummary to the Flight shape expected by FlightCardComponent. */
  toFlight(b: BookingSummary): Flight {
    return {
      flightNumber: b.flightNumber,
      provider: b.provider,
      origin: b.origin,
      destination: b.destination,
      departureTime: b.departureTime,
      arrivalTime: b.arrivalTime,
      durationMinutes: b.durationMinutes,
      cabinClass: b.cabinClass as Flight['cabinClass'],
      pricePerPerson: b.pricePerPerson,
      totalPrice: b.totalPrice,
      isInternational: false
    };
  }

  toggleDetail(ref: string): void {
    this.expandedRef.update(cur => cur === ref ? null : ref);
  }

  newSearch(): void {
    this.router.navigate(['/search']);
  }
}

