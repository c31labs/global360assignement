import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { NotificationService } from '../notifications/notification.service';

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, readonly string[]>;
}

/**
 * Translates RFC 7807 ProblemDetails into user-facing notifications and re-throws so
 * caller-specific recovery (e.g. form-level error UI) can still run. Keeping this in
 * one place avoids spraying try/catch through every service call.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      notifications.error(buildMessage(error));
      return throwError(() => error);
    }),
  );
};

function buildMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Cannot reach the API. Check that the server is running.';
  }

  const problem = error.error as ProblemDetails | null;
  if (problem?.errors) {
    const first = Object.values(problem.errors).flat()[0];
    if (first) {
      return first;
    }
  }

  return problem?.detail || problem?.title || `${error.status} ${error.statusText || 'request failed'}`;
}
