import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { NotificationService } from './notification.service';

@Component({
  selector: 'app-notification-host',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ul class="notifications" role="status" aria-live="polite">
      @for (item of notifications.items(); track item.id) {
        <li [class]="'notification notification--' + item.kind">
          <span class="notification__message">{{ item.message }}</span>
          <button
            type="button"
            class="notification__dismiss"
            aria-label="Dismiss notification"
            (click)="notifications.dismiss(item.id)">
            &times;
          </button>
        </li>
      }
    </ul>
  `,
  styles: [
    `
      :host {
        position: fixed;
        top: 1rem;
        right: 1rem;
        z-index: 1000;
        width: min(360px, calc(100vw - 2rem));
      }
      .notifications {
        list-style: none;
        margin: 0;
        padding: 0;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .notification {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.625rem 0.875rem;
        border-radius: 0.5rem;
        background: #1f2937;
        color: #f9fafb;
        font-size: 0.9rem;
        box-shadow: 0 6px 14px rgba(0, 0, 0, 0.18);
      }
      .notification--success {
        background: #064e3b;
      }
      .notification--error {
        background: #7f1d1d;
      }
      .notification__message {
        flex: 1;
      }
      .notification__dismiss {
        background: transparent;
        border: none;
        color: inherit;
        font-size: 1.1rem;
        cursor: pointer;
        line-height: 1;
      }
    `,
  ],
})
export class NotificationHostComponent {
  readonly notifications = inject(NotificationService);
}
