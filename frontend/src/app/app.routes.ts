import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'search' },
  {
    path: 'search',
    loadComponent: () =>
      import('./features/search/search.component').then((m) => m.SearchComponent),
    title: 'Search Flights | SkyRoute'
  },
  {
    path: 'results',
    loadComponent: () =>
      import('./features/results/results.component').then((m) => m.ResultsComponent),
    title: 'Flight Results | SkyRoute'
  },
  {
    path: 'booking',
    loadComponent: () =>
      import('./features/booking/booking.component').then((m) => m.BookingComponent),
    title: 'Booking | SkyRoute'
  },
  {
    path: 'confirmation',
    loadComponent: () =>
      import('./features/confirmation/confirmation.component').then((m) => m.ConfirmationComponent),
    title: 'Confirmation | SkyRoute'
  },
  { path: '**', redirectTo: 'search' }
];
