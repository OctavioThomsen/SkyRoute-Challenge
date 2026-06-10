import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BookingRequest, DocumentType } from '../../core/models/booking.model';
import { BookingService } from '../../core/services/booking.service';
import { BookingStateService } from '../../core/services/booking-state.service';
import { FlightCardComponent } from '../../shared/components/flight-card/flight-card.component';

@Component({
  selector: 'app-booking',
  imports: [ReactiveFormsModule, CurrencyPipe, FlightCardComponent],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.scss'
})
export class BookingComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly bookingService = inject(BookingService);
  private readonly bookingState = inject(BookingStateService);
  private readonly router = inject(Router);

  readonly flight = this.bookingState.selectedFlight;
  readonly passengers = computed(() => this.bookingState.searchRequest()?.passengers ?? 1);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // International routes require a passport; domestic routes use a national ID.
  readonly documentType = computed<DocumentType>(() =>
    this.flight()?.isInternational ? 'Passport' : 'NationalId'
  );
  readonly documentLabel = computed(() =>
    this.documentType() === 'Passport' ? 'Passport number' : 'National ID'
  );

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.required]]
  });

  ngOnInit(): void {
    if (!this.flight()) {
      this.router.navigate(['/search']);
      return;
    }
    this.applyDocumentValidators();
  }

  private applyDocumentValidators(): void {
    const control = this.form.controls.documentNumber;
    const pattern =
      this.documentType() === 'Passport' ? /^[A-Za-z0-9]{6,9}$/ : /^[A-Za-z0-9]{7,8}$/;
    control.setValidators([Validators.required, Validators.pattern(pattern)]);
    control.updateValueAndValidity();
  }

  hasError(controlName: string, error?: string): boolean {
    const control = this.form.get(controlName);
    if (!control || !(control.touched || control.dirty)) {
      return false;
    }
    return error ? control.hasError(error) : control.invalid;
  }

  submit(): void {
    this.errorMessage.set(null);
    this.form.markAllAsTouched();
    const flight = this.flight();
    if (this.form.invalid || !flight) {
      return;
    }

    const value = this.form.getRawValue();
    const request: BookingRequest = {
      flightNumber: flight.flightNumber,
      provider: flight.provider,
      origin: flight.origin,
      destination: flight.destination,
      departureTime: flight.departureTime,
      arrivalTime: flight.arrivalTime,
      durationMinutes: flight.durationMinutes,
      cabinClass: flight.cabinClass,
      pricePerPerson: flight.pricePerPerson,
      totalPrice: flight.totalPrice,
      passengers: this.passengers(),
      passengerDetails: {
        fullName: value.fullName,
        email: value.email,
        documentNumber: value.documentNumber,
        documentType: this.documentType()
      }
    };

    this.submitting.set(true);
    this.bookingService.create(request).subscribe({
      next: (response) => {
        this.submitting.set(false);
        this.bookingState.setConfirmation(response);
        this.router.navigate(['/confirmation']);
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(err?.userMessage ?? 'Unable to complete the booking. Please try again.');
      }
    });
  }

  back(): void {
    this.router.navigate(['/results']);
  }
}
