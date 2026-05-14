import { Task } from './task.model';

let counter = 0;

export function makeTask(overrides: Partial<Task> = {}): Task {
  counter += 1;
  const now = new Date('2026-05-14T12:00:00Z').toISOString();
  return {
    id: overrides.id ?? `task-${counter}`,
    title: overrides.title ?? `Task ${counter}`,
    description: overrides.description ?? null,
    status: overrides.status ?? 'Todo',
    priority: overrides.priority ?? 'Medium',
    dueDate: overrides.dueDate ?? null,
    assignee: overrides.assignee ?? null,
    createdAt: overrides.createdAt ?? now,
    updatedAt: overrides.updatedAt ?? now,
  };
}
