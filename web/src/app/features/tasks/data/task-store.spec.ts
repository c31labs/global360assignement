import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../core/notifications/notification.service';
import { TaskApiService } from './task-api.service';
import { TaskStore } from './task-store';
import { makeTask } from './task.fixtures';
import { Task } from './task.model';

class StubNotifications {
  readonly messages: { kind: string; message: string }[] = [];
  success(message: string) {
    this.messages.push({ kind: 'success', message });
  }
  error(message: string) {
    this.messages.push({ kind: 'error', message });
  }
  info(message: string) {
    this.messages.push({ kind: 'info', message });
  }
}

describe('TaskStore', () => {
  let store: TaskStore;
  let api: jasmine.SpyObj<TaskApiService>;
  let notifications: StubNotifications;

  beforeEach(() => {
    api = jasmine.createSpyObj<TaskApiService>('TaskApiService', [
      'list',
      'create',
      'update',
      'changeStatus',
      'delete',
    ]);
    notifications = new StubNotifications();

    TestBed.configureTestingModule({
      providers: [
        TaskStore,
        { provide: TaskApiService, useValue: api },
        { provide: NotificationService, useValue: notifications },
      ],
    });
    store = TestBed.inject(TaskStore);
  });

  it('groups tasks by status', () => {
    const todo = makeTask({ id: '1', status: 'Todo' });
    const inProgress = makeTask({ id: '2', status: 'InProgress' });
    const done = makeTask({ id: '3', status: 'Done' });
    api.list.and.returnValue(of<Task[]>([todo, inProgress, done]));

    store.load();

    expect(store.grouped().Todo.map((t) => t.id)).toEqual(['1']);
    expect(store.grouped().InProgress.map((t) => t.id)).toEqual(['2']);
    expect(store.grouped().Done.map((t) => t.id)).toEqual(['3']);
  });

  it('prepends new tasks and notifies on create', (done) => {
    const existing = makeTask({ id: 'old' });
    const created = makeTask({ id: 'new', title: 'Brand new' });
    api.list.and.returnValue(of<Task[]>([existing]));
    api.create.and.returnValue(of(created));
    store.load();

    store
      .create({
        title: 'Brand new',
        description: null,
        priority: 'Medium',
        dueDate: null,
        assignee: null,
      })
      .subscribe(() => {
        expect(store.tasks().map((t) => t.id)).toEqual(['new', 'old']);
        expect(notifications.messages.at(-1)?.message).toContain('Brand new');
        done();
      });
  });

  it('removes deleted tasks from the local list', (done) => {
    const a = makeTask({ id: 'a' });
    const b = makeTask({ id: 'b' });
    api.list.and.returnValue(of<Task[]>([a, b]));
    api.delete.and.returnValue(of(undefined));
    store.load();

    store.delete('a').subscribe(() => {
      expect(store.tasks().map((t) => t.id)).toEqual(['b']);
      done();
    });
  });

  it('keeps loaded=false when load fails so the UI can retry', () => {
    api.list.and.returnValue(throwError(() => new Error('boom')));

    store.load();

    expect(store.loaded()).toBeFalse();
    expect(store.loading()).toBeFalse();
  });
});
