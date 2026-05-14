export type TaskStatus = 'Todo' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export const TASK_STATUSES: readonly TaskStatus[] = ['Todo', 'InProgress', 'Done'] as const;
export const TASK_PRIORITIES: readonly TaskPriority[] = ['Low', 'Medium', 'High'] as const;

export interface Task {
  readonly id: string;
  readonly title: string;
  readonly description: string | null;
  readonly status: TaskStatus;
  readonly priority: TaskPriority;
  readonly dueDate: string | null;
  readonly assignee: string | null;
  readonly createdAt: string;
  readonly updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDate: string | null;
  assignee: string | null;
}

export interface UpdateTaskRequest extends CreateTaskRequest {}

export interface ChangeStatusRequest {
  status: TaskStatus;
}

export const STATUS_LABEL: Record<TaskStatus, string> = {
  Todo: 'To do',
  InProgress: 'In progress',
  Done: 'Done',
};

export const PRIORITY_LABEL: Record<TaskPriority, string> = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
};
