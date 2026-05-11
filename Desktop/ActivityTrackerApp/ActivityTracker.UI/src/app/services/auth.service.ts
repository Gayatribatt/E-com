import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiService } from './api.service';
import { AuthResponse, LoginRequest } from '../models';

const TOKEN_KEY = 'activity_tracker_token';
const ROLE_KEY = 'activity_tracker_role';
const USERNAME_KEY = 'activity_tracker_username';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private readonly api: ApiService,
    private readonly router: Router
  ) {}

  login(request: LoginRequest) {
    return this.api.login(request).pipe(
      tap((response) => this.persistSession(response))
    );
  }

  logout() {
    this.api.logout().subscribe({ error: () => undefined });
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(USERNAME_KEY);
    void this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem(TOKEN_KEY);
  }

  isAdmin(): boolean {
    return localStorage.getItem(ROLE_KEY) === 'Admin';
  }

  getUsername(): string {
    return localStorage.getItem(USERNAME_KEY) ?? 'Unknown';
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private persistSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(ROLE_KEY, response.role);
    localStorage.setItem(USERNAME_KEY, response.username);
  }
}
