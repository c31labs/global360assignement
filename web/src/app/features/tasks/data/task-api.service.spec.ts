import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../../core/tokens/api-base-url.token';
import { TaskApiService } from './task-api.service';
import { makeTask } from './task.fixtures';

describe('TaskApiService', () => {
  let service: TaskApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'http://api.test' },
      ],
    });
    service = TestBed.inject(TaskApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('GETs the tasks endpoint with the configured base URL', () => {
    const fixture = [makeTask({ title: 'A' }), makeTask({ title: 'B' })];

    service.list().subscribe((result) => expect(result).toEqual(fixture));

    const req = http.expectOne('http://api.test/api/tasks');
    expect(req.request.method).toBe('GET');
    req.flush(fixture);
  });

  it('POSTs the create request body unchanged', () => {
    const body = {
      title: 'Plan',
      description: null,
      priority: 'High' as const,
      dueDate: null,
      assignee: null,
    };

    service.create(body).subscribe();

    const req = http.expectOne('http://api.test/api/tasks');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush(makeTask(body));
  });

  it('PATCHes the status endpoint with the new status', () => {
    service.changeStatus('abc', { status: 'Done' }).subscribe();

    const req = http.expectOne('http://api.test/api/tasks/abc/status');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'Done' });
    req.flush(makeTask({ id: 'abc', status: 'Done' }));
  });

  it('DELETEs by id', () => {
    service.delete('abc').subscribe();

    const req = http.expectOne('http://api.test/api/tasks/abc');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
