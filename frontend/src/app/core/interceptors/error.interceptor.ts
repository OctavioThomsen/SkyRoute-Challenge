import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

/**
 * Normalizes HTTP errors into a user-friendly message attached to the thrown error,
 * so components can display a consistent error banner.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'Something went wrong. Please try again.';

      if (error.status === 0) {
        message = 'Unable to reach the server. Check your connection and try again.';
      } else if (error.status === 400) {
        message = 'Please review the form. Some details are invalid.';
      } else if (error.status >= 500) {
        message = 'The server encountered an error. Please try again shortly.';
      }

      return throwError(() => ({ ...error, userMessage: message }));
    })
  );
};
