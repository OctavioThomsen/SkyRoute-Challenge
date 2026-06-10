import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Flight, SearchRequest } from '../models/flight.model';

@Injectable({ providedIn: 'root' })
export class FlightService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/flights`;

  search(request: SearchRequest): Observable<Flight[]> {
    return this.http.post<Flight[]>(`${this.baseUrl}/search`, request);
  }
}
