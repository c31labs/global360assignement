import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import {
  CreateTaskRequest,
  PRIORITY_LABEL,
  TASK_PRIORITIES,
  Task,
  TaskPriority,
} from '../data/task.model';
import { todayIsoDate } from '../util/today';

interface TaskFormShape {
  title: FormControl<string>;
  description: FormControl<string>;
  priority: FormControl<TaskPriority>;
  dueDate: FormControl<string>;
  assignee: FormControl<string>;
}

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss',
})
export class TaskFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder).nonNullable;

  @Input() task: Task | null = null;
  @Input() submitting = false;

  @Output() readonly save = new EventEmitter<CreateTaskRequest>();
  @Output() readonly cancel = new EventEmitter<void>();

  readonly priorities = TASK_PRIORITIES;
  readonly priorityLabel = PRIORITY_LABEL;
  readonly minDueDate = todayIsoDate();

  readonly form = this.fb.group<TaskFormShape>({
    title: this.fb.control('', [Validators.required, Validators.maxLength(200)]),
    description: this.fb.control('', [Validators.maxLength(2000)]),
    priority: this.fb.control<TaskPriority>('Medium'),
    dueDate: this.fb.control(''),
    assignee: this.fb.control('', [Validators.maxLength(120)]),
  });

  get isEditing(): boolean {
    return this.task !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('task' in changes) {
      this.resetFromTask();
    }
  }

  submit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.save.emit({
      title: value.title.trim(),
      description: value.description.trim() ? value.description.trim() : null,
      priority: value.priority,
      dueDate: value.dueDate ? new Date(value.dueDate).toISOString() : null,
      assignee: value.assignee.trim() ? value.assignee.trim() : null,
    });
  }

  private resetFromTask(): void {
    if (this.task) {
      this.form.reset({
        title: this.task.title,
        description: this.task.description ?? '',
        priority: this.task.priority,
        dueDate: this.task.dueDate ? this.task.dueDate.substring(0, 10) : '',
        assignee: this.task.assignee ?? '',
      });
    } else {
      this.form.reset({
        title: '',
        description: '',
        priority: 'Medium',
        dueDate: '',
        assignee: '',
      });
    }
  }
}
