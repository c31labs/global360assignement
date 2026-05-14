import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';

import {
  CreateTaskRequest,
  STATUS_LABEL,
  TASK_STATUSES,
  Task,
} from '../data/task.model';
import { TaskStore } from '../data/task-store';
import { TaskCardComponent } from './task-card.component';
import { TaskFormComponent } from './task-form.component';

@Component({
  selector: 'app-task-board',
  standalone: true,
  imports: [CommonModule, TaskFormComponent, TaskCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './task-board.component.html',
  styleUrl: './task-board.component.scss',
})
export class TaskBoardComponent implements OnInit {
  readonly store = inject(TaskStore);
  readonly statuses = TASK_STATUSES;
  readonly statusLabel = STATUS_LABEL;

  readonly editingTask = signal<Task | null>(null);
  readonly submitting = signal(false);
  readonly busyIds = signal<ReadonlySet<string>>(new Set());

  readonly grouped = computed(() => this.store.grouped());
  readonly totals = computed(() => this.store.tasks().length);

  ngOnInit(): void {
    this.store.load();
  }

  refresh(): void {
    this.store.load();
  }

  startEdit(task: Task): void {
    this.editingTask.set(task);
  }

  cancelEdit(): void {
    this.editingTask.set(null);
  }

  handleSave(request: CreateTaskRequest): void {
    this.submitting.set(true);
    const editing = this.editingTask();
    const observable = editing
      ? this.store.update(editing.id, request)
      : this.store.create(request);

    observable.subscribe({
      next: () => {
        this.editingTask.set(null);
      },
      complete: () => this.submitting.set(false),
      error: () => this.submitting.set(false),
    });
  }

  changeStatus(task: Task, status: Task['status']): void {
    if (task.status === status) {
      return;
    }
    this.markBusy(task.id);
    this.store.changeStatus(task.id, { status }).subscribe({
      complete: () => this.unmarkBusy(task.id),
      error: () => this.unmarkBusy(task.id),
    });
  }

  remove(task: Task): void {
    const confirmed = typeof window === 'undefined'
      ? true
      : window.confirm(`Delete '${task.title}'? This cannot be undone.`);
    if (!confirmed) {
      return;
    }

    this.markBusy(task.id);
    this.store.delete(task.id).subscribe({
      complete: () => this.unmarkBusy(task.id),
      error: () => this.unmarkBusy(task.id),
    });
  }

  isBusy(id: string): boolean {
    return this.busyIds().has(id);
  }

  private markBusy(id: string): void {
    this.busyIds.update((current) => {
      const next = new Set(current);
      next.add(id);
      return next;
    });
  }

  private unmarkBusy(id: string): void {
    this.busyIds.update((current) => {
      const next = new Set(current);
      next.delete(id);
      return next;
    });
  }
}
