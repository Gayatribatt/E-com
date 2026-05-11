export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  userId: number;
  username: string;
  role: 'Admin' | 'User';
}

export interface RegisterRequest {
  username: string;
  password: string;
  role: 'Admin' | 'User';
}

export interface TaskItem {
  id: number;
  title: string;
  description: string;
  isCompleted: boolean;
  createdByUserId: number;
}

export interface UpsertTask {
  title: string;
  description: string;
  isCompleted: boolean;
}

export interface AuditLog {
  id: number;
  userId?: number;
  action: string;
  entityName: string;
  entityId: string;
  oldValuesJson: string;
  newValuesJson: string;
  ipAddress: string;
  timestampUtc: string;
  changedFields: Array<{ field: string; oldValue?: string; newValue?: string }>;
}

export interface PagedResult<T> {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  items: T[];
}

export interface ActivitySummary {
  totalAuditLogs: number;
  totalActivityLogs: number;
  failedLoginAttempts: number;
  successfulLogins: number;
  totalDeletes: number;
}

export interface RecentActivity {
  id: number;
  userId?: number;
  activityType: string;
  description: string;
  endpoint: string;
  statusCode: number;
  timestampUtc: string;
}
