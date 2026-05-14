import { InjectionToken } from '@angular/core';

/**
 * Base URL the API client should prefix to every request.
 *
 * Defaults to an empty string so requests like `/api/tasks` go through the
 * dev-server proxy (`proxy.conf.json`) or the same origin in production.
 * Tests override it with a deterministic value.
 */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => '',
});
