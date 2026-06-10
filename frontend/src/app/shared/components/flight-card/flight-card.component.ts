import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { Flight } from '../../../core/models/flight.model';
import { getAirport } from '../../../core/data/airports';
import { DurationPipe } from '../../pipes/duration.pipe';

@Component({
  selector: 'app-flight-card',
  imports: [CurrencyPipe, DatePipe, DurationPipe],
  templateUrl: './flight-card.component.html',
  styleUrl: './flight-card.component.scss'
})
export class FlightCardComponent {
  readonly flight = input.required<Flight>();
  readonly passengers = input<number>(1);
  readonly selectable = input<boolean>(true);
  readonly select = output<Flight>();

  originCity(): string {
    return getAirport(this.flight().origin)?.city ?? this.flight().origin;
  }

  destinationCity(): string {
    return getAirport(this.flight().destination)?.city ?? this.flight().destination;
  }

  onSelect(): void {
    this.select.emit(this.flight());
  }
}
