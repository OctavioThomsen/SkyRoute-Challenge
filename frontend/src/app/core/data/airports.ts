import { Airport } from '../models/airport.model';

/**
 * The fixed set of airports supported by SkyRoute. Mirrors the backend registry.
 */
export const AIRPORTS: Airport[] = [
  { code: 'ATL', name: 'Hartsfield-Jackson International', city: 'Atlanta, Georgia', country: 'USA' },
  { code: 'JFK', name: 'John F. Kennedy International', city: 'New York, New York', country: 'USA' },
  { code: 'MIA', name: 'Miami International', city: 'Miami, Florida', country: 'USA' },
  { code: 'LAX', name: 'Los Angeles International', city: 'Los Angeles, California', country: 'USA' },
  { code: 'AEP', name: 'Aeroparque Jorge Newbery', city: 'Buenos Aires', country: 'Argentina' },
  { code: 'EZE', name: 'Ministro Pistarini International', city: 'Buenos Aires', country: 'Argentina' }
];

export function getAirport(code: string): Airport | undefined {
  return AIRPORTS.find((a) => a.code === code);
}

/**
 * A route is international when its endpoints are in different countries.
 */
export function isInternationalRoute(origin: string, destination: string): boolean {
  const from = getAirport(origin);
  const to = getAirport(destination);
  if (!from || !to) {
    return false;
  }
  return from.country !== to.country;
}
