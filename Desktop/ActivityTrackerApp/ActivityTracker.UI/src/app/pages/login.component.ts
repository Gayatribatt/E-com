import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  template: `
    <section class="login-shell">
      <div class="card">
        <h2>Login</h2>
        <p class="subtitle">Sign in to access dashboard analytics.</p>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <label for="username">Username</label>
          <input id="username" type="text" formControlName="username" />
          <label for="password">Password</label>
          <input id="password" type="password" formControlName="password" />
          <button type="submit" [disabled]="form.invalid || loading">
            {{ loading ? 'Signing in...' : 'Login' }}
          </button>
        </form>
        <p class="error" *ngIf="error">{{ error }}</p>
        <p class="hint">Use admin / Admin@123</p>
      </div>
    </section>
  `,
  styles: [`
    .login-shell {
      min-height: calc(100vh - 160px);
      display: grid;
      place-items: center;
      padding: 20px 12px;
    }
    .card {
      width: min(100%, 440px);
      border: 1px solid #334155;
      border-radius: 14px;
      padding: 20px;
      background: rgba(2, 6, 23, 0.72);
      backdrop-filter: blur(4px);
      box-shadow: 0 12px 30px rgba(15, 23, 42, 0.25);
    }
    h2 { margin: 0; color: #f8fafc; font-size: 30px; }
    .subtitle { margin: 8px 0 18px; color: #94a3b8; font-size: 13px; }
    form { display: grid; gap: 10px; }
    label { color: #e2e8f0; font-size: 14px; }
    input {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #475569;
      border-radius: 8px;
      background: rgba(255, 255, 255, 0.95);
      font-size: 14px;
    }
    button {
      margin-top: 12px;
      padding: 10px 12px;
      border-radius: 8px;
      border: 0;
      background: linear-gradient(90deg, #2563eb, #0ea5e9);
      color: #fff;
      font-weight: 600;
      cursor: pointer;
    }
    button:disabled { opacity: 0.7; cursor: not-allowed; }
    .error { color: #fca5a5; margin-top: 12px; font-size: 13px; }
    .hint { color: #94a3b8; font-size: 12px; margin-top: 12px; }

    @media (max-width: 768px) {
      .login-shell { min-height: calc(100vh - 120px); padding: 16px 10px; }
      .card { padding: 16px; border-radius: 12px; }
      h2 { font-size: 26px; }
    }

    @media (max-width: 480px) {
      .login-shell { min-height: calc(100vh - 100px); padding: 12px 8px; }
      .card { width: 100%; padding: 14px; border-radius: 10px; }
      h2 { font-size: 24px; }
      .subtitle, .hint { font-size: 12px; }
      input, button { font-size: 14px; }
    }
  `],
})
export class LoginComponent {
  loading = false;
  error = '';
  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {
    this.form = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth.login(this.form.getRawValue() as { username: string; password: string }).subscribe({
      next: () => {
        this.loading = false;
        void this.router.navigate(['/tasks']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.error ?? 'Login failed.';
      },
    });
  }
}
