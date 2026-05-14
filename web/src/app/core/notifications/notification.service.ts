import { Injectable, signal } from '@angular/core';

export type NotificationKind = 'success' | 'error' | 'info';

export interface Notification {
  readonly id: number;
  readonly kind: NotificationKind;
  readonly message: string;
}

/**
 * Minimal in-memory notification bus. Components push messages; a single host component
 * renders them. Auto-dismissed after a few seconds so transient feedback doesn't pile up.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly _items = signal<readonly Notification[]>([]);
  private nextId = 1;

  readonly items = this._items.asReadonly();

  success(message: string): void {
    this.push('success', message);
  }

  error(message: string): void {
    this.push('error', message);
  }

  info(message: string): void {
    this.push('info', message);
  }

  dismiss(id: number): void {
    this._items.update((current) => current.filter((n) => n.id !== id));
  }

  private push(kind: NotificationKind, message: string): void {
    const id = this.nextId++;
    this._items.update((current) => [...current, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), kind === 'error' ? 6000 : 3500);
  }
}
