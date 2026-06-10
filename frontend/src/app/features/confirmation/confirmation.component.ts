import { Component, inject, OnInit } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { BookingStateService } from '../../core/services/booking-state.service';

@Component({
  selector: 'app-confirmation',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './confirmation.component.html',
  styleUrl: './confirmation.component.scss'
})
export class ConfirmationComponent implements OnInit {
  private readonly bookingState = inject(BookingStateService);
  private readonly router = inject(Router);

  readonly confirmation = this.bookingState.confirmation;

  ngOnInit(): void {
    if (!this.confirmation()) {
      this.router.navigate(['/search']);
    }
  }

  bookAnother(): void {
    this.bookingState.clear();
    this.router.navigate(['/search']);
  }
}
