import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { NotificationService } from '../notifications/notification.service';
import { errorInterceptor } from './error.interceptor';

class CapturingNotifications {
  readonly errors: string[] = [];
  error(message: string) {
    this.errors.push(message);
  }
  success(_: string) {}
  info(_: string) {}
}

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let notifications: CapturingNotifications;

  beforeEach(() => {
    notifications = new CapturingNotifications();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: NotificationService, useValue: notifications },
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('surfaces validation problem details to the notification service', (done) => {
    http.get('/api/tasks').subscribe({
      error: () => {
        expect(notifications.errors[0]).toBe('Title is required.');
        done();
      },
    });

    const req = controller.expectOne('/api/tasks');
    req.flush(
      {
        title: 'Invalid request',
        errors: { Title: ['Title is required.'] },
      },
      { status: 400, statusText: 'Bad Request' },
    );
  });

  it('falls back to a helpful message when the server is unreachable', (done) => {
    http.get('/api/tasks').subscribe({
      error: () => {
        expect(notifications.errors[0]).toContain('Cannot reach the API');
        done();
      },
    });

    const req = controller.expectOne('/api/tasks');
    req.error(new ProgressEvent('error'), { status: 0, statusText: '' });
  });
});
