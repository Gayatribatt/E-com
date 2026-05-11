import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ActivitySummary,
  AuthResponse,
  AuditLog,
  LoginRequest,
  PagedResult,
  RecentActivity,
  RegisterRequest,
  TaskItem,
  UpsertTask,
} from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, request);
  }

  register(request: RegisterRequest): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/auth/register`, request);
  }

  logout(): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/auth/logout`, {});
  }

  getTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${this.baseUrl}/tasks`);
  }

  createTask(request: UpsertTask): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${this.baseUrl}/tasks`, request);
  }

  updateTask(id: number, request: UpsertTask): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.baseUrl}/tasks/${id}`, request);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/tasks/${id}`);
  }

  getAuditLogs(filter: Record<string, string>): Observable<PagedResult<AuditLog>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value?.trim()) {
        params = params.set(key, value);
      }
    }
    return this.http.get<PagedResult<AuditLog>>(`${this.baseUrl}/audit-logs`, { params });
  }

  exportAuditCsv(filter: Record<string, string>): Observable<Blob> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value?.trim()) {
        params = params.set(key, value);
      }
    }
    return this.http.get(`${this.baseUrl}/audit-logs/export/csv`, {
      params,
      responseType: 'blob',
    });
  }

  getDashboardSummary(): Observable<ActivitySummary> {
    return this.http.get<ActivitySummary>(`${this.baseUrl}/dashboard/summary`);
  }

  getRecentActivities(): Observable<RecentActivity[]> {
    return this.http.get<RecentActivity[]>(`${this.baseUrl}/dashboard/recent-activities`);
  }

  authHeaders(token: string): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }
}
