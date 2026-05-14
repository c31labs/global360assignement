import { CommonModule, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';

import {
  PRIORITY_LABEL,
  STATUS_LABEL,
  TASK_STATUSES,
  Task,
  TaskStatus,
} from '../data/task.model';

@Component({
  selector: 'app-task-card',
  standalone: true,
  imports: [CommonModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './task-card.component.html',
  styleUrl: './task-card.component.scss',
})
export class TaskCardComponent {
  @Input({ required: true }) task!: Task;
  @Input() busy = false;

  @Output() readonly statusChange = new EventEmitter<TaskStatus>();
  @Output() readonly edit = new EventEmitter<Task>();
  @Output() readonly remove = new EventEmitter<Task>();

  readonly statuses = TASK_STATUSES;
  readonly statusLabel = STATUS_LABEL;
  readonly priorityLabel = PRIORITY_LABEL;

  onStatusChange(value: string): void {
    this.statusChange.emit(value as TaskStatus);
  }
}
