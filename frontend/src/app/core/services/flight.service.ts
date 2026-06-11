import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SearchRequest, SearchResult } from '../models/flight.model';

@Injectable({ providedIn: 'root' })
export class FlightService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/flights`;

  search(request: SearchRequest): Observable<SearchResult> {
    return this.http.post<SearchResult>(`${this.baseUrl}/search`, request);
  }
}
