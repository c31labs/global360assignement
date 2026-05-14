import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../../../core/tokens/api-base-url.token';
import {
  ChangeStatusRequest,
  CreateTaskRequest,
  Task,
  UpdateTaskRequest,
} from './task.model';

/**
 * Thin HTTP wrapper. No state, no business rules — the component layer composes
 * these calls. Errors bubble up via RxJS and are handled by the global error
 * interceptor (see core/http/error.interceptor.ts).
 */
@Injectable({ providedIn: 'root' })
export class TaskApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  list(): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.baseUrl}/api/tasks`);
  }

  create(request: CreateTaskRequest): Observable<Task> {
    return this.http.post<Task>(`${this.baseUrl}/api/tasks`, request);
  }

  update(id: string, request: UpdateTaskRequest): Observable<Task> {
    return this.http.put<Task>(`${this.baseUrl}/api/tasks/${id}`, request);
  }

  changeStatus(id: string, request: ChangeStatusRequest): Observable<Task> {
    return this.http.patch<Task>(`${this.baseUrl}/api/tasks/${id}/status`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/tasks/${id}`);
  }
}
