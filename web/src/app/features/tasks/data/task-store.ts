import { Injectable, computed, inject, signal } from '@angular/core';
import { finalize, tap } from 'rxjs';

import { NotificationService } from '../../../core/notifications/notification.service';
import { TaskApiService } from './task-api.service';
import {
  ChangeStatusRequest,
  CreateTaskRequest,
  Task,
  TaskStatus,
  UpdateTaskRequest,
} from './task.model';

/**
 * Single source of truth for the task screen. Signals expose the synchronous read model
 * the components bind to; methods return observables so callers can chain UI actions
 * (e.g. closing a form on success). The store does not handle errors itself — the global
 * interceptor surfaces them — but it does clear the loading flag whichever way a request
 * resolves.
 */
@Injectable({ providedIn: 'root' })
export class TaskStore {
  private readonly api = inject(TaskApiService);
  private readonly notifications = inject(NotificationService);

  private readonly _tasks = signal<readonly Task[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _loaded = signal<boolean>(false);

  readonly tasks = this._tasks.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly loaded = this._loaded.asReadonly();

  readonly grouped = computed(() => {
    const byStatus: Record<TaskStatus, Task[]> = { Todo: [], InProgress: [], Done: [] };
    for (const task of this._tasks()) {
      byStatus[task.status].push(task);
    }
    return byStatus;
  });

  load(): void {
    this._loading.set(true);
    this.api
      .list()
      .pipe(finalize(() => this._loading.set(false)))
      .subscribe({
        next: (tasks) => {
          this._tasks.set(tasks);
          this._loaded.set(true);
        },
        error: () => {
          // Surface stays the interceptor's responsibility; we keep loaded=false so the UI can retry.
        },
      });
  }

  create(request: CreateTaskRequest) {
    return this.api.create(request).pipe(
      tap((task) => {
        this._tasks.update((current) => [task, ...current]);
        this.notifications.success(`Created '${task.title}'`);
      }),
    );
  }

  update(id: string, request: UpdateTaskRequest) {
    return this.api.update(id, request).pipe(
      tap((task) => {
        this.replace(task);
        this.notifications.success(`Updated '${task.title}'`);
      }),
    );
  }

  changeStatus(id: string, request: ChangeStatusRequest) {
    return this.api.changeStatus(id, request).pipe(tap((task) => this.replace(task)));
  }

  delete(id: string) {
    return this.api.delete(id).pipe(
      tap(() => {
        const removed = this._tasks().find((t) => t.id === id);
        this._tasks.update((current) => current.filter((t) => t.id !== id));
        if (removed) {
          this.notifications.success(`Deleted '${removed.title}'`);
        }
      }),
    );
  }

  private replace(task: Task) {
    this._tasks.update((current) => current.map((t) => (t.id === task.id ? task : t)));
  }
}
