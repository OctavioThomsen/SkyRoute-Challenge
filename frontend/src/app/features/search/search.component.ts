import { Component, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { AIRPORTS } from '../../core/data/airports';
import { CabinClass, SearchRequest } from '../../core/models/flight.model';
import { FlightService } from '../../core/services/flight.service';
import { BookingStateService } from '../../core/services/booking-state.service';

@Component({
  selector: 'app-search',
  imports: [ReactiveFormsModule],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent {
  private readonly fb = inject(FormBuilder);
  private readonly flightService = inject(FlightService);
  private readonly bookingState = inject(BookingStateService);
  private readonly router = inject(Router);

  readonly airports = AIRPORTS;
  readonly cabinClasses: { value: CabinClass; label: string }[] = [
    { value: 'Economy', label: 'Economy' },
    { value: 'Business', label: 'Business' },
    { value: 'FirstClass', label: 'First Class' }
  ];
  readonly today = new Date().toISOString().split('T')[0];

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      origin: ['', Validators.required],
      destination: ['', Validators.required],
      departureDate: ['', Validators.required],
      passengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
      cabinClass: ['Economy' as CabinClass, Validators.required]
    },
    { validators: [sameAirportValidator, pastDateValidator] }
  );

  readonly sameAirport = computed(() => this.form.errors?.['sameAirport'] ?? false);

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
    if (this.form.invalid) {
      return;
    }

    const request = this.form.getRawValue() as SearchRequest;
    this.loading.set(true);

    this.flightService.search(request).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.bookingState.setSearch(request, result);
        this.router.navigate(['/results']);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err?.userMessage ?? 'Unable to search flights. Please try again.');
      }
    });
  }
}

function sameAirportValidator(group: AbstractControl): ValidationErrors | null {
  const origin = group.get('origin')?.value;
  const destination = group.get('destination')?.value;
  if (origin && destination && origin === destination) {
    return { sameAirport: true };
  }
  return null;
}

function pastDateValidator(group: AbstractControl): ValidationErrors | null {
  const value = group.get('departureDate')?.value;
  if (!value) {
    return null;
  }
  const selected = new Date(value + 'T00:00:00');
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return selected < today ? { pastDate: true } : null;
}
