import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a duration given in minutes as a compact "Xh Ym" string.
 */
@Pipe({ name: 'duration' })
export class DurationPipe implements PipeTransform {
  transform(totalMinutes: number | null | undefined): string {
    if (totalMinutes == null || totalMinutes < 0) {
      return '';
    }
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    if (hours === 0) {
      return `${minutes}m`;
    }
    if (minutes === 0) {
      return `${hours}h`;
    }
    return `${hours}h ${minutes}m`;
  }
}
