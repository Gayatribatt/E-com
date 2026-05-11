import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './auth.guard';
import { LoginComponent } from './pages/login.component';
import { TasksComponent } from './pages/tasks.component';
import { AuditLogsComponent } from './pages/audit-logs.component';
import { DashboardComponent } from './pages/dashboard.component';
import { UsersComponent } from './pages/users.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'tasks', component: TasksComponent, canActivate: [authGuard] },
  { path: 'audit-logs', component: AuditLogsComponent, canActivate: [adminGuard] },
  { path: 'dashboard', component: DashboardComponent, canActivate: [adminGuard] },
  { path: 'users', component: UsersComponent, canActivate: [adminGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: '**', redirectTo: 'tasks' },
];
